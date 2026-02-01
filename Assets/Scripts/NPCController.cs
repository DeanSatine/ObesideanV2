using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float directionChangeInterval = 3f;
    [SerializeField] private float moveRadius = 10f;
    [SerializeField] private float bounceForce = 300f;
    [SerializeField] private float bounceInterval = 2f;
    [SerializeField] private float randomSpinSpeed = 50f;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject deathVFXPrefab;

    [Header("Death Physics")]
    [SerializeField] private float bounciness = 0.9f;
    [SerializeField] private float spinTorqueMultiplier = 500f;
    [SerializeField] private float npcMass = 50f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float bobOffset;
    private float directionTimer;
    private float bounceTimer;
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
        bounceTimer = Random.Range(0f, bounceInterval);
        
        ApplyRandomColor();
        ChooseNewTarget();
        SetupPhysics();
    }

    private void SetupPhysics()
    {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = npcMass;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 0.3f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.None;

        PhysicsMaterial bouncyMaterial = new PhysicsMaterial("BouncyNPC");
        bouncyMaterial.bounciness = bounciness;
        bouncyMaterial.bounceCombine = PhysicsMaterialCombine.Maximum;
        bouncyMaterial.frictionCombine = PhysicsMaterialCombine.Minimum;
        bouncyMaterial.dynamicFriction = 0.1f;
        bouncyMaterial.staticFriction = 0.1f;

        col = GetComponent<Collider>();
        if (col != null)
        {
            col.material = bouncyMaterial;
            col.isTrigger = false;
        }
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

        bounceTimer -= Time.deltaTime;
        if (bounceTimer <= 0f)
        {
            bounceTimer = bounceInterval;
            DoRandomBounce();
        }

        Vector3 directionToTarget = targetPosition - transform.position;
        directionToTarget.y = 0;

        if (directionToTarget.magnitude > 0.5f)
        {
            Vector3 moveForce = directionToTarget.normalized * moveSpeed;
            rb.AddForce(moveForce, ForceMode.Force);
        }

        rb.AddTorque(Random.insideUnitSphere * randomSpinSpeed * Time.deltaTime, ForceMode.Force);

        if (rb.linearVelocity.magnitude > moveSpeed * 2f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed * 2f;
        }
    }

    private void DoRandomBounce()
    {
        Vector3 bounceDirection = new Vector3(
            Random.Range(-0.5f, 0.5f),
            1f,
            Random.Range(-0.5f, 0.5f)
        ).normalized;

        rb.AddForce(bounceDirection * bounceForce, ForceMode.Impulse);

        Vector3 randomTorque = Random.insideUnitSphere * randomSpinSpeed;
        rb.AddTorque(randomTorque, ForceMode.Impulse);
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

        if (deathVFXPrefab != null)
        {
            GameObject vfx = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        rb.linearDamping = 0.1f;
        rb.angularDamping = 0.1f;

        rb.AddForce(force, ForceMode.Impulse);
        
        Vector3 randomSpin = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        rb.AddTorque(randomSpin * spinTorqueMultiplier, ForceMode.Impulse);

        Destroy(gameObject, 8f);
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

    private void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector3 direction = (transform.position - collision.contacts[0].point).normalized;
            direction.y = 1f;
            Die(direction * 500f);
        }
    }
}
