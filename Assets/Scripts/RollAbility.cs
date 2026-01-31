using System.Collections;
using UnityEngine;

public class RollAbility : MonoBehaviour
{
    [SerializeField] private float rollDistance = 12f;
    [SerializeField] private float rollDuration = 0.5f;
    [SerializeField] private float rollDamage = 800f;
    [SerializeField] private float damageRadius = 5f;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        Vector3 startPos = transform.position;
        Vector3 rollDirection = transform.forward;
        Vector3 endPos = startPos + rollDirection * rollDistance;

        float elapsed = 0f;
        Rigidbody rb = controller.GetRigidbody();

        while (elapsed < rollDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / rollDuration;

            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);

            transform.Rotate(Vector3.right, 720f * Time.deltaTime);

            Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius);
            foreach (Collider hit in hits)
            {
                if (hit.gameObject != gameObject)
                {
                    DestructibleBuilding building = hit.GetComponent<DestructibleBuilding>();
                    if (building != null)
                    {
                        Vector3 direction = (hit.transform.position - transform.position).normalized;
                        building.ApplyDamage(direction * rollDamage * Time.deltaTime, hit.ClosestPoint(transform.position));
                    }

                    Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                    if (hitRb != null)
                    {
                        Vector3 force = (hit.transform.position - transform.position).normalized * rollDamage;
                        hitRb.AddForce(force * Time.deltaTime, ForceMode.Impulse);
                    }
                }
            }

            yield return new WaitForFixedUpdate();
        }

        controller.SetAbilityState(false);
    }
}
