using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ControlEffects : MonoBehaviour
{
    // URP资源和渲染器数据
    private UniversalRenderPipelineAsset urpRenderer;
    private Renderer2DData rendererData;
    private ScanLineJitterFeature feature;

    [Header("效果开关")]
    public bool enableScanLineJitter = true;
    public bool enableColorShift = true;
    public bool enableNoise = true;
    public bool enableGlitch = true;
    public bool enableWaveEffect = true;
    public bool enableBlackAndWhite = true;

    [Header("扫描线抖动效果")]
    [Range(0, 1)]
    public float jitterIntensity = 0.195f;
    [Range(0, 100)]
    public float jitterFrequency = 6f;
    [Range(0, 5)]
    public float scanLineThickness = 0f;
    [Range(0, 5)]
    public float scanLineSpeed = 4.2f;

    [Header("颜色效果")]
    [Range(0, 1)]
    public float colorShiftIntensity = 0.05f;
    [Range(0, 1)]
    public float noiseIntensity = 0.1f;
    [Range(0, 1)]
    public float glitchProbability = 0.05f;

    [Header("波浪效果")]
    [Range(0, 1)]
    public float waveIntensity = 0.2f;
    [Range(0, 20)]
    public float waveFrequency = 10f;
    [Range(0, 5)]
    public float waveSpeed = 2f;

    [Header("黑白效果")]
    [Range(0, 1)]
    public float bwEffect = 0f;
    [Range(0, 20)]
    public float bwNoiseScale = 10f;
    [Range(0, 1)]
    public float bwNoiseIntensity = 0.2f;
    [Range(0, 10)]
    public float bwFlickerSpeed = 5f;

    [Header("颜色校正")]
    [Range(-1, 1)]
    public float colorCorrection = 0f;
    [Range(-1, 1)]
    public float hueShift = 0f;
    [Range(0, 2)]
    public float saturation = 1f;
    [Range(0, 2)]
    public float brightness = 1f;
    [Range(0, 2)]
    public float contrast = 1f;
    [Range(-1, 1)]
    public float redOffset = 0f;
    [Range(-1, 1)]
    public float greenOffset = 0f;
    [Range(-1, 1)]
    public float blueOffset = 0f;

    // Inspector中显示的参数
    [Header("预设")]
    public bool subtleGlitchPreset = true;
    public bool intenseGlitchPreset = false;
    public bool colorShiftPreset = false;
    public bool noisePreset = false;

    void Start()
    {
        // 初始化URP资源
        urpRenderer = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        if (urpRenderer == null)
        {
            Debug.LogError("未找到URP资源！请确保项目正确设置了Universal Render Pipeline。");
            Debug.LogError("请检查：Edit > Project Settings > Graphics > Scriptable Render Pipeline Settings");
            return;
        }

        // Debug.Log("成功获取URP资源");

        // 获取渲染器数据
        rendererData = GetRendererData();
        if (rendererData == null)
        {
            Debug.LogError("未找到Renderer2DData！请确保在URP Asset中设置了默认渲染器。");
            Debug.LogError("请检查：1. URP Asset中的Renderer List是否为空");
            Debug.LogError("2. 是否已创建Renderer2D Data");
            Debug.LogError("3. 默认渲染器索引是否正确");
            return;
        }

        // Debug.Log("成功获取渲染器数据");

        // 获取特效组件
        feature = GetRendererFeature<ScanLineJitterFeature>();
        if (feature == null)
        {
            Debug.LogError("未找到ScanLineJitterFeature！请确保已添加该渲染特性。");
            Debug.LogError("请检查：Renderer Data中的Renderer Features列表");
            return;
        }

        // Debug.Log("成功获取ScanLineJitterFeature");

        // 初始化效果设置
        ApplyDefaultSettings();
        // 默认禁用渲染特性
        DisableScanLineJitterFeature();
        // Debug.Log("效果初始化完成");
    }

    void Update()
    {
        // 每帧更新效果参数
        UpdateEffectSettings();
    }

    // 更新效果参数到ScanLineJitterFeature
    private void UpdateEffectSettings()
    {
        if (feature == null) return;

        // 基本参数
        feature.settings.jitterIntensity = enableScanLineJitter ? jitterIntensity : 0f;
        feature.settings.jitterFrequency = jitterFrequency;
        feature.settings.scanLineThickness = scanLineThickness;
        feature.settings.scanLineSpeed = scanLineSpeed;
        feature.settings.colorShiftIntensity = enableColorShift ? colorShiftIntensity : 0f;
        feature.settings.noiseIntensity = enableNoise ? noiseIntensity : 0f;
        feature.settings.glitchProbability = enableGlitch ? glitchProbability : 0f;

        // 波浪效果
        feature.settings.waveIntensity = enableWaveEffect ? waveIntensity : 0f;
        feature.settings.waveFrequency = waveFrequency;
        feature.settings.waveSpeed = waveSpeed;

        // 黑白效果
        feature.settings.bwEffect = enableBlackAndWhite ? bwEffect : 0f;
        feature.settings.bwNoiseScale = bwNoiseScale;
        feature.settings.bwNoiseIntensity = bwNoiseIntensity;
        feature.settings.bwFlickerSpeed = bwFlickerSpeed;

        // 颜色校正
        feature.settings.colorCorrection = colorCorrection;
        feature.settings.hueShift = hueShift;
        feature.settings.saturation = saturation;
        feature.settings.brightness = brightness;
        feature.settings.contrast = contrast;
        feature.settings.redOffset = redOffset;
        feature.settings.greenOffset = greenOffset;
        feature.settings.blueOffset = blueOffset;
    }

    private Renderer2DData GetRendererData()
    {
        // 默认使用索引0
        int defaultRendererIndex = 0;

        // 通过反射获取默认渲染器索引字段
        var indexField = typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (indexField != null)
        {
            defaultRendererIndex = (int)indexField.GetValue(urpRenderer);
        }

        // 通过反射获取渲染器数据列表
        var field = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field == null)
        {
            Debug.LogError("无法获取渲染器数据列表！");
            return null;
        }

        var rendererDataList = field.GetValue(urpRenderer) as ScriptableRendererData[];
        if (rendererDataList == null || rendererDataList.Length == 0)
        {
            Debug.LogError("渲染器数据列表为空！");
            return null;
        }

        // 打印一下渲染器数据列表的信息，以便调试
        // Debug.Log($"渲染器数据列表包含 {rendererDataList.Length} 个渲染器");
        for (int i = 0; i < rendererDataList.Length; i++)
        {
            if (rendererDataList[i] != null)
            {
                // Debug.Log($"渲染器 {i}: {rendererDataList[i].GetType().Name}");
            }
            else
            {
                Debug.Log($"渲染器 {i}: 为空");
            }
        }

        // 使用默认渲染器索引
        if (defaultRendererIndex >= 0 && defaultRendererIndex < rendererDataList.Length)
        {
            var rendererData = rendererDataList[defaultRendererIndex];

            // 检查是否为2D渲染器数据
            if (rendererData is Renderer2DData)
            {
                return rendererData as Renderer2DData;
            }
            else
            {
                Debug.LogWarning($"默认渲染器类型为 {rendererData?.GetType().Name}，而不是Renderer2DData");

                // 尝试查找Renderer2DData
                for (int i = 0; i < rendererDataList.Length; i++)
                {
                    if (rendererDataList[i] is Renderer2DData)
                    {
                        Debug.Log($"在索引 {i} 找到Renderer2DData");
                        return rendererDataList[i] as Renderer2DData;
                    }
                }

                Debug.LogError("找不到Renderer2DData");
                return null;
            }
        }
        else
        {
            Debug.LogError($"默认渲染器索引 {defaultRendererIndex} 超出范围！");
            return null;
        }
    }

    private T GetRendererFeature<T>() where T : ScriptableRendererFeature
    {
        if (rendererData == null) return null;

        foreach (var feature in rendererData.rendererFeatures)
        {
            if (feature is T typedFeature)
            {
                return typedFeature;
            }
        }
        Debug.LogWarning($"未找到类型为 {typeof(T).Name} 的渲染特性！");
        return null;
    }

    private void ApplyDefaultSettings()
    {
        if (feature != null)
        {
            // 从实际特效复制初始参数到Inspector
            jitterIntensity = feature.settings.jitterIntensity;
            jitterFrequency = feature.settings.jitterFrequency;
            scanLineThickness = feature.settings.scanLineThickness;
            scanLineSpeed = feature.settings.scanLineSpeed;
            colorShiftIntensity = feature.settings.colorShiftIntensity;
            noiseIntensity = feature.settings.noiseIntensity;
            glitchProbability = feature.settings.glitchProbability;

            waveIntensity = feature.settings.waveIntensity;
            waveFrequency = feature.settings.waveFrequency;
            waveSpeed = feature.settings.waveSpeed;

            bwEffect = feature.settings.bwEffect;
            bwNoiseScale = feature.settings.bwNoiseScale;
            bwNoiseIntensity = feature.settings.bwNoiseIntensity;
            bwFlickerSpeed = feature.settings.bwFlickerSpeed;

            colorCorrection = feature.settings.colorCorrection;
            hueShift = feature.settings.hueShift;
            saturation = feature.settings.saturation;
            brightness = feature.settings.brightness;
            contrast = feature.settings.contrast;
            redOffset = feature.settings.redOffset;
            greenOffset = feature.settings.greenOffset;
            blueOffset = feature.settings.blueOffset;
        }
    }

    // 提供公共方法来调整效果
    public void AdjustJitterIntensity(float intensity)
    {
        jitterIntensity = Mathf.Clamp01(intensity);
        if (feature != null)
        {
            feature.settings.jitterIntensity = jitterIntensity;
        }
    }

    public void AdjustGlitchProbability(float probability)
    {
        glitchProbability = Mathf.Clamp01(probability);
        if (feature != null)
        {
            feature.settings.glitchProbability = glitchProbability;
        }
    }

    public void ApplySubtlePreset(bool enable)
    {
        subtleGlitchPreset = enable;
        if (feature != null)
        {
            feature.settings.subtleGlitchPreset = subtleGlitchPreset;
        }
    }

    // 添加预设效果方法
    [ContextMenu("应用微妙故障效果")]
    public void ApplySubtleEffect()
    {
        subtleGlitchPreset = true;
        intenseGlitchPreset = false;
        colorShiftPreset = false;
        noisePreset = false;

        jitterIntensity = 0.2f;
        glitchProbability = 0.05f;
        colorShiftIntensity = 0.02f;
        noiseIntensity = 0.03f;

        enableWaveEffect = false;
        enableBlackAndWhite = false;

        UpdateEffectSettings();
    }

    [ContextMenu("应用强烈故障效果")]
    public void ApplyIntenseEffect()
    {
        subtleGlitchPreset = false;
        intenseGlitchPreset = true;
        colorShiftPreset = false;
        noisePreset = false;

        jitterIntensity = 0.6f;
        glitchProbability = 0.1f;
        colorShiftIntensity = 0.05f;
        noiseIntensity = 0.08f;

        enableWaveEffect = true;
        waveIntensity = 0.3f;
        enableBlackAndWhite = false;

        UpdateEffectSettings();
    }

    [ContextMenu("应用颜色偏移效果")]
    public void ApplyColorShiftEffect()
    {
        subtleGlitchPreset = false;
        intenseGlitchPreset = false;
        colorShiftPreset = true;
        noisePreset = false;

        jitterIntensity = 0.1f;
        glitchProbability = 0.02f;
        colorShiftIntensity = 0.08f;
        noiseIntensity = 0.02f;

        enableWaveEffect = false;
        enableBlackAndWhite = false;

        UpdateEffectSettings();
    }

    [ContextMenu("应用噪点效果")]
    public void ApplyNoiseEffect()
    {
        subtleGlitchPreset = false;
        intenseGlitchPreset = false;
        colorShiftPreset = false;
        noisePreset = true;

        jitterIntensity = 0.1f;
        glitchProbability = 0.02f;
        colorShiftIntensity = 0.01f;
        noiseIntensity = 0.1f;

        enableWaveEffect = false;
        enableBlackAndWhite = false;

        UpdateEffectSettings();
    }

    [ContextMenu("应用老电影效果")]
    public void ApplyOldFilmEffect()
    {
        subtleGlitchPreset = false;
        intenseGlitchPreset = false;
        colorShiftPreset = false;
        noisePreset = true;

        jitterIntensity = 0.3f;
        jitterFrequency = 8f;
        scanLineThickness = 1.5f;
        scanLineSpeed = 0.8f;
        glitchProbability = 0.03f;
        colorShiftIntensity = 0.0f;
        noiseIntensity = 0.08f;

        enableWaveEffect = false;
        enableBlackAndWhite = true;
        bwEffect = 0.8f;
        bwNoiseScale = 10f;
        bwNoiseIntensity = 0.2f;
        bwFlickerSpeed = 5f;

        UpdateEffectSettings();
    }

    [ContextMenu("重置所有效果")]
    public void ResetAllEffects()
    {
        subtleGlitchPreset = false;
        intenseGlitchPreset = false;
        colorShiftPreset = false;
        noisePreset = false;

        jitterIntensity = 0.0f;
        jitterFrequency = 5f;
        scanLineThickness = 1f;
        scanLineSpeed = 0.5f;
        glitchProbability = 0.0f;
        colorShiftIntensity = 0.0f;
        noiseIntensity = 0.0f;

        enableWaveEffect = false;
        waveIntensity = 0.0f;
        enableBlackAndWhite = false;
        bwEffect = 0f;
        bwNoiseScale = 10f;
        bwNoiseIntensity = 0.2f;
        bwFlickerSpeed = 5f;

        UpdateEffectSettings();
    }

    [ContextMenu("应用默认扫描线效果")]
    public void ApplyScanLineEffect()
    {
        enableScanLineJitter = true;
        enableColorShift = false;
        enableNoise = false;
        enableGlitch = false;
        enableWaveEffect = false;
        enableBlackAndWhite = false;

        jitterIntensity = 0.1f;
        jitterFrequency = 10f;
        scanLineThickness = 2f;
        scanLineSpeed = 1f;

        UpdateEffectSettings();
    }

    [ContextMenu("应用波浪扫描效果")]
    public void ApplyWaveScanEffect()
    {
        enableScanLineJitter = true;
        enableColorShift = false;
        enableNoise = false;
        enableGlitch = false;
        enableWaveEffect = true;
        enableBlackAndWhite = false;

        jitterIntensity = 0.1f;
        jitterFrequency = 10f;
        scanLineThickness = 2f;
        scanLineSpeed = 1f;
        waveIntensity = 0.2f;
        waveFrequency = 10f;
        waveSpeed = 2f;

        UpdateEffectSettings();
    }

    [ContextMenu("应用复古电影效果")]
    public void ApplyVintageEffect()
    {
        enableScanLineJitter = true;
        enableColorShift = false;
        enableNoise = true;
        enableGlitch = false;
        enableWaveEffect = false;
        enableBlackAndWhite = true;

        jitterIntensity = 0.1f;
        jitterFrequency = 10f;
        scanLineThickness = 2f;
        scanLineSpeed = 1f;
        noiseIntensity = 0.1f;
        bwEffect = 1f;
        bwNoiseScale = 10f;
        bwNoiseIntensity = 0.2f;
        bwFlickerSpeed = 5f;
        contrast = 1.2f;
        brightness = 0.9f;

        UpdateEffectSettings();
    }

    [ContextMenu("应用故障艺术效果")]
    public void ApplyGlitchArtEffect()
    {
        enableScanLineJitter = true;
        enableColorShift = true;
        enableNoise = true;
        enableGlitch = true;
        enableWaveEffect = true;
        enableBlackAndWhite = false;

        jitterIntensity = 0.1f;
        jitterFrequency = 10f;
        scanLineThickness = 2f;
        scanLineSpeed = 1f;
        colorShiftIntensity = 0.05f;
        noiseIntensity = 0.1f;
        glitchProbability = 0.05f;
        waveIntensity = 0.2f;
        waveFrequency = 10f;
        waveSpeed = 2f;
        hueShift = 0.2f;
        saturation = 1.2f;

        UpdateEffectSettings();
    }

    // 启用ScanLineJitterFeature渲染特性
    public void EnableScanLineJitterFeature()
    {
        if (feature == null) return;

        if (!feature.isActive)
        {
            // 启用渲染特性
            feature.SetActive(true);
            Debug.Log("启用ScanLineJitterFeature");
        }
    }

    // 禁用ScanLineJitterFeature渲染特性
    public void DisableScanLineJitterFeature()
    {
        if (feature == null) return;

        if (feature.isActive)
        {
            // 禁用渲染特性
            feature.SetActive(false);
            Debug.Log("禁用ScanLineJitterFeature");
        }
    }

    // 获取渲染特性的活动状态
    public bool IsFeatureActive()
    {
        return feature != null && feature.isActive;
    }
}
