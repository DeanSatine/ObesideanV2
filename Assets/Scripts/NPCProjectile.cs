using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class NPCProjectile : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 10f;
    [SerializeField] private GameObject impactVFXPrefab;
    [SerializeField] private GameObject trailVFXPrefab;
    
    private Rigidbody rb;
    private bool hasHit = false;
    private GameObject trailInstance;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;
        
        if (trailVFXPrefab != null)
        {
            trailInstance = Instantiate(trailVFXPrefab, transform.position, Quaternion.identity, transform);
        }
        
        Destroy(gameObject, lifetime);
    }

    public void Launch(Vector3 direction, float speed)
    {
        if (rb != null)
        {
            rb.linearVelocity = direction.normalized * speed;
            transform.forward = direction.normalized;
        }
    }
    
    private void Update()
    {
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            transform.forward = rb.linearVelocity.normalized;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;
        
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                hasHit = true;
                CreateImpactVFX();
                Destroy(gameObject);
            }
        }
        else if (!other.isTrigger && !other.CompareTag("Boss") && other.gameObject.layer != LayerMask.NameToLayer("Ignore Raycast"))
        {
            hasHit = true;
            CreateImpactVFX();
            Destroy(gameObject);
        }
    }

    private void CreateImpactVFX()
    {
        if (impactVFXPrefab != null)
        {
            GameObject vfx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }
    
    private void OnDestroy()
    {
        if (trailInstance != null)
        {
            trailInstance.transform.SetParent(null);
            Destroy(trailInstance, 2f);
        }
    }
}
