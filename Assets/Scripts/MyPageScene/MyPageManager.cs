using UnityEngine;

public class MyPageManager : MonoBehaviour
{
    public static MyPageManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        NetworkManager.Instance.OnMessageReceived += HandleServerMessage;
    }

    private void OnDisable()
    {
        NetworkManager.Instance.OnMessageReceived -= HandleServerMessage;
    }

    private void Start()
    {
        if (ClientSession.Instance.IsLoggedIn)
        {
            // 닉네임 표시
            MyPageUI.Instance.ApplyNickname(ClientSession.Instance.Nickname);

            // 외형 요청
            RequestAppearance();
        }
    }

    private void RequestAppearance()
    {
        var req = new AppearanceRequest
        {
            nickname = ClientSession.Instance.Nickname
        };

        NetworkManager.Instance.SendJson(req);
        Debug.Log("[MyPage] Requested appearance info");
    }

    public void RequestNicknameChange(string newNick)
    {
        if (!ClientSession.Instance.IsLoggedIn)
        {
            Debug.LogError("[MyPage] Not logged in");
            return;
        }

        var req = new NicknameChangeRequest
        {
            oldNickname = ClientSession.Instance.Nickname,
            newNickname = newNick
        };

        NetworkManager.Instance.SendJson(req);

        Debug.Log($"[MyPage] Sent nickname change request → {req.oldNickname} -> {req.newNickname}");
    }
    private void HandleServerMessage(string json)
    {
        // 닉네임 변경 처리
        if (json.Contains("\"type\":\"nickname_change\""))
        {
            var res = JsonUtility.FromJson<NicknameChangeResponse>(json);

            if (res.result)
            {
                ClientSession.Instance.UpdateNickname(res.newNickname);
                MyPageUI.Instance.ApplyNickname(res.newNickname);
            }
            else
            {
                MyPageUI.Instance.ShowError(res.msg);
            }
            return;
        }

        if (json.Contains("\"type\":\"appearance_info\""))
        {
            var info = JsonUtility.FromJson<AppearanceResponse>(json);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                // 2) 캐릭터에 외형 적용
                CharacterAppearance appearance =
                    FindFirstObjectByType<CharacterAppearance>();

                if (appearance != null)
                    appearance.ApplyAppearance(info);
            });

            Debug.Log("[MyPage] Applied appearance_info to UI & character");
            return;
        }
    }

}
