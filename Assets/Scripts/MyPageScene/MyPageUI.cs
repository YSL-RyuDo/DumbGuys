using UnityEngine;
using TMPro;

public class MyPageUI : MonoBehaviour
{
    public static MyPageUI Instance;

    [Header("Nickname UI")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private TextMeshProUGUI errorText;

    private void Awake()
    {
        Instance = this;
    }

    public void ApplyNickname(string nickname)
    {
        if (nicknameText != null)
            nicknameText.text = nickname;

        if (errorText != null)
            errorText.text = "";
    }

    public void OnClickChangeNickname()
    {
        string newNick = nicknameInput.text;
        if (string.IsNullOrWhiteSpace(newNick))
        {
            ShowError("닉네임을 입력하세요.");
            return;
        }

        MyPageManager.Instance.RequestNicknameChange(newNick);
    }

    public void ShowError(string msg)
    {
        if (errorText != null)
            errorText.text = msg;
    }
}
