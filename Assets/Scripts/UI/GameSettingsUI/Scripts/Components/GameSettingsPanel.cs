using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 游戏设置面板 - 整合所有游戏设置的主控制脚本
/// </summary>
public class GameSettingsPanel : MonoBehaviour
{
    [Header("面板引用")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private TabGroup tabGroup;

    [Header("音频设置")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("图形设置")]
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider brightnessSlider;

    [Header("控制设置")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private Toggle invertYAxisToggle;

    // 存储设置的键名
    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string QualityLevelKey = "QualityLevel";
    private const string FullscreenKey = "Fullscreen";
    private const string BrightnessKey = "Brightness";
    private const string MouseSensitivityKey = "MouseSensitivity";
    private const string InvertYAxisKey = "InvertYAxis";

    private bool isInitialized = false;

    private void Start()
    {
        InitializeSettings();
        AddListeners();
    }

    /// <summary>
    /// 初始化所有设置
    /// </summary>
    private void InitializeSettings()
    {
        // 音频设置
        if (masterVolumeSlider != null)
        {
            float masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 0.75f);
            masterVolumeSlider.value = masterVolume;
            SetMasterVolume(masterVolume);
        }

        if (musicVolumeSlider != null)
        {
            float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f);
            musicVolumeSlider.value = musicVolume;
            SetMusicVolume(musicVolume);
        }

        if (sfxVolumeSlider != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 0.75f);
            sfxVolumeSlider.value = sfxVolume;
            SetSFXVolume(sfxVolume);
        }

        // 图形设置
        if (qualityDropdown != null)
        {
            int qualityLevel = PlayerPrefs.GetInt(QualityLevelKey, QualitySettings.GetQualityLevel());
            qualityDropdown.value = qualityLevel;
            SetQualityLevel(qualityLevel);
        }

        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            fullscreenToggle.isOn = isFullscreen;
            SetFullscreen(isFullscreen);
        }

        if (brightnessSlider != null)
        {
            float brightness = PlayerPrefs.GetFloat(BrightnessKey, 1.0f);
            brightnessSlider.value = brightness;
            SetBrightness(brightness);
        }

        // 控制设置
        if (mouseSensitivitySlider != null)
        {
            float sensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, 1.0f);
            mouseSensitivitySlider.value = sensitivity;
            SetMouseSensitivity(sensitivity);
        }

        if (invertYAxisToggle != null)
        {
            bool invertY = PlayerPrefs.GetInt(InvertYAxisKey, 0) == 1;
            invertYAxisToggle.isOn = invertY;
            SetInvertYAxis(invertY);
        }

        isInitialized = true;
    }

    /// <summary>
    /// 添加UI监听器
    /// </summary>
    private void AddListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(SetQualityLevel);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.AddListener(SetBrightness);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);

        if (invertYAxisToggle != null)
            invertYAxisToggle.onValueChanged.AddListener(SetInvertYAxis);
    }

    /// <summary>
    /// 显示或隐藏设置面板
    /// </summary>
    /// <param name="show">是否显示</param>
    public void TogglePanel(bool show)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(show);

            // 如果打开面板，选择默认标签
            if (show && tabGroup != null)
            {
                tabGroup.SelectTabByIndex(0);
            }
        }
    }

    /// <summary>
    /// 切换设置面板的显示状态
    /// </summary>
    public void TogglePanel()
    {
        if (settingsPanel != null)
        {
            TogglePanel(!settingsPanel.activeSelf);
        }
    }

    #region 音频设置
    public void SetMasterVolume(float volume)
    {
        if (audioMixer != null)
        {
            // 转换为分贝值（对数刻度）
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
            if (isInitialized)
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, volume);
            }
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
            if (isInitialized)
            {
                PlayerPrefs.SetFloat(MusicVolumeKey, volume);
            }
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20);
            if (isInitialized)
            {
                PlayerPrefs.SetFloat(SFXVolumeKey, volume);
            }
        }
    }
    #endregion

    #region 图形设置
    public void SetQualityLevel(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        if (isInitialized)
        {
            PlayerPrefs.SetInt(QualityLevelKey, qualityIndex);
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        if (isInitialized)
        {
            PlayerPrefs.SetInt(FullscreenKey, isFullscreen ? 1 : 0);
        }
    }

    public void SetBrightness(float brightness)
    {
        // 这里需要实现亮度调整的具体逻辑
        // 可能需要使用后处理效果或其他方式来调整亮度
        if (isInitialized)
        {
            PlayerPrefs.SetFloat(BrightnessKey, brightness);
        }
    }
    #endregion

    #region 控制设置
    public void SetMouseSensitivity(float sensitivity)
    {
        // 将灵敏度值传递给相关控制脚本
        // 例如：playerController.mouseSensitivity = sensitivity;
        if (isInitialized)
        {
            PlayerPrefs.SetFloat(MouseSensitivityKey, sensitivity);
        }
    }

    public void SetInvertYAxis(bool invertY)
    {
        // 将Y轴反转设置传递给相关控制脚本
        // 例如：playerController.invertYAxis = invertY;
        if (isInitialized)
        {
            PlayerPrefs.SetInt(InvertYAxisKey, invertY ? 1 : 0);
        }
    }
    #endregion

    /// <summary>
    /// 重置所有设置为默认值
    /// </summary>
    public void ResetToDefaults()
    {
        // 临时禁用初始化标志，避免重复保存
        isInitialized = false;

        // 重置并应用所有设置
        if (masterVolumeSlider != null) masterVolumeSlider.value = 0.75f;
        if (musicVolumeSlider != null) musicVolumeSlider.value = 0.75f;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = 0.75f;
        if (qualityDropdown != null) qualityDropdown.value = 2; // 中等质量
        if (fullscreenToggle != null) fullscreenToggle.isOn = true;
        if (brightnessSlider != null) brightnessSlider.value = 1.0f;
        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = 1.0f;
        if (invertYAxisToggle != null) invertYAxisToggle.isOn = false;

        // 重新启用初始化标志
        isInitialized = true;

        // 保存默认设置
        PlayerPrefs.SetFloat(MasterVolumeKey, 0.75f);
        PlayerPrefs.SetFloat(MusicVolumeKey, 0.75f);
        PlayerPrefs.SetFloat(SFXVolumeKey, 0.75f);
        PlayerPrefs.SetInt(QualityLevelKey, 2);
        PlayerPrefs.SetInt(FullscreenKey, 1);
        PlayerPrefs.SetFloat(BrightnessKey, 1.0f);
        PlayerPrefs.SetFloat(MouseSensitivityKey, 1.0f);
        PlayerPrefs.SetInt(InvertYAxisKey, 0);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 应用当前设置并关闭面板
    /// </summary>
    public void ApplyAndClose()
    {
        PlayerPrefs.Save();
        TogglePanel(false);
    }
}