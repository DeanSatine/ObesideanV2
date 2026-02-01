using UnityEngine;

public class DestructibleBuilding : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float buildingMass = 500f;
    [SerializeField] private float pushForce = 300f;
    
    private bool isKnockedDown = false;
    private Rigidbody rb;

    private void OnCollisionEnter(Collision collision)
    {
        if (isKnockedDown) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            KnockDownBuilding(collision);
        }
    }

    private void KnockDownBuilding(Collision collision)
    {
        isKnockedDown = true;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = buildingMass;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Vector3 pushDirection = (transform.position - collision.contacts[0].point).normalized;
        pushDirection.y = 0.3f;

        rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
        
        Vector3 randomTorque = new Vector3(
            Random.Range(-pushForce * 0.5f, pushForce * 0.5f),
            Random.Range(-pushForce * 0.3f, pushForce * 0.3f),
            Random.Range(-pushForce * 0.5f, pushForce * 0.5f)
        );
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    public void ApplyDamage(Vector3 force, Vector3 hitPoint)
    {
        if (!isKnockedDown)
        {
            isKnockedDown = true;

            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = buildingMass;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * pushForce, ForceMode.Impulse);
    }
}
