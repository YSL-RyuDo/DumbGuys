using UnityEngine;

public class TutorialWinZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var appearance = other.GetComponent<CharacterAppearance>();
        if (appearance == null)
            return;

        // 플레이어만 처리
        if (appearance.CompareTag("Player"))
        {
            Debug.Log("[TutorialWinZone] Player reached the goal!");

            // 튜토리얼 매니저 호출
            var tutorialManager = FindFirstObjectByType<Tutorial0Manager>();
            if (tutorialManager != null)
            {
                tutorialManager.OnTutorialComplete();
            }
        }
    }
}
