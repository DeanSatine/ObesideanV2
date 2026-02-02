using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Graphics")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Dropdown qualityDropdown;
    [SerializeField] private Dropdown resolutionDropdown;

    [Header("Gameplay")]
    [SerializeField] private Toggle screenShakeToggle;
    [SerializeField] private Slider sensitivitySlider;

    private const string MasterVolumePref = "MasterVolume";
    private const string MusicVolumePref = "MusicVolume";
    private const string SFXVolumePref = "SFXVolume";
    private const string ScreenShakePref = "ScreenShake";
    private const string SensitivityPref = "Sensitivity";

    private void Start()
    {
        LoadSettings();
        SetupListeners();
    }

    private void SetupListeners()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.AddListener(SetQuality);
        }

        if (screenShakeToggle != null)
        {
            screenShakeToggle.onValueChanged.AddListener(SetScreenShake);
        }

        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        }
    }

    private void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat(MasterVolumePref, 0.75f);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumePref, 0.75f);
        float sfxVolume = PlayerPrefs.GetFloat(SFXVolumePref, 0.75f);
        bool screenShake = PlayerPrefs.GetInt(ScreenShakePref, 1) == 1;
        float sensitivity = PlayerPrefs.GetFloat(SensitivityPref, 1f);

        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVolume;
        if (screenShakeToggle != null) screenShakeToggle.isOn = screenShake;
        if (sensitivitySlider != null) sensitivitySlider.value = sensitivity;

        if (fullscreenToggle != null) fullscreenToggle.isOn = Screen.fullScreen;
        if (qualityDropdown != null) qualityDropdown.value = QualitySettings.GetQualityLevel();
    }

    public void SetMasterVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
        }
        PlayerPrefs.SetFloat(MasterVolumePref, volume);
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
        }
        PlayerPrefs.SetFloat(MusicVolumePref, volume);
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f);
        }
        PlayerPrefs.SetFloat(SFXVolumePref, volume);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetScreenShake(bool enabled)
    {
        PlayerPrefs.SetInt(ScreenShakePref, enabled ? 1 : 0);
    }

    public void SetSensitivity(float sensitivity)
    {
        PlayerPrefs.SetFloat(SensitivityPref, sensitivity);
    }

    public static bool IsScreenShakeEnabled()
    {
        return PlayerPrefs.GetInt(ScreenShakePref, 1) == 1;
    }

    public static float GetSensitivity()
    {
        return PlayerPrefs.GetFloat(SensitivityPref, 1f);
    }
}
