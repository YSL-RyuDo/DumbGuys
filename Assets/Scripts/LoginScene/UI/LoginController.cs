using UnityEngine;

public class LoginController : MonoBehaviour
{
    [SerializeField] private LoginUI loginUI;

    private LoginValidator validator;
    private LoginService service;
    private bool isSignUpMode = false;

    private void Awake()
    {
        validator = new LoginValidator();
        service = new LoginService();
    }

    private void OnEnable()
    {
        EventBus.OnLoginRequest += HandleLogin;
        EventBus.OnChangeModeRequest += HandleChangeMode;
        EventBus.OnQuitRequest += HandleQuit;
    }

    private void OnDisable()
    {
        EventBus.OnLoginRequest -= HandleLogin;
        EventBus.OnChangeModeRequest -= HandleChangeMode;
        EventBus.OnQuitRequest -= HandleQuit;
    }

    private void HandleLogin()
    {
        string id = loginUI.Id;
        string pw = loginUI.Password;
        string nick = loginUI.Nickname;

        if (!isSignUpMode)
        {
            if (validator.ValidateLogin(id, pw))
                service.Login(id, pw);
        }
        else
        {
            if (validator.ValidateSignUp(id, pw, nick))
                service.SignUp(id, pw, nick);
        }
    }

    private void HandleChangeMode()
    {
        isSignUpMode = !isSignUpMode;
        loginUI.SetNicknameVisible(isSignUpMode);
        loginUI.SetLoginButtonText(isSignUpMode ? "Sign Up" : "Login");
        Debug.Log(isSignUpMode ? "Sign Up Mode" : "Login Mode");
    }

    private void HandleQuit()
    {
        Debug.Log("Game Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
