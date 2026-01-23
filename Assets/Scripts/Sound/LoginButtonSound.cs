using UnityEngine;

public class LoginButtonSound : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip clickSound;

    public void OnClickLogin()
    {
        audioSource.PlayOneShot(clickSound);
    }
}
