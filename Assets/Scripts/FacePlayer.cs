using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private bool lockXAxis = false;
    [SerializeField] private bool lockYAxis = false;
    [SerializeField] private bool lockZAxis = true;
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    
    [Header("Target")]
    [SerializeField] private Transform customTarget;
    
    private Transform targetTransform;
    private Camera mainCamera;

    private void Start()
    {
        if (customTarget != null)
        {
            targetTransform = customTarget;
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetTransform = player.transform;
            }
            else
            {
                mainCamera = Camera.main;
            }
        }
    }

    private void Update()
    {
        if (targetTransform == null && mainCamera == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                targetTransform = player.transform;
            }
            else
            {
                mainCamera = Camera.main;
            }
            return;
        }

        Vector3 targetPosition = targetTransform != null ? targetTransform.position : mainCamera.transform.position;
        Vector3 directionToTarget = targetPosition - transform.position;
        
        if (lockXAxis) directionToTarget.x = 0;
        if (lockYAxis) directionToTarget.y = 0;
        if (lockZAxis) directionToTarget.z = 0;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            targetRotation *= Quaternion.Euler(rotationOffset);

            if (smoothRotation)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }
}
