using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class BossController : MonoBehaviour, IAbilityUser
{
    [Header("Boss Stats")]
    [SerializeField] private float maxHealth = 200f;
    [SerializeField] private float detectionRange = 50f;
    [SerializeField] private float attackRange = 30f;

    [Header("UI References")]
    [SerializeField] private HealthBarUI healthBarUI;
    
    [Header("Death Settings")]
    [SerializeField] private string danceAnimationTrigger = "Dance";
    [SerializeField] private float danceDeathDuration = 5f;
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private float explosionForce = 20f;
    
    [Header("Ragdoll Settings")]
    [SerializeField] private RagdollController ragdollController;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;

    [Header("AI Settings")]
    [SerializeField] private float moveSelectionCooldown = 5f;
    [SerializeField] private float repositionSpeed = 12f;
    
    [Header("Range-Based AI")]
    [SerializeField] private float closeRangeThreshold = 10f;
    [SerializeField] private float midRangeThreshold = 20f;
    [SerializeField] private float farRangeThreshold = 35f;
    [SerializeField] private float preferredAttackRange = 15f;
    
    [Header("Low HP Behavior")]
    [SerializeField] private float lowHPThreshold = 0.3f;
    [SerializeField] private float fleeDuration = 3f;
    [SerializeField] private float fleeSpeed = 20f;
    [SerializeField] private float fleeSpinSpeed = 720f;
    [SerializeField] private float fleeDistance = 30f;
    
    private bool isFleeing;
    private float fleeStartTime;
    private Vector3 fleeTargetPosition;

    [Header("Ability Cooldowns")]
    [SerializeField] private float dashSlashCooldown = 4f;
    [SerializeField] private float jumpSlamCooldown = 6f;
    [SerializeField] private float laserCooldown = 10f;
    [SerializeField] private float rollCooldown = 3f;
    [SerializeField] private float chompCooldown = 2f;

    [Header("Ability References")]
    [SerializeField] private DashSlashAbility dashSlash;
    [SerializeField] private JumpSlamAbility jumpSlam;
    [SerializeField] private SubwayBeamAbility subwayBeam;
    [SerializeField] private RollAbility roll;
    [SerializeField] private ChompAbility chomp;

    private Rigidbody rb;
    private Animator animator;
    private Transform playerTransform;
    private float currentHealth;
    private bool isPerformingAbility;
    private bool isDead;

    private float lastDashTime = -999f;
    private float lastJumpTime = -999f;
    private float lastLaserTime = -999f;
    private float lastRollTime = -999f;
    private float lastChompTime = -999f;
    private float nextMoveTime = -999f;

    private enum BossAbility
    {
        DashSlash,
        JumpSlam,
        Laser,
        SubwayBeam,
        Roll,
        Chomp
    }
    
    private BossAbility lastAbilityUsed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 3000f;
        rb.linearDamping = 1f;
        rb.angularDamping = 2f;

        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        dashSlash = GetComponent<DashSlashAbility>();
        jumpSlam = GetComponent<JumpSlamAbility>();
        subwayBeam = GetComponent<SubwayBeamAbility>();
        roll = GetComponent<RollAbility>();
        chomp = GetComponent<ChompAbility>();
        
        if (ragdollController == null)
        {
            ragdollController = GetComponent<RagdollController>();
        }
        
        UpdateHealthUI();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;
        
        if (ragdollController != null && ragdollController.IsRagdolling())
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > detectionRange) return;

        if (isFleeing)
        {
            HandleFleeBehavior();
            return;
        }

        if (!isPerformingAbility)
        {
            LookAtPlayer();

            float healthPercentage = currentHealth / maxHealth;
            if (healthPercentage <= lowHPThreshold && Time.time >= nextMoveTime)
            {
                StartFleeing();
                return;
            }

            if (ShouldRepositionCloser(distanceToPlayer))
            {
                MoveTowardsPlayer(distanceToPlayer);
            }
            else if (ShouldRepositionAway(distanceToPlayer))
            {
                MoveAwayFromPlayer();
            }

            if (Time.time >= nextMoveTime && IsInAttackRange(distanceToPlayer))
            {
                SelectAbilityBasedOnRange(distanceToPlayer);
            }
        }
    }

    private void LookAtPlayer()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0f;
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private void MoveTowardsPlayer(float currentDistance)
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        Vector3 targetPosition = transform.position + directionToPlayer * repositionSpeed * Time.deltaTime;
        rb.MovePosition(targetPosition);
    }

    private void MoveAwayFromPlayer()
    {
        Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
        directionAwayFromPlayer.y = 0f;

        Vector3 targetPosition = transform.position + directionAwayFromPlayer * (repositionSpeed * 0.7f) * Time.deltaTime;
        rb.MovePosition(targetPosition);
    }

    private bool ShouldRepositionCloser(float distance)
    {
        return distance > preferredAttackRange + 5f;
    }

    private bool ShouldRepositionAway(float distance)
    {
        return distance < closeRangeThreshold - 3f;
    }

    private bool IsInAttackRange(float distance)
    {
        return distance <= attackRange;
    }

    private void StartFleeing()
    {
        isFleeing = true;
        fleeStartTime = Time.time;
        
        Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
        directionAwayFromPlayer.y = 0f;
        fleeTargetPosition = transform.position + directionAwayFromPlayer * fleeDistance;
        
        nextMoveTime = Time.time + fleeDuration + 2f;
        
        if (audioEvents != null)
        {
            audioEvents.PlayDash(transform.position, true);
        }
    }

    private void HandleFleeBehavior()
    {
        if (Time.time >= fleeStartTime + fleeDuration)
        {
            isFleeing = false;
            return;
        }

        transform.Rotate(Vector3.up, fleeSpinSpeed * Time.deltaTime);

        Vector3 directionToFlee = (fleeTargetPosition - transform.position).normalized;
        directionToFlee.y = 0f;
        Vector3 targetPosition = transform.position + directionToFlee * fleeSpeed * Time.deltaTime;
        rb.MovePosition(targetPosition);
    }

    private void SelectAbilityBasedOnRange(float distance)
    {
        BossAbility selectedAbility;

        if (distance <= closeRangeThreshold)
        {
            int rand = Random.Range(0, 3);
            selectedAbility = rand switch
            {
                0 => BossAbility.DashSlash,
                1 => BossAbility.Chomp,
                _ => BossAbility.Roll
            };
        }
        else if (distance <= midRangeThreshold)
        {
            int rand = Random.Range(0, 3);
            selectedAbility = rand switch
            {
                0 => BossAbility.DashSlash,
                1 => BossAbility.JumpSlam,
                _ => BossAbility.SubwayBeam
            };
        }
        else
        {
            int rand = Random.Range(0, 2);
            selectedAbility = rand == 0 ? BossAbility.JumpSlam : BossAbility.SubwayBeam;
        }

        if (selectedAbility == lastAbilityUsed)
        {
            SelectAbilityBasedOnRange(distance);
            return;
        }

        lastAbilityUsed = selectedAbility;
        nextMoveTime = Time.time + moveSelectionCooldown;
        StartCoroutine(ExecuteAbility(selectedAbility));
    }

    private IEnumerator ExecuteAbility(BossAbility ability)
    {
        switch (ability)
        {
            case BossAbility.DashSlash:
                if (dashSlash != null)
                {
                    lastDashTime = Time.time;
                    animator.SetTrigger("DashSlash");
                    yield return dashSlash.Execute(this);
                }
                break;

            case BossAbility.JumpSlam:
                if (jumpSlam != null)
                {
                    lastJumpTime = Time.time;
                    animator.SetTrigger("JumpSlam");
                    yield return jumpSlam.Execute(this);
                }
                break;

            case BossAbility.Laser:
                if (subwayBeam != null)
                {
                    lastLaserTime = Time.time;
                    animator.SetTrigger("Laser");
                    yield return subwayBeam.Execute(this);
                }
                break;
            
            case BossAbility.SubwayBeam:
                if (subwayBeam != null)
                {
                    lastLaserTime = Time.time;
                    animator.SetTrigger("SubwayBeam");
                    yield return subwayBeam.Execute(this);
                }
                break;

            case BossAbility.Roll:
                if (roll != null)
                {
                    lastRollTime = Time.time;
                    animator.SetTrigger("Roll");
                    yield return roll.Execute(this);
                }
                break;

            case BossAbility.Chomp:
                if (chomp != null)
                {
                    lastChompTime = Time.time;
                    animator.SetTrigger("Chomp");
                    yield return chomp.Execute(this);
                }
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateHealthUI();
        
        if (audioEvents != null)
        {
            audioEvents.PlayBossHit(transform.position);
        }

        if (ragdollController != null && currentHealth > 0f)
        {
            ragdollController.TriggerRagdoll(damage);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth, maxHealth);
        }
    }

    private void Die()
    {
        isDead = true;
        isPerformingAbility = false;
        
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (animator != null && !string.IsNullOrEmpty(danceAnimationTrigger))
        {
            animator.SetTrigger(danceAnimationTrigger);
        }

        yield return new WaitForSeconds(danceDeathDuration);
        
        if (audioEvents != null)
        {
            audioEvents.PlayExplosion(transform.position);
        }

        if (explosionVFXPrefab != null)
        {
            GameObject explosion = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5f);
        }

        if (ragdollController != null)
        {
            ragdollController.EnablePermanentRagdoll(Vector3.up * explosionForce);
        }
        else if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(Vector3.up * explosionForce, ForceMode.VelocityChange);
        }

        Debug.Log("Boss has been defeated!");
        
        Destroy(gameObject, 10f);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public Rigidbody GetRigidbody() => rb;

    public Animator GetAnimator() => animator;

    public void SetAbilityState(bool active)
    {
        isPerformingAbility = active;
    }
}
