using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class VictoryCelebration : MonoBehaviour
{
    public static VictoryCelebration Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject victoryPrefab;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private CanvasGroup victoryTextCanvasGroup;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Header("Camera Settings")]
    [SerializeField] private Transform mapCenter;
    [SerializeField] private float cameraOrbitDuration = 15f;
    [SerializeField] private float cameraStartRadius = 100f;
    [SerializeField] private float cameraEndRadius = 30f;
    [SerializeField] private float cameraMinHeight = 30f;
    [SerializeField] private float cameraMaxHeight = 80f;
    [SerializeField] private int numberOfOrbits = 2;
    [SerializeField] private float cameraTiltAngle = 15f;
    [SerializeField] private AnimationCurve cameraZoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve cameraSpeedCurve = new AnimationCurve(
        new Keyframe(0f, 0.3f),
        new Keyframe(0.25f, 1f),
        new Keyframe(0.75f, 1f),
        new Keyframe(1f, 0.2f)
    );
    [SerializeField] private AnimationCurve cameraHeightCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.3f, 0.7f),
        new Keyframe(0.6f, 0.4f),
        new Keyframe(1f, 1f)
    );
    [SerializeField] private AnimationCurve cameraTiltCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve cameraFOVCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float minFOV = 50f;
    [SerializeField] private float maxFOV = 70f;

    [Header("Celebration Settings")]
    [SerializeField] private string danceAnimationTrigger = "Dance";
    [SerializeField] private float delayBeforeCameraPan = 0.5f;
    [SerializeField] private float delayBeforeDance = 1f;
    [SerializeField] private float delayBeforeVictoryPrefab = 0.5f;
    [SerializeField] private float totalCelebrationDuration = 7f;

    [Header("Text Animation")]
    [SerializeField] private float textPopDuration = 0.8f;
    [SerializeField] private float textScaleMultiplier = 1.5f;
    [SerializeField] private AnimationCurve textPopCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private string menuSceneName = "Menu";

    private bool isCelebrating;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform originalCameraParent;
    private Vector3 originalTextScale;
    private float originalFOV;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
                playerAnimator = player.GetComponent<Animator>();
            }
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        if (mainCamera != null)
        {
            originalFOV = mainCamera.fieldOfView;
        }
        
        if (mapCenter == null)
        {
            GameObject centerObj = new GameObject("Victory Map Center");
            mapCenter = centerObj.transform;
            mapCenter.position = FindMapCenter();
        }

        if (victoryPrefab != null)
        {
            victoryPrefab.SetActive(false);
        }

        if (victoryText != null)
        {
            if (victoryTextCanvasGroup == null)
            {
                victoryTextCanvasGroup = victoryText.GetComponent<CanvasGroup>();
                if (victoryTextCanvasGroup == null)
                {
                    victoryTextCanvasGroup = victoryText.gameObject.AddComponent<CanvasGroup>();
                }
            }
            
            victoryTextCanvasGroup.alpha = 0f;
            originalTextScale = victoryText.transform.localScale;
            victoryText.transform.localScale = Vector3.zero;
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
        }
    }

    public void StartVictoryCelebration()
    {
        if (isCelebrating) return;
        
        isCelebrating = true;
        StartCoroutine(CelebrationSequence());
    }

    private IEnumerator CelebrationSequence()
    {
        DisablePlayerControls();

        yield return new WaitForSeconds(delayBeforeCameraPan);

        yield return StartCoroutine(CinematicMapPan());

        yield return new WaitForSeconds(delayBeforeDance);

        TriggerPlayerDance();

        yield return new WaitForSeconds(delayBeforeVictoryPrefab);

        ActivateVictoryPrefab();

        StartCoroutine(ShowVictoryText());

        yield return new WaitForSeconds(totalCelebrationDuration);

        yield return StartCoroutine(FadeToBlackAndReturnToMenu());
    }

    private void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("[VictoryCelebration] Player controls disabled");
        }

        Rigidbody playerRb = playerTransform?.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
        }
    }

    private Vector3 FindMapCenter()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        if (allObjects.Length == 0) return Vector3.zero;

        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.activeInHierarchy && obj.transform.parent == null)
            {
                sum += obj.transform.position;
                count++;
            }
        }

        return count > 0 ? sum / count : Vector3.zero;
    }

    private IEnumerator CinematicMapPan()
    {
        if (mainCamera == null || mapCenter == null)
        {
            Debug.LogWarning("[VictoryCelebration] Camera or map center not found!");
            yield break;
        }

        originalCameraParent = mainCamera.transform.parent;
        originalCameraPosition = mainCamera.transform.position;
        originalCameraRotation = mainCamera.transform.rotation;

        mainCamera.transform.SetParent(null);

        float totalAngle = 360f * numberOfOrbits;
        float startAngle = 0f;
        
        Vector3 startPosition = mainCamera.transform.position;
        Vector3 toCamera = startPosition - mapCenter.position;
        toCamera.y = 0;
        
        if (toCamera.magnitude > 0.1f)
        {
            startAngle = Mathf.Atan2(toCamera.z, toCamera.x) * Mathf.Rad2Deg;
        }

        float elapsed = 0f;

        while (elapsed < cameraOrbitDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cameraOrbitDuration;
            
            float speedMultiplier = cameraSpeedCurve.Evaluate(t);
            float currentAngle = startAngle + (totalAngle * t);
            
            float angleRad = currentAngle * Mathf.Deg2Rad;
            
            float heightT = cameraHeightCurve.Evaluate(t);
            float currentHeight = Mathf.Lerp(cameraMinHeight, cameraMaxHeight, heightT);
            
            float zoomT = cameraZoomCurve.Evaluate(t);
            float currentRadius = Mathf.Lerp(cameraStartRadius, cameraEndRadius, zoomT);
            
            Vector3 offset = new Vector3(
                Mathf.Cos(angleRad) * currentRadius,
                currentHeight,
                Mathf.Sin(angleRad) * currentRadius
            );
            
            mainCamera.transform.position = mapCenter.position + offset;
            
            Vector3 lookAtPoint = mapCenter.position;
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPoint - mainCamera.transform.position);
            
            float tiltT = cameraTiltCurve.Evaluate(t);
            float currentTilt = Mathf.Lerp(-cameraTiltAngle, cameraTiltAngle, tiltT);
            targetRotation *= Quaternion.Euler(0f, 0f, currentTilt);
            
            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation, 
                targetRotation, 
                5f * Time.deltaTime
            );
            
            float fovT = cameraFOVCurve.Evaluate(t);
            mainCamera.fieldOfView = Mathf.Lerp(minFOV, maxFOV, fovT);

            yield return null;
        }
        
        mainCamera.fieldOfView = originalFOV;

        Debug.Log("[VictoryCelebration] Cinematic map pan complete");
    }

    private void TriggerPlayerDance()
    {
        if (playerAnimator != null && !string.IsNullOrEmpty(danceAnimationTrigger))
        {
            playerAnimator.SetTrigger(danceAnimationTrigger);
            Debug.Log("[VictoryCelebration] Player dance animation triggered");
        }
        else
        {
            Debug.LogWarning("[VictoryCelebration] Player animator or dance trigger not set!");
        }
    }

    private void ActivateVictoryPrefab()
    {
        if (victoryPrefab != null)
        {
            victoryPrefab.SetActive(true);
            Debug.Log("[VictoryCelebration] Victory prefab activated");
        }
        else
        {
            Debug.LogWarning("[VictoryCelebration] Victory prefab not assigned!");
        }
    }

    private IEnumerator ShowVictoryText()
    {
        if (victoryText == null || victoryTextCanvasGroup == null)
        {
            Debug.LogWarning("[VictoryCelebration] Victory text not assigned!");
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < textPopDuration)
        {
            elapsed += Time.deltaTime;
            float t = textPopCurve.Evaluate(elapsed / textPopDuration);

            victoryText.transform.localScale = Vector3.Lerp(Vector3.zero, originalTextScale * textScaleMultiplier, t);
            victoryTextCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        victoryText.transform.localScale = originalTextScale * textScaleMultiplier;
        victoryTextCanvasGroup.alpha = 1f;

        Debug.Log("[VictoryCelebration] Victory text displayed");
    }

    private IEnumerator FadeToBlackAndReturnToMenu()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogWarning("[VictoryCelebration] Fade canvas group not assigned!");
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;

        Debug.Log("[VictoryCelebration] Fade complete, loading menu...");

        UnityEngine.SceneManagement.SceneManager.LoadScene(menuSceneName);
    }

    public void ResetCamera()
    {
        if (mainCamera != null && originalCameraParent != null)
        {
            mainCamera.transform.SetParent(originalCameraParent);
            mainCamera.transform.localPosition = originalCameraPosition;
            mainCamera.transform.localRotation = originalCameraRotation;
        }
    }
}
