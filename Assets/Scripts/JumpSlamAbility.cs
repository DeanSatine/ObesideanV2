using System.Collections;
using UnityEngine;

public class JumpSlamAbility : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float jumpForwardDistance = 8f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float shockwaveRadius = 12f;
    [SerializeField] private float shockwaveForce = 2000f;
    [SerializeField] private float upwardForceMultiplier = 2.5f;
    [SerializeField] private float bounceForce = 3000f;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject landingVFXPrefab;
    [SerializeField] private GameObject shockwaveVFXPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;

    private bool shouldTriggerOnAnimation = true;

    public IEnumerator Execute(IAbilityUser user)
    {
        user.SetAbilityState(true);
        
        bool isBoss = user is BossController;
        
        if (audioEvents != null)
        {
            audioEvents.PlayJump(transform.position, isBoss);
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * jumpForwardDistance;
        float elapsed = 0f;

        Rigidbody rb = user.GetRigidbody();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.useGravity = false;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            Vector3 horizontalMovement = Vector3.Lerp(startPos, targetPos, t);
            Vector3 targetPosition = new Vector3(horizontalMovement.x, startPos.y + height, horizontalMovement.z);

            Vector3 velocity = (targetPosition - transform.position) / Time.fixedDeltaTime;
            rb.linearVelocity = velocity;

            yield return new WaitForFixedUpdate();
        }

        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (!shouldTriggerOnAnimation)
        {
            TriggerShockwave();
        }

        user.SetAbilityState(false);
    }

    public void TriggerShockwave()
    {
        bool isBoss = GetComponent<BossController>() != null;
        
        if (audioEvents != null)
        {
            audioEvents.PlayLand(transform.position, isBoss);
            audioEvents.PlayGroundSlam(transform.position);
            audioEvents.PlayShockwave(transform.position);
        }
        
        if (landingVFXPrefab != null)
        {
            GameObject vfx = Instantiate(landingVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        if (shockwaveVFXPrefab != null)
        {
            GameObject vfx = Instantiate(shockwaveVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, shockwaveRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                DestructibleBuilding building = hit.GetComponent<DestructibleBuilding>();
                if (building != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    direction.y = 0;
                    Vector3 bounceDirection = (direction * 0.3f + Vector3.up).normalized;
                    building.ApplyDamage(bounceDirection * bounceForce, hit.ClosestPoint(transform.position));
                }

                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null && building == null)
                {
                    Vector3 force = (hit.transform.position - transform.position).normalized;
                    force.y = upwardForceMultiplier;
                    hitRb.AddForce(force * shockwaveForce, ForceMode.Impulse);
                }

                NPCController npc = hit.GetComponent<NPCController>();
                if (npc != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    direction.y = upwardForceMultiplier;
                    npc.Die(direction * shockwaveForce);
                }
                
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(shockwaveForce * 0.01f);
                }
                
                BossController boss = hit.GetComponent<BossController>();
                if (boss != null && !boss.IsDead())
                {
                    boss.TakeDamage(shockwaveForce * 0.025f);
                }
            }
        }
    }
}
