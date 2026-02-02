using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Header("Time Settings")]
    [SerializeField] private float normalTimeScale = 1f;
    [SerializeField] private float slowMotionScale = 0.3f;
    [SerializeField] private float transitionSpeed = 5f;

    private float targetTimeScale = 1f;
    private Coroutine timeCoroutine;

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
    }

    private void Update()
    {
        Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, Time.unscaledDeltaTime * transitionSpeed);
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    public void SlowMotion(float duration, float scale = -1f)
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        
        targetTimeScale = scale < 0 ? slowMotionScale : scale;
        timeCoroutine = StartCoroutine(ResetTimeAfterDelay(duration));
    }

    public void FreezeFrame(float duration)
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        
        targetTimeScale = 0f;
        timeCoroutine = StartCoroutine(ResetTimeAfterDelay(duration));
    }

    public void ResetTime()
    {
        if (timeCoroutine != null)
        {
            StopCoroutine(timeCoroutine);
        }
        
        targetTimeScale = normalTimeScale;
    }

    private IEnumerator ResetTimeAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        targetTimeScale = normalTimeScale;
        timeCoroutine = null;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}
