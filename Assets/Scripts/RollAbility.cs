using System.Collections;
using UnityEngine;

public class RollAbility : MonoBehaviour
{
    [SerializeField] private float rollDistance = 12f;
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollDamage = 800f;
    [SerializeField] private float damageRadius = 5f;

    private bool isRolling;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);
        isRolling = true;

        Vector3 rollDirection = transform.forward;
        float rollSpeed = rollDistance / rollDuration;

        float elapsed = 0f;
        Rigidbody rb = controller.GetRigidbody();
        
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

        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        isRolling = false;
        controller.SetAbilityState(false);
    }

    public void TriggerDamage()
    {
        if (!isRolling) return;

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
            }
        }
    }
}
