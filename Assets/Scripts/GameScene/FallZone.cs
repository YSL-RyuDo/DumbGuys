using UnityEngine;

public class FallZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어나 적 둘 다 CharacterAppearance를 가지고 있음
        var appearance = other.GetComponent<CharacterAppearance>();
        if (appearance == null)
            return;

        // GameSceneController를 찾아서 리스폰 요청
        GameSceneController controller = FindFirstObjectByType<GameSceneController>();
        if (controller != null)
        {
            controller.RespawnCharacter(appearance);
        }
    }
}
