using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("SFX Pool")]
    [SerializeField] private AudioSource audioSourcePrefab;
    [SerializeField] private int poolSize = 25;

    private List<AudioSource> audioSources;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] musicClips;
    private int currentIndex = 0;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CreatePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (musicClips.Length == 0 || musicSource == null)
        {
            Debug.LogWarning("No music clips or AudioSource assigned.");
            return;
        }

        PlayCurrentTrack();
    }

    void Update()
    {
        if (!musicSource.isPlaying)
        {
            PlayNextTrack();
        }
    }

    void PlayCurrentTrack()
    {
        musicSource.clip = musicClips[currentIndex];
        musicSource.Play();
    }

    void PlayNextTrack()
    {
        currentIndex = (currentIndex + 1) % musicClips.Length;
        PlayCurrentTrack();
    }

    #region SFX Audio Pool

    private void CreatePool()
    {
        audioSources = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = Instantiate(audioSourcePrefab, transform);
            src.playOnAwake = false;
            audioSources.Add(src);
        }
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource src in audioSources)
        {
            if (!src.isPlaying)
                return src;
        }

        // Fallback: reuse the first one (or expand pool if needed)
        return audioSources[0];
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        AudioSource src = GetAvailableSource();
        src.clip = clip;
        src.volume = volume;
        src.Play();
    }

    #endregion
}
