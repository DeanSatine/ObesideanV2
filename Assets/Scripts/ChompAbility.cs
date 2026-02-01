using System.Collections;
using UnityEngine;

public class ChompAbility : MonoBehaviour
{
    [SerializeField] private float chompRadius = 5f;
    [SerializeField] private float damagePerChomp = 300f;
    [SerializeField] private float chompInterval = 0.5f;
    [SerializeField] private Transform mouthPosition;

    private float nextChompTime;

    public IEnumerator Execute(PlayerController controller)
    {
        if (Time.time >= nextChompTime)
        {
            nextChompTime = Time.time + chompInterval;
        }
        
        yield return null;
    }

    public void OnChompAnimationEvent()
    {
        PerformChomp();
    }

    private void PerformChomp()
    {
        Vector3 chompCenter = mouthPosition != null ? mouthPosition.position : transform.position + transform.forward * 2f;
        
        Collider[] hits = Physics.OverlapSphere(chompCenter, chompRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                DestructibleBuilding building = hit.GetComponent<DestructibleBuilding>();
                if (building != null)
                {
                    Vector3 direction = (hit.transform.position - chompCenter).normalized;
                    direction.y = 0f;
                    building.ApplyDamage(direction * damagePerChomp, hit.ClosestPoint(chompCenter));
                }

                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null && rb.gameObject != gameObject)
                {
                    Vector3 force = (hit.transform.position - chompCenter).normalized * damagePerChomp;
                    rb.AddForce(force, ForceMode.Impulse);
                }
            }
        }
    }
}
