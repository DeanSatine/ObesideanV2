using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    
    [Header("UI References")]
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private DamageFlashEffect damageFlash;
    
    [Header("Ragdoll Settings")]
    [SerializeField] private RagdollController ragdollController;
    
    [Header("Audio")]
    [SerializeField] private AudioEvents audioEvents;
    
    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        
        if (ragdollController == null)
        {
            ragdollController = GetComponent<RagdollController>();
        }
        
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);

        UpdateHealthUI();

        if (damageFlash != null)
        {
            damageFlash.Flash();
        }
        
        if (audioEvents != null)
        {
            audioEvents.PlayPlayerHit(transform.position);
        }
        
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakePlayerHit();
        }

        if (ragdollController != null && currentHealth > 0f)
        {
            ragdollController.TriggerRagdoll(damage);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateHealthUI();
    }
    
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        Debug.Log("Player health restored to full!");
    }

    private void UpdateHealthUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth, maxHealth);
        }
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public bool IsDead()
    {
        return currentHealth <= 0f;
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        
        if (ragdollController != null)
        {
            ragdollController.EnablePermanentRagdoll(Vector3.up * 5f);
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
}
