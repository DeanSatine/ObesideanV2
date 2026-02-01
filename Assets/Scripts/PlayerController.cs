using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour, IAbilityUser
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 8f;
    [SerializeField] private float rotationSpeed = 720f;
    [SerializeField] private float knockdownForce = 500f;

    [Header("Camera Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private float cameraHeight = 8f;
    [SerializeField] private CameraShake cameraShake;

    [Header("Ability Cooldowns")]
    [SerializeField] private float dashSlashCooldown = 3f;
    [SerializeField] private float jumpSlamCooldown = 5f;
    [SerializeField] private float subwayBeamCooldown = 8f;
    [SerializeField] private float rollCooldown = 2f;
    [SerializeField] private float chompCooldown = 1f;
    [SerializeField] private float doubleJumpWindow = 0.3f;

    [Header("Ability References")]
    [SerializeField] private DashSlashAbility dashSlash;
    [SerializeField] private JumpSlamAbility jumpSlam;
    [SerializeField] private SubwayBeamAbility subwayBeam;
    [SerializeField] private RollAbility roll;
    [SerializeField] private ChompAbility chomp;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;
    [SerializeField] private float footstepInterval = 0.5f;

    private Rigidbody rb;
    private Animator animator;
    private RagdollController ragdollController;
    private Vector2 moveInput;
    private Vector2 mouseLookDelta;
    private Vector3 currentVelocity;
    private bool isPerformingAbility;

    private float lastDashTime = -999f;
    private float lastJumpTime = -999f;
    private float lastSubwayBeamTime = -999f;
    private float lastRollTime = -999f;
    private float lastChompTime = -999f;
    private float firstJumpPressTime = -999f;
    private float lastFootstepTime = -999f;
    private int jumpPressCount;

    private bool isLeftMousePressed;
    private bool isRightMousePressed;
    private float leftMouseHoldTime;

    private float cameraPitch = 0f;

    private InputAction moveAction;
    private InputAction leftClickAction;
    private InputAction rightClickAction;
    private InputAction jumpAction;
    private InputAction rollAction;
    private InputAction mouseLookAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.mass = 2000f;
        rb.linearDamping = 1f;
        rb.angularDamping = 2f;

        animator = GetComponent<Animator>();
        ragdollController = GetComponent<RagdollController>();

        dashSlash = GetComponent<DashSlashAbility>();
        jumpSlam = GetComponent<JumpSlamAbility>();
        subwayBeam = GetComponent<SubwayBeamAbility>();
        roll = GetComponent<RollAbility>();
        chomp = GetComponent<ChompAbility>();

        SetupInputActions();
    }

    private void SetupInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value, "<Keyboard>/w");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        
        mouseLookAction = new InputAction("MouseLook", InputActionType.Value);
        mouseLookAction.AddBinding("<Mouse>/delta");
        
        leftClickAction = new InputAction("LeftClick", InputActionType.Button, "<Mouse>/leftButton");
        rightClickAction = new InputAction("RightClick", InputActionType.Button, "<Mouse>/rightButton");
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        rollAction = new InputAction("Roll", InputActionType.Button, "<Keyboard>/leftShift");

        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        mouseLookAction.performed += ctx => mouseLookDelta = ctx.ReadValue<Vector2>();
        mouseLookAction.canceled += ctx => mouseLookDelta = Vector2.zero;

        leftClickAction.started += ctx => 
        {
            isLeftMousePressed = true;
            leftMouseHoldTime = 0f;
        };
        leftClickAction.canceled += ctx => 
        {
            isLeftMousePressed = false;
            if (leftMouseHoldTime < 0.5f && !isPerformingAbility)
            {
                TryDashSlash();
            }
            leftMouseHoldTime = 0f;
        };

        rightClickAction.started += ctx => isRightMousePressed = true;
        rightClickAction.canceled += ctx => isRightMousePressed = false;

        jumpAction.performed += ctx => 
        {
            if (Time.time - firstJumpPressTime <= doubleJumpWindow)
            {
                jumpPressCount++;
                if (jumpPressCount >= 2)
                {
                    TryJumpSlam();
                    jumpPressCount = 0;
                    firstJumpPressTime = -999f;
                }
            }
            else
            {
                jumpPressCount = 1;
                firstJumpPressTime = Time.time;
            }
        };

        rollAction.performed += ctx => 
        {
            if (Time.time - lastRollTime >= rollCooldown && !isPerformingAbility && roll != null)
            {
                lastRollTime = Time.time;
                if (animator != null)
                    animator.SetTrigger("Roll");
                StartCoroutine(roll.Execute(this));
            }
        };
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        mouseLookAction?.Enable();
        leftClickAction?.Enable();
        rightClickAction?.Enable();
        jumpAction?.Enable();
        rollAction?.Enable();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        mouseLookAction?.Disable();
        leftClickAction?.Disable();
        rightClickAction?.Disable();
        jumpAction?.Disable();
        rollAction?.Disable();
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void TryDashSlash()
    {
        if (Time.time - lastDashTime >= dashSlashCooldown && !isPerformingAbility && dashSlash != null)
        {
            lastDashTime = Time.time;
            if (animator != null)
                animator.SetTrigger("DashSlash");
            StartCoroutine(dashSlash.Execute(this));
        }
    }

    private void TryJumpSlam()
    {
        if (Time.time - lastJumpTime >= jumpSlamCooldown && !isPerformingAbility && jumpSlam != null)
        {
            lastJumpTime = Time.time;
            if (animator != null)
                animator.SetTrigger("JumpSlam");
            StartCoroutine(jumpSlam.Execute(this));
        }
    }

    private void TrySubwayBeam()
    {
        if (Time.time - lastSubwayBeamTime >= subwayBeamCooldown && !isPerformingAbility && subwayBeam != null)
        {
            lastSubwayBeamTime = Time.time;
            isLeftMousePressed = false;
            isRightMousePressed = false;
            if (animator != null)
                animator.SetTrigger("SubwayBeam");
            StartCoroutine(subwayBeam.Execute(this));
        }
    }

    private void TryChomp()
    {
        if (Time.time - lastChompTime >= chompCooldown && chomp != null)
        {
            lastChompTime = Time.time;
            if (animator != null)
                animator.SetBool("IsChomping", true);
            StartCoroutine(chomp.Execute(this));
        }
    }

    private void Update()
    {
        if (ragdollController != null && ragdollController.IsRagdolling())
            return;
        
        HandleMouseLook();
        
        if (isLeftMousePressed && isRightMousePressed)
        {
            TrySubwayBeam();
        }
        else if (isLeftMousePressed)
        {
            leftMouseHoldTime += Time.deltaTime;
            
            if (leftMouseHoldTime >= 0.5f)
            {
                TryChomp();
            }
        }
        else
        {
            if (animator != null)
                animator.SetBool("IsChomping", false);
        }
        
        if (animator != null)
            animator.SetFloat("Speed", moveInput.magnitude);
    }

    private void HandleMouseLook()
    {
        float mouseX = mouseLookDelta.x * mouseSensitivity;
        float mouseY = mouseLookDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -80f, 80f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (ragdollController != null && ragdollController.IsRagdolling())
            return;
        
        if (!isPerformingAbility)
        {
            KeepPlayerUpright();
            HandleMovement();
        }
    }

    private void KeepPlayerUpright()
    {
        Quaternion currentRotation = transform.rotation;
        Vector3 currentEuler = currentRotation.eulerAngles;
        
        Quaternion targetRotation = Quaternion.Euler(0f, currentEuler.y, 0f);
        
        transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, 10f * Time.fixedDeltaTime);
    }

    private void HandleMovement()
    {
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        Vector3 targetDirection = (forward * moveInput.y + right * moveInput.x).normalized;
        Vector3 targetVelocity = targetDirection * moveSpeed;

        if (targetDirection.magnitude > 0.1f)
        {
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                acceleration * Time.fixedDeltaTime
            );
            
            if (audioEvents != null && Time.time - lastFootstepTime >= footstepInterval)
            {
                audioEvents.PlayFootstep(transform.position, false);
                lastFootstepTime = Time.time;
            }
        }
        else
        {
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                Vector3.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = new Vector3(
            currentVelocity.x,
            rb.linearVelocity.y,
            currentVelocity.z
        );
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
        if (animator != null)
        {
            animator.SetBool("IsPerformingAbility", performing);
        }
    }

    public Rigidbody GetRigidbody() => rb;

    public void OnFootstep()
    {
        if (cameraShake != null)
        {
            cameraShake.ShakeFootstep();
        }
    }
    public Animator GetAnimator() => animator;
}
