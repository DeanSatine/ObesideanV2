using System.Collections;
using UnityEngine;

public class JumpSlamAbility : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 10f;
    [SerializeField] private float jumpForwardDistance = 8f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private float shockwaveRadius = 12f;
    [SerializeField] private float shockwaveForce = 2000f;
    [SerializeField] private LayerMask damageLayer;

    public IEnumerator Execute(PlayerController controller)
    {
        controller.SetAbilityState(true);

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + transform.forward * jumpForwardDistance;
        float elapsed = 0f;

        Rigidbody rb = controller.GetRigidbody();

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
            pos.y = startPos.y + height;

            rb.MovePosition(pos);

            yield return new WaitForFixedUpdate();
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
                    direction.y = 0.5f;
                    building.ApplyDamage(direction * shockwaveForce, hit.ClosestPoint(transform.position));
                }

                Rigidbody hitRb = hit.GetComponent<Rigidbody>();
                if (hitRb != null)
                {
                    Vector3 force = (hit.transform.position - transform.position).normalized;
                    force.y = 0.5f;
                    hitRb.AddForce(force * shockwaveForce, ForceMode.Impulse);
                }
            }
        }

        controller.SetAbilityState(false);
    }
}
