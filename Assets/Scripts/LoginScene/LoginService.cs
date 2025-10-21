using UnityEngine;

public class LoginService
{
    public void Login(string id, string pw)
    {
        var packet = new LoginPacket
        {
            cmd = "login",
            id = id,
            password = pw
        };

        NetworkManager.Instance.SendJson(packet);
        Debug.Log($"[LoginService] Sent Login Packet: {id}");
    }

    public void SignUp(string id, string pw, string nick)
    {
        var packet = new LoginPacket
        {
            cmd = "signup",
            id = id,
            password = pw,
            nickname = nick
        };

        NetworkManager.Instance.SendJson(packet);
        Debug.Log($"[LoginService] Sent SignUp Packet: {id}, Nickname: {nick}");
    }
}
