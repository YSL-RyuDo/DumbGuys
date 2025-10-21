using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class ChooseSceneController : MonoBehaviour
{
    [Header("Player Info UI")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text enemyScoreText;

    [Header("Map UI References")]
    [SerializeField] private GameObject mapChooseUI;
    [SerializeField] private GameObject selectMapUI;
    [SerializeField] private RectTransform arrow;
    [SerializeField] private TMP_Text countdownText;

    [Header("Map Toggles & Buttons")]
    [SerializeField] private Toggle[] mapToggles; // Map0Toggle, Map1Toggle, Map2Toggle
    [SerializeField] private Button[] mapButtons; // Map0Button, Map1Button, Map2Button
    [SerializeField] private RectTransform[] mapImages; // Map0Image, Map1Image, Map2Image

    private int selectedMap = -1;
    private float countdown = 10f;
    private bool hasChosen = false;

    private string enemyNickname = "";
    private int playerScore = 0;
    private int enemyScore = 0;

    private void Start()
    {
        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;

        // 플레이어 이름 세팅
        playerNameText.text = ClientSession.Instance.Nickname;
        playerScoreText.text = "0";
        enemyScoreText.text = "0";

        RequestMapChooseStart();

        // 각 버튼 클릭 시 Toggle on
        for (int i = 0; i < mapButtons.Length; i++)
        {
            int index = i;
            mapButtons[i].onClick.AddListener(() =>
            {
                if (!hasChosen)
                    OnMapButtonClicked(index);
            });
        }

        StartCoroutine(CountdownRoutine());
    }

    private void RequestMapChooseStart()
    {
        if (!ClientSession.Instance.IsLoggedIn)
        {
            Debug.LogWarning("[ChooseScene] Not logged in - cannot request map_choose_start.");
            return;
        }

        string nickname = ClientSession.Instance.Nickname;
        var req = new MapChooseStartRequest(nickname);

        NetworkManager.Instance.SendJson(req);
        Debug.Log($"[ChooseScene] Sent map_choose_start_request for {nickname}");
    }


    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    // === 맵 버튼 클릭 ===
    private void OnMapButtonClicked(int index)
    {
        if (hasChosen) return;

        hasChosen = true;
        selectedMap = index;
        mapToggles[index].isOn = true;

        Debug.Log($"[ChooseScene] Player selected Map {selectedMap}");
        SendSelection();

        // 모든 버튼 클릭 비활성화
        foreach (var btn in mapButtons)
            btn.interactable = false;
    }

    // === 카운트다운 ===
    private IEnumerator CountdownRoutine()
    {
        int remaining = Mathf.CeilToInt(countdown); // 20부터 시작

        while (remaining > 0 && !hasChosen)
        {
            countdownText.text = remaining.ToString();
            yield return new WaitForSeconds(1f); // 1초 간격으로 대기
            remaining--;
        }

        if (!hasChosen)
        {
            Debug.Log("[ChooseScene] Time over. Sending default -1 selection.");
            selectedMap = -1;
            SendSelection();

            foreach (var btn in mapButtons)
                btn.interactable = false;
        }
    }


    private void SendSelection()
    {
        string nickname = ClientSession.Instance.Nickname;
        var packet = new MapSelectPacket
        {
            nickname = nickname,
            mapIndex = selectedMap
        };

        NetworkManager.Instance.SendJson(packet);
        Debug.Log($"[ChooseScene] Sent map selection: {selectedMap}");
    }

    // === 서버 응답 처리 ===
    private void HandleServerMessage(string json)
    {
        // 매치 시작 시 정보 수신
        if (json.Contains("\"type\":\"map_choose_start\""))
        {
            var data = JsonUtility.FromJson<MapChooseStartPacket>(json);
            enemyNickname = data.enemyNickname;
            playerScore = data.playerScore;
            enemyScore = data.enemyScore;

            // UI에 표시
            playerNameText.text = data.playerNickname;
            enemyNameText.text = data.enemyNickname;
            playerScoreText.text = playerScore.ToString();
            enemyScoreText.text = enemyScore.ToString();

            Debug.Log($"[ChooseScene] Map Choose Start: {data.playerNickname}({playerScore}) vs {data.enemyNickname}({enemyScore})");

            if (data.usedMaps != null && data.usedMaps.Length > 0)
            {
                foreach (int mapIndex in data.usedMaps)
                {
                    if (mapIndex >= 0 && mapIndex < mapButtons.Length)
                    {
                        mapButtons[mapIndex].interactable = false;
                        mapToggles[mapIndex].interactable = false;
                        Debug.Log($"[ChooseScene] Map {mapIndex} disabled (already used)");
                    }
                }
            }

            // 맵 선택 UI 활성화 및 카운트다운 시작
            mapChooseUI.SetActive(true);
            selectMapUI.SetActive(false);
            StartCoroutine(CountdownRoutine());
        }
        // 맵 결과 수신
        else if (json.Contains("\"type\":\"map_result\""))
        {
            var result = JsonUtility.FromJson<MapResultPacket>(json);
            Debug.Log($"[ChooseScene] Final map result received: {result.selectedMap}");

            mapChooseUI.SetActive(false);
            selectMapUI.SetActive(true);
            selectedMap = result.selectedMap;

            // 3초 후 룰렛 시작
            DOVirtual.DelayedCall(3f, () =>
            {
                StartCoroutine(SmoothRouletteArrow(result.selectedMap));
            });
        }
    }

    // === 룰렛 애니메이션 ===
    private IEnumerator SmoothRouletteArrow(int finalIndex)
    {
        Debug.Log("[ChooseScene] Starting smooth roulette...");

        int totalRounds = 3;
        int currentIndex = 0;
        int totalSteps = totalRounds * mapImages.Length + finalIndex;

        while (currentIndex < totalSteps)
        {
            float t = (float)currentIndex / totalSteps;
            float speed = Mathf.Lerp(0.05f, 0.25f, Mathf.Pow(t, 2));

            int nextIndex = (currentIndex % mapImages.Length);
            RectTransform targetImage = mapImages[nextIndex];

            arrow.DOLocalMoveX(targetImage.localPosition.x, speed)
                 .SetEase(Ease.InOutSine);

            yield return new WaitForSeconds(speed);
            currentIndex++;
        }

        // --- 최종 도착 ---
        RectTransform finalImage = mapImages[finalIndex];
        arrow.DOLocalMoveX(finalImage.localPosition.x, 0.8f)
             .SetEase(Ease.OutQuart)
             .OnComplete(() =>
             {
                 Debug.Log($"[ChooseScene] Arrow stopped at Map {finalIndex}");
                 StartCoroutine(StartGameCountdown(finalIndex));
             });
    }

    private IEnumerator StartGameCountdown(int finalIndex)
    {
        int countdown = 5;
        countdownText.gameObject.SetActive(true);

        while (countdown > 0)
        {
            countdownText.text = countdown.ToString();

            // 살짝 커졌다 작아지는 연출
            countdownText.transform
                .DOScale(1.3f, 0.3f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => countdownText.transform.DOScale(1f, 0.2f));

            yield return new WaitForSeconds(1f);
            countdown--;
        }

        countdownText.text = "0";
        Debug.Log("[ChooseScene] Countdown complete - loading map...");

        // --- 맵 번호에 따라 씬 전환 ---
        string nextScene = "";
        switch (finalIndex)
        {
            case 0:
                nextScene = "Map0Scene";
                break;
            case 1:
                nextScene = "Map1Scene";
                break;
            case 2:
                nextScene = "Map2Scene";
                break;
            default:
                nextScene = "Map0Scene"; // fallback
                break;
        }

        Debug.Log($"[ChooseScene] Loading Scene: {nextScene}");
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene);
    }

}

