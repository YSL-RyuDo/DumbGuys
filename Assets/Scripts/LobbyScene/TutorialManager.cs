using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;
    private int tutorialStage = 0;
    public bool IsTutorialDone { get; private set; } = false;

    private void Awake() => Instance = this;

    public void SetStage(int stage)
    {
        tutorialStage = Mathf.Max(0, stage);
        Debug.Log($"[Tutorial] Restored to stage {tutorialStage}");
    }

    public int GetStage() => tutorialStage;

    public void StartTutorial()
    {
        IsTutorialDone = false;
        Debug.Log($"[Tutorial] Starting Tutorial0 at stage {tutorialStage}");
        ShowStage0Message();
    }

    // [튜토리얼0 단계]
    private void ShowStage0Message()
    {
        var lobby = LobbyButtonHandler.Instance;
        if (lobby == null) return;

        // Next 버튼 누르면 튜토리얼 다음 단계로 진행
        lobby.ShowTutorialMessage(
            "Welcome to DumbGuys!\nLet's learn the basics!",
            true // Next 버튼 활성화 (다음 단계로 진행)
        );

        // Next 버튼 눌렀을 때 다음 단계로 넘어가게 설정
        if (lobby.tutorialNextButton != null)
        {
            lobby.tutorialNextButton.onClick.RemoveAllListeners();
            lobby.tutorialNextButton.onClick.AddListener(() =>
            {
                Debug.Log("[Tutorial] Next clicked at Stage0 → advance");
                lobby.HideTutorialMessage();

                var tutorial = TutorialManager.Instance;
                if (tutorial != null && !tutorial.IsTutorialDone)
                {
                    tutorial.OnNextTutorial0Step();
                }
            });
        }
    }

    public void OnNextTutorial0Step()
    {
        tutorialStage++;
        Debug.Log($"[Tutorial0] Step advanced → {tutorialStage}");

        var lobby = LobbyButtonHandler.Instance;
        if (lobby == null) return;

        switch (tutorialStage)
        {
            case 1:
                lobby.ShowTutorialMessage("Click the MODE button to continue!", true);
                lobby.SetAllButtonsInteractable(false);
                lobby.SetButtonInteractable("Mode", true);
                lobby.HighlightButtonLoop("Mode");
                break;

            case 2:
                lobby.StopHighlightButton("Mode");
                lobby.ShowTutorialMessage("Now click SINGLEPLAY!", true);
                lobby.SetAllButtonsInteractable(false);
                lobby.SetButtonInteractable("Single", true);
                lobby.HighlightButtonLoop("Single");
                break;

            case 3:
                lobby.StopHighlightButton("Single");
                lobby.ShowTutorialMessage("Select TUTORIAL 0 to start playing!", true);
                lobby.SetAllButtonsInteractable(false);
                lobby.SetButtonInteractable("Tutorial0", true);
                lobby.HighlightButtonLoop("Tutorial0");
                break;

            case 4:
                lobby.StopHighlightButton("Tutorial0");
                lobby.ShowTutorialMessage("Finally, press PLAY to begin your training!", true);
                lobby.SetAllButtonsInteractable(false);
                lobby.SetButtonInteractable("Play", true);
                lobby.HighlightButtonLoop("Play");
                break;

            case 5:
                MarkTutorialCompleted();
                break;
        }
    }


    public void MarkTutorialCompleted()
    {
        if (IsTutorialDone) return;

        IsTutorialDone = true;
        tutorialStage = 0;

        Debug.Log("[Tutorial] Tutorial0 completed.");

        var req = new TutorialCompleteRequest
        {
            cmd = "set_tutorial_done",
            nickname = ClientSession.Instance?.Nickname
        };
        NetworkManager.Instance?.SendJson(req);

        var lobby = LobbyButtonHandler.Instance;
        lobby?.HideTutorialMessage();
        lobby?.StopAllHighlights();
    }
}
