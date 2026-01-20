using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class Tutorial0Manager : MonoBehaviour
{
    public static Tutorial0Manager Instance;

    [Header("Spawn Points (0~5)")]
    [SerializeField] private Transform playerSpawn0;
    [SerializeField] private Transform playerSpawn1;
    [SerializeField] private Transform playerSpawn2;
    [SerializeField] private Transform playerSpawn3;
    [SerializeField] private Transform playerSpawn4;
    [SerializeField] private Transform playerSpawn5;

    [Header("Player")]
    [SerializeField] private CharacterAppearance playerAppearance;

    [Header("Cubes (0~8)")]
    [SerializeField] private GameObject[] cubes;

    [Header("Enemies (0~2)")]
    [SerializeField] private GameObject[] enemies; // 미리 배치된 Enemy0,1,2 (SetActive false)

    [Header("UI Elements")]
    [SerializeField] private GameObject tutorialHintPanel;
    [SerializeField] private TextMeshProUGUI tutorialHintText;
    [SerializeField] private float hintShowSeconds = 2.0f;

    [Header("Nickname UI")]
    [SerializeField] private TextMeshProUGUI nicknameText;

    [Header("Tutorial Item UI")]
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite defaultItemSprite;
    [SerializeField] private Sprite speedUpIcon;
    [SerializeField] private Sprite attackUpIcon;
    [SerializeField] private Sprite healIcon;
    [SerializeField] private Sprite pushIcon;
    [SerializeField] private Sprite jumpUpIcon;

    [Header("Tutorial HP UI")]
    [SerializeField] private Slider hpSlider;

    private bool spawnApplied = false;
    private bool isTutorialActive = true;
    private bool healGiven = false;
    private int currentSpawnIndex = 0;
    private int defeatedEnemyCount = 0;

    private Coroutine hideHintRoutine;
    private Coroutine itemLoopRoutine;
    private ItemPickup.ItemType? currentItem = null;
    private bool hasItem = false;
    private bool appearanceApplied = false;

    private void Awake() => Instance = this;

    private void Start()
    {
        RequestPlayerAppearance();
        ApplySpawnPosition();
    }

    private void Update()
    {
        var player = FindFirstObjectByType<PlayerCharacter>();
        if (player == null) return;

        // 아이템 사용
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            if (currentItem != null)
            {
                player.UseItem(currentItem.Value);
                ClearTutorialItemUI();
            }
        }

        // HP UI 실시간 업데이트
        UpdateTutorialHP(player);
    }

    public void BeginFirstHintSequence()
    {
        StartCoroutine(FirstHintSequence());
    }

    private IEnumerator FirstHintSequence()
    {
        // 외형은 이미 요청되어 있거나, 도착하면 자동 적용됨
        playerAppearance.gameObject.SetActive(true);

        if (nicknameText != null && ClientSession.Instance != null)
            nicknameText.text = ClientSession.Instance.Nickname;

        var player = FindFirstObjectByType<PlayerCharacter>();
        if (player != null)
            player.SetInputLocked(true);

        for (int i = 0; i < cubes.Length; i++)
            if (cubes[i] != null)
                cubes[i].SetActive(i == 0);

        ShowHint("Use WASD to move and SPACE to jump.\nGo toward the first cube!");
        yield return new WaitForSeconds(hintShowSeconds);

        if (player != null)
            player.SetInputLocked(false);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void RequestPlayerAppearance()
    {
        if (NetworkManager.Instance == null || ClientSession.Instance == null)
            return;

        var req = new AppearanceRequest
        {
            nickname = ClientSession.Instance.Nickname
        };

        NetworkManager.Instance.SendJson(req);

        NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }


    private void HandleServerMessage(string json)
    {
        if (!json.Contains("\"type\":\"appearance_info\""))
            return;

        var data = JsonUtility.FromJson<AppearanceResponse>(json);

        if (data.nickname != ClientSession.Instance.Nickname)
            return;

        playerAppearance.ApplyAppearance(data);

        appearanceApplied = true;
    }


    private void ApplySpawnPosition()
    {
        if (spawnApplied) return;
        spawnApplied = true;
        playerAppearance.transform.position = playerSpawn0.position;
    }

    public void OnHintStepReached(int stepIndex)
    {
        if (!isTutorialActive) return;
        var player = FindFirstObjectByType<PlayerCharacter>();
        if (player == null) return;

        switch (stepIndex)
        {
            case 0:
                ShowHint("Nice! Move to the next cube!");
                ActivateNextCube(1);
                break;

            case 1:
                ShowHint("Good job! Head to the next cube!");
                ActivateNextCube(2);
                break;

            case 2:
                currentSpawnIndex = 1;
                ShowHint("Try jumping with SPACE!");
                ActivateNextCube(3);
                break;

            case 3:
                currentSpawnIndex = 2;
                ShowHint("You obtained a SpeedUp item! Press CTRL to use!");
                ActivateNextCube(4);
                GiveItemWithRespawn(player, ItemPickup.ItemType.SpeedUp);
                break;

            case 4:
                currentSpawnIndex = 3;
                ShowHint("You obtained a JumpUp item! Press CTRL to use!");
                ActivateNextCube(5);
                GiveItemWithRespawn(player, ItemPickup.ItemType.JumpUp);
                break;

            case 5:
                currentSpawnIndex = 4;
                DeactivateAllCubes();
                ShowHint("Use your left attack now!");
                GiveItemWithRespawn(player, ItemPickup.ItemType.AttackUp);
                ActivateEnemy(0);
                break;

            case 6:
                ShowHint("Use your Push skill! Press CTRL!");
                DeactivateAllCubes();
                GiveItemWithRespawn(player, ItemPickup.ItemType.Push);
                ActivateEnemy(1);
                break;

            case 7:
                ShowHint("You obtained a Heal item! Try using it!");
                DeactivateAllCubes();
                ActivateEnemy(2);

                if (itemLoopRoutine != null)
                {
                    StopCoroutine(itemLoopRoutine);
                    itemLoopRoutine = null;
                }

                if (!healGiven)
                {
                    healGiven = true;
                    player.TakeDamage(50);
                    StartCoroutine(GiveHealOnce(player));
                }
                break;

            case 8:
                currentSpawnIndex = 5;
                ShowHint("Excellent! One last cube remains!");
                ActivateNextCube(9); // 마지막 큐브 등장
                break;

            case 9:
                OnTutorialComplete(); // 로비로 이동
                break;

            default:
                Debug.Log($"[Tutorial0] Undefined step index: {stepIndex}");
                break;
        }
    }

    private void ActivateNextCube(int nextIndex)
    {
        if (cubes == null || nextIndex >= cubes.Length) return;
        for (int i = 0; i < cubes.Length; i++)
            if (cubes[i] != null)
                cubes[i].SetActive(i == nextIndex);
    }

    private void ActivateEnemy(int index)
    {
        if (enemies == null || index >= enemies.Length) return;

        // 모든 적 비활성화
        for (int i = 0; i < enemies.Length; i++)
            if (enemies[i] != null)
                enemies[i].SetActive(false);

        if (enemies[index] != null)
        {
            enemies[index].SetActive(true);
            var hp = enemies[index].GetComponent<EnemyHealth>();
            if (hp != null)
            {
                hp.enabled = true;
                hp.ResetHealth(); // 체력 초기화
            }
        }

        Debug.Log($"[Tutorial0] Enemy {index} activated.");
    }

    public void OnEnemyDefeated(EnemyHealth enemy)
    {
        defeatedEnemyCount++;
        Debug.Log($"[Tutorial0] Enemy defeated count = {defeatedEnemyCount}");

        switch (defeatedEnemyCount)
        {
            case 1: // Enemy0 사망 → Cube6 표시
                ShowHint("Enemy defeated! Proceed to the next cube!");
                ActivateNextCube(6);
                break;

            case 2: // Enemy1 사망 → Cube7 표시
                ShowHint("Well done! Move forward!");
                ActivateNextCube(7);
                break;

            case 3: // Enemy2 사망 → Cube8 표시
                ShowHint("All enemies cleared! Move to the final cube!");
                ActivateNextCube(8);
                break;
        }
    }



    private void ShowHint(string msg)
    {
        if (tutorialHintPanel == null || tutorialHintText == null) return;
        tutorialHintText.text = msg;
        tutorialHintPanel.SetActive(true);

        if (hideHintRoutine != null)
            StopCoroutine(hideHintRoutine);
        hideHintRoutine = StartCoroutine(HideHintAfterSeconds(hintShowSeconds));
    }

    private IEnumerator HideHintAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (tutorialHintPanel != null)
            tutorialHintPanel.SetActive(false);
        hideHintRoutine = null;
    }

    private void GiveItemWithRespawn(PlayerCharacter player, ItemPickup.ItemType type)
    {
        if (itemLoopRoutine != null)
        {
            StopCoroutine(itemLoopRoutine);
            itemLoopRoutine = null;
        }

        itemLoopRoutine = StartCoroutine(ItemRespawnLoop(player, type));
    }

    private IEnumerator ItemRespawnLoop(PlayerCharacter player, ItemPickup.ItemType type)
    {
        player.OnItemPicked(type);
        UpdateTutorialItemUI(type);

        while (isTutorialActive && player != null)
        {
            yield return new WaitForSeconds(5f);
            player.OnItemPicked(type);
            UpdateTutorialItemUI(type);
        }
    }

    private IEnumerator GiveHealOnce(PlayerCharacter player)
    {
        yield return new WaitForSeconds(1f);
        player.OnItemPicked(ItemPickup.ItemType.Heal);
        UpdateTutorialItemUI(ItemPickup.ItemType.Heal);
    }

    private void UpdateTutorialItemUI(ItemPickup.ItemType type)
    {
        Sprite newSprite = type switch
        {
            ItemPickup.ItemType.SpeedUp => speedUpIcon,
            ItemPickup.ItemType.AttackUp => attackUpIcon,
            ItemPickup.ItemType.Heal => healIcon,
            ItemPickup.ItemType.Push => pushIcon,
            ItemPickup.ItemType.JumpUp => jumpUpIcon,
            _ => defaultItemSprite
        };

        if (itemImage != null)
        {
            itemImage.sprite = newSprite;
            hasItem = true;
            currentItem = type;
        }
    }

    private void ClearTutorialItemUI()
    {
        if (itemImage != null)
            itemImage.sprite = defaultItemSprite;
        hasItem = false;
        currentItem = null;
    }

    public void UpdateTutorialHP(PlayerCharacter player)
    {
        if (hpSlider == null || player == null) return;
        float ratio = Mathf.Clamp01((float)player.CurrentHealth / player.MaxHealth);
        hpSlider.value = ratio;
    }

    public void RespawnCharacter(CharacterAppearance character)
    {
        if (character != playerAppearance) return;

        Transform targetSpawn = currentSpawnIndex switch
        {
            1 => playerSpawn1,
            2 => playerSpawn2,
            3 => playerSpawn3,
            4 => playerSpawn4,
            5 => playerSpawn5,
            _ => playerSpawn0
        };

        character.transform.position = targetSpawn.position;
        var rb = character.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;
    }

    public void OnTutorialComplete()
    {
        if (!isTutorialActive) return;
        isTutorialActive = false;
        ShowHint("Tutorial complete. Returning to Lobby...");

        var req = new TutorialCompleteRequest
        {
            nickname = ClientSession.Instance.Nickname
        };

        NetworkManager.Instance?.SendJson(req);
        StartCoroutine(DelayedSceneTransition("LobbyScene", 2.0f));
    }

    private IEnumerator DelayedSceneTransition(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    private void DeactivateAllCubes()
    {
        if (cubes == null) return;

        for (int i = 0; i < cubes.Length; i++)
            if (cubes[i] != null)
                cubes[i].SetActive(false);
    }

}
