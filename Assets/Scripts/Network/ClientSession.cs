using UnityEngine;

public class ClientSession : MonoBehaviour
{
    public static ClientSession Instance { get; private set; }

    public string Nickname { get; private set; }
    public bool IsLoggedIn { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetLoginData(string nickname)
    {
        Nickname = nickname;
        IsLoggedIn = true;
    }

    public void UpdateNickname(string newNickname)
    {
        Nickname = newNickname;
    }

    public void Logout()
    {
        IsLoggedIn = false;
        Nickname = string.Empty;
    }
}
