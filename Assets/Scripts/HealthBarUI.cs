using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
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

    private float targetFillAmount = 1f;
    private float currentFillAmount = 1f;

    private void Update()
    {
        if (smoothTransition && fillImage != null)
        {
            currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            fillImage.fillAmount = currentFillAmount;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (fillImage == null) return;

        float healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
        targetFillAmount = healthPercentage;

        if (!smoothTransition)
        {
            fillImage.fillAmount = targetFillAmount;
            currentFillAmount = targetFillAmount;
        }

        fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercentage / lowHealthThreshold);

        if (healthText != null && showHealthText)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }

    public void SetFillAmount(float amount)
    {
        targetFillAmount = Mathf.Clamp01(amount);
        
        if (!smoothTransition && fillImage != null)
        {
            fillImage.fillAmount = targetFillAmount;
            currentFillAmount = targetFillAmount;
        }
    }
}
