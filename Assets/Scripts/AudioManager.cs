using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSourcePrefab;
    
    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float musicVolume = 0.6f;
    
    [Header("Ambience")]
    [SerializeField] private AudioClip crowdScreamingClip;
    [SerializeField] private float ambienceVolume = 0.4f;
    
    [Header("SFX Pool Settings")]
    [SerializeField] private int sfxPoolSize = 10;
    
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeAudioSources();
        InitializeSFXPool();
    }

    private void Start()
    {
        PlayMusic();
        PlayAmbience();
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (ambienceSource == null)
        {
            GameObject ambienceObj = new GameObject("AmbienceSource");
            ambienceObj.transform.SetParent(transform);
            ambienceSource = ambienceObj.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.playOnAwake = false;
        }
        
        if (sfxSourcePrefab == null)
        {
            GameObject sfxPrefabObj = new GameObject("SFXSourcePrefab");
            sfxPrefabObj.transform.SetParent(transform);
            sfxSourcePrefab = sfxPrefabObj.AddComponent<AudioSource>();
            sfxSourcePrefab.playOnAwake = false;
            sfxSourcePrefab.loop = false;
        }
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sfxObj = new GameObject($"SFXSource_{i}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            sfxObj.SetActive(false);
            sfxPool.Enqueue(source);
        }
    }

    private void PlayMusic()
    {
        if (musicSource != null && musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    private void PlayAmbience()
    {
        if (ambienceSource != null && crowdScreamingClip != null)
        {
            ambienceSource.clip = crowdScreamingClip;
            ambienceSource.volume = ambienceVolume;
            ambienceSource.Play();
        }
    }
    
    public void PlayMusic(AudioClip clip, float volume = 0.6f)
    {
        if (musicSource == null || clip == null) return;
        
        musicSource.clip = clip;
        musicSource.volume = volume;
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
    
    public void PlayAmbience(AudioClip clip, float volume = 0.4f)
    {
        if (ambienceSource == null || clip == null) return;
        
        ambienceSource.clip = clip;
        ambienceSource.volume = volume;
        if (!ambienceSource.isPlaying)
        {
            ambienceSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
    
    public void StopAmbience()
    {
        if (ambienceSource != null && ambienceSource.isPlaying)
        {
            ambienceSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3? position = null)
    {
        if (clip == null) return;

        AudioSource source = GetAvailableSFXSource();
        if (source == null) return;

        source.gameObject.SetActive(true);
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        
        if (position.HasValue)
        {
            source.transform.position = position.Value;
            source.spatialBlend = 1f;
            source.minDistance = 5f;
            source.maxDistance = 50f;
        }
        else
        {
            source.spatialBlend = 0f;
        }

        source.Play();
        activeSfxSources.Add(source);
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlaySFX(clip, volume, pitch, position);
    }

    private AudioSource GetAvailableSFXSource()
    {
        if (sfxPool.Count > 0)
        {
            return sfxPool.Dequeue();
        }

        foreach (AudioSource source in activeSfxSources)
        {
            if (!source.isPlaying)
            {
                activeSfxSources.Remove(source);
                return source;
            }
        }

        GameObject sfxObj = new GameObject($"SFXSource_Extra");
        sfxObj.transform.SetParent(transform);
        AudioSource newSource = sfxObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        newSource.loop = false;
        return newSource;
    }

    private void Update()
    {
        for (int i = activeSfxSources.Count - 1; i >= 0; i--)
        {
            if (!activeSfxSources[i].isPlaying)
            {
                activeSfxSources[i].gameObject.SetActive(false);
                sfxPool.Enqueue(activeSfxSources[i]);
                activeSfxSources.RemoveAt(i);
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        if (ambienceSource != null)
            ambienceSource.volume = ambienceVolume;
    }
}
