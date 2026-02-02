using UnityEngine;
using UnityEngine.UI;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float comboTimeout = 3f;
    [SerializeField] private int[] comboThresholds = { 5, 10, 25, 50, 100 };
    [SerializeField] private string[] comboNames = { "Nice!", "Great!", "Awesome!", "Amazing!", "GODLIKE!" };

    [Header("UI")]
    [SerializeField] private Text comboText;
    [SerializeField] private Text comboNameText;
    [SerializeField] private GameObject comboPanel;

    private int currentCombo;
    private float comboTimer;
    private int highestCombo;

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

    private void Start()
    {
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    public void AddKill()
    {
        currentCombo++;
        comboTimer = comboTimeout;

        if (currentCombo > highestCombo)
        {
            highestCombo = currentCombo;
        }

        UpdateComboUI();
    }

    private void UpdateComboUI()
    {
        if (comboPanel != null)
        {
            comboPanel.SetActive(currentCombo > 1);
        }

        if (comboText != null)
        {
            comboText.text = $"{currentCombo}x Combo";
        }

        if (comboNameText != null)
        {
            string comboName = GetComboName();
            comboNameText.text = comboName;
            comboNameText.enabled = !string.IsNullOrEmpty(comboName);
        }
    }

    private string GetComboName()
    {
        for (int i = comboThresholds.Length - 1; i >= 0; i--)
        {
            if (currentCombo >= comboThresholds[i])
            {
                return comboNames[i];
            }
        }
        return "";
    }

    private void ResetCombo()
    {
        currentCombo = 0;
        
        if (comboPanel != null)
        {
            comboPanel.SetActive(false);
        }
    }

    public int GetCurrentCombo()
    {
        return currentCombo;
    }

    public int GetHighestCombo()
    {
        return highestCombo;
    }
}
