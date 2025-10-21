using UnityEngine;
using TMPro;

public class ResultSceneController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text enemyNameText;
    [SerializeField] private TMP_Text resultText;

    [Header("Characters")]
    [SerializeField] private CharacterAppearance playerAppearance;
    [SerializeField] private CharacterAppearance enemyAppearance;

    private string playerNickname;
    private string enemyNickname;
    private string winnerName;

    private void Start()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;

        playerNickname = ClientSession.Instance.Nickname;

        RequestWinnerInfo();
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void RequestWinnerInfo()
    {
        var packet = new GameResultRequest
        {
            nickname = playerNickname
        };

        NetworkManager.Instance.SendJson(packet);
        Debug.Log($"[ResultScene] Requested game result for {playerNickname}");
    }


    private void UpdateResultText()
    {
        if (string.IsNullOrEmpty(winnerName)) return;

        if (winnerName == playerNickname)
        {
            resultText.text = "YOU WIN!";
            resultText.color = Color.yellow;
        }
        else
        {
            resultText.text = "YOU LOSE...";
            resultText.color = Color.gray;
        }
    }

    private void RequestAppearances()
    {
        var reqPlayer = new AppearanceRequest { nickname = playerNickname };
        NetworkManager.Instance.SendJson(reqPlayer);
        Debug.Log($"[ResultScene] Requested appearance for player: {playerNickname}");

        var reqEnemy = new AppearanceRequest { nickname = enemyNickname };
        NetworkManager.Instance.SendJson(reqEnemy);
        Debug.Log($"[ResultScene] Requested appearance for enemy: {enemyNickname}");
    }

    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"game_result\""))
        {
            var data = JsonUtility.FromJson<GameResultPacket>(json);
            winnerName = data.winner;
            enemyNickname = data.enemy;

            playerNameText.text = playerNickname;
            enemyNameText.text = enemyNickname;

            Debug.Log($"[ResultScene] Winner: {winnerName}, Enemy: {enemyNickname}");

            UpdateResultText();
            RequestAppearances();
        }
        else if (json.Contains("\"type\":\"appearance_info\""))
        {
            var data = JsonUtility.FromJson<AppearanceResponse>(json);
            if (data.nickname == playerNickname)
                playerAppearance.ApplyAppearance(data);
            else if (data.nickname == enemyNickname)
                enemyAppearance.ApplyAppearance(data);
        }
    }

    public void OnClickLobbyButton()
    {
        var packet = new LeaveRoomPacket
        {
            nickname = playerNickname
        };

        NetworkManager.Instance.SendJson(packet);
        Debug.Log($"[ResultScene] {playerNickname} sent leave_room request.");

        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}
