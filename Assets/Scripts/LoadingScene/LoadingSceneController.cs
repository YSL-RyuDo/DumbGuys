using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;


public class LoadingSceneController : MonoBehaviour
{
    [Header("UI Movement")]
    [SerializeField] private RectTransform leftImage;
    [SerializeField] private RectTransform rightImage;
    [SerializeField] private float moveDistance = 800f;
    [SerializeField] private float duration = 0.8f;

    [Header("Model References")]
    [SerializeField] private CharacterAppearance playerAppearance;
    [SerializeField] private CharacterAppearance enemyAppearance;

    private Vector3 leftOriginPos;
    private Vector3 rightOriginPos;

    void Start()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived += HandleServerMessage;

        RequestPlayerAppearance();

        if (!ClientSession.Instance.IsLoggedIn)
        {
            Debug.LogError("[Lobby] Not logged in.");
            return;
        }

        string nickname = ClientSession.Instance.Nickname;

        // 익명 객체 대신 명시적 클래스 사용
        var req = new MatchRequest(nickname);
        NetworkManager.Instance.SendJson(req);

        Debug.Log($"[Lobby] Matchmaking started for {nickname}");

        leftOriginPos = leftImage.localPosition;
        rightOriginPos = rightImage.localPosition;
        MoveImages();
    }

    void OnDestroy()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void RequestPlayerAppearance()
    {
        if (!ClientSession.Instance.IsLoggedIn) return;

        string nickname = ClientSession.Instance.Nickname;
        var req = new AppearanceRequest { nickname = nickname };
        NetworkManager.Instance.SendJson(req);
    }

    private void HandleServerMessage(string json)
    {
        if (json.Contains("\"type\":\"appearance_info\""))
        {
            var data = JsonUtility.FromJson<AppearanceResponse>(json);
            playerAppearance?.ApplyAppearance(data);
        }
        else if (json.Contains("\"type\":\"enemy_info\""))
        {
            var data = JsonUtility.FromJson<AppearanceResponse>(json);
            enemyAppearance?.ApplyAppearance(data);
            Debug.Log($"[LoadingScene] Applied enemy appearance ({data.nickname})");

            // 등장 연출 (살짝 내려오게)
            AppearEnemySmoothly();
        }
        else if (json.Contains("\"type\":\"match_found\""))
        {
            string opponent = ExtractOpponentName(json);
            if (!string.IsNullOrEmpty(opponent))
            {
                var req = new AppearanceRequest
                {
                    cmd = "get_enemy_info",
                    nickname = opponent
                };

                NetworkManager.Instance.SendJson(req);
                Debug.Log($"[LoadingScene] Match found! Requesting enemy appearance for {opponent}");
            }
        }

    }

    private void MoveImages()
    {
        Vector3 leftTargetPos = leftImage.localPosition + leftImage.transform.up * moveDistance;
        Vector3 rightTargetPos = rightImage.localPosition - rightImage.transform.up * moveDistance;

        leftImage.DOLocalMove(leftTargetPos, duration).SetEase(Ease.OutCubic);
        rightImage.DOLocalMove(rightTargetPos, duration).SetEase(Ease.OutCubic);
    }

    private string ExtractOpponentName(string json)
    {
        int start = json.IndexOf("\"opponent\":\"");
        if (start == -1)
            return "";

        start += 12; // "opponent":" ← 이 길이
        int end = json.IndexOf("\"", start);
        if (end == -1)
            return "";

        return json.Substring(start, end - start);
    }

    private void AppearEnemySmoothly()
    {
        if (enemyAppearance == null)
        {
            Debug.LogWarning("[LoadingScene] enemyAppearance is null!");
            return;
        }

        Transform enemyTransform = enemyAppearance.transform;

        // 시작 위치: 위로 3~5 정도 띄우고
        Vector3 startPos = enemyTransform.position + Vector3.up * 3f;
        Vector3 endPos = new Vector3(enemyTransform.position.x, 0f, enemyTransform.position.z);

        // 위치 초기화
        enemyTransform.position = startPos;

        // 천천히 내려오며 착지
        enemyTransform.DOMove(endPos, 1.0f)
        .SetEase(Ease.OutQuart) // 더 부드럽고 잔진동 없는 착지
        .OnStart(() =>
        {
            Debug.Log("[LoadingScene] Enemy is entering the scene...");
        })
        .OnComplete(() =>
        {
            Debug.Log("[LoadingScene] Enemy landed smoothly!");
        });

    }



}
