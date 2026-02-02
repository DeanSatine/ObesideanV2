using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "game";
    [SerializeField] private string menuSceneName = "Menu";
    
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool useFade = false;

    private bool isTransitioning;

    private void Start()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        if (fadeCanvasGroup != null && useFade)
        {
            FadeIn();
        }
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
        }
    }

    public void OnPlayButtonClicked()
    {
        if (isTransitioning) return;
        
        if (useFade && fadeCanvasGroup != null)
        {
            StartCoroutine(FadeOutAndLoadScene(gameSceneName));
        }
        else
        {
            LoadGameScene();
        }
    }

    public void OnQuitButtonClicked()
    {
        if (isTransitioning) return;
        
        QuitGame();
    }

    public void LoadGameScene()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        SceneManager.LoadScene(gameSceneName);
    }

    public void LoadMenuScene()
    {
        if (isTransitioning) return;
        
        isTransitioning = true;
        SceneManager.LoadScene(menuSceneName);
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    public void ToggleSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void OpenSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettingsPanel()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void FadeIn()
    {
        StartCoroutine(FadeCoroutine(1f, 0f));
    }

    private System.Collections.IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeCoroutine(0f, 1f));
        SceneManager.LoadScene(sceneName);
    }

    private System.Collections.IEnumerator FadeCoroutine(float startAlpha, float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;
        
        float elapsed = 0f;
        fadeCanvasGroup.alpha = startAlpha;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        fadeCanvasGroup.alpha = targetAlpha;
    }
}
