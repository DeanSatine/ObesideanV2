using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIEffects : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.95f;
    [SerializeField] private float pulseMaxScale = 1.05f;
    [SerializeField] private AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Fade In Settings")]
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeInDelay = 0f;
    [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Glow Settings")]
    [SerializeField] private bool enableGlow = true;
    [SerializeField] private float glowSpeed = 3f;
    [SerializeField] private float glowMinIntensity = 0.5f;
    [SerializeField] private float glowMaxIntensity = 1f;
    [SerializeField] private Color glowColor = Color.white;

    private TextMeshProUGUI tmpText;
    private Image image;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private Color originalColor;
    private bool isInitialized = false;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null && (tmpText != null || image != null))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalScale = transform.localScale;

        if (tmpText != null)
        {
            originalColor = tmpText.color;
        }
        else if (image != null)
        {
            originalColor = image.color;
        }

        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;

        if (fadeInOnStart)
        {
            StartCoroutine(FadeIn());
        }

        if (enablePulse)
        {
            StartCoroutine(PulseEffect());
        }

        if (enableGlow && tmpText != null)
        {
            StartCoroutine(GlowEffect());
        }
    }

    private IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = 0f;

        yield return new WaitForSeconds(fadeInDelay);

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeInCurve.Evaluate(elapsed / fadeInDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator PulseEffect()
    {
        while (enablePulse)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float curvedT = pulseCurve.Evaluate(t);
            float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, curvedT);
            transform.localScale = originalScale * scale;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    private IEnumerator GlowEffect()
    {
        if (tmpText == null) yield break;

        while (enableGlow)
        {
            float t = Mathf.PingPong(Time.time * glowSpeed, 1f);
            float intensity = Mathf.Lerp(glowMinIntensity, glowMaxIntensity, t);

            Color targetColor = Color.Lerp(originalColor, glowColor, intensity - glowMinIntensity);
            targetColor.a = originalColor.a;
            tmpText.color = targetColor;

            yield return null;
        }

        tmpText.color = originalColor;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (isInitialized)
        {
            transform.localScale = originalScale;

            if (tmpText != null)
            {
                tmpText.color = originalColor;
            }
        }
    }
}
