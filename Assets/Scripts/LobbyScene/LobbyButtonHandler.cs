using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyButtonHandler : MonoBehaviour
{
    public void OnClickPlay()
    {
        SceneManager.LoadScene("LoadingScene");
    }

    public void OnClickExit()
    {
        Debug.Log("[Lobby] Exit button clicked");
        Application.Quit();
    }
}
