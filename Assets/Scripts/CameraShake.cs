using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float footstepShakeDuration = 0.1f;
    [SerializeField] private float footstepShakeAmount = 0.02f;
    
    private Vector3 originalPosition;
    private float shakeTimer;
    private float currentShakeAmount;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            transform.localPosition = originalPosition + Random.insideUnitSphere * currentShakeAmount;
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }

    public void ShakeFootstep()
    {
        TriggerShake(footstepShakeAmount, footstepShakeDuration);
    }

    public void TriggerShake(float amount, float duration)
    {
        currentShakeAmount = amount;
        shakeTimer = duration;
    }
}
