using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class LobbyButtonHandler : MonoBehaviour
{
    public static LobbyButtonHandler Instance;
    private string selectedSceneName = null;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialMessagePanel;
    [SerializeField] private GameObject gameModeUI;
    [SerializeField] private GameObject tutorialActionPanel;

    [SerializeField] private Button modeButton;
    [SerializeField] private Button singlePlayButton;
    [SerializeField] private Button multiPlayButton;
    [SerializeField] private Button tutorial0Button;
    [SerializeField] private Button tutorial1Button;
    [SerializeField] private Button map0Button;
    [SerializeField] private Button map1Button;
    [SerializeField] private Button map2Button;
    [SerializeField] private Button lobbyButton;
    [SerializeField] private Button modesButton;

    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private Image modeButtonIcon;

    [Header("Sprites")]
    [SerializeField] private Sprite tutorial0SourceImage;
    [SerializeField] private Sprite tutorial1SourceImage;
    [SerializeField] private Sprite map0SourceImage;
    [SerializeField] private Sprite map1SourceImage;
    [SerializeField] private Sprite map2SourceImage;

    private bool isMultiMode = true; // 멀티 모드 진입 여부

    private void Awake()
    {
        Instance = this;
    }

    // === PLAY 버튼 ===
    public void OnClickPlay()
    {
        var tutorial = TutorialManager.Instance;

        if (tutorial != null)
        {
            // Case 1: 튜토리얼 중인데 아직 준비 안됨
            if (tutorial.IsTutorialRunning && tutorial.nextTutorialIndex < 0)
            {
                Debug.Log("[Lobby] Play button pressed during tutorial (not ready).");
                tutorial.OnOtherButtonClicked();
                return;
            }

            // Case 2: 다음 튜토리얼 씬으로 이동해야 함
            if (tutorial.nextTutorialIndex >= 0)
            {
                string sceneName = $"Tutorial{tutorial.nextTutorialIndex}Scene";
                Debug.Log($"[Lobby] Play button pressed -> Loading {sceneName}");
                SceneManager.LoadScene(sceneName);
                return;
            }
        }

        if (!string.IsNullOrEmpty(selectedSceneName))
        {
            Debug.Log($"[Lobby] Play button pressed -> Loading selected scene: {selectedSceneName}");
            SceneManager.LoadScene(selectedSceneName);
            return;
        }

        // Case 3: 멀티 모드면 바로 로딩씬으로
        if (isMultiMode)
        {
            Debug.Log("[Lobby] Play button pressed (Multi Mode) -> LoadingScene");
            SceneManager.LoadScene("LoadingScene");
            return;
        }
    }

    // === EXIT 버튼 ===
    public void OnClickExit()
    {
        var tutorial = TutorialManager.Instance;

        // 튜토리얼 중이면 경고
        if (tutorial != null && tutorial.IsTutorialRunning)
        {
            Debug.Log("[Lobby] Exit button pressed during tutorial.");
            tutorial.OnOtherButtonClicked();
            return;
        }

        Debug.Log("[Lobby] Exit button clicked");
        Application.Quit();
    }

    // === MODE 버튼 ===
    public void OnClickMode()
    {
        var tutorial = TutorialManager.Instance;

        if (tutorial != null && !tutorial.IsTutorialCompleted)
        {
            Debug.Log("[Lobby] ModeButton clicked (Tutorial Active)");
            tutorial.OnModeButtonClicked_Tutorial();
            return;
        }

        Debug.Log("[Lobby] ModeButton clicked (Normal Mode)");
        if (gameModeUI != null)
        {
            bool newState = !gameModeUI.activeSelf;
            gameModeUI.SetActive(newState);
            Debug.Log($"[Lobby] GameModeUI {(newState ? "opened" : "closed")} (normal)");
        }
    }

    // === SINGLEPLAY 버튼 ===
    public void OnClickSinglePlay()
    {
        var tutorial = TutorialManager.Instance;

        if (tutorial != null && !tutorial.IsTutorialCompleted)
        {
            Debug.Log("[Lobby] SinglePlay clicked (Tutorial Active)");
            tutorial.OnSinglePlayButtonClicked_Tutorial();
            return;
        }

        Debug.Log("[Lobby] SinglePlay clicked (Normal Mode)");
        isMultiMode = false;

        if (tutorialActionPanel != null)
        {
            tutorialActionPanel.SetActive(true);
            tutorialActionPanel.transform.SetAsLastSibling();
            Debug.Log("[Lobby] TutorialActionPanel opened (Single Mode)");
        }
    }

    // === MULTIPLAY 버튼 ===
    public void OnClickMultiPlay()
    {
        var tutorial = TutorialManager.Instance;

        if (tutorial != null && !tutorial.IsTutorialCompleted)
        {
            Debug.Log("[Lobby] MultiPlay clicked (Tutorial Active)");
            tutorial.OnOtherButtonClicked(); // 튜토리얼 중에는 금지
            return;
        }

        Debug.Log("[Lobby] MultiPlay clicked (Normal Mode)");
        isMultiMode = true;

        if (gameModeUI != null)
            gameModeUI.SetActive(false);

        if (tutorialActionPanel != null)
        {
            tutorialActionPanel.SetActive(true);
            tutorialActionPanel.transform.SetAsLastSibling();
            Debug.Log("[Lobby] TutorialActionPanel opened (Multi Mode)");
        }
    }

    public void OnClickTutorial0()
    {
        var tutorial = TutorialManager.Instance;

        if (tutorial != null && !tutorial.IsTutorialCompleted)
        {
            Debug.Log("[Lobby] Tutorial0 clicked (Tutorial Active)");
            HideTutorialMessage();
            SetGameModeUIActive(false);
            SetTutorialPanelActive(false);
            SetModeButtonSprite("Tutorial0");

            // 튜토리얼 완료 및 씬 지정
            tutorial.nextTutorialIndex = 0;
            tutorial.MarkTutorialCompleted();
            selectedSceneName = "Tutorial0Scene";
            Debug.Log("[Lobby] Tutorial0 selected -> Next Scene: Tutorial0Scene");
            return;
        }

        // 일반 모드
        Debug.Log("[Lobby] Tutorial0 clicked (Normal Mode)");
        SetModeButtonSprite("Tutorial0");
        SetGameModeUIActive(false);
        SetTutorialPanelActive(false);
        UpdateTutorialSelectionUI(tutorial0Button);
        selectedSceneName = "Tutorial0Scene";
    }

    // === 튜토리얼 / 맵 버튼 ===
    public void OnClickTutorial1()
    {
        Debug.Log("[Lobby] Tutorial1 button clicked");
        SetModeButtonSprite("Tutorial1");
        SetGameModeUIActive(false);
        SetTutorialPanelActive(false);
        UpdateTutorialSelectionUI(tutorial1Button);
        selectedSceneName = "Tutorial1Scene";
    }

    public void OnClickMap0()
    {
        Debug.Log("[Lobby] Map0 button clicked");
        SetModeButtonSprite("Map0");
        SetGameModeUIActive(false);
        SetTutorialPanelActive(false);
        UpdateTutorialSelectionUI(map0Button);
        selectedSceneName = "Map0Scene";
    }

    public void OnClickMap1()
    {
        Debug.Log("[Lobby] Map1 button clicked");
        SetModeButtonSprite("Map1");
        SetGameModeUIActive(false);
        SetTutorialPanelActive(false);
        UpdateTutorialSelectionUI(map1Button);
        selectedSceneName = "Map1Scene";
    }

    public void OnClickMap2()
    {
        Debug.Log("[Lobby] Map2 button clicked");
        SetModeButtonSprite("Map2");
        SetGameModeUIActive(false);
        SetTutorialPanelActive(false);
        UpdateTutorialSelectionUI(map2Button);
        selectedSceneName = "Map2Scene";
    }
    private void UpdateTutorialSelectionUI(Button selected)
    {
        // 선택된 버튼을 시각적으로 강조
        selected.transform.DOKill();
        selected.transform.DOScale(1.1f, 0.3f)
            .SetLoops(2, LoopType.Yoyo)
            .SetEase(Ease.OutBack);
    }

    // === 튜토리얼에서 UI 제어 요청할 때 사용 ===
    public void SetGameModeUIActive(bool active)
    {
        if (gameModeUI != null)
        {
            gameModeUI.SetActive(active);
            Debug.Log($"[Lobby] GameModeUI {(active ? "opened" : "closed")} by TutorialManager");
        }
    }

    public void SetTutorialPanelActive(bool active)
    {
        if (tutorialActionPanel != null)
        {
            tutorialActionPanel.SetActive(active);
            Debug.Log($"[Lobby] TutorialActionPanel {(active ? "opened" : "closed")} by TutorialManager");
        }
    }

    // === 튜토리얼 메시지 ===
    public void ShowTutorialMessage(string message)
    {
        tutorialMessagePanel.SetActive(true);
        tutorialText.text = message;

        // 기존 코루틴이 돌고 있다면 중복 방지
        StopAllCoroutines();
        StartCoroutine(HideTutorialMessageAfterDelay(3f));
    }

    private IEnumerator HideTutorialMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        tutorialMessagePanel.SetActive(false);
        Debug.Log("[Lobby] Tutorial message auto-hidden after delay");
    }

    public void HideTutorialMessage()
    {
        tutorialMessagePanel.SetActive(false);
    }

    // === 버튼 상호작용 제어 ===
    public void SetAllButtonsInteractable(bool state)
    {
        modeButton.interactable = state;
        singlePlayButton.interactable = state;
        multiPlayButton.interactable = state;
        tutorial0Button.interactable = state;
        tutorial1Button.interactable = state;
        map0Button.interactable = state;
        map1Button.interactable = state;
        map2Button.interactable = state;
        lobbyButton.interactable = state;
        modesButton.interactable = state;
    }

    public void SetButtonInteractable(string buttonName, bool state)
    {
        switch (buttonName)
        {
            case "Mode": modeButton.interactable = state; break;
            case "Single": singlePlayButton.interactable = state; break;
            case "Multi": multiPlayButton.interactable = state; break;
            case "Tutorial0": tutorial0Button.interactable = state; break;
            case "Tutorial1": tutorial1Button.interactable = state; break;
        }
    }

    // === 버튼 강조 ===
    public void HighlightButton(string buttonName)
    {
        Button btn = null;
        switch (buttonName)
        {
            case "Mode": btn = modeButton; break;
            case "Single": btn = singlePlayButton; break;
            case "Multi": btn = multiPlayButton; break;
            case "Tutorial0": btn = tutorial0Button; break;
        }

        if (btn == null) return;

        var tr = btn.transform;
        tr.DOKill();
        tr.DOScale(1.2f, 0.4f)
          .SetLoops(2, LoopType.Yoyo)
          .SetEase(Ease.OutBack);
    }

    // === 모드 버튼 스프라이트 교체 ===
    public void SetModeButtonSprite(string key)
    {
        switch (key)
        {
            case "Tutorial0": modeButtonIcon.sprite = tutorial0SourceImage; break;
            case "Tutorial1": modeButtonIcon.sprite = tutorial1SourceImage; break;
            case "Map0": modeButtonIcon.sprite = map0SourceImage; break;
            case "Map1": modeButtonIcon.sprite = map1SourceImage; break;
            case "Map2": modeButtonIcon.sprite = map2SourceImage; break;
        }
        Canvas.ForceUpdateCanvases();
    }
}
