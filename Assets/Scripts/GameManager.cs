using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Victory Settings")]
    [SerializeField] private string victorySceneName = "VictoryScene";
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private float victorySceneDuration = 5f;
    
    [Header("Defeat Settings")]
    [SerializeField] private Image defeatImage;
    [SerializeField] private float defeatImageDuration = 3f;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private bool isGameEnding = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1f;
            fadeCanvasGroup.gameObject.SetActive(true);
        }
        
        if (defeatImage != null)
        {
            defeatImage.gameObject.SetActive(false);
        }
    }
    
    private void Start()
    {
        if (fadeCanvasGroup != null)
        {
            StartCoroutine(FadeFromBlack());
        }
    }

    public void OnBossDefeated()
    {
        if (isGameEnding) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.RestoreFullHealth();
            }
        }

        GameObject[] remainingBosses = GameObject.FindGameObjectsWithTag("Boss");
        
        int aliveBossCount = 0;
        foreach (GameObject boss in remainingBosses)
        {
            BossController bossController = boss.GetComponent<BossController>();
            if (bossController != null && !bossController.IsDead())
            {
                aliveBossCount++;
            }
        }

        if (aliveBossCount == 0)
        {
            isGameEnding = true;
            StartCoroutine(VictorySequence());
        }
    }
    
    public void OnPlayerDeath()
    {
        if (isGameEnding) return;
        
        isGameEnding = true;
        StartCoroutine(DefeatSequence());
    }

    private IEnumerator VictorySequence()
    {
        if (VictoryCelebration.Instance != null)
        {
            VictoryCelebration.Instance.StartVictoryCelebration();
        }
        else
        {
            yield return new WaitForSeconds(2f);
            
            yield return StartCoroutine(FadeToBlack());
            
            SceneManager.LoadScene(victorySceneName);
            
            yield return new WaitForSeconds(victorySceneDuration);
            
            yield return StartCoroutine(FadeToBlack());
            
            SceneManager.LoadScene(menuSceneName);
        }
    }
    
    private IEnumerator DefeatSequence()
    {
        yield return new WaitForSeconds(2f);
        
        if (defeatImage != null)
        {
            defeatImage.gameObject.SetActive(true);
            CanvasGroup defeatCanvasGroup = defeatImage.GetComponent<CanvasGroup>();
            if (defeatCanvasGroup == null)
            {
                defeatCanvasGroup = defeatImage.gameObject.AddComponent<CanvasGroup>();
            }
            
            yield return StartCoroutine(FadeIn(defeatCanvasGroup));
            
            yield return new WaitForSeconds(defeatImageDuration);
            
            yield return StartCoroutine(FadeOut(defeatCanvasGroup));
        }
        
        yield return StartCoroutine(FadeToBlack());
        
        SceneManager.LoadScene(menuSceneName);
    }
    
    private IEnumerator FadeToBlack()
    {
        if (fadeCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        fadeCanvasGroup.alpha = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeFromBlack()
    {
        if (fadeCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        fadeCanvasGroup.alpha = 1f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = 0f;
    }
    
    private IEnumerator FadeIn(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        canvasGroup.alpha = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
    
    private IEnumerator FadeOut(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        canvasGroup.alpha = 1f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
