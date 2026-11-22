using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource runningSource;

    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip chaseMusic;

    [Header("SFX")]
    public AudioClip runningSFX;
    public AudioClip cameraSFX;
    public AudioClip screamSFX;
    public AudioClip collectedSFX;
    public AudioClip collectibleIdleSFX;

    [Header("Snail - Scream Variations")]
    public AudioClip[] audioClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    // looping bgm
    public void PlayMusic(AudioClip musicClip)
    {
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip sfxClip)
    {
        sfxSource.PlayOneShot(sfxClip);
    }

    // SNAIL SCREAM VARIATIONS
    public void PlayRandomScreams()
    {
        if (audioClips == null || audioClips.Length == 0) return;

        AudioClip clip = audioClips[Random.Range(0, audioClips.Length)]; // use a random scream audio clip
        sfxSource.PlayOneShot(clip);
    }

    // player running
    public void PlayPlayerRunning(AudioClip clip)
    {
        if (runningSource.isPlaying) return;
        runningSource.clip = clip;
        runningSource.loop = true;
        runningSource.Play();
    }

    public void StopPlayerRunning()
    {
        if (runningSource.isPlaying)
        {
            runningSource.Stop();
        }
    }

}
