using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text playerNickText;
    [SerializeField] private TMP_Text enemyNickText;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button lobbyButton;
    [SerializeField] private RectTransform enemyInfoPanel;

    private bool isCounting = false;

    private void Start()
    {
        if (ClientSession.Instance != null && ClientSession.Instance.IsLoggedIn)
            playerNickText.text = ClientSession.Instance.Nickname;
        else
            playerNickText.text = "Unknown";

        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(OnClickLobby);

        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;

        countText.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void OnClickLobby()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"match_found\""))
        {
            string opponent = ExtractOpponentName(json);
            if (!string.IsNullOrEmpty(opponent))
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    enemyNickText.text = opponent;

                    // EnemyInfo 이동
                    if (enemyInfoPanel != null)
                    {
                        enemyInfoPanel.DOAnchorPosX(585f, 0.8f).SetEase(Ease.OutCubic);
                    }

                    // 로비 버튼 숨기기
                    if (lobbyButton != null)
                        lobbyButton.gameObject.SetActive(false);

                    // 카운트다운 시작
                    if (!isCounting)
                    {
                        isCounting = true;
                        StartCoroutine(StartCountdown());
                    }
                });
            }
        }
        else if (json.Contains("\"type\":\"match_start\""))
        {
            Debug.Log("[LoadingScene] Match officially started! Moving to ChooseScene...");
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                SceneManager.LoadScene("ChooseScene");
            });
        }
    }

    private IEnumerator StartCountdown()
    {
        countText.gameObject.SetActive(true);
        for (int i = 3; i >= 0; i--)
        {
            countText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        // 0초 도달 → 서버에 준비 완료 패킷 전송
        string nickname = ClientSession.Instance.Nickname;
        var readyPacket = new MatchReadyRequest(nickname); // 클래스 사용
        NetworkManager.Instance.SendJson(readyPacket);

        Debug.Log("[LoadingScene] Countdown done. Sent match_ready to server.");
    }


    private string ExtractOpponentName(string json)
    {
        int start = json.IndexOf("\"opponent\":\"") + "\"opponent\":\"".Length;
        int end = json.IndexOf("\"", start);
        if (start > 0 && end > start)
            return json.Substring(start, end - start);
        return string.Empty;
    }
}
