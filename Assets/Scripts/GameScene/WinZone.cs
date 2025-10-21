using UnityEngine;

public class WinZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var appearance = other.GetComponent<CharacterAppearance>();
        if (appearance == null) return;

        // 본인(플레이어)만 서버에 알림
        if (appearance.CompareTag("Player"))
        {
            string nickname = ClientSession.Instance.Nickname;

            var packet = new PlayerWinPacket
            {
                nickname = nickname
            };

            NetworkManager.Instance.SendJson(packet);
            Debug.Log($"[WinZone] {nickname} reached the goal! Sent PlayerWinPacket.");
        }
    }
}
