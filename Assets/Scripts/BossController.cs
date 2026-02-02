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
    [SerializeField] private WorldSpaceHealthBar worldHealthBar;
    
    [Header("Death Settings")]
    [SerializeField] private string danceAnimationTrigger = "Dance";
    [SerializeField] private float danceDeathDuration = 5f;
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private float explosionForce = 20f;
    [SerializeField] private float bounceUpForce = 15f;
    [SerializeField] private float bounceSpinTorque = 500f;
    [SerializeField] private float disappearDelay = 2f;
    
    [Header("Ragdoll Settings")]
    [SerializeField] private RagdollController ragdollController;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;

    [Header("AI Settings")]
    [SerializeField] private float moveSelectionCooldown = 0.2f;
    [SerializeField] private float repositionSpeed = 12f;
    
    [Header("Formation Settings")]
    [SerializeField] private float formationRadius = 15f;
    [SerializeField] private float formationChangeInterval = 3f;
    [SerializeField] private float circlingSpeed = 8f;
    [SerializeField] private bool useFormationMovement = true;
    
    [Header("Arena Boundaries")]
    [SerializeField] private Vector2 arenaMinBounds = new Vector2(-30f, -30f);
    [SerializeField] private Vector2 arenaMaxBounds = new Vector2(265f, 230f);
    [SerializeField] private float boundaryBuffer = 5f;
    
    [Header("Building Destruction")]
    [SerializeField] private float buildingDetectionRadius = 30f;
    [SerializeField] private float buildingRollChance = 0.3f;
    [SerializeField] private float minBuildingsToRoll = 2;
    [SerializeField] private LayerMask buildingLayer;
    
    private float formationAngle;
    private float nextFormationChangeTime;
    private bool isSeekingBuildings;
    
    [Header("Range-Based AI")]
    [SerializeField] private float closeRangeThreshold = 10f;
    [SerializeField] private float midRangeThreshold = 20f;
    [SerializeField] private float farRangeThreshold = 35f;
    [SerializeField] private float preferredAttackRange = 15f;
    
    [Header("Low HP Behavior")]
    [SerializeField] private float lowHPThreshold = 0.3f;
    [SerializeField] private float fleeDuration = 1f;
    [SerializeField] private float fleeSpeed = 20f;
    [SerializeField] private float fleeSpinSpeed = 720f;
    [SerializeField] private float fleeDistance = 30f;
    
    private bool isFleeing;
    private float fleeStartTime;
    private Vector3 fleeTargetPosition;

    [Header("Ability Cooldowns")]
    [SerializeField] private float dashSlashCooldown = 0.5f;
    [SerializeField] private float jumpSlamCooldown = 2f;
    [SerializeField] private float laserCooldown = 3f;
    [SerializeField] private float rollCooldown = 1f;
    [SerializeField] private float chompCooldown = 0.5f;

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
    private float nextMoveTime;
    private float lastTargetCheckTime;

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
        FindTarget();
        nextMoveTime = Time.time + 1f;
        
        GameObject[] allBosses = GameObject.FindGameObjectsWithTag("Boss");
        int bossIndex = System.Array.IndexOf(allBosses, gameObject);
        formationAngle = (bossIndex * 360f / allBosses.Length) * Mathf.Deg2Rad;
        nextFormationChangeTime = Time.time + Random.Range(0f, formationChangeInterval);
        
        Debug.Log($"Boss initialized. Attack range: {attackRange}, Reposition speed: {repositionSpeed}, Detection range: {detectionRange}");
    }
    
    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"Boss targeting player");
        }
        else
        {
            Debug.LogWarning("Boss could not find player!");
        }
    }

    private void Update()
    {
        if (isDead) return;
        
        if (Time.time - lastTargetCheckTime > 3f)
        {
            FindTarget();
            lastTargetCheckTime = Time.time;
        }
        
        if (playerTransform == null) return;
        
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
            
            if (Random.value < 0.01f && !isSeekingBuildings)
            {
                MoveTowardsBuildings();
            }

            if (ShouldRepositionCloser(distanceToPlayer))
            {
                if (useFormationMovement)
                {
                    MoveInFormation();
                }
                else
                {
                    MoveTowardsPlayer(distanceToPlayer);
                }
            }
            else if (ShouldRepositionAway(distanceToPlayer))
            {
                MoveAwayFromPlayer();
            }
            else if (useFormationMovement)
            {
                CircleAroundPlayer();
            }

            if (Time.time >= nextMoveTime && IsInAttackRange(distanceToPlayer))
            {
                Debug.Log($"Boss selecting ability. Distance: {distanceToPlayer:F1}m, Next move time ready: {Time.time >= nextMoveTime}");
                SelectAbilityBasedOnRange(distanceToPlayer);
            }
            else if (Time.time < nextMoveTime)
            {
                Debug.Log($"Boss waiting for cooldown. Time until next move: {nextMoveTime - Time.time:F1}s");
            }
        }
    }

    private void LookAtPlayer()
    {
        if (playerTransform == null) return;
        
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0f;
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void MoveTowardsPlayer(float currentDistance)
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        directionToPlayer.y = 0f;

        Vector3 targetPosition = transform.position + directionToPlayer * repositionSpeed * Time.deltaTime;
        targetPosition = ClampToBounds(targetPosition);
        rb.MovePosition(targetPosition);
        
        Debug.Log($"Boss moving towards target. Distance: {currentDistance:F1}m, Speed: {repositionSpeed}");
    }

    private void MoveAwayFromPlayer()
    {
        Vector3 directionAwayFromPlayer = (transform.position - playerTransform.position).normalized;
        directionAwayFromPlayer.y = 0f;

        Vector3 targetPosition = transform.position + directionAwayFromPlayer * (repositionSpeed * 0.7f) * Time.deltaTime;
        targetPosition = ClampToBounds(targetPosition);
        rb.MovePosition(targetPosition);
    }
    
    private Vector3 ClampToBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, arenaMinBounds.x + boundaryBuffer, arenaMaxBounds.x - boundaryBuffer);
        position.z = Mathf.Clamp(position.z, arenaMinBounds.y + boundaryBuffer, arenaMaxBounds.y - boundaryBuffer);
        return position;
    }
    
    private void MoveInFormation()
    {
        if (playerTransform == null) return;
        
        if (Time.time >= nextFormationChangeTime)
        {
            formationAngle += Random.Range(-45f, 45f) * Mathf.Deg2Rad;
            nextFormationChangeTime = Time.time + formationChangeInterval;
        }
        
        Vector3 offset = new Vector3(
            Mathf.Cos(formationAngle) * formationRadius,
            0f,
            Mathf.Sin(formationAngle) * formationRadius
        );
        
        Vector3 targetPosition = playerTransform.position + offset;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        
        Vector3 newPosition = transform.position + direction * repositionSpeed * Time.deltaTime;
        newPosition = ClampToBounds(newPosition);
        rb.MovePosition(newPosition);
    }
    
    private void CircleAroundPlayer()
    {
        if (playerTransform == null) return;
        
        formationAngle += circlingSpeed * Time.deltaTime * Mathf.Deg2Rad;
        
        Vector3 offset = new Vector3(
            Mathf.Cos(formationAngle) * formationRadius,
            0f,
            Mathf.Sin(formationAngle) * formationRadius
        );
        
        Vector3 targetPosition = playerTransform.position + offset;
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0f;
        
        Vector3 newPosition = transform.position + direction * (repositionSpeed * 0.5f) * Time.deltaTime;
        newPosition = ClampToBounds(newPosition);
        rb.MovePosition(newPosition);
    }
    
    private void MoveTowardsBuildings()
    {
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, buildingDetectionRadius);
        DestructibleBuilding closestBuilding = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider col in nearbyObjects)
        {
            DestructibleBuilding building = col.GetComponent<DestructibleBuilding>();
            if (building != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestBuilding = building;
                }
            }
        }
        
        if (closestBuilding != null)
        {
            Vector3 direction = (closestBuilding.transform.position - transform.position).normalized;
            direction.y = 0f;
            
            Vector3 newPosition = transform.position + direction * (repositionSpeed * 0.3f) * Time.deltaTime;
            rb.MovePosition(newPosition);
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
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
        fleeTargetPosition = ClampToBounds(fleeTargetPosition);
        
        nextMoveTime = Time.time + fleeDuration + 0.5f;
        
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
        targetPosition = ClampToBounds(targetPosition);
        rb.MovePosition(targetPosition);
    }

    private void SelectAbilityBasedOnRange(float distance)
    {
        if (ShouldRollThroughBuildings())
        {
            lastAbilityUsed = BossAbility.Roll;
            nextMoveTime = Time.time + moveSelectionCooldown;
            StartCoroutine(ExecuteAbility(BossAbility.Roll));
            return;
        }
        
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
    
    private bool ShouldRollThroughBuildings()
    {
        if (Time.time < lastRollTime + rollCooldown) return false;
        if (Random.value > buildingRollChance) return false;
        
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, buildingDetectionRadius);
        int buildingCount = 0;
        Vector3 averageBuildingPosition = Vector3.zero;
        
        foreach (Collider col in nearbyObjects)
        {
            DestructibleBuilding building = col.GetComponent<DestructibleBuilding>();
            if (building != null)
            {
                buildingCount++;
                averageBuildingPosition += col.transform.position;
            }
        }
        
        if (buildingCount >= minBuildingsToRoll)
        {
            averageBuildingPosition /= buildingCount;
            Vector3 directionToBuildings = (averageBuildingPosition - transform.position).normalized;
            directionToBuildings.y = 0f;
            
            if (directionToBuildings != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToBuildings);
                transform.rotation = targetRotation;
            }
            
            isSeekingBuildings = true;
            Debug.Log($"Boss targeting {buildingCount} buildings for destruction!");
            return true;
        }
        
        return false;
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
                    isSeekingBuildings = false;
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
        
        GameFeel.OnBossDamaged(transform.position, damage);
        
        if (audioEvents != null)
        {
            audioEvents.PlayBossHit(transform.position);
        }
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeBossHit();
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
        
        if (worldHealthBar != null)
        {
            worldHealthBar.UpdateHealth(currentHealth, maxHealth);
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
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            Vector3 randomSpin = new Vector3(
                Random.Range(-bounceSpinTorque, bounceSpinTorque),
                Random.Range(-bounceSpinTorque, bounceSpinTorque),
                Random.Range(-bounceSpinTorque, bounceSpinTorque)
            );
            
            rb.AddForce(Vector3.up * bounceUpForce, ForceMode.VelocityChange);
            rb.AddTorque(randomSpin);
        }

        GameFeel.OnBossKilled(transform.position);
        
        if (audioEvents != null)
        {
            audioEvents.PlayExplosion(transform.position);
        }

        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeBossDeath();
        }

        yield return new WaitForSeconds(disappearDelay);

        if (explosionVFXPrefab != null)
        {
            GameObject explosion = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 5f);
        }

        Debug.Log("Boss has been defeated!");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBossDefeated();
        }
        
        Destroy(gameObject, 0.5f);
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
    
    public AudioEvents GetAudioEvents() => audioEvents;

    public void SetAbilityState(bool active)
    {
        isPerformingAbility = active;
    }
}
