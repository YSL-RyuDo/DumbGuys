using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LoginUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField idInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField nicknameInput;

    [SerializeField] private Button loginButton;
    [SerializeField] private Button changeModeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private TMP_Text loginButtonLabel;
    [SerializeField] private TMP_Text messageText;

    public string Id => idInput.text.Trim();
    public string Password => passwordInput.text.Trim();
    public string Nickname => nicknameInput.text.Trim();

    private void Awake()
    {
        nicknameInput.gameObject.SetActive(false);

        loginButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX("mixkit-classic-click");
            EventBus.RaiseLogin();
        });
        changeModeButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX("mixkit-classic-click");
            EventBus.RaiseChangeMode();
        });
        quitButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.PlaySFX("mixkit-arcade-retro-game-over");
            EventBus.RaiseQuit();
        });

        loginButtonLabel.text = "Login";
    }

    private void OnEnable()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMessageReceived += HandleNetworkMessage;
            NetworkManager.Instance.OnLoginSuccess += HandleLoginSuccess;
        }
    }

    private void OnDisable()
    {
        if (NetworkManager.Instance != null)
        {
            NetworkManager.Instance.OnMessageReceived -= HandleNetworkMessage;
            NetworkManager.Instance.OnLoginSuccess -= HandleLoginSuccess;
        }
    }

    private void HandleNetworkMessage(string msg)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            messageText.text = msg;
        });
    }

    private void HandleLoginSuccess()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void SetNicknameVisible(bool visible) => nicknameInput.gameObject.SetActive(visible);

    public void SetLoginButtonText(string text) => loginButtonLabel.text = text;
}
