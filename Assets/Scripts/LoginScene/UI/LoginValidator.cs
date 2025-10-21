using UnityEngine;

public class LoginValidator
{
    public bool ValidateLogin(string id, string pw)
    {
        bool ok = true;
        if (string.IsNullOrEmpty(id)) { Debug.LogWarning("ID is required."); ok = false; }
        if (string.IsNullOrEmpty(pw)) { Debug.LogWarning("Password is required."); ok = false; }
        return ok;
    }

    public bool ValidateSignUp(string id, string pw, string nick)
    {
        bool ok = true;
        if (string.IsNullOrEmpty(id)) { Debug.LogWarning("ID is required."); ok = false; }
        if (string.IsNullOrEmpty(pw)) { Debug.LogWarning("Password is required."); ok = false; }
        if (string.IsNullOrEmpty(nick)) { Debug.LogWarning("Nickname is required."); ok = false; }
        return ok;
    }
}
