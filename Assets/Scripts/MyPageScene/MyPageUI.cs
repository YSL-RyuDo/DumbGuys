using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MyPageUI : MonoBehaviour
{
    public static MyPageUI Instance;

    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI coin0Text;
    [SerializeField] private TextMeshProUGUI coin1Text;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI rankScoreText;

    public Transform contentParent;
    public GameObject itemPrefab;

    public Sprite[] rankSprites;
    public Image rankIcon;

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyNickname(string nick)
    {
        if (nicknameText != null)
            nicknameText.text = nick;
    }

    public void ApplyPlayerLevel(int level)
    {
        if (levelText != null)
            levelText.text = "Lv. " + level;
    }

    public void ApplyCoin0(int value)
    {
        if (coin0Text != null)
            coin0Text.text = value.ToString();
    }

    public void ApplyCoin1(int value)
    {
        if (coin1Text != null)
            coin1Text.text = value.ToString();
    }

    public void ApplyRank(int rank)
    {
        if (rankIcon != null && rank >= 0 && rank < rankSprites.Length)
            rankIcon.sprite = rankSprites[rank];

        if (rankText != null)
            rankText.text = rank.ToString();
    }

    public void ApplyRankScore(int value)
    {
        if (rankScoreText != null)
            rankScoreText.text = value.ToString();
    }

    public void CreateItemList(string category, StoreItemData[] items, int[] owned)
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        if (items == null || owned == null)
            return;

        //------------- 0번 기본 아이템 추가 -----------------

        {
            GameObject go = Instantiate(itemPrefab, contentParent);

            Image img = go.transform.Find("Image").GetComponent<Image>();
            Button equipBtn = go.transform.Find("exButton").GetComponent<Button>();

            Transform iconObj = go.transform.Find("Coin1Icon");
            Transform priceObj = go.transform.Find("PriceText");
            Transform purchaseObj = go.transform.Find("PurchaseButton");

            if (iconObj != null) iconObj.gameObject.SetActive(false);
            if (priceObj != null) priceObj.gameObject.SetActive(false);
            if (purchaseObj != null) purchaseObj.gameObject.SetActive(false);

            string path = $"Items/{category}/{category}0";
            Sprite sp = Resources.Load<Sprite>(path);
            if (img != null)
                img.sprite = sp;

            equipBtn.onClick.AddListener(() =>
            {
                MyPageManager.Instance.ApplyItemToPlayer(category, 0);
                MyPageManager.Instance.RequestChangeAppearance(category, 0);
            });
        }

        //---------------- 1 ~ N 아이템 -----------------------

        for (int i = 0; i < items.Length; i++)
        {
            StoreItemData item = items[i];
            int itemId = item.id;

            if (itemId < 0 || itemId >= owned.Length)
                continue;

            if (owned[itemId] == 0)
                continue; // 보유하지 않음 → 표시 X

            GameObject go = Instantiate(itemPrefab, contentParent);

            Image img = go.transform.Find("Image").GetComponent<Image>();
            Button equipBtn = go.transform.Find("exButton").GetComponent<Button>();

            Transform iconObj = go.transform.Find("Coin1Icon");
            Transform priceObj = go.transform.Find("PriceText");
            Transform purchaseObj = go.transform.Find("PurchaseButton");

            if (iconObj != null) iconObj.gameObject.SetActive(false);
            if (priceObj != null) priceObj.gameObject.SetActive(false);
            if (purchaseObj != null) purchaseObj.gameObject.SetActive(false);

            string path = $"Items/{category}/{category}{itemId}";
            Sprite sp = Resources.Load<Sprite>(path);
            if (img != null)
                img.sprite = sp;

            int capturedID = itemId;
            equipBtn.onClick.AddListener(() =>
            {
                MyPageManager.Instance.ApplyItemToPlayer(category, capturedID);
                MyPageManager.Instance.RequestChangeAppearance(category, capturedID);
            });
        }
    }

    public void OnClickResetItems()
    {
        MyPageManager.Instance.ResetAllItems();
    }

    public void OnClickGoLobby()
    {
        MyPageManager.Instance.GoLobby();
    }

}
