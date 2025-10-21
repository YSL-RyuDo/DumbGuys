using System;

[Serializable]
public class LoginPacket
{
    public string cmd;      // "login" or "signup"
    public string id;
    public string password;
    public string nickname;
}

