using System.Collections;
using UnityEngine;

public class ChompAbility : MonoBehaviour
{
    [SerializeField] private float chompRadius = 5f;
    [SerializeField] private float damagePerChomp = 300f;
    [SerializeField] private float chompInterval = 0.5f;
    [SerializeField] private Transform mouthPosition;

    [Header("VFX Prefabs")]
    [SerializeField] private GameObject chompVFXPrefab;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;

    private float nextChompTime;

    public IEnumerator Execute(IAbilityUser user)
    {
        if (Time.time >= nextChompTime)
        {
            nextChompTime = Time.time + chompInterval;
        }
        
        yield return null;
    }

    public void OnChompAnimationEvent()
    {
        PerformChomp();
    }

    private void PerformChomp()
    {
        Vector3 chompCenter = mouthPosition != null ? mouthPosition.position : transform.position + transform.forward * 2f;
        
        if (audioEvents != null)
        {
            audioEvents.PlayChomp(chompCenter);
        }
        
        if (chompVFXPrefab != null)
        {
            GameObject vfx = Instantiate(chompVFXPrefab, chompCenter, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        Collider[] hits = Physics.OverlapSphere(chompCenter, chompRadius);
        foreach (Collider hit in hits)
        {
            if (hit.gameObject != gameObject)
            {
                DestructibleBuilding building = hit.GetComponent<DestructibleBuilding>();
                if (building != null)
                {
                    Vector3 direction = (hit.transform.position - chompCenter).normalized;
                    direction.y = 0f;
                    building.ApplyDamage(direction * damagePerChomp, hit.ClosestPoint(chompCenter));
                }

                Rigidbody rb = hit.GetComponent<Rigidbody>();
                if (rb != null && rb.gameObject != gameObject)
                {
                    Vector3 force = (hit.transform.position - chompCenter).normalized * damagePerChomp;
                    rb.AddForce(force, ForceMode.Impulse);
                }

                NPCController npc = hit.GetComponent<NPCController>();
                if (npc != null)
                {
                    Vector3 direction = (hit.transform.position - chompCenter).normalized;
                    npc.Die(direction * damagePerChomp);
                }
                
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damagePerChomp * 0.05f);
                }
                
                BossController boss = hit.GetComponent<BossController>();
                if (boss != null && !boss.IsDead())
                {
                    boss.TakeDamage(damagePerChomp * 0.1f);
                }
            }
        }
    }
}
