using UnityEngine;
using UnityEngine.SceneManagement;

public class StoreManager : MonoBehaviour
{
    private string currentCategory = "Accessories";

    private StoreItemData[] storeItems; // 서버에서 받아오는 전체 아이템 목록
    public int[] ownedItems;           // 서버에서 받아오는 보유 상태 (0/1 배열)

    private bool receivedStoreItems = false;
    private bool receivedOwnedItems = false;

    public static StoreManager Instance { get; private set; }

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

    private void Start()
    {
        if (!ClientSession.Instance.IsLoggedIn)
        {
            Debug.LogError("[Store] No session found.");
            return;
        }

        string nickname = ClientSession.Instance.Nickname;

        var coinReq = new CoinInfoRequest(nickname);
        NetworkManager.Instance.SendJson(coinReq);

        Debug.Log($"[Store] Requested coin info for: {nickname}");

        RequestStoreItems(currentCategory);
        RequestOwnedItems(currentCategory);
    }

    private void OnEnable()
    {
        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }

    private void OnDisable()
    {
        NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void RequestStoreItems(string category)
    {
        var req = new StoreItemRequest
        {
            cmd = "get_store_items",
            category = category
        };

        NetworkManager.Instance.SendJson(req);
        Debug.Log($"[Store] Requested store items for category: {category}");
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
        Debug.Log($"[Store] Requested owned items for category: {category}");
    }

    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"coin_info\""))
        {
            var res = JsonUtility.FromJson<CoinInfoResponse>(json);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                StoreUI.Instance.ApplyCoin(res);
            });
        }
        else if (json.Contains("\"type\":\"store_items\""))
        {
            var res = JsonUtility.FromJson<StoreItemResponse>(json);

            storeItems = res.items;
            receivedStoreItems = true;

            Debug.Log($"[Store] Received items for {res.category}, count = {res.items.Length}");
            TryCreateUI();
        }
        else if (json.Contains("\"type\":\"owned_items\""))
        {
            var res = JsonUtility.FromJson<OwnedItemsResponse>(json);

            Debug.Log($"[Store] Owned items received for {res.category}");

            ownedItems = res.owned;
            receivedOwnedItems = true;

            TryCreateUI();
        }
        else if (json.Contains("\"type\":\"buy_item_result\""))
        {
            var res = JsonUtility.FromJson<BuyItemResult>(json);

            if (res.result)
            {
                StoreUI.Instance.UpdateCoin1(res.coin1);

                // 갱신 위해 다시 읽기
                RequestOwnedItems(currentCategory);

                Debug.Log("[Store] Purchase success!");
            }
            else
            {
                Debug.LogWarning($"[Store] Purchase failed: {res.msg}");
            }
        }
        else if (json.Contains("\"type\":\"gacha_result\""))
        {
            var res = JsonUtility.FromJson<GachaResponse>(json);

            if (res.result)
            {
                StoreUI.Instance.UpdateCoin0(res.coin0);

                StoreUI.Instance.ShowGachaResult(res.category, res.id);

                RequestOwnedItems(res.category);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    StoreUI.Instance.CreateItemList(res.category, storeItems);
                });
            }
            else
            {
                StoreUI.Instance.ShowPopup("가챠 실패: " + res.msg);
            }

            return;
        }

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

    private void TryCreateUI()
    {
        if (!receivedStoreItems || !receivedOwnedItems)
            return;

        StoreUI.Instance.CreateItemList(currentCategory, storeItems);
    }
    public void RequestBuyItem(string category, int id)
    {
        var req = new BuyItemRequest
        {
            nickname = ClientSession.Instance.Nickname,
            category = category,
            id = id
        };

        NetworkManager.Instance.SendJson(req);
        Debug.Log($"[Store] Sent buy request → {category}{id}");
    }

    public void ChangeCategory(string category)
    {
        Debug.Log($"[Store] Category changed to {category}");
        currentCategory = category;

        receivedStoreItems = false;
        receivedOwnedItems = false;

        RequestStoreItems(category);
        RequestOwnedItems(category);
    }

    public void ResetAllItems()
    {
        string[] allCategories =
        {
            "Accessories", "Hairstyle", "Hat", "Outerwear",
            "Pants", "Shoes", "Gloves", "Glasses",
            "FullBody", "Faces"
        };

        foreach (string c in allCategories)
        {
            ApplyItemToPlayer(c, 0);
        }

        Debug.Log("[Store] All items reset to default (0)");
    }

    public void GoLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
