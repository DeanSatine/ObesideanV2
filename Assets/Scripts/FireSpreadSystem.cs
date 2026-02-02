using System.Collections.Generic;
using UnityEngine;

public class FireSpreadSystem : MonoBehaviour
{
    [Header("Fire Settings")]
    [SerializeField] private GameObject fireVFXPrefab;
    [SerializeField] private float spreadInterval = 0.5f;
    [SerializeField] private float spreadRadius = 15f;
    [SerializeField] private int maxFirePoints = 150;
    [SerializeField] private float fireLifetime = 30f;
    [SerializeField] private float damagePerSecond = 50f;
    [SerializeField] private float damageRadius = 3f;
    [SerializeField] private int firesPerSpread = 3;
    
    [Header("Spread Area")]
    [SerializeField] private Vector3 spreadAreaCenter = Vector3.zero;
    [SerializeField] private Vector3 spreadAreaSize = new Vector3(200f, 0f, 200f);
    [SerializeField] private float groundCheckHeight = 50f;
    
    [Header("Initial Fire Points")]
    [SerializeField] private int initialFireCount = 3;
    [SerializeField] private bool autoStartOnAwake = true;

    private List<FirePoint> activeFirePoints = new List<FirePoint>();
    private float nextSpreadTime;
    private bool isActive;

    private class FirePoint
    {
        public GameObject fireInstance;
        public Vector3 position;
        public float spawnTime;
        public HashSet<Collider> damagedTargets = new HashSet<Collider>();
        public float lastDamageTime;
    }

    private void Start()
    {
        if (autoStartOnAwake)
        {
            StartFireSpread();
        }
    }

    private void Update()
    {
        if (!isActive) return;

        CleanupExpiredFires();
        DamageTargetsInFire();

        if (Time.time >= nextSpreadTime && activeFirePoints.Count < maxFirePoints)
        {
            SpreadFire();
            nextSpreadTime = Time.time + spreadInterval;
        }
    }

    public void StartFireSpread()
    {
        if (isActive) return;
        
        isActive = true;
        
        for (int i = 0; i < initialFireCount; i++)
        {
            Vector3 randomPos = GetRandomPositionInArea();
            SpawnFireAtPosition(randomPos);
        }
        
        nextSpreadTime = Time.time + spreadInterval;
    }

    public void StopFireSpread()
    {
        isActive = false;
        ClearAllFires();
    }

    private void SpreadFire()
    {
        if (activeFirePoints.Count == 0)
        {
            Vector3 randomPos = GetRandomPositionInArea();
            SpawnFireAtPosition(randomPos);
            return;
        }

        for (int i = 0; i < firesPerSpread; i++)
        {
            FirePoint sourcePoint = activeFirePoints[Random.Range(0, activeFirePoints.Count)];
            
            Vector3 randomOffset = new Vector3(
                Random.Range(-spreadRadius, spreadRadius),
                0f,
                Random.Range(-spreadRadius, spreadRadius)
            );
            
            Vector3 newPosition = sourcePoint.position + randomOffset;
            
            if (IsPositionInArea(newPosition) && !IsFireNearby(newPosition, 2f))
            {
                SpawnFireAtPosition(newPosition);
            }
        }
    }

    private void SpawnFireAtPosition(Vector3 position)
    {
        if (fireVFXPrefab == null) return;

        Vector3 groundPosition = GetGroundPosition(position);
        if (groundPosition == Vector3.zero) return;

        GameObject fireInstance = Instantiate(fireVFXPrefab, groundPosition, Quaternion.identity);
        
        FirePoint firePoint = new FirePoint
        {
            fireInstance = fireInstance,
            position = groundPosition,
            spawnTime = Time.time,
            lastDamageTime = Time.time
        };
        
        activeFirePoints.Add(firePoint);
    }

    private Vector3 GetGroundPosition(Vector3 position)
    {
        Vector3 rayStart = new Vector3(position.x, spreadAreaCenter.y + groundCheckHeight, position.z);
        
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundCheckHeight * 2f))
        {
            return hit.point;
        }
        
        return Vector3.zero;
    }

    private Vector3 GetRandomPositionInArea()
    {
        float halfWidth = spreadAreaSize.x / 2f;
        float halfDepth = spreadAreaSize.z / 2f;
        
        Vector3 randomPos = new Vector3(
            spreadAreaCenter.x + Random.Range(-halfWidth, halfWidth),
            spreadAreaCenter.y,
            spreadAreaCenter.z + Random.Range(-halfDepth, halfDepth)
        );
        
        return randomPos;
    }

    private bool IsPositionInArea(Vector3 position)
    {
        float halfWidth = spreadAreaSize.x / 2f;
        float halfDepth = spreadAreaSize.z / 2f;
        
        return position.x >= spreadAreaCenter.x - halfWidth &&
               position.x <= spreadAreaCenter.x + halfWidth &&
               position.z >= spreadAreaCenter.z - halfDepth &&
               position.z <= spreadAreaCenter.z + halfDepth;
    }

    private bool IsFireNearby(Vector3 position, float minDistance)
    {
        foreach (FirePoint firePoint in activeFirePoints)
        {
            float distance = Vector3.Distance(position, firePoint.position);
            if (distance < minDistance)
            {
                return true;
            }
        }
        return false;
    }

    private void CleanupExpiredFires()
    {
        for (int i = activeFirePoints.Count - 1; i >= 0; i--)
        {
            FirePoint firePoint = activeFirePoints[i];
            
            if (Time.time - firePoint.spawnTime >= fireLifetime)
            {
                if (firePoint.fireInstance != null)
                {
                    Destroy(firePoint.fireInstance);
                }
                activeFirePoints.RemoveAt(i);
            }
        }
    }

    private void DamageTargetsInFire()
    {
        foreach (FirePoint firePoint in activeFirePoints)
        {
            Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, damageRadius);
            
            foreach (Collider hitCollider in hitColliders)
            {
                PlayerHealth playerHealth = hitCollider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    if (Time.time - firePoint.lastDamageTime >= 1f)
                    {
                        playerHealth.TakeDamage(damagePerSecond);
                        firePoint.lastDamageTime = Time.time;
                    }
                }
            }
        }
    }

    private void ClearAllFires()
    {
        foreach (FirePoint firePoint in activeFirePoints)
        {
            if (firePoint.fireInstance != null)
            {
                Destroy(firePoint.fireInstance);
            }
        }
        activeFirePoints.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(spreadAreaCenter, spreadAreaSize);
        
        Gizmos.color = Color.yellow;
        foreach (FirePoint firePoint in activeFirePoints)
        {
            Gizmos.DrawWireSphere(firePoint.position, damageRadius);
        }
    }
}
