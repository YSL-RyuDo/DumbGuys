using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StoreUI : MonoBehaviour
{
    public static StoreUI Instance;

    [Header("Coins")]
    [SerializeField] private TextMeshProUGUI coin0Text;
    [SerializeField] private TextMeshProUGUI coin1Text;

    [Header("Item UI")]
    public Transform contentParent;     // ScrollView Content
    public GameObject itemPrefab;       // 상점 아이템 프리팹

    [SerializeField] private GameObject gachaPopup;   // 프리팹 UI
    [SerializeField] private Image gachaItemImage;    // 결과 이미지
    [SerializeField] private TextMeshProUGUI gachaItemText;
    [SerializeField] private GameObject popup;
    [SerializeField] private TextMeshProUGUI popupText;

    private void Awake()
    {
        Instance = this;
    }

    public void OnClickGacha()
    {
        Debug.Log("[Store] Gacha button clicked");

        var req = new GachaRequest
        {
            cmd = "gacha",
            nickname = ClientSession.Instance.Nickname
        };

        NetworkManager.Instance.SendJson(req);
        Debug.Log("[Store] Sent gacha request to server");
    }


    public void ApplyCoin(CoinInfoResponse res)
    {
        if (coin0Text != null)
            coin0Text.text = res.coin0.ToString();

        if (coin1Text != null)
            coin1Text.text = res.coin1.ToString();
    }

    public void CreateItemList(string category, StoreItemData[] items)
    {
        if (items == null)
        {
            Debug.LogError("[StoreUI] items is null");
            return;
        }

        // 기존 목록 삭제
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        foreach (StoreItemData item in items)
        {
            GameObject go = Instantiate(itemPrefab, contentParent);

            // 가격 표시
            go.transform.Find("PriceText").GetComponent<TextMeshProUGUI>().text =
                item.price.ToString();

            // 아이템 이미지 적용
            ApplyItemImage(go, category, item.id);

            // 구매 버튼
            Button purchaseBtn = go.transform.Find("PurchaseButton").GetComponent<Button>();

            // 보유 여부 확인
            bool owned = OwnedItemLocalCheck(category, item.id);
            purchaseBtn.gameObject.SetActive(!owned);

            purchaseBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[Store] Purchase → {category}{item.id}");
                StoreManager.Instance.RequestBuyItem(category, item.id);
            });

            // 장착 버튼 (exButton)
            Button exBtn = go.transform.Find("exButton").GetComponent<Button>();
            exBtn.onClick.AddListener(() =>
            {
                Debug.Log($"[Store] Apply mesh → {category}{item.id}");
                StoreManager.Instance.ApplyItemToPlayer(category, item.id);
            });
        }
    }

    private void ApplyItemImage(GameObject go, string category, int id)
    {
        string path = $"Items/{category}/{category}{id}";
        Sprite sp = Resources.Load<Sprite>(path);

        if (sp != null)
        {
            go.transform.Find("Image").GetComponent<Image>().sprite = sp;
        }
        else
        {
            Debug.LogWarning($"[Store] Image not found: {path}");
        }
    }
    private bool OwnedItemLocalCheck(string category, int id)
    {
        if (StoreManager.Instance.ownedItems == null)
            return false;

        if (id < 0 || id >= StoreManager.Instance.ownedItems.Length)
            return false;

        return StoreManager.Instance.ownedItems[id] == 1;
    }

    public void UpdateCoin1(int newValue)
    {
        coin1Text.text = newValue.ToString();
    }

    public void UpdateCoin0(int newValue)
    {
        if (coin0Text != null)
            coin0Text.text = newValue.ToString();
    }

    public void ShowGachaResult(string category, int id)
    {
        if (gachaPopup == null)
        {
            Debug.LogError("[StoreUI] gachaPopup not assigned!");
            return;
        }

        gachaPopup.SetActive(true);

        // 이미지 로드
        string path = $"Items/{category}/{category}{id}";
        Sprite sp = Resources.Load<Sprite>(path);

        if (gachaItemImage != null)
            gachaItemImage.sprite = sp;

        if (gachaItemText != null)
            gachaItemText.text = $"{category} {id} get!";
    }

    public void HideGachaPopup()
    {
        if (gachaPopup != null)
            gachaPopup.SetActive(false);
    }

    public void ShowPopup(string msg)
    {
        if (popup == null)
        {
            Debug.LogError("[StoreUI] popup object not assigned!");
            return;
        }

        popupText.text = msg;
        popup.SetActive(true);
    }

    public void HidePopup()
    {
        if (popup != null)
            popup.SetActive(false);
    }

}
