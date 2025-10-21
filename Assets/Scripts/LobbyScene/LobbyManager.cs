using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private CharacterAppearance characterAppearance;
    [SerializeField] private LobbyUI lobbyUI;

    private void Start()
    {
        if (!ClientSession.Instance.IsLoggedIn)
        {
            Debug.LogError("[Lobby] No session found.");
            return;
        }

        string nickname = ClientSession.Instance.Nickname;

        // 1. 플레이어 정보 요청
        var playerReq = new PlayerInfoRequest(nickname);
        NetworkManager.Instance.SendJson(playerReq);
        Debug.Log($"[Client] Requested player info for: {nickname}");

        // 2. 외형 정보 요청
        var appearanceReq = new AppearanceRequest { nickname = nickname };
        NetworkManager.Instance.SendJson(appearanceReq);
        Debug.Log($"[Client] Requested appearance info for: {nickname}");
    }

    private void OnEnable()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }

    private void OnDisable()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"player_info\""))
        {
            var info = JsonUtility.FromJson<PlayerInfoResponse>(json);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                lobbyUI.ApplyPlayerInfo(info);
            });
        }
        else if (json.Contains("\"type\":\"appearance_info\""))
        {
            var info = JsonUtility.FromJson<AppearanceResponse>(json);
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                characterAppearance.ApplyAppearance(info);
            });
        }
    }
}
