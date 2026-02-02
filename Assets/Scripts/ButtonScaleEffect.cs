using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonScaleEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float pressScale = 0.95f;
    [SerializeField] private float animationSpeed = 10f;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private float hoverRotation = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Quaternion targetRotation;
    private Quaternion originalRotation;
    private bool isHovering;
    private bool isPressed;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
        originalRotation = transform.localRotation;
        targetRotation = originalRotation;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        
        if (enableRotation)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateScale();
        
        if (enableRotation)
        {
            targetRotation = originalRotation * Quaternion.Euler(0, 0, hoverRotation);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        UpdateScale();
        
        if (enableRotation)
        {
            targetRotation = originalRotation;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        UpdateScale();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        UpdateScale();
    }

    private void UpdateScale()
    {
        if (isPressed)
        {
            targetScale = originalScale * pressScale;
        }
        else if (isHovering)
        {
            targetScale = originalScale * hoverScale;
        }
        else
        {
            targetScale = originalScale;
        }
    }
}
