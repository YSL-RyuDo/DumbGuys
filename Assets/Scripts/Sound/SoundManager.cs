using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Clips")]
    public AudioClip[] bgmClips;
    public AudioClip[] sfxClips;

    private Dictionary<string, AudioClip> bgmDict = new();
    private Dictionary<string, AudioClip> sfxDict = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var clip in bgmClips)
            bgmDict[clip.name] = clip;

        foreach (var clip in sfxClips)
            sfxDict[clip.name] = clip;
    }

    // BGM 재생
    public void PlayBGM(string name)
    {
        if (!bgmDict.TryGetValue(name, out var clip)) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // SFX 재생 (겹쳐서 가능)
    public void PlaySFX(string name)
    {
        if (!sfxDict.TryGetValue(name, out var clip)) return;

        sfxSource.PlayOneShot(clip);
    }
}
