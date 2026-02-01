using System.Collections;
using UnityEngine;

public class DashSlashAbility : MonoBehaviour
{
    [SerializeField] private float dashDistance = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float slashRadius = 8f;
    [SerializeField] private float slashDamage = 1000f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem dashTrailVFX;
    [SerializeField] private ParticleSystem slashVFX;

    private bool shouldTriggerOnAnimation = true;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        if (dashTrailVFX != null)
        {
            dashTrailVFX.Play();
        }

        Vector3 dashDirection = transform.forward;
        float dashSpeed = dashDistance / dashDuration;

        float elapsed = 0f;
        Rigidbody rb = controller.GetRigidbody();
        
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 desiredVelocity = dashDirection * dashSpeed;
            rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);

            if (Physics.Raycast(transform.position, dashDirection, out RaycastHit hit, 1f))
            {
                if (!hit.collider.isTrigger)
                {
                    break;
                }
            }

            yield return new WaitForFixedUpdate();
        }

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        if (dashTrailVFX != null)
        {
            dashTrailVFX.Stop();
        }

        if (!shouldTriggerOnAnimation)
        {
            TriggerDamage();
        }

        controller.SetAbilityState(false);
    }

    public void TriggerDamage()
    {
        if (slashVFX != null)
        {
            slashVFX.transform.position = transform.position;
            slashVFX.Play();
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, slashRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                DestructibleBuilding building = hit.GetComponent<DestructibleBuilding>();
                if (building != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    building.ApplyDamage(direction * slashDamage, hit.ClosestPoint(transform.position));
                }

                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null)
                {
                    Vector3 force = (hit.transform.position - transform.position).normalized * slashDamage;
                    hitRb.AddForce(force, ForceMode.Impulse);
                }

                NPCController npc = hit.GetComponent<NPCController>();
                if (npc != null)
                {
                    Vector3 direction = (hit.transform.position - transform.position).normalized;
                    npc.Die(direction * slashDamage);
                }
            }
        }
    }
}
