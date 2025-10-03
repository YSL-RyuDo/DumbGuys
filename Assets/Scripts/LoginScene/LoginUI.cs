using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField idInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField nicknameInput;

    [SerializeField] private Button loginButton;
    [SerializeField] private Button changeModeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text loginButtonLabel; // 버튼 라벨 (Login / Sign Up)

    public string Id => idInput.text.Trim();
    public string Password => passwordInput.text.Trim();
    public string Nickname => nicknameInput.text.Trim();

    private void Awake()
    {
        nicknameInput.gameObject.SetActive(false);

        loginButton.onClick.AddListener(() => EventBus.RaiseLogin());
        changeModeButton.onClick.AddListener(() => EventBus.RaiseChangeMode());
        quitButton.onClick.AddListener(() => EventBus.RaiseQuit());

        // 초기 버튼 텍스트
        loginButtonLabel.text = "Login";
    }

    public void SetNicknameVisible(bool visible) => nicknameInput.gameObject.SetActive(visible);

    public void SetLoginButtonText(string text) => loginButtonLabel.text = text;
}
