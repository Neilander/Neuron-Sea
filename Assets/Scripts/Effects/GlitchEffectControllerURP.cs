using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlitchEffectControllerURP : MonoBehaviour
{
    [Header("URP设置")]
    public UniversalRenderPipelineAsset urpAsset;
    public int rendererIndex = 0; // 默认为0，通常使用主渲染器

    [Header("触发设置")]
    public KeyCode temporaryGlitchKey = KeyCode.G;
    public KeyCode toggleGlitchKey = KeyCode.H;
    public KeyCode cyclePresentKey = KeyCode.J;
    public KeyCode waveOnlyKey = KeyCode.K;
    public KeyCode blackWhiteKey = KeyCode.B;
    public KeyCode waveBlackWhiteKey = KeyCode.V;
    public KeyCode fixPinkColorKey = KeyCode.C;

    [Header("效果设置")]
    public float temporaryGlitchDuration = 1.5f;
    [Range(0, 1)]
    public float jitterIntensity = 0.1f;
    [Range(0, 1)]
    public float colorShiftIntensity = 0.05f;
    [Range(0, 1)]
    public float noiseIntensity = 0.1f;
    [Range(0, 1)]
    public float waveIntensity = 0.2f;
    [Range(0, 1)]
    public float bwEffectIntensity = 0.8f;

    [Header("颜色校正")]
    public bool applyColorCorrectionAtStart = true;
    [Range(0, 1)]
    public float colorCorrectionIntensity = 1.0f;
    public float hueShift = -60f;
    public float saturation = 0.8f;
    public float brightness = 1.0f;
    public float contrast = 1.1f;
    public float redOffset = -0.2f;
    public float greenOffset = 0.1f;
    public float blueOffset = 0.1f;

    private bool isGlitchActive = false;
    private bool isColorCorrectionActive = false;
    private float glitchTimer = 0f;
    private int currentPreset = 0;
    private ScanLineJitterFeature glitchFeature;

    void Start()
    {
        // 如果未指定URP资产，尝试获取当前使用的资产
        if (urpAsset == null)
        {
            urpAsset = (UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
            if (urpAsset == null)
            {
                Debug.LogError("无法找到URP资产！请确保项目使用通用渲染管线。");
                return;
            }
        }

        // 获取故障效果特性
        glitchFeature = GetGlitchFeature();
        if (glitchFeature == null)
        {
            Debug.LogError("无法找到ScanLineJitterFeature！请确保已将其添加到URP渲染器特性中。");
        }
        else
        {
            // 初始化为关闭状态
            SetGlitchEnabled(false);

            // 如果需要，应用颜色校正
            if (applyColorCorrectionAtStart)
            {
                ApplyColorCorrection();
            }
        }
    }

    // 使用反射来获取渲染器数据和特性
    private ScanLineJitterFeature GetGlitchFeature()
    {
        // 使用反射获取scriptableRendererData
        Type urpAssetType = urpAsset.GetType();
        FieldInfo renderDataListFieldInfo = urpAssetType.GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);

        if (renderDataListFieldInfo == null)
        {
            Debug.LogError("无法通过反射找到m_RendererDataList字段。URP版本可能不兼容。");
            return null;
        }

        ScriptableRendererData[] rendererDatas = (ScriptableRendererData[])renderDataListFieldInfo.GetValue(urpAsset);

        if (rendererDatas == null || rendererDatas.Length == 0 || rendererIndex >= rendererDatas.Length)
        {
            Debug.LogError($"无法获取渲染器数据或索引{rendererIndex}超出范围。");
            return null;
        }

        ScriptableRendererData rendererData = rendererDatas[rendererIndex];

        // 查找ScanLineJitterFeature
        foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
        {
            if (feature is ScanLineJitterFeature)
            {
                return (ScanLineJitterFeature)feature;
            }
        }

        return null;
    }

    void Update()
    {
        if (glitchFeature == null)
            return;

        // 处理临时故障效果
        if (glitchTimer > 0)
        {
            glitchTimer -= Time.deltaTime;
            if (glitchTimer <= 0)
            {
                SetGlitchEnabled(isGlitchActive); // 恢复到之前的状态
            }
        }

        // 检查按键输入
        if (Input.GetKeyDown(temporaryGlitchKey))
        {
            TriggerTemporaryGlitch();
        }
        else if (Input.GetKeyDown(toggleGlitchKey))
        {
            ToggleGlitchEffect();
        }
        else if (Input.GetKeyDown(cyclePresentKey))
        {
            CycleGlitchPreset();
        }
        else if (Input.GetKeyDown(waveOnlyKey))
        {
            ApplyWaveOnlyMode();
        }
        else if (Input.GetKeyDown(blackWhiteKey))
        {
            ApplyBlackAndWhiteMode();
        }
        else if (Input.GetKeyDown(waveBlackWhiteKey))
        {
            ApplyWaveBlackAndWhiteMode();
        }
        else if (Input.GetKeyDown(fixPinkColorKey))
        {
            ToggleColorCorrection();
        }
    }

    // 触发临时故障效果
    public void TriggerTemporaryGlitch()
    {
        glitchTimer = temporaryGlitchDuration;

        // 保存当前设置
        float savedJitter = glitchFeature.settings.jitterIntensity;
        float savedColor = glitchFeature.settings.colorShiftIntensity;
        float savedNoise = glitchFeature.settings.noiseIntensity;
        float savedWave = glitchFeature.settings.waveIntensity;
        float savedBW = glitchFeature.settings.bwEffect;

        // 应用临时设置
        glitchFeature.settings.jitterIntensity = jitterIntensity;
        glitchFeature.settings.colorShiftIntensity = colorShiftIntensity;
        glitchFeature.settings.noiseIntensity = noiseIntensity;
        glitchFeature.settings.waveIntensity = waveIntensity;
        glitchFeature.settings.glitchProbability = 0.1f;
        glitchFeature.settings.bwEffect = bwEffectIntensity * 0.5f; // 临时效果时使用较弱的黑白效果

        Debug.Log("临时故障效果已触发！");
    }

    // 切换故障效果开/关
    public void ToggleGlitchEffect()
    {
        isGlitchActive = !isGlitchActive;
        SetGlitchEnabled(isGlitchActive);
        Debug.Log(isGlitchActive ? "故障效果已启用！" : "故障效果已禁用！");
    }

    // 循环切换预设
    public void CycleGlitchPreset()
    {
        currentPreset = (currentPreset + 1) % 3;

        switch (currentPreset)
        {
            case 0:
                glitchFeature.settings.subtleGlitchPreset = true;
                Debug.Log("应用轻微故障预设");
                break;
            case 1:
                glitchFeature.settings.mediumGlitchPreset = true;
                Debug.Log("应用中等故障预设");
                break;
            case 2:
                glitchFeature.settings.intenseGlitchPreset = true;
                Debug.Log("应用强烈故障预设");
                break;
        }

        isGlitchActive = true;
    }

    // 应用仅波浪模式
    public void ApplyWaveOnlyMode()
    {
        glitchFeature.settings.waveOnlyPreset = true;
        isGlitchActive = true;
        Debug.Log("应用仅波浪效果模式");
    }

    // 应用黑白模式
    public void ApplyBlackAndWhiteMode()
    {
        glitchFeature.settings.blackAndWhitePreset = true;
        isGlitchActive = true;
        Debug.Log("应用黑白效果模式");
    }

    // 应用波浪+黑白模式
    public void ApplyWaveBlackAndWhiteMode()
    {
        glitchFeature.settings.waveBlackAndWhitePreset = true;
        isGlitchActive = true;
        Debug.Log("应用波浪+黑白效果组合模式");
    }

    // 应用颜色校正
    public void ApplyColorCorrection()
    {
        glitchFeature.settings.colorCorrection = colorCorrectionIntensity;
        glitchFeature.settings.hueShift = hueShift;
        glitchFeature.settings.saturation = saturation;
        glitchFeature.settings.brightness = brightness;
        glitchFeature.settings.contrast = contrast;
        glitchFeature.settings.redOffset = redOffset;
        glitchFeature.settings.greenOffset = greenOffset;
        glitchFeature.settings.blueOffset = blueOffset;

        isColorCorrectionActive = true;
        Debug.Log("应用颜色校正以修复粉色问题");
    }

    // 移除颜色校正
    public void RemoveColorCorrection()
    {
        glitchFeature.settings.colorCorrection = 0f;
        isColorCorrectionActive = false;
        Debug.Log("移除颜色校正");
    }

    // 切换颜色校正
    public void ToggleColorCorrection()
    {
        if (isColorCorrectionActive)
        {
            RemoveColorCorrection();
        }
        else
        {
            ApplyColorCorrection();
        }
    }

    // 设置故障效果启用/禁用
    private void SetGlitchEnabled(bool enabled)
    {
        if (glitchFeature == null)
            return;

        // 如果禁用效果，将所有参数设为0，保留颜色校正
        if (!enabled)
        {
            glitchFeature.settings.jitterIntensity = 0;
            glitchFeature.settings.colorShiftIntensity = 0;
            glitchFeature.settings.noiseIntensity = 0;
            glitchFeature.settings.glitchProbability = 0;
            glitchFeature.settings.waveIntensity = 0;
            glitchFeature.settings.bwEffect = 0;
            // 不修改颜色校正参数，保持当前状态
        }
        else if (currentPreset == 0) // 应用轻微预设作为默认
        {
            glitchFeature.settings.subtleGlitchPreset = true;
            // 不修改颜色校正参数，保持当前状态
        }
    }
}