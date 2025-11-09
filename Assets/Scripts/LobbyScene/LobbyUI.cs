using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [Header("Text UI")]
    [SerializeField] private TMP_Text NickNameText;
    [SerializeField] private TMP_Text LevelText;
    [SerializeField] private TMP_Text Coin0Text;
    [SerializeField] private TMP_Text Coin1Text;
    [SerializeField] private TMP_Text RankText;

    [Header("Slider UI")]
    [SerializeField] private Slider PlayerEXP;
    [SerializeField] private Slider RankScoreSlider;

    [Header("Image UI")]
    [SerializeField] private Image RankImage;
    [SerializeField] private Image IconImage;
    [SerializeField] private Image IconBorderImage;

    [Header("Sprites")]
    [SerializeField] private Sprite[] RankSprites;
    [SerializeField] private Sprite[] IconSprites;
    [SerializeField] private Sprite[] IconBorderSprites;

    [Header("Chat System")]
    [SerializeField] private Transform chatContent;
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button chatSendButton;
    [SerializeField] private GameObject chatMessagePrefab;
    [SerializeField] private ScrollRect chatScrollView;

    private void Start()
    {
        // 버튼 클릭 시 서버로 메시지 전송
        chatSendButton.onClick.AddListener(SendChatMessage);

        // 서버 수신 등록
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    public void ApplyPlayerInfo(PlayerInfoResponse info)
    {
        // === Text ===
        NickNameText.text = info.nickname;
        LevelText.text = $"Lv. {info.level}";
        Coin0Text.text = info.coin0.ToString();
        Coin1Text.text = info.coin1.ToString();

        string[] rankNames = {
            "Iron", "Bronze", "Silver", "Gold",
            "Platinum", "Diamond", "Master",
            "Grandmaster", "Challenger"
        };

        string rankName = (info.rank >= 0 && info.rank < rankNames.Length)
            ? rankNames[info.rank]
            : "Unknown";

        RankText.text = rankName;

        // === Slider ===
        PlayerEXP.maxValue = info.level * 100;
        PlayerEXP.value = info.exp;

        RankScoreSlider.maxValue = info.rank * 100;
        RankScoreSlider.value = info.rankScore;

        // === Images ===
        if (info.rank >= 0 && info.rank < RankSprites.Length)
            RankImage.sprite = RankSprites[info.rank];

        if (info.icon >= 0 && info.icon < IconSprites.Length)
            IconImage.sprite = IconSprites[info.icon];

        if (info.iconBorder >= 0 && info.iconBorder < IconBorderSprites.Length)
            IconBorderImage.sprite = IconBorderSprites[info.iconBorder];

        Debug.Log($"[LobbyUI] Updated player info: {info.nickname}, Lv.{info.level}, Rank:{info.rank}");
    }

    private void SendChatMessage()
    {
        string msg = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

        // 로컬에 바로 표시
        AddChatMessage(ClientSession.Instance.Nickname, msg);

        // 서버 전송
        var chatReq = new ChatRequest
        {
            sender = ClientSession.Instance.Nickname,
            message = msg
        };
        NetworkManager.Instance.SendJson(chatReq);

        chatInputField.text = "";
    }

    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"chat_message\""))
        {
            var data = JsonUtility.FromJson<ChatMessagePacket>(json);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                AddChatMessage(data.sender, data.message);
            });
        }
    }

    public void AddChatMessage(string sender, string message)
    {
        GameObject newMsg = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text msgText = newMsg.GetComponentInChildren<TMP_Text>();
        msgText.text = $"[{sender}] {message}";

        Canvas.ForceUpdateCanvases();
        chatScrollView.verticalNormalizedPosition = 0;
    }
}
