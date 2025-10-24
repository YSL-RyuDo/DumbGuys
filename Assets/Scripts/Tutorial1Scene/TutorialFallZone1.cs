using UnityEngine;

public class TutorialFallZone1 : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var appearance = other.GetComponent<CharacterAppearance>();
        if (appearance == null)
            return;

        // 튜토리얼 매니저를 찾아서 리스폰 처리
        var controller = FindFirstObjectByType<Tutorial1Manager>();
        if (controller != null)
        {
            Debug.Log("[TutorialFallZone] Player fell. Respawning...");
            controller.RespawnCharacter(appearance);
        }
    }
}
