using UnityEngine;

[CreateAssetMenu(fileName = "AudioEvents", menuName = "Audio/Audio Events", order = 1)]
public class AudioEvents : ScriptableObject
{
    [Header("Music & Ambience")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip ambienceClip;
    
    [Header("Player SFX")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip rollSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip landSound;
    [SerializeField] private AudioClip slashSound;
    [SerializeField] private AudioClip chompSound;
    
    [Header("Boss SFX")]
    [SerializeField] private AudioClip[] bossFootstepSounds;
    [SerializeField] private AudioClip bossDashSound;
    [SerializeField] private AudioClip bossRollSound;
    [SerializeField] private AudioClip bossJumpSound;
    [SerializeField] private AudioClip bossLandSound;
    [SerializeField] private AudioClip bossSlashSound;
    [SerializeField] private AudioClip bossLaserChargeSound;
    [SerializeField] private AudioClip bossLaserFireSound;
    
    [Header("Laser Beam SFX")]
    [SerializeField] private AudioClip laserBeamStartSound;
    [SerializeField] private AudioClip laserBeamLoopSound;
    [SerializeField] private AudioClip laserBeamEndSound;
    [SerializeField] private AudioClip laserImpactSound;
    
    [Header("Slam SFX")]
    [SerializeField] private AudioClip groundSlamImpactSound;
    [SerializeField] private AudioClip shockwaveSound;
    
    [Header("Damage SFX")]
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip bossHitSound;
    [SerializeField] private AudioClip explosionSound;

    [Header("Volume Settings")]
    [SerializeField] private float footstepVolume = 0.3f;
    [SerializeField] private float abilityVolume = 0.7f;
    [SerializeField] private float impactVolume = 0.8f;
    [SerializeField] private float laserVolume = 0.6f;
    
    public void PlayMusic()
    {
        if (musicClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicClip);
        }
    }
    
    public void PlayAmbience()
    {
    }

    public void PlayFootstep(Vector3 position, bool isBoss = false)
    {
        AudioClip[] clips = isBoss ? bossFootstepSounds : footstepSounds;
        if (clips == null || clips.Length == 0) return;
        
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        float pitch = Random.Range(0.9f, 1.1f);
        AudioManager.Instance?.PlaySFXAtPoint(clip, position, footstepVolume, pitch);
    }

    public void PlayDash(Vector3 position, bool isBoss = false)
    {
        AudioClip clip = isBoss ? bossDashSound : dashSound;
        if (clip == null) return;
        
        float pitch = Random.Range(0.95f, 1.05f);
        AudioManager.Instance?.PlaySFXAtPoint(clip, position, abilityVolume, pitch);
    }

    public void PlayRoll(Vector3 position, bool isBoss = false)
    {
        AudioClip clip = isBoss ? bossRollSound : rollSound;
        if (clip == null) return;
        
        float pitch = Random.Range(0.95f, 1.05f);
        AudioManager.Instance?.PlaySFXAtPoint(clip, position, abilityVolume, pitch);
    }

    public void PlayJump(Vector3 position, bool isBoss = false)
    {
        AudioClip clip = isBoss ? bossJumpSound : jumpSound;
        if (clip == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(clip, position, abilityVolume);
    }

    public void PlayLand(Vector3 position, bool isBoss = false)
    {
        AudioClip clip = isBoss ? bossLandSound : landSound;
        if (clip == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(clip, position, abilityVolume);
    }

    public void PlaySlash(Vector3 position, bool isBoss = false)
    {
        AudioClip clip = isBoss ? bossSlashSound : slashSound;
        if (clip == null) return;
        
        float pitch = Random.Range(0.9f, 1.1f);
        AudioManager.Instance?.PlaySFXAtPoint(clip, position, abilityVolume, pitch);
    }

    public void PlayChomp(Vector3 position)
    {
        if (chompSound == null) return;
        
        float pitch = Random.Range(0.95f, 1.05f);
        AudioManager.Instance?.PlaySFXAtPoint(chompSound, position, abilityVolume, pitch);
    }

    public void PlayLaserCharge(Vector3 position, bool isBoss = false)
    {
        if (bossLaserChargeSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(bossLaserChargeSound, position, laserVolume);
    }

    public void PlayLaserBeamStart(Vector3 position)
    {
        if (laserBeamStartSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(laserBeamStartSound, position, laserVolume);
    }

    public void PlayLaserBeamLoop(Vector3 position)
    {
        if (laserBeamLoopSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(laserBeamLoopSound, position, laserVolume * 0.7f);
    }

    public void PlayLaserBeamEnd(Vector3 position)
    {
        if (laserBeamEndSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(laserBeamEndSound, position, laserVolume * 0.8f);
    }

    public void PlayLaserImpact(Vector3 position)
    {
        if (laserImpactSound == null) return;
        
        float pitch = Random.Range(0.95f, 1.05f);
        AudioManager.Instance?.PlaySFXAtPoint(laserImpactSound, position, impactVolume * 0.6f, pitch);
    }

    public void PlayGroundSlam(Vector3 position)
    {
        if (groundSlamImpactSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(groundSlamImpactSound, position, impactVolume);
    }

    public void PlayShockwave(Vector3 position)
    {
        if (shockwaveSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(shockwaveSound, position, impactVolume * 0.8f);
    }

    public void PlayPlayerHit(Vector3 position)
    {
        if (playerHitSound == null) return;
        
        float pitch = Random.Range(0.9f, 1.1f);
        AudioManager.Instance?.PlaySFXAtPoint(playerHitSound, position, abilityVolume, pitch);
    }

    public void PlayBossHit(Vector3 position)
    {
        if (bossHitSound == null) return;
        
        float pitch = Random.Range(0.9f, 1.1f);
        AudioManager.Instance?.PlaySFXAtPoint(bossHitSound, position, abilityVolume, pitch);
    }

    public void PlayExplosion(Vector3 position)
    {
        if (explosionSound == null) return;
        
        AudioManager.Instance?.PlaySFXAtPoint(explosionSound, position, impactVolume);
    }
}
