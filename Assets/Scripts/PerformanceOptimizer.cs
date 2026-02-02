using UnityEngine;

public class PerformanceOptimizer : MonoBehaviour
{
    [Header("Object Pooling")]
    [SerializeField] private bool enableObjectPooling = true;
    [SerializeField] private int initialPoolSize = 20;

    [Header("LOD Settings")]
    [SerializeField] private bool enableDynamicLOD = true;
    [SerializeField] private float lodDistance = 50f;

    [Header("Performance Monitoring")]
    [SerializeField] private bool showFPS = true;
    [SerializeField] private KeyCode toggleFPSKey = KeyCode.F3;

    private float deltaTime;
    private bool showFPSCounter;
    private GUIStyle style;

    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
        
        style = new GUIStyle();
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 24;
        style.normal.textColor = Color.white;
        
        showFPSCounter = showFPS;
    }

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (Input.GetKeyDown(toggleFPSKey))
        {
            showFPSCounter = !showFPSCounter;
        }
    }

    private void OnGUI()
    {
        if (!showFPSCounter) return;

        float fps = 1.0f / deltaTime;
        float ms = deltaTime * 1000.0f;
        
        Color textColor = fps >= 55 ? Color.green : fps >= 30 ? Color.yellow : Color.red;
        style.normal.textColor = textColor;

        string text = $"FPS: {fps:0.} ({ms:0.0} ms)";
        GUI.Label(new Rect(10, 10, 200, 30), text, style);
    }

    public static void OptimizeGameObject(GameObject obj)
    {
        if (obj == null) return;

        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }
}
