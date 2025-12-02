using UnityEngine;
using UnityEngine.SceneManagement;

public class MyPageManager : MonoBehaviour
{
    public static MyPageManager Instance { get; private set; }

    private string currentCategory = "Accessories";

    private StoreItemData[] storeItems;
    public int[] ownedItems;

    private bool receivedStoreItems = false;
    private bool receivedOwnedItems = false;

    [Header("Default Meshes (0번 아이템)")]
    public Mesh defaultAccessories;
    public Mesh defaultHairstyle;
    public Mesh defaultHat;
    public Mesh defaultOuterwear;
    public Mesh defaultPants;
    public Mesh defaultShoes;
    public Mesh defaultGloves;
    public Mesh defaultGlasses;
    public Mesh defaultFullBody;
    public Mesh defaultFaces;
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }

    private void OnDisable()
    {
        NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void Start()
    {
        if (!ClientSession.Instance.IsLoggedIn)
            return;

        MyPageUI.Instance.ApplyNickname(ClientSession.Instance.Nickname);

        RequestPlayerInfo();
        RequestAppearance();

        RequestStoreItems(currentCategory);
        RequestOwnedItems(currentCategory);
    }

    private void RequestPlayerInfo()
    {
        var req = new PlayerInfoRequest(ClientSession.Instance.Nickname);
        NetworkManager.Instance.SendJson(req);
    }

    private void RequestAppearance()
    {
        var req = new AppearanceRequest
        {
            nickname = ClientSession.Instance.Nickname
        };
        NetworkManager.Instance.SendJson(req);
    }

    private void RequestStoreItems(string category)
    {
        var req = new StoreItemRequest
        {
            cmd = "get_store_items",
            category = category
        };
        NetworkManager.Instance.SendJson(req);
    }

    private void RequestOwnedItems(string category)
    {
        var req = new OwnedItemRequest
        {
            cmd = "get_owned_items",
            nickname = ClientSession.Instance.Nickname,
            category = category
        };
        NetworkManager.Instance.SendJson(req);
    }

    public void ChangeCategory(string category)
    {
        Debug.Log("[MyPage] Category changed to " + category);
        currentCategory = category;

        receivedStoreItems = false;
        receivedOwnedItems = false;

        RequestStoreItems(category);
        RequestOwnedItems(category);
    }

    private void TryBuildUI()
    {
        if (!receivedStoreItems || !receivedOwnedItems)
            return;

        MyPageUI.Instance.CreateItemList(currentCategory, storeItems, ownedItems);
    }

    public void ResetAllItems()
    {
        string[] categories =
        {
        "Accessories", "Hairstyle", "Hat", "Outerwear",
        "Pants", "Shoes", "Gloves", "Glasses",
        "FullBody", "Faces"
    };

        CharacterAppearance appearance = FindFirstObjectByType<CharacterAppearance>();

        if (appearance == null)
        {
            Debug.LogError("[MyPage] CharacterAppearance not found");
            return;
        }

        // 1) 클라이언트 외형 즉시 초기화
        foreach (string c in categories)
            ApplyItemToPlayer(c, 0);

        Debug.Log("[MyPage] All items reset to default");

        // 2) 서버에 저장 (각 카테고리 id=0)
        foreach (string c in categories)
        {
            RequestChangeAppearance(c, 0);
        }

        Debug.Log("[MyPage] Server appearance reset request sent");
    }


    public void ApplyItemToPlayer(string category, int id)
    {
        Transform target = GameObject.Find($"Player/Base_Mesh/{category}")?.transform;

        if (target == null)
        {
            Debug.LogError($"[Store] Category not found: {category}");
            return;
        }

        SkinnedMeshRenderer targetRenderer = target.GetComponent<SkinnedMeshRenderer>();
        if (targetRenderer == null)
        {
            Debug.LogError($"[Store] No SkinnedMeshRenderer in: {category}");
            return;
        }

        if (id == 0)
        {
            Mesh def = GetDefaultMesh(category);

            if (def != null)
            {
                targetRenderer.sharedMesh = def;
                Debug.Log($"[Store] Applied default mesh for {category}");
            }
            else
            {
                Debug.LogWarning($"[Store] Default mesh not set for {category}");
            }
            return;
        }

        string path = $"Items/{category}/{category}{id}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"[Store] Prefab not found at: {path}");
            return;
        }

        SkinnedMeshRenderer prefabRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();

        if (prefabRenderer == null)
        {
            Debug.LogError($"[Store] Prefab has no SkinnedMeshRenderer: {path}");
            return;
        }

        targetRenderer.sharedMesh = prefabRenderer.sharedMesh;

        Debug.Log($"[Store] Applied mesh: {category}{id}");
    }

    private Mesh GetDefaultMesh(string category)
    {
        switch (category)
        {
            case "Accessories": return defaultAccessories;
            case "Hairstyle": return defaultHairstyle;
            case "Hat": return defaultHat;
            case "Outerwear": return defaultOuterwear;
            case "Pants": return defaultPants;
            case "Shoes": return defaultShoes;
            case "Gloves": return defaultGloves;
            case "Glasses": return defaultGlasses;
            case "FullBody": return defaultFullBody;
            case "Faces": return defaultFaces;
        }
        return null;
    }

    public void RequestChangeAppearance(string category, int id)
    {
        var req = new AppearanceChangeRequest
        {
            nickname = ClientSession.Instance.Nickname,
            category = category,
            id = id
        };

        NetworkManager.Instance.SendJson(req);

        Debug.Log($"[MyPage] Sent appearance change → {category} = {id}");
    }

    public void GoLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }


    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"nickname_change\""))
        {
            var res = JsonUtility.FromJson<NicknameChangeResponse>(json);

            if (res.result)
            {
                ClientSession.Instance.UpdateNickname(res.newNickname);
                MyPageUI.Instance.ApplyNickname(res.newNickname);
            }
            return;
        }

        else if(json.Contains("\"type\":\"appearance_info\""))
        {
            var info = JsonUtility.FromJson<AppearanceResponse>(json);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var appearance = FindFirstObjectByType<CharacterAppearance>();
                if (appearance != null)
                    appearance.ApplyAppearance(info);
            });
            return;
        }

        else if(json.Contains("\"type\":\"player_info\""))
        {
            var res = JsonUtility.FromJson<PlayerInfoResponse>(json);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                MyPageUI.Instance.ApplyNickname(res.nickname);
                MyPageUI.Instance.ApplyPlayerLevel(res.level);
                MyPageUI.Instance.ApplyCoin0(res.coin0);
                MyPageUI.Instance.ApplyCoin1(res.coin1);
                MyPageUI.Instance.ApplyRank(res.rank);
                MyPageUI.Instance.ApplyRankScore(res.rankScore);
            });
            return;
        }

        else if(json.Contains("\"type\":\"store_items\""))
        {
            var res = JsonUtility.FromJson<StoreItemResponse>(json);
            storeItems = res.items;

            receivedStoreItems = true;
            TryBuildUI();
            return;
        }

        else if (json.Contains("\"type\":\"owned_items\""))
        {
            var res = JsonUtility.FromJson<OwnedItemsResponse>(json);
            ownedItems = res.owned;

            receivedOwnedItems = true;
            TryBuildUI();
            return;
        }

        else if (json.Contains("\"type\":\"appearance_update\""))
        {
            var res = JsonUtility.FromJson<AppearanceResponse>(json);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var appearance = FindFirstObjectByType<CharacterAppearance>();
                if (appearance != null)
                {
                    appearance.ApplyAppearance(res);
                }
            });

            Debug.Log("[MyPage] Appearance updated");
            return;
        }
    }
}
