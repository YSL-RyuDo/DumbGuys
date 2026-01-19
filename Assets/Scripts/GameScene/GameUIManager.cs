using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public class GameUIManager : MonoBehaviour
{
    [Header("===== Player Info =====")]
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text kdaText;
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Image rankingImage;

    [Header("===== Ranking View =====")]
    [SerializeField] private Transform rankingContent; // Content
    [SerializeField] private TMP_Text playerText;
    [SerializeField] private TMP_Text enemyText;
    [SerializeField] private Sprite firstRankSprite;
    [SerializeField] private Sprite secondRankSprite;

    [Header("===== Item =====")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite defaultItemSprite; // 아이템 없을 때 기본
    private bool hasItem = false;

    [Header("===== Chat System =====")]
    [SerializeField] private Transform chatContent;      // Chat prefab이 올라갈 곳
    [SerializeField] private TMP_InputField chatInputField;
    [SerializeField] private Button chatSendButton;
    [SerializeField] private GameObject chatMessagePrefab; // (Text 하나만 있는 프리팹)
    [SerializeField] private ScrollRect chatScrollView;

    [Header("===== Item Icons =====")]
    [SerializeField] private Sprite speedUpIcon;
    [SerializeField] private Sprite attackUpIcon;
    [SerializeField] private Sprite healIcon;
    [SerializeField] private Sprite pushIcon;
    [SerializeField] private Sprite jumpUpIcon;

    private bool isChatting = false;
    public static bool IsChatting { get; private set; } = false;
    private PlayerCharacter player;
    private CharacterAppearance enemy;
    private Transform winZone;
    private ItemPickup.ItemType? currentHeldItem = null;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerCharacter>();
        enemy = GameObject.FindGameObjectWithTag("Enemy")?.GetComponent<CharacterAppearance>();

        if (player == null)
            Debug.LogWarning("[GameUIManager] PlayerCharacter with tag 'Player' not found!");

        if (enemy == null)
            Debug.LogWarning("[GameUIManager] Enemy CharacterAppearance with tag 'Enemy' not found!");

        winZone = GameObject.Find("WinZone")?.transform;

        nicknameText.text = ClientSession.Instance.Nickname;
        //kdaText.text = "0 / 0";
        hpSlider.value = 1f;

        chatSendButton.onClick.AddListener(SendChatMessage);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateHP();
        UpdateRanking();
        HandleChatToggle();

        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            HandleItemUse(player);
        }
    }

    // =========================
    // Player Info Update
    // =========================
    public void UpdateHP()
    {
        if (player == null) return;
        float ratio = Mathf.Clamp01((float)player.CurrentHealth / player.MaxHealth);
        hpSlider.value = ratio;
    }

    public void UpdateKDA(int kill, int death)
    {
        kdaText.text = $"{kill} / {death}";
    }

    // =========================
    // Ranking System
    // =========================
    private void UpdateRanking()
    {
        if (player == null || enemy == null || winZone == null) return;

        float playerDist = Vector3.Distance(player.transform.position, winZone.position);
        float enemyDist = Vector3.Distance(enemy.transform.position, winZone.position);

        bool playerIsFirst = playerDist < enemyDist;

        // 랭킹 이미지 업데이트
        rankingImage.sprite = playerIsFirst ? firstRankSprite : secondRankSprite;

        // RectTransform 가져오기
        RectTransform playerRect = playerText.GetComponent<RectTransform>();
        RectTransform enemyRect = enemyText.GetComponent<RectTransform>();

        // 기본 위치 설정 (Y축 기준)
        Vector2 playerTop = new Vector2(0, 0);
        Vector2 enemyBottom = new Vector2(0, -130); // CellSize + Spacing 만큼
        Vector2 playerBottom = new Vector2(0, -130);
        Vector2 enemyTop = new Vector2(0, 0);

        // 자연스러운 전환
        if (playerIsFirst)
        {
            playerRect.DOAnchorPos(playerTop, 0.15f).SetEase(Ease.InOutQuad);
            enemyRect.DOAnchorPos(enemyBottom, 0.15f).SetEase(Ease.InOutQuad);
        }
        else
        {
            playerRect.DOAnchorPos(playerBottom, 0.15f).SetEase(Ease.InOutQuad);
            enemyRect.DOAnchorPos(enemyTop, 0.15f).SetEase(Ease.InOutQuad);
        }
    }

    // =========================
    // Item System
    // =========================
    public void UpdateItem(Sprite newItemSprite)
    {
        if (!hasItem)
        {
            itemImage.sprite = newItemSprite;
            hasItem = true;
        }
    }

    public void ClearItem()
    {
        hasItem = false;
        itemImage.sprite = defaultItemSprite;
    }

    // =========================
    // Chat System
    // =========================
    private void HandleChatToggle()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isChatting)
            {
                OpenChat();
            }
            else
            {
                if (!string.IsNullOrEmpty(chatInputField.text))
                    SendChatMessage();

                CloseChat();
            }
        }
    }

    private void OpenChat()
    {
        isChatting = true;
        IsChatting = true; // 전역 상태 반영
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        chatInputField.gameObject.SetActive(true);
        chatInputField.text = "";
        chatInputField.ActivateInputField();
    }

    private void CloseChat()
    {
        isChatting = false;
        IsChatting = false; // 전역 상태 반영
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        chatInputField.DeactivateInputField();
    }


    private void SendChatMessage()
    {
        string msg = chatInputField.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;

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

    public void AddChatMessage(string sender, string message)
    {
        GameObject newMsg = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text msgText = newMsg.GetComponentInChildren<TMP_Text>();
        msgText.text = $"[{sender}] {message}";

        Canvas.ForceUpdateCanvases();
        chatScrollView.verticalNormalizedPosition = 0;
    }

    // 아이템

    public void OnItemPicked(ItemPickup.ItemType itemType)
    {
        if (currentHeldItem != null)
        {
            Debug.Log($"[UI] Already holding an item ({currentHeldItem}), ignoring new pickup.");
            return; // 이미 아이템이 있으면 무시
        }

        currentHeldItem = itemType;
        hasItem = true;
        Debug.Log($"[UI] Item picked: {itemType}");

        // 아이템 이미지 표시
        Sprite itemSprite = GetItemSprite(itemType);
        itemImage.sprite = itemSprite != null ? itemSprite : defaultItemSprite;
    }

    public void HandleItemUse(PlayerCharacter player)
    {
        if (currentHeldItem == null) return;

        Debug.Log($"[UI] Using item: {currentHeldItem}");
        player.UseItem(currentHeldItem.Value);

        // 사용 후 초기화
        ClearItem();
        currentHeldItem = null;
    }

    public Sprite GetItemSprite(ItemPickup.ItemType type)
    {
        switch (type)
        {
            case ItemPickup.ItemType.SpeedUp: return speedUpIcon;
            case ItemPickup.ItemType.AttackUp: return attackUpIcon;
            case ItemPickup.ItemType.Heal: return healIcon;
            case ItemPickup.ItemType.Push: return pushIcon;
            case ItemPickup.ItemType.JumpUp: return jumpUpIcon;
            default: return defaultItemSprite;
        }
    }

    public void SetPlayerName(string nickname)
    {
        playerText.text = nickname;
        nicknameText.text = nickname;
    }

    public void SetEnemyName(string nickname)
    {
        enemyText.text = nickname;
    }

}
