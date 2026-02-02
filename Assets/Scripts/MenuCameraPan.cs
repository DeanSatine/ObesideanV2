using UnityEngine;

public class MenuCameraPan : MonoBehaviour
{
    [Header("Movement Mode")]
    [SerializeField] private PanMode panMode = PanMode.CircularOrbit;
    
    [Header("Circular Orbit Settings")]
    [SerializeField] private Transform orbitCenter;
    [SerializeField] private float orbitRadius = 50f;
    [SerializeField] private float orbitSpeed = 5f;
    [SerializeField] private float orbitHeight = 20f;
    [SerializeField] private float heightVariation = 5f;
    [SerializeField] private float heightVariationSpeed = 0.5f;
    
    [Header("Waypoint Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float waypointSpeed = 2f;
    [SerializeField] private float waypointPauseTime = 2f;
    [SerializeField] private bool loopWaypoints = true;
    
    [Header("Linear Pan Settings")]
    [SerializeField] private Vector3 panDirection = Vector3.right;
    [SerializeField] private float panSpeed = 5f;
    [SerializeField] private float panDistance = 100f;
    [SerializeField] private bool reversePan = true;
    
    [Header("Look At Settings")]
    [SerializeField] private Transform lookAtTarget;
    [SerializeField] private bool smoothRotation = true;
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private Vector3 lookAtOffset = Vector3.zero;
    
    [Header("Camera Sway")]
    [SerializeField] private bool enableSway = true;
    [SerializeField] private float swayAmount = 2f;
    [SerializeField] private float swaySpeed = 1f;
    
    private float orbitAngle;
    private int currentWaypointIndex;
    private float waypointPauseTimer;
    private Vector3 startPosition;
    private float linearPanProgress;
    private bool panReversing;
    private Vector3 swayOffset;

    public enum PanMode
    {
        CircularOrbit,
        Waypoints,
        LinearPan
    }

    private void Start()
    {
        startPosition = transform.position;
        
        if (panMode == PanMode.CircularOrbit && orbitCenter == null)
        {
            GameObject centerObj = new GameObject("Orbit Center");
            orbitCenter = centerObj.transform;
            orbitCenter.position = startPosition - transform.forward * orbitRadius;
        }
        
        if (panMode == PanMode.Waypoints && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }

    private void Update()
    {
        switch (panMode)
        {
            case PanMode.CircularOrbit:
                UpdateCircularOrbit();
                break;
            case PanMode.Waypoints:
                UpdateWaypointMovement();
                break;
            case PanMode.LinearPan:
                UpdateLinearPan();
                break;
        }
        
        UpdateCameraRotation();
        
        if (enableSway)
        {
            UpdateCameraSway();
        }
    }

    private void UpdateCircularOrbit()
    {
        if (orbitCenter == null) return;
        
        orbitAngle += orbitSpeed * Time.deltaTime;
        
        float heightOffset = Mathf.Sin(Time.time * heightVariationSpeed) * heightVariation;
        
        float x = orbitCenter.position.x + Mathf.Cos(orbitAngle * Mathf.Deg2Rad) * orbitRadius;
        float z = orbitCenter.position.z + Mathf.Sin(orbitAngle * Mathf.Deg2Rad) * orbitRadius;
        float y = orbitCenter.position.y + orbitHeight + heightOffset;
        
        Vector3 targetPosition = new Vector3(x, y, z) + swayOffset;
        transform.position = targetPosition;
    }

    private void UpdateWaypointMovement()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        if (waypointPauseTimer > 0f)
        {
            waypointPauseTimer -= Time.deltaTime;
            return;
        }
        
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 targetPosition = targetWaypoint.position + swayOffset;
        
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, waypointSpeed * Time.deltaTime);
        
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            waypointPauseTimer = waypointPauseTime;
            currentWaypointIndex++;
            
            if (currentWaypointIndex >= waypoints.Length)
            {
                if (loopWaypoints)
                {
                    currentWaypointIndex = 0;
                }
                else
                {
                    currentWaypointIndex = waypoints.Length - 1;
                }
            }
        }
    }

    private void UpdateLinearPan()
    {
        if (!panReversing)
        {
            linearPanProgress += panSpeed * Time.deltaTime;
            if (linearPanProgress >= panDistance)
            {
                if (reversePan)
                {
                    panReversing = true;
                }
                else
                {
                    linearPanProgress = 0f;
                }
            }
        }
        else
        {
            linearPanProgress -= panSpeed * Time.deltaTime;
            if (linearPanProgress <= 0f)
            {
                panReversing = false;
            }
        }
        
        Vector3 targetPosition = startPosition + panDirection.normalized * linearPanProgress + swayOffset;
        transform.position = targetPosition;
    }

    private void UpdateCameraRotation()
    {
        if (lookAtTarget == null)
        {
            if (panMode == PanMode.CircularOrbit && orbitCenter != null)
            {
                Vector3 lookPosition = orbitCenter.position + lookAtOffset;
                
                if (smoothRotation)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookPosition - transform.position);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                else
                {
                    transform.LookAt(lookPosition);
                }
            }
        }
        else
        {
            Vector3 lookPosition = lookAtTarget.position + lookAtOffset;
            
            if (smoothRotation)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookPosition - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                transform.LookAt(lookPosition);
            }
        }
    }

    private void UpdateCameraSway()
    {
        float swayX = Mathf.PerlinNoise(Time.time * swaySpeed, 0f) * swayAmount - swayAmount / 2f;
        float swayY = Mathf.PerlinNoise(0f, Time.time * swaySpeed) * swayAmount - swayAmount / 2f;
        float swayZ = Mathf.PerlinNoise(Time.time * swaySpeed, Time.time * swaySpeed) * swayAmount - swayAmount / 2f;
        
        swayOffset = new Vector3(swayX, swayY, swayZ);
    }

    private void OnDrawGizmosSelected()
    {
        if (panMode == PanMode.CircularOrbit && orbitCenter != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(orbitCenter.position, 1f);
            
            Gizmos.color = Color.yellow;
            for (int i = 0; i < 32; i++)
            {
                float angle1 = (i / 32f) * 360f * Mathf.Deg2Rad;
                float angle2 = ((i + 1) / 32f) * 360f * Mathf.Deg2Rad;
                
                Vector3 point1 = new Vector3(
                    orbitCenter.position.x + Mathf.Cos(angle1) * orbitRadius,
                    orbitCenter.position.y + orbitHeight,
                    orbitCenter.position.z + Mathf.Sin(angle1) * orbitRadius
                );
                
                Vector3 point2 = new Vector3(
                    orbitCenter.position.x + Mathf.Cos(angle2) * orbitRadius,
                    orbitCenter.position.y + orbitHeight,
                    orbitCenter.position.z + Mathf.Sin(angle2) * orbitRadius
                );
                
                Gizmos.DrawLine(point1, point2);
            }
        }
        
        if (panMode == PanMode.Waypoints && waypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                
                Gizmos.DrawWireSphere(waypoints[i].position, 2f);
                
                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
                else if (loopWaypoints && waypoints[0] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[0].position);
                }
            }
        }
        
        if (panMode == PanMode.LinearPan)
        {
            Gizmos.color = Color.magenta;
            Vector3 startPos = Application.isPlaying ? startPosition : transform.position;
            Vector3 endPos = startPos + panDirection.normalized * panDistance;
            
            Gizmos.DrawLine(startPos, endPos);
            Gizmos.DrawWireSphere(startPos, 2f);
            Gizmos.DrawWireSphere(endPos, 2f);
        }
        
        if (lookAtTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(lookAtTarget.position + lookAtOffset, 1.5f);
            Gizmos.DrawLine(transform.position, lookAtTarget.position + lookAtOffset);
        }
    }
}
