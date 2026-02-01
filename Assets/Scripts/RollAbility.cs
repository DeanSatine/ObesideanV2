using System.Collections;
using UnityEngine;

public class RollAbility : MonoBehaviour
{
    [SerializeField] private float rollDistance = 12f;
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollDamage = 800f;
    [SerializeField] private float damageRadius = 5f;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject rollTrailVFXPrefab;
    [SerializeField] private GameObject impactVFXPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;

    private GameObject rollTrailInstance;
    private ParticleSystem rollTrailVFX;

    private bool isRolling;

    public IEnumerator Execute(IAbilityUser user)
    {
        user.SetAbilityState(true);
        isRolling = true;
        
        bool isBoss = user is BossController;
        
        if (audioEvents != null)
        {
            audioEvents.PlayRoll(transform.position, isBoss);
        }

        if (rollTrailVFXPrefab != null)
        {
            rollTrailInstance = Instantiate(rollTrailVFXPrefab, transform.position, Quaternion.identity, transform);
            rollTrailVFX = rollTrailInstance.GetComponent<ParticleSystem>();
            if (rollTrailVFX != null) rollTrailVFX.Play();
        }

        Vector3 rollDirection = transform.forward;
        float rollSpeed = rollDistance / rollDuration;

        float elapsed = 0f;
        Rigidbody rb = user.GetRigidbody();
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 desiredVelocity = rollDirection * rollSpeed;
            rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);

            transform.Rotate(Vector3.right, 720f * Time.deltaTime);

            if (Physics.Raycast(transform.position, rollDirection, out RaycastHit hit, 1f))
            {
                if (!hit.collider.isTrigger)
                {
                    break;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (rollTrailVFX != null)
        {
            rollTrailVFX.Stop();
        }

        if (rollTrailInstance != null)
        {
            Destroy(rollTrailInstance, 2f);
        }

        isRolling = false;
        user.SetAbilityState(false);
    }

    public void TriggerDamage()
    {
        if (!isRolling) return;

        if (impactVFXPrefab != null)
        {
            GameObject vfx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                DestructibleBuilding building = hit.GetComponent<DestructibleBuilding>();
                if (building != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    building.ApplyDamage(direction * rollDamage, hit.ClosestPoint(transform.position));
                }

                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null)
                {
                    Vector3 force = (hit.transform.position - transform.position).normalized * rollDamage;
                    hitRb.AddForce(force, ForceMode.Impulse);
                }

                NPCController npc = hit.GetComponent<NPCController>();
                if (npc != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    npc.Die(direction * rollDamage);
                }
                
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(rollDamage * 0.025f);
                }
                
                BossController boss = hit.GetComponent<BossController>();
                if (boss != null && !boss.IsDead())
                {
                    boss.TakeDamage(rollDamage * 0.05f);
                }
            }
        }
    }
}
