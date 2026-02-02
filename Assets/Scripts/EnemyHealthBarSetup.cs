using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarSetup : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 11.68f, 0);
    [SerializeField] private Vector2 healthBarSize = new Vector2(3f, 0.5f);
    [SerializeField] private Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color fillColor = Color.green;
    
    private WorldSpaceHealthBar worldHealthBar;
    private BossController bossController;

    private void Awake()
    {
        bossController = GetComponent<BossController>();
        if (bossController != null)
        {
            CreateHealthBar();
        }
    }

    private void CreateHealthBar()
    {
        GameObject healthBarObj = new GameObject("WorldHealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBarObj.transform.localPosition = Vector3.zero;
        healthBarObj.transform.localRotation = Quaternion.identity;
        healthBarObj.transform.localScale = Vector3.one;

        Canvas canvas = healthBarObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = healthBarObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform canvasRect = healthBarObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;

        GameObject backgroundObj = new GameObject("Background");
        backgroundObj.transform.SetParent(healthBarObj.transform);
        backgroundObj.transform.localPosition = Vector3.zero;
        backgroundObj.transform.localRotation = Quaternion.identity;
        backgroundObj.transform.localScale = Vector3.one;

        Image backgroundImage = backgroundObj.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(healthBarObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localRotation = Quaternion.identity;
        fillObj.transform.localScale = Vector3.one;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fillColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        worldHealthBar = healthBarObj.AddComponent<WorldSpaceHealthBar>();
        
        System.Reflection.FieldInfo fillImageField = typeof(WorldSpaceHealthBar).GetField("fillImage", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fillImageField != null)
        {
            fillImageField.SetValue(worldHealthBar, fillImage);
        }
        
        System.Reflection.FieldInfo followTargetField = typeof(WorldSpaceHealthBar).GetField("followTarget", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (followTargetField != null)
        {
            followTargetField.SetValue(worldHealthBar, transform);
        }
        
        System.Reflection.FieldInfo offsetField = typeof(WorldSpaceHealthBar).GetField("offset", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (offsetField != null)
        {
            offsetField.SetValue(worldHealthBar, healthBarOffset);
        }
        
        System.Reflection.FieldInfo faceCameraField = typeof(WorldSpaceHealthBar).GetField("faceCamera", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (faceCameraField != null)
        {
            faceCameraField.SetValue(worldHealthBar, true);
        }
        
        System.Reflection.FieldInfo fullHealthColorField = typeof(WorldSpaceHealthBar).GetField("fullHealthColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fullHealthColorField != null)
        {
            fullHealthColorField.SetValue(worldHealthBar, fillColor);
        }
        
        System.Reflection.FieldInfo lowHealthColorField = typeof(WorldSpaceHealthBar).GetField("lowHealthColor", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (lowHealthColorField != null)
        {
            lowHealthColorField.SetValue(worldHealthBar, new Color(1f, 0.5f, 0f));
        }
        
        AssignHealthBarToBoss();
    }

    private void AssignHealthBarToBoss()
    {
        if (bossController != null && worldHealthBar != null)
        {
            System.Type bossType = bossController.GetType();
            System.Reflection.FieldInfo worldHealthBarField = bossType.GetField("worldHealthBar", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (worldHealthBarField != null)
            {
                worldHealthBarField.SetValue(bossController, worldHealthBar);
                Debug.Log($"[EnemyHealthBarSetup] Successfully assigned WorldHealthBar to {gameObject.name}");
            }
            else
            {
                Debug.LogError($"[EnemyHealthBarSetup] Could not find worldHealthBar field on BossController for {gameObject.name}");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyHealthBarSetup] Cannot assign health bar on {gameObject.name} - bossController: {bossController != null}, worldHealthBar: {worldHealthBar != null}");
        }
    }
}
