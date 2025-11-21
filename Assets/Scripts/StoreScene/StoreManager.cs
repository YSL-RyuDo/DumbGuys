using UnityEngine;

public class StoreManager : MonoBehaviour
{
    private string currentCategory = "Accessories";

    private StoreItemData[] storeItems; // 서버에서 받아오는 전체 아이템 목록
    public int[] ownedItems;           // 서버에서 받아오는 보유 상태 (0/1 배열)

    private bool receivedStoreItems = false;
    private bool receivedOwnedItems = false;

    public static StoreManager Instance { get; private set; }

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

        // 3) 보유 목록 요청
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
                // UI 적용
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
                // 코인 UI 갱신
                StoreUI.Instance.UpdateCoin1(res.coin1);

                // ★ 새로 CSV 읽어온 owned_items 요청하기
                RequestOwnedItems(currentCategory);

                Debug.Log("[Store] Purchase success!");
            }
            else
            {
                Debug.LogWarning($"[Store] Purchase failed: {res.msg}");
            }
        }


    }
    public void ApplyItemToPlayer(string category, int id)
    {
        // 플레이어의 해당 카테고리 SkinnedMeshRenderer 찾기
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

        // 프리팹 로드
        string path = $"Items/{category}/{category}{id}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"[Store] Prefab not found at: {path}");
            return;
        }

        // 프리팹의 SkinnedMeshRenderer 가져오기
        SkinnedMeshRenderer prefabRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();

        if (prefabRenderer == null)
        {
            Debug.LogError($"[Store] Prefab has no SkinnedMeshRenderer: {path}");
            return;
        }

        // Mesh 교체
        targetRenderer.sharedMesh = prefabRenderer.sharedMesh;

        Debug.Log($"[Store] Applied mesh: {category}{id}");
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


}
