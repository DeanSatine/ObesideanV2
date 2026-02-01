using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField] private GameObject npcPrefab;
    [SerializeField] private int npcCount = 50;
    [SerializeField] private float spawnRadius = 50f;
    [SerializeField] private Vector3 spawnCenter = Vector3.zero;
    [SerializeField] private float spawnHeight = 0.5f;

    [Header("Optimization")]
    [SerializeField] private float cullingDistance = 100f;
    [SerializeField] private int maxActiveNPCs = 30;

    private List<GameObject> allNPCs = new List<GameObject>();
    private Transform playerTransform;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        SpawnNPCs();
    }

    private void SpawnNPCs()
    {
        for (int i = 0; i < npcCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnCenter + new Vector3(randomCircle.x, spawnHeight, randomCircle.y);

            GameObject npc = Instantiate(npcPrefab, spawnPos, Quaternion.identity, transform);
            npc.name = $"NPC_{i}";
            allNPCs.Add(npc);
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        OptimizeNPCs();
    }

    private void OptimizeNPCs()
    {
        List<NPCDistance> npcDistances = new List<NPCDistance>();

        foreach (GameObject npc in allNPCs)
        {
            if (npc == null) continue;

            float distance = Vector3.Distance(playerTransform.position, npc.transform.position);
            npcDistances.Add(new NPCDistance { npc = npc, distance = distance });
        }

        npcDistances.Sort((a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < npcDistances.Count; i++)
        {
            GameObject npc = npcDistances[i].npc;
            float distance = npcDistances[i].distance;

            bool shouldBeActive = i < maxActiveNPCs && distance < cullingDistance;
            
            if (npc.activeSelf != shouldBeActive)
            {
                npc.SetActive(shouldBeActive);
            }
        }
    }

    private struct NPCDistance
    {
        public GameObject npc;
        public float distance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnCenter, cullingDistance);
    }
}
