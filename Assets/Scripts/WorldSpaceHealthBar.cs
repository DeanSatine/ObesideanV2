using UnityEngine;
using UnityEngine.UI;

public class WorldSpaceHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Text healthText;
    
    [Header("Settings")]
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;
    
    [Header("World Position")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, 0);
    [SerializeField] private bool faceCamera = true;

    private Camera mainCamera;
    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (smoothTransition && fillImage != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            fillImage.fillAmount = currentFillAmount;
        }
        
        if (followTarget != null)
        {
            transform.position = followTarget.position + offset;
            
            if (faceCamera && mainCamera != null)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
            }
        }
    }

    public void SetFollowTarget(Transform target)
    {
        followTarget = target;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (fillImage == null)
        {
            Debug.LogWarning($"[WorldSpaceHealthBar] fillImage is null on {gameObject.name}!");
            return;
        }

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;
        
        Debug.Log($"[WorldSpaceHealthBar] {gameObject.name}: UpdateHealth called - {currentHealth}/{maxHealth} = {healthPercentage}, target fill: {targetFillAmount}");

        if (!smoothTransition)
        {
            currentFillAmount = targetFillAmount;
            if (fillImage != null)
            {
                fillImage.fillAmount = currentFillAmount;
            }
        }

        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage / lowHealthThreshold);

        if (healthText != null && showHealthText)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
