using System.Collections;
using UnityEngine;

public class SubwayBeamAbility : MonoBehaviour
{
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private float beamDuration = 4f;
    [SerializeField] private float beamRange = 50f;
    [SerializeField] private float coneAngle = 90f;
    [SerializeField] private float damagePerSecond = 500f;
    [SerializeField] private float sweepSpeed = 30f;
    [SerializeField] private float explosionForce = 5000f;
    [SerializeField] private float explosionUpwardBias = 1.5f;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject leftBeamVFXPrefab;
    [SerializeField] private GameObject rightBeamVFXPrefab;
    [SerializeField] private GameObject leftEyeGlowVFXPrefab;
    [SerializeField] private GameObject rightEyeGlowVFXPrefab;
    [SerializeField] private GameObject hitImpactVFXPrefab;
    [SerializeField] private GameObject explosionVFXPrefab;

    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;

    private ParticleSystem leftBeamVFX;
    private ParticleSystem rightBeamVFX;
    private ParticleSystem leftEyeGlowVFX;
    private ParticleSystem rightEyeGlowVFX;
    private ParticleSystem hitImpactVFX;

    private GameObject leftBeamInstance;
    private GameObject rightBeamInstance;
    private GameObject leftGlowInstance;
    private GameObject rightGlowInstance;
    private GameObject hitImpactInstance;

    private ParticleSystem.ShapeModule leftBeamShape;
    private ParticleSystem.ShapeModule rightBeamShape;
    private bool isInitialized = false;

    private void InitializeVFX()
    {
        if (isInitialized) return;

        if (leftBeamVFXPrefab != null && leftEye != null)
        {
            leftBeamInstance = Instantiate(leftBeamVFXPrefab, leftEye.position, Quaternion.identity);
            leftBeamVFX = leftBeamInstance.GetComponentInChildren<ParticleSystem>();
            if (leftBeamVFX == null)
            {
                leftBeamVFX = leftBeamInstance.GetComponent<ParticleSystem>();
            }
            if (leftBeamVFX != null)
            {
                leftBeamShape = leftBeamVFX.shape;
                var main = leftBeamVFX.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                leftBeamVFX.Stop();
                Debug.Log("Left Beam VFX initialized");
            }
            else
            {
                Debug.LogWarning("Left Beam VFX prefab does not have a ParticleSystem component!");
            }
        }
        else
        {
            if (leftBeamVFXPrefab == null) Debug.LogWarning("Left Beam VFX Prefab not assigned!");
            if (leftEye == null) Debug.LogWarning("Left Eye transform not assigned!");
        }

        if (rightBeamVFXPrefab != null && rightEye != null)
        {
            rightBeamInstance = Instantiate(rightBeamVFXPrefab, rightEye.position, Quaternion.identity);
            rightBeamVFX = rightBeamInstance.GetComponentInChildren<ParticleSystem>();
            if (rightBeamVFX == null)
            {
                rightBeamVFX = rightBeamInstance.GetComponent<ParticleSystem>();
            }
            if (rightBeamVFX != null)
            {
                rightBeamShape = rightBeamVFX.shape;
                var main = rightBeamVFX.main;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                rightBeamVFX.Stop();
                Debug.Log("Right Beam VFX initialized");
            }
            else
            {
                Debug.LogWarning("Right Beam VFX prefab does not have a ParticleSystem component!");
            }
        }
        else
        {
            if (rightBeamVFXPrefab == null) Debug.LogWarning("Right Beam VFX Prefab not assigned!");
            if (rightEye == null) Debug.LogWarning("Right Eye transform not assigned!");
        }

        if (leftEyeGlowVFXPrefab != null && leftEye != null)
        {
            leftGlowInstance = Instantiate(leftEyeGlowVFXPrefab, leftEye.position, Quaternion.identity, leftEye);
            leftEyeGlowVFX = leftGlowInstance.GetComponentInChildren<ParticleSystem>();
            if (leftEyeGlowVFX == null)
            {
                leftEyeGlowVFX = leftGlowInstance.GetComponent<ParticleSystem>();
            }
            if (leftEyeGlowVFX != null) leftEyeGlowVFX.Stop();
        }

        if (rightEyeGlowVFXPrefab != null && rightEye != null)
        {
            rightGlowInstance = Instantiate(rightEyeGlowVFXPrefab, rightEye.position, Quaternion.identity, rightEye);
            rightEyeGlowVFX = rightGlowInstance.GetComponentInChildren<ParticleSystem>();
            if (rightEyeGlowVFX == null)
            {
                rightEyeGlowVFX = rightGlowInstance.GetComponent<ParticleSystem>();
            }
            if (rightEyeGlowVFX != null) rightEyeGlowVFX.Stop();
        }

        if (hitImpactVFXPrefab != null)
        {
            hitImpactInstance = Instantiate(hitImpactVFXPrefab);
            hitImpactVFX = hitImpactInstance.GetComponentInChildren<ParticleSystem>();
            if (hitImpactVFX == null)
            {
                hitImpactVFX = hitImpactInstance.GetComponent<ParticleSystem>();
            }
            if (hitImpactVFX != null)
            {
                hitImpactVFX.Stop();
                hitImpactInstance.SetActive(false);
            }
        }

        isInitialized = true;
    }

    private void OnDestroy()
    {
        CleanupVFX();
    }

    private void CleanupVFX()
    {
        if (leftBeamInstance != null) Destroy(leftBeamInstance);
        if (rightBeamInstance != null) Destroy(rightBeamInstance);
        if (leftGlowInstance != null) Destroy(leftGlowInstance);
        if (rightGlowInstance != null) Destroy(rightGlowInstance);
        if (hitImpactInstance != null) Destroy(hitImpactInstance);
    }

    public IEnumerator Execute(IAbilityUser user)
    {
        user.SetAbilityState(true);
        InitializeVFX();

        bool isBoss = user is BossController;

        if (audioEvents != null)
        {
            audioEvents.PlayLaserCharge(transform.position, isBoss);
        }

        yield return new WaitForSeconds(0.5f);

        if (audioEvents != null)
        {
            audioEvents.PlayLaserBeamStart(transform.position);
        }

        if (leftBeamVFX != null) leftBeamVFX.Play();
        if (rightBeamVFX != null) rightBeamVFX.Play();
        if (leftEyeGlowVFX != null) leftEyeGlowVFX.Play();
        if (rightEyeGlowVFX != null) rightEyeGlowVFX.Play();

        float elapsed = 0f;
        float sweepAngle = -coneAngle / 2f;
        float loopSoundInterval = 0.3f;
        float lastLoopTime = 0f;

        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            sweepAngle += sweepSpeed * Time.deltaTime;

            if (sweepAngle > coneAngle / 2f)
                sweepAngle = -coneAngle / 2f;

            Vector3 sweepDirection = Quaternion.Euler(0, sweepAngle, 0) * transform.forward;

            FireBeam(leftEye, sweepDirection, leftBeamVFX, leftBeamShape);
            FireBeam(rightEye, sweepDirection, rightBeamVFX, rightBeamShape);

            if (audioEvents != null && elapsed - lastLoopTime >= loopSoundInterval)
            {
                audioEvents.PlayLaserBeamLoop(transform.position);
                lastLoopTime = elapsed;
            }

            yield return null;
        }

        if (audioEvents != null)
        {
            audioEvents.PlayLaserBeamEnd(transform.position);
        }

        if (leftBeamVFX != null) leftBeamVFX.Stop();
        if (rightBeamVFX != null) rightBeamVFX.Stop();
        if (leftEyeGlowVFX != null) leftEyeGlowVFX.Stop();
        if (rightEyeGlowVFX != null) rightEyeGlowVFX.Stop();

        user.SetAbilityState(false);
    }

    private void FireBeam(Transform eye, Vector3 direction, ParticleSystem beamVFX, ParticleSystem.ShapeModule beamShape)
    {
        if (eye == null) return;

        RaycastHit hit;
        Vector3 endPoint = eye.position + direction * beamRange;
        float distance = beamRange;
        bool didHit = false;

        if (Physics.Raycast(eye.position, direction, out hit, beamRange))
        {
            endPoint = hit.point;
            distance = hit.distance;
            didHit = true;

            bool shouldExplode = false;

            DestructibleBuilding building = hit.collider.GetComponent<DestructibleBuilding>();
            if (building != null)
            {
                building.ApplyDamage(direction * damagePerSecond * Time.deltaTime, hit.point);
                shouldExplode = true;
            }

            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 explosionDirection = direction;
                explosionDirection.y += explosionUpwardBias;
                rb.AddForce(explosionDirection.normalized * explosionForce * Time.deltaTime, ForceMode.Force);
                shouldExplode = true;
            }

            NPCController npc = hit.collider.GetComponent<NPCController>();
            if (npc != null)
            {
                Vector3 explosionDirection = direction;
                explosionDirection.y += explosionUpwardBias;
                npc.Die(explosionDirection.normalized * explosionForce);
                shouldExplode = true;
            }

            PlayerHealth playerHealth = hit.collider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerSecond * Time.deltaTime * 0.01f);
                shouldExplode = true;
            }

            BossController boss = hit.collider.GetComponent<BossController>();
            if (boss != null && !boss.IsDead())
            {
                boss.TakeDamage(damagePerSecond * Time.deltaTime * 0.02f);
                shouldExplode = true;
            }

            if (shouldExplode)
            {
                if (audioEvents != null && Random.value < 0.05f)
                {
                    audioEvents.PlayLaserImpact(hit.point);
                }

                if (explosionVFXPrefab != null && Random.value < 0.1f)
                {
                    GameObject explosion = Instantiate(explosionVFXPrefab, hit.point, Quaternion.identity);
                    Destroy(explosion, 3f);
                }

                if (didHit && hitImpactVFX != null)
                {
                    if (!hitImpactInstance.activeSelf)
                    {
                        hitImpactInstance.SetActive(true);
                    }
                    hitImpactVFX.transform.position = hit.point;
                    hitImpactVFX.transform.rotation = Quaternion.LookRotation(hit.normal);
                    if (!hitImpactVFX.isPlaying)
                    {
                        hitImpactVFX.Play();
                    }
                }
            }
            else
            {
                if (hitImpactVFX != null && hitImpactVFX.isPlaying)
                {
                    hitImpactVFX.Stop();
                    hitImpactInstance.SetActive(false);
                }
            }

            if (beamVFX != null)
            {
                beamVFX.transform.position = eye.position;
                beamVFX.transform.rotation = Quaternion.LookRotation(direction);

                beamShape.scale = new Vector3(0.5f, 0.5f, distance);
            }
        }
    }
}
