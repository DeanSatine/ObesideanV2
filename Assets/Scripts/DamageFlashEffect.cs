using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageFlashEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Image flashImage;
    [SerializeField] private Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private Coroutine currentFlash;

    private void Awake()
    {
        if (flashImage == null)
        {
            GameObject flashObj = new GameObject("DamageFlash");
            flashObj.transform.SetParent(transform, false);
            
            flashImage = flashObj.AddComponent<Image>();
            RectTransform rt = flashImage.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
            
            flashImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
            flashImage.raycastTarget = false;
        }
        else
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }
    }

    public void Flash()
    {
        if (currentFlash != null)
        {
            StopCoroutine(currentFlash);
        }
        currentFlash = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            float alpha = flashCurve.Evaluate(t) * flashColor.a;

            Color c = flashColor;
            c.a = alpha;
            flashImage.color = c;

            yield return null;
        }

        Color finalColor = flashColor;
        finalColor.a = 0f;
        flashImage.color = finalColor;

        currentFlash = null;
    }
}
