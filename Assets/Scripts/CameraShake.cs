using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }
    
    [Header("Footstep Shake")]
    [SerializeField] private float footstepShakeDuration = 0.1f;
    [SerializeField] private float footstepShakeAmount = 0.02f;
    
    [Header("Combat Shake - Player")]
    [SerializeField] private float playerHitDuration = 0.15f;
    [SerializeField] private float playerHitAmount = 0.3f;
    [SerializeField] private float playerDealDamageDuration = 0.1f;
    [SerializeField] private float playerDealDamageAmount = 0.15f;
    
    [Header("Combat Shake - Boss")]
    [SerializeField] private float bossHitDuration = 0.2f;
    [SerializeField] private float bossHitAmount = 0.4f;
    [SerializeField] private float bossAbilityDuration = 0.25f;
    [SerializeField] private float bossAbilityAmount = 0.35f;
    [SerializeField] private float bossDeathDuration = 0.8f;
    [SerializeField] private float bossDeathAmount = 0.6f;
    
    [Header("Combat Shake - Abilities")]
    [SerializeField] private float dashSlashDuration = 0.2f;
    [SerializeField] private float dashSlashAmount = 0.25f;
    [SerializeField] private float jumpSlamDuration = 0.4f;
    [SerializeField] private float jumpSlamAmount = 0.5f;
    [SerializeField] private float beamDuration = 0.15f;
    [SerializeField] private float beamAmount = 0.2f;
    [SerializeField] private float rollDuration = 0.25f;
    [SerializeField] private float rollAmount = 0.3f;
    [SerializeField] private float chompDuration = 0.15f;
    [SerializeField] private float chompAmount = 0.2f;
    
    [Header("Shake Behavior")]
    [SerializeField] private float shakeDecay = 2f;
    [SerializeField] private bool useTrauma = true;
    
    private Vector3 originalPosition;
    private float shakeTimer;
    private float currentShakeAmount;
    private float trauma;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (useTrauma)
        {
            if (trauma > 0)
            {
                trauma = Mathf.Max(0f, trauma - shakeDecay * Time.deltaTime);
                float shake = trauma * trauma;
                transform.localPosition = originalPosition + Random.insideUnitSphere * shake;
            }
            else
            {
                transform.localPosition = originalPosition;
            }
        }
        else
        {
            if (shakeTimer > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * currentShakeAmount;
                shakeTimer -= Time.deltaTime;
            }
            else
            {
                transform.localPosition = originalPosition;
            }
        }
    }

    public void ShakeFootstep()
    {
        TriggerShake(footstepShakeAmount, footstepShakeDuration);
    }
    
    public void ShakePlayerHit()
    {
        TriggerShake(playerHitAmount, playerHitDuration);
    }
    
    public void ShakePlayerDealDamage()
    {
        TriggerShake(playerDealDamageAmount, playerDealDamageDuration);
    }
    
    public void ShakeBossHit()
    {
        TriggerShake(bossHitAmount, bossHitDuration);
    }
    
    public void ShakeBossAbility()
    {
        TriggerShake(bossAbilityAmount, bossAbilityDuration);
    }
    
    public void ShakeBossDeath()
    {
        TriggerShake(bossDeathAmount, bossDeathDuration);
    }
    
    public void ShakeDashSlash()
    {
        TriggerShake(dashSlashAmount, dashSlashDuration);
    }
    
    public void ShakeJumpSlam()
    {
        TriggerShake(jumpSlamAmount, jumpSlamDuration);
    }
    
    public void ShakeBeam()
    {
        TriggerShake(beamAmount, beamDuration);
    }
    
    public void ShakeRoll()
    {
        TriggerShake(rollAmount, rollDuration);
    }
    
    public void ShakeChomp()
    {
        TriggerShake(chompAmount, chompDuration);
    }

    public void TriggerShake(float amount, float duration)
    {
        if (useTrauma)
        {
            trauma = Mathf.Min(1f, trauma + amount);
        }
        else
        {
            currentShakeAmount = Mathf.Max(currentShakeAmount, amount);
            shakeTimer = Mathf.Max(shakeTimer, duration);
        }
    }
}
