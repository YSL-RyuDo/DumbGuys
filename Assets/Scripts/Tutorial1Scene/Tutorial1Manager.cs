using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial1Manager : MonoBehaviour
{
    public static Tutorial1Manager Instance;

    [Header("Spawn Point")]
    [SerializeField] private Transform playerSpawnA;

    [Header("Player")]
    [SerializeField] private CharacterAppearance playerAppearance;

    [Header("Cubes (3 steps)")]
    [SerializeField] private GameObject cube0;
    [SerializeField] private GameObject cube1;
    [SerializeField] private GameObject cube2;

    [Header("UI Elements")]
    [SerializeField] private GameObject tutorialHintPanel;
    [SerializeField] private TMPro.TextMeshProUGUI tutorialHintText;
    [SerializeField] private float hintShowSeconds = 2.0f;

    private bool spawnApplied = false;
    private bool isTutorialActive = true;
    private int currentHintStep = 0;
    private Coroutine hideHintRoutine;

    private void Awake() => Instance = this;

    private void Start()
    {
        ApplySpawnPosition();
        RequestPlayerAppearance();

        cube0?.SetActive(true);
        cube1?.SetActive(false);
        cube2?.SetActive(false);

        ShowHint("Move with WASD and jump with SPACE.\nHead to the first cube!");
    }

    private void RequestPlayerAppearance()
    {
        if (NetworkManager.Instance == null || ClientSession.Instance == null)
            return;

        string nickname = ClientSession.Instance.Nickname;
        var req = new AppearanceRequest { nickname = nickname };
        NetworkManager.Instance.SendJson(req);
        Debug.Log($"[Tutorial1] Requested player appearance for {nickname}");

        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }

    private void HandleServerMessage(string json)
    {
        if (!json.Contains("\"type\":\"appearance_info\"")) return;

        var data = JsonUtility.FromJson<AppearanceResponse>(json);
        if (data.nickname != ClientSession.Instance.Nickname) return;

        playerAppearance.ApplyAppearance(data);
        //var player = FindFirstObjectByType<PlayerCharacter>();
        //if (player != null)
        //    player.SetInputLocked(false);

        Debug.Log("[Tutorial1] Player appearance applied and input unlocked.");
    }

    private void ApplySpawnPosition()
    {
        if (spawnApplied) return;
        spawnApplied = true;

        playerAppearance.transform.position = playerSpawnA.position;
        Debug.Log("[Tutorial1] Player spawned at position A");
    }

    public void OnHintStepReached(int stepIndex)
    {
        if (!isTutorialActive) return;
        if (stepIndex != currentHintStep) return;

        if (stepIndex == 0)
        {
            currentHintStep = 1;
            ShowHint("Nice jump! Now go to the next cube!");
            cube0?.SetActive(false);
            cube1?.SetActive(true);
        }
        else if (stepIndex == 1)
        {
            currentHintStep = 2;
            ShowHint("Great! Head to the last cube!");
            cube1?.SetActive(false);
            cube2?.SetActive(true);
        }
        else if (stepIndex == 2)
        {
            ShowHint("That's the way! Keep moving forward!");
            cube2?.SetActive(false);
            // 마지막은 방향 안내만
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
        tutorialHintPanel?.SetActive(false);
        hideHintRoutine = null;
    }

    public void RespawnCharacter(CharacterAppearance character)
    {
        if (character != playerAppearance) return;

        character.transform.position = playerSpawnA.position;
        var rb = character.GetComponent<Rigidbody>();
        if (rb != null) rb.velocity = Vector3.zero;

        Debug.Log("[Tutorial1] Player respawned at spawn point A.");
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

        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.SendJson(req);
            Debug.Log($"[Tutorial0] Sent tutorial completion packet for {req.nickname}");
        }

        StartCoroutine(DelayedSceneTransition("LobbyScene", 2.0f));
    }

    private IEnumerator DelayedSceneTransition(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

}
