using UnityEngine;
using UnityEditor;

public class DestructibleBuildingSetup : EditorWindow
{
    private Vector3 minBounds = new Vector3(-23.61f, -10f, -56.69f);
    private Vector3 maxBounds = new Vector3(0.97f, 50f, 0.6f);
    private int addedCount = 0;
    private int skippedCount = 0;
    private int removedCount = 0;

    [MenuItem("Tools/Setup Destructible Buildings")]
    public static void ShowWindow()
    {
        GetWindow<DestructibleBuildingSetup>("Destructible Buildings");
    }

    private void OnGUI()
    {
        GUILayout.Label("Destructible Building Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("This will add DestructibleBuilding components to all buildings within the specified bounds.", MessageType.Info);
        
        GUILayout.Space(10);
        GUILayout.Label("Playable Area Bounds:", EditorStyles.boldLabel);
        minBounds = EditorGUILayout.Vector3Field("Min Bounds", minBounds);
        maxBounds = EditorGUILayout.Vector3Field("Max Bounds", maxBounds);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Add DestructibleBuilding to Buildings in Bounds", GUILayout.Height(40)))
        {
            AddDestructibleBuildingsInBounds();
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Remove DestructibleBuilding from Buildings Outside Bounds", GUILayout.Height(40)))
        {
            RemoveDestructibleBuildingsOutsideBounds();
        }
        
        GUILayout.Space(10);
        GUILayout.Label($"Added: {addedCount} | Skipped (out of bounds): {skippedCount} | Removed: {removedCount}");
    }

    private void AddDestructibleBuildingsInBounds()
    {
        addedCount = 0;
        skippedCount = 0;

        GameObject parentObj = GameObject.Find("PQ_Remake_AKIHABARA");
        if (parentObj == null)
        {
            Debug.LogError("PQ_Remake_AKIHABARA not found in scene!");
            return;
        }

        Transform[] allTransforms = parentObj.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (t.name.Contains("005339_08932_-") && 
                t.GetComponent<MeshRenderer>() != null)
            {
                Vector3 worldPos = t.position;

                if (IsInsideBounds(worldPos))
                {
                    if (t.GetComponent<DestructibleBuilding>() == null)
                    {
                        if (t.GetComponent<Collider>() == null)
                        {
                            Undo.AddComponent<MeshCollider>(t.gameObject);
                        }
                        
                        DestructibleBuilding db = Undo.AddComponent<DestructibleBuilding>(t.gameObject);
                        
                        SerializedObject serializedObject = new SerializedObject(db);
                        serializedObject.FindProperty("buildingMass").floatValue = 500f;
                        serializedObject.FindProperty("pushForce").floatValue = 300f;
                        serializedObject.ApplyModifiedProperties();
                        
                        EditorUtility.SetDirty(t.gameObject);
                        addedCount++;
                    }
                }
                else
                {
                    skippedCount++;
                }
            }
        }

        Debug.Log($"Setup Complete! Added {addedCount} buildings, skipped {skippedCount} (out of bounds)");
    }

    private bool IsInsideBounds(Vector3 position)
    {
        return position.x >= minBounds.x && position.x <= maxBounds.x &&
               position.y >= minBounds.y && position.y <= maxBounds.y &&
               position.z >= minBounds.z && position.z <= maxBounds.z;
    }

    private void RemoveDestructibleBuildingsOutsideBounds()
    {
        removedCount = 0;

        GameObject parentObj = GameObject.Find("PQ_Remake_AKIHABARA");
        if (parentObj == null)
        {
            Debug.LogError("PQ_Remake_AKIHABARA not found in scene!");
            return;
        }

        DestructibleBuilding[] allBuildings = parentObj.GetComponentsInChildren<DestructibleBuilding>(true);

        foreach (DestructibleBuilding db in allBuildings)
        {
            Vector3 worldPos = db.transform.position;

            if (!IsInsideBounds(worldPos))
            {
                Undo.DestroyObjectImmediate(db);
                removedCount++;
            }
        }

        Debug.Log($"Cleanup Complete! Removed {removedCount} DestructibleBuilding components from outside bounds");
    }
}
