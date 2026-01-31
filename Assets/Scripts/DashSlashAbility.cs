using System.Collections;
using UnityEngine;

public class DashSlashAbility : MonoBehaviour
{
    [SerializeField] private float dashDistance = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float slashRadius = 8f;
    [SerializeField] private float slashDamage = 1000f;
    [SerializeField] private LayerMask damageLayer;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        Vector3 startPos = transform.position;
        Vector3 dashDirection = transform.forward;
        Vector3 endPos = startPos + dashDirection * dashDistance;

        float elapsed = 0f;
        Rigidbody rb = controller.GetRigidbody();

        while (elapsed < dashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / dashDuration;

            Vector3 newPos = Vector3.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);

            yield return new WaitForFixedUpdate();
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
            }
        }

        controller.SetAbilityState(false);
    }
}
