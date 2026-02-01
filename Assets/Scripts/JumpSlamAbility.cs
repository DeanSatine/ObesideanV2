using System.Collections;
using UnityEngine;

public class JumpSlamAbility : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float jumpForwardDistance = 8f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float shockwaveRadius = 12f;
    [SerializeField] private float shockwaveForce = 2000f;
    [SerializeField] private float upwardForceMultiplier = 1.5f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem landingVFX;
    [SerializeField] private ParticleSystem shockwaveVFX;

    private bool shouldTriggerOnAnimation = true;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * jumpForwardDistance;
        float elapsed = 0f;

        Rigidbody rb = controller.GetRigidbody();
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

        controller.SetAbilityState(false);
    }

    public void TriggerShockwave()
    {
        if (landingVFX != null)
        {
            landingVFX.transform.position = transform.position;
            landingVFX.Play();
        }

        if (shockwaveVFX != null)
        {
            shockwaveVFX.transform.position = transform.position;
            shockwaveVFX.Play();
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
                    direction.y = upwardForceMultiplier;
                    building.ApplyDamage(direction * shockwaveForce, hit.ClosestPoint(transform.position));
                }

                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null)
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
            }
        }
    }
}
