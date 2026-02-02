using UnityEngine;
using UnityEngine.UI;

public class HitMarker : MonoBehaviour
{
    public static HitMarker Instance { get; private set; }

    [Header("Hit Marker")]
    [SerializeField] private Image hitMarkerImage;
    [SerializeField] private float displayDuration = 0.1f;
    [SerializeField] private Color normalHitColor = Color.white;
    [SerializeField] private Color criticalHitColor = Color.yellow;
    [SerializeField] private Color killColor = Color.red;

    private float hitMarkerTimer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (hitMarkerImage != null)
        {
            hitMarkerImage.enabled = false;
        }
    }

    private void Update()
    {
        if (hitMarkerTimer > 0)
        {
            hitMarkerTimer -= Time.deltaTime;
            
            if (hitMarkerImage != null)
            {
                Color color = hitMarkerImage.color;
                color.a = hitMarkerTimer / displayDuration;
                hitMarkerImage.color = color;
            }

            if (hitMarkerTimer <= 0 && hitMarkerImage != null)
            {
                hitMarkerImage.enabled = false;
            }
        }
    }

    public void ShowHit(bool isCritical = false)
    {
        if (hitMarkerImage == null) return;

        hitMarkerImage.color = isCritical ? criticalHitColor : normalHitColor;
        hitMarkerImage.enabled = true;
        hitMarkerTimer = displayDuration;
    }

    public void ShowKill()
    {
        if (hitMarkerImage == null) return;

        hitMarkerImage.color = killColor;
        hitMarkerImage.enabled = true;
        hitMarkerTimer = displayDuration * 2f;
    }
}
