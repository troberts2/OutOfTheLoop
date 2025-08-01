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

    public void PlaySound(AudioClip clip, bool randomPitch = false)
    {
        AudioSource src = GetAvailableSource();
        src.clip = clip;
        if(randomPitch)
        {
            src.pitch = Random.Range(0.9f, 1.1f);
        }
        else
        {
            src.pitch = 1f;
        }
        src.Play();
    }

    #endregion
}
