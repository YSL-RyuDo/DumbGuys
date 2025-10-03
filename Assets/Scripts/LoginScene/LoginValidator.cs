using UnityEngine;

public class LoginValidator
{
    public bool ValidateLogin(string id, string pw)
    {
        bool ok = true;
        if (string.IsNullOrEmpty(id)) { Debug.LogWarning("아이디 미입력"); ok = false; }
        if (string.IsNullOrEmpty(pw)) { Debug.LogWarning("패스워드 미입력"); ok = false; }
        return ok;
    }

    public bool ValidateSignUp(string id, string pw, string nick)
    {
        bool ok = true;
        if (string.IsNullOrEmpty(id)) { Debug.LogWarning("아이디 미입력"); ok = false; }
        if (string.IsNullOrEmpty(pw)) { Debug.LogWarning("패스워드 미입력"); ok = false; }
        if (string.IsNullOrEmpty(nick)) { Debug.LogWarning("닉네임 미입력"); ok = false; }
        return ok;
    }
}
