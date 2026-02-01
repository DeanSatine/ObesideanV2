using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float directionChangeInterval = 3f;
    [SerializeField] private float moveRadius = 10f;

    [Header("VFX")]
    [SerializeField] private ParticleSystem deathVFX;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float bobOffset;
    private float directionTimer;
    private bool isDead = false;
    private Rigidbody rb;
    private Collider col;
    private MeshRenderer meshRenderer;
    private Material materialInstance;

    private void Awake()
    {
        startPosition = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        directionTimer = Random.Range(0f, directionChangeInterval);
        
        ApplyRandomColor();
        ChooseNewTarget();
    }

    private void ApplyRandomColor()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            materialInstance = new Material(meshRenderer.sharedMaterial);
            materialInstance.color = new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                1f
            );
            meshRenderer.material = materialInstance;
        }
    }

    private void Update()
    {
        if (isDead) return;

        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            directionTimer = directionChangeInterval;
            ChooseNewTarget();
        }

        Vector3 horizontalTarget = Vector3.MoveTowards(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(targetPosition.x, 0, targetPosition.z),
            moveSpeed * Time.deltaTime
        );

        float bobAmount = Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;
        
        transform.position = new Vector3(
            horizontalTarget.x,
            startPosition.y + bobAmount,
            horizontalTarget.z
        );

        if (horizontalTarget != new Vector3(transform.position.x, 0, transform.position.z))
        {
            Vector3 lookDirection = targetPosition - transform.position;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(lookDirection),
                    5f * Time.deltaTime
                );
            }
        }
    }

    private void ChooseNewTarget()
    {
        Vector2 randomCircle = Random.insideUnitCircle * moveRadius;
        targetPosition = startPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    public void Die(Vector3 force)
    {
        if (isDead) return;

        isDead = true;

        if (deathVFX != null)
        {
            deathVFX.transform.position = transform.position;
            deathVFX.transform.SetParent(null);
            deathVFX.Play();
            Destroy(deathVFX.gameObject, deathVFX.main.duration);
        }

        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = 50f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 100f, ForceMode.Impulse);

        col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, 5f);
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector3 direction = (transform.position - other.transform.position).normalized;
            direction.y = 1f;
            Die(direction * 500f);
        }
    }
}
