using UnityEngine;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    private int tutorialStep = 0;
    private Coroutine subscribeRoutine;

    // 다음 튜토리얼 씬 인덱스 (-1이면 없음)
    public int nextTutorialIndex = -1;

    public bool IsTutorialCompleted => tutorialStep >= 4;
    public bool IsTutorialRunning => tutorialStep < 4;
    private bool isWarningActive = false;
    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        subscribeRoutine = StartCoroutine(WaitAndSubscribe());
    }

    private void OnDisable()
    {
        if (EventTriggerHandler.Instance != null)
            EventTriggerHandler.Instance.OnScreenClicked -= HandleScreenClick;

        if (subscribeRoutine != null)
            StopCoroutine(subscribeRoutine);
    }

    private IEnumerator WaitAndSubscribe()
    {
        while (EventTriggerHandler.Instance == null)
            yield return null;

        EventTriggerHandler.Instance.OnScreenClicked += HandleScreenClick;
        Debug.Log("[Tutorial] Subscribed to EventTriggerHandler");
    }

    private void HandleScreenClick(GameObject clickedObj)
    {
        OnScreenClick();
    }

    // === 튜토리얼 시작 ===
    public void StartTutorial()
    {
        Debug.Log("[Tutorial] Starting tutorial...");
        tutorialStep = 0;
        nextTutorialIndex = -1;

        // UI 요청은 LobbyButtonHandler에 위임
        var lobby = LobbyButtonHandler.Instance;
        if (lobby != null)
        {
            lobby.ShowTutorialMessage("Welcome to the game!\nClick anywhere to begin.");
            lobby.SetGameModeUIActive(false);
            lobby.SetTutorialPanelActive(false);
            //lobby.SetAllButtonsInteractable(false);
        }
    }

    // === 클릭 시 진행 ===
    public void OnScreenClick()
    {
        var lobby = LobbyButtonHandler.Instance;
        if (lobby == null) return;

        // 경고창이 떠 있는 상태라면 → 클릭 시 닫고 리턴
        if (isWarningActive)
        {
            lobby.HideTutorialMessage();
            isWarningActive = false;
            Debug.Log("[Tutorial] Warning message dismissed by click.");
            return;
        }

        if (IsTutorialCompleted) return;

        Debug.Log($"[Tutorial] OnScreenClick Step = {tutorialStep}");

        switch (tutorialStep)
        {
            case 0:
                tutorialStep = 1;
                lobby.HideTutorialMessage();
                lobby.SetAllButtonsInteractable(false);
                lobby.SetButtonInteractable("Mode", true);
                lobby.HighlightButton("Mode");
                Debug.Log("[Tutorial] Step 0 -> Waiting for Mode button.");
                break;

            case 2:
                lobby.HideTutorialMessage();
                lobby.HighlightButton("Single");
                Debug.Log("[Tutorial] Step 2 -> SINGLEPLAY highlighted.");
                break;
        }
    }

    // === 모드 버튼 클릭 (튜토리얼) ===
    public void OnModeButtonClicked_Tutorial()
    {
        if (IsTutorialCompleted) return;
        var lobby = LobbyButtonHandler.Instance;
        if (lobby == null) return;

        if (tutorialStep == 1)
        {
            tutorialStep = 2;
            Debug.Log("[Tutorial] Mode button clicked (tutorial)");

            lobby.SetGameModeUIActive(true);
            lobby.SetAllButtonsInteractable(false);
            lobby.SetButtonInteractable("Single", true);
            lobby.ShowTutorialMessage(
                "The left button is MULTIPLAYER.\nThe right button is SINGLEPLAYER.\n\nClick SINGLEPLAYER to continue."
            );
        }
        else
        {
            isWarningActive = true;
            lobby.ShowTutorialMessage("You can't press that yet!\nFollow the instructions.");
            lobby.HighlightButton("Mode");
        }

    }

    // === 싱글플레이 버튼 클릭 (튜토리얼) ===
    public void OnSinglePlayButtonClicked_Tutorial()
    {
        if (IsTutorialCompleted) return;
        var lobby = LobbyButtonHandler.Instance;
        if (lobby == null) return;

        if (tutorialStep == 2)
        {
            tutorialStep = 3;
            Debug.Log("[Tutorial] SinglePlay clicked (tutorial)");

            lobby.SetAllButtonsInteractable(false);
            lobby.SetButtonInteractable("Tutorial0", true);
            lobby.SetTutorialPanelActive(true);
            lobby.ShowTutorialMessage(
                "Let's begin with movement!\nPress the TUTORIAL 0 button to start."
            );
        }
        else
        {
            lobby.ShowTutorialMessage("You can't press that yet!\nFollow the instructions.");
            lobby.HighlightButton("Single");
        }
    }

    // === 기타 잘못된 버튼 클릭 ===
    public void OnOtherButtonClicked()
    {
        if (IsTutorialCompleted) return;
        LobbyButtonHandler.Instance?.ShowTutorialMessage("You can't press that yet!");
    }

    // === 서버에서 완료 상태 수신 ===
    public void MarkTutorialCompleted()
    {
        tutorialStep = 4;
        nextTutorialIndex = -1;

        var lobby = LobbyButtonHandler.Instance;
        if (lobby != null)
        {
            lobby.HideTutorialMessage();
            lobby.SetGameModeUIActive(false);
            lobby.SetTutorialPanelActive(false);
        }

        Debug.Log("[Tutorial] Marked as completed (from server data)");
    }
}
