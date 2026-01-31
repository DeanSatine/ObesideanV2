using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float knockdownForce = 500f;

    [Header("Ability Cooldowns")]
    [SerializeField] private float dashSlashCooldown = 3f;
    [SerializeField] private float jumpSlamCooldown = 5f;
    [SerializeField] private float laserEyesCooldown = 8f;
    [SerializeField] private float rollCooldown = 2f;

    [Header("Ability References")]
    [SerializeField] private DashSlashAbility dashSlash;
    [SerializeField] private JumpSlamAbility jumpSlam;
    [SerializeField] private LaserEyesAbility laserEyes;
    [SerializeField] private RollAbility roll;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isPerformingAbility;

    private float lastDashTime = -999f;
    private float lastJumpTime = -999f;
    private float lastLaserTime = -999f;
    private float lastRollTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 1000f;

        dashSlash = GetComponent<DashSlashAbility>();
        jumpSlam = GetComponent<JumpSlamAbility>();
        laserEyes = GetComponent<LaserEyesAbility>();
        roll = GetComponent<RollAbility>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnDashSlash(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastDashTime >= dashSlashCooldown && !isPerformingAbility && dashSlash != null)
        {
            lastDashTime = Time.time;
            StartCoroutine(dashSlash.Execute(this));
        }
    }

    public void OnJumpSlam(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastJumpTime >= jumpSlamCooldown && !isPerformingAbility && jumpSlam != null)
        {
            lastJumpTime = Time.time;
            StartCoroutine(jumpSlam.Execute(this));
        }
    }

    public void OnLaserEyes(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastLaserTime >= laserEyesCooldown && !isPerformingAbility && laserEyes != null)
        {
            lastLaserTime = Time.time;
            StartCoroutine(laserEyes.Execute(this));
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time - lastRollTime >= rollCooldown && !isPerformingAbility && roll != null)
        {
            lastRollTime = Time.time;
            StartCoroutine(roll.Execute(this));
        }
    }

    private void FixedUpdate()
    {
        if (!isPerformingAbility)
        {
            HandleMovement();
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);

        if (movement.magnitude > 0.1f)
        {
            rb.linearVelocity = new Vector3(
                movement.x * moveSpeed,
                rb.linearVelocity.y,
                movement.z * moveSpeed
            );

            Quaternion targetRotation = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
        }
        else
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        DestructibleBuilding building = collision.gameObject.GetComponent<DestructibleBuilding>();
        if (building != null && rb.linearVelocity.magnitude > 3f)
        {
            building.ApplyDamage(rb.linearVelocity.normalized * knockdownForce, collision.GetContact(0).point);
        }
    }

    public void SetAbilityState(bool performing)
    {
        isPerformingAbility = performing;
    }

    public Rigidbody GetRigidbody() => rb;
}
