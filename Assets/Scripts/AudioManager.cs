using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private float musicVolume = 0.54f;
    
    [Header("NPC Ambience")]
    [SerializeField] private AudioClip npcScreamingClip;
    [SerializeField] private float npcAmbienceVolume = 0.4f;
    [SerializeField] private float npcScreamInterval = 5f;
    [SerializeField] private float npcScreamVariance = 2f;
    
    [Header("Attack VFX Sounds")]
    [SerializeField] private AudioClip rollAttackSound;
    [SerializeField] private AudioClip jumpAttackSound;
    [SerializeField] private AudioClip slashAttackSound;
    [SerializeField] private AudioClip chompAttackSound;
    [SerializeField] private float attackSoundVolume = 0.7f;
    
    [Header("Movement & Ability Sounds")]
    [SerializeField] private AudioClip footstepSound;
    [SerializeField] private AudioClip laserEyesSound;
    [SerializeField] private float footstepVolume = 0.5f;
    [SerializeField] private float laserEyesVolume = 0.7f;
    
    [Header("SFX Pool Settings")]
    [SerializeField] private int sfxPoolSize = 20;
    
    private AudioSource musicSource;
    private AudioSource npcAmbienceSource;
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    private float nextNPCScreamTime;

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
        ScheduleNextNPCScream();
    }

    private void InitializeAudioSources()
    {
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(transform);
        musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        
        GameObject npcAmbienceObj = new GameObject("NPCAmbienceSource");
        npcAmbienceObj.transform.SetParent(transform);
        npcAmbienceSource = npcAmbienceObj.AddComponent<AudioSource>();
        npcAmbienceSource.loop = false;
        npcAmbienceSource.playOnAwake = false;
        npcAmbienceSource.spatialBlend = 0f;
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
    
    private void ScheduleNextNPCScream()
    {
        nextNPCScreamTime = Time.time + npcScreamInterval + Random.Range(-npcScreamVariance, npcScreamVariance);
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
        
        if (Time.time >= nextNPCScreamTime)
        {
            PlayNPCScream();
            ScheduleNextNPCScream();
        }
    }
    
    private void PlayNPCScream()
    {
        if (npcScreamingClip != null && npcAmbienceSource != null)
        {
            npcAmbienceSource.clip = npcScreamingClip;
            npcAmbienceSource.volume = npcAmbienceVolume;
            npcAmbienceSource.pitch = Random.Range(0.9f, 1.1f);
            npcAmbienceSource.Play();
        }
    }
    
    public void PlayMusic(AudioClip clip, float volume = 0.54f)
    {
        if (musicSource == null || clip == null) return;
        
        musicSource.clip = clip;
        musicSource.volume = volume;
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
    
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Stop();
        }
    }
    
    public void PlayRollAttack(Vector3 position)
    {
        if (rollAttackSound != null)
        {
            PlaySFXAtPoint(rollAttackSound, position, attackSoundVolume, Random.Range(0.95f, 1.05f));
        }
    }
    
    public void PlayJumpAttack(Vector3 position)
    {
        if (jumpAttackSound != null)
        {
            PlaySFXAtPoint(jumpAttackSound, position, attackSoundVolume, Random.Range(0.95f, 1.05f));
        }
    }
    
    public void PlaySlashAttack(Vector3 position)
    {
        if (slashAttackSound != null)
        {
            PlaySFXAtPoint(slashAttackSound, position, attackSoundVolume, Random.Range(0.9f, 1.1f));
        }
    }
    
    public void PlayChompAttack(Vector3 position)
    {
        if (chompAttackSound != null)
        {
            PlaySFXAtPoint(chompAttackSound, position, attackSoundVolume, Random.Range(0.95f, 1.05f));
        }
    }
    
    public void PlayFootstep(Vector3 position)
    {
        if (footstepSound != null)
        {
            PlaySFXAtPoint(footstepSound, position, footstepVolume, Random.Range(0.95f, 1.05f));
        }
    }
    
    public void PlayLaserEyes(Vector3 position)
    {
        if (laserEyesSound != null)
        {
            PlaySFXAtPoint(laserEyesSound, position, laserEyesVolume, 1f);
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

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
    
    public void SetNPCAmbienceVolume(float volume)
    {
        npcAmbienceVolume = Mathf.Clamp01(volume);
    }
    
    public void SetAttackSoundVolume(float volume)
    {
        attackSoundVolume = Mathf.Clamp01(volume);
    }
}
