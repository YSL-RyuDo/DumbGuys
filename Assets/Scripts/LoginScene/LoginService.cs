using UnityEngine;

public class LoginService
{
    public void Login(string id, string pw)
    {
        Debug.Log($"로그인 성공! (ID: {id})");
    }

    public void SignUp(string id, string pw, string nick)
    {
        Debug.Log($"회원가입 성공! (ID: {id}, Nick: {nick})");
    }
}
