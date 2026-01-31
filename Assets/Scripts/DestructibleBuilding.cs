using UnityEngine;

public class DestructibleBuilding : MonoBehaviour
{
    [SerializeField] private float health = 1000f;
    [SerializeField] private float destructionThreshold = 0f;
    [SerializeField] private bool breakIntoChunks = true;
    [SerializeField] private GameObject destroyedPrefab;

    private Rigidbody rb;
    private bool isDestroyed;
    private float currentHealth;

    private void Awake()
    {
        currentHealth = health;

        if (breakIntoChunks && rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 500f;
            rb.isKinematic = true;
        }
    }

    public void ApplyDamage(Vector3 force, Vector3 hitPoint)
    {
        if (isDestroyed) return;

        currentHealth -= force.magnitude;

        if (currentHealth <= destructionThreshold)
        {
            Destroy();
        }
        else if (rb != null)
        {
            if (rb.isKinematic && currentHealth < health * 0.5f)
            {
                rb.isKinematic = false;
                BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
                if (boxCollider == null)
                {
                    boxCollider = gameObject.AddComponent<BoxCollider>();
                }
            }

            if (!rb.isKinematic)
            {
                rb.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
            }
        }
    }

    private void Destroy()
    {
        isDestroyed = true;

        if (destroyedPrefab != null)
        {
            Instantiate(destroyedPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}
