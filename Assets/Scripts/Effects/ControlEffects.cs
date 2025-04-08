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

    // Inspector中显示的参数
    [Header("扫描线抖动效果")]
    [Range(0, 1)]
    public float jitterIntensity = 0.2f;

    [Range(0, 10)]
    public float jitterFrequency = 5f;

    [Range(0, 2)]
    public float scanLineThickness = 1f;

    [Range(0, 2)]
    public float scanLineSpeed = 0.5f;

    [Header("颜色效果")]
    [Range(0, 0.1f)]
    public float colorShiftIntensity = 0.02f;

    [Range(0, 0.1f)]
    public float noiseIntensity = 0.05f;

    [Range(0, 0.1f)]
    public float glitchProbability = 0.02f;

    [Header("预设")]
    public bool subtleGlitchPreset = true;
    public bool intenseGlitchPreset = false;
    public bool colorShiftPreset = false;
    public bool noisePreset = false;

    [Header("波浪效果")]
    public bool enableWaveEffect = false;
    [Range(0, 1)]
    public float waveIntensity = 0.2f;

    [Header("黑白效果")]
    public bool enableBlackAndWhite = false;
    [Range(0, 1)]
    public float blackAndWhiteIntensity = 0.5f;

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

        Debug.Log("成功获取URP资源");

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

        Debug.Log("成功获取渲染器数据");

        // 获取特效组件
        feature = GetRendererFeature<ScanLineJitterFeature>();
        if (feature == null)
        {
            Debug.LogError("未找到ScanLineJitterFeature！请确保已添加该渲染特性。");
            Debug.LogError("请检查：Renderer Data中的Renderer Features列表");
            return;
        }

        Debug.Log("成功获取ScanLineJitterFeature");

        // 初始化效果设置
        ApplyDefaultSettings();
        Debug.Log("效果初始化完成");
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
        feature.settings.jitterIntensity = jitterIntensity;
        feature.settings.jitterFrequency = jitterFrequency;
        feature.settings.scanLineThickness = scanLineThickness;
        feature.settings.scanLineSpeed = scanLineSpeed;
        feature.settings.colorShiftIntensity = colorShiftIntensity;
        feature.settings.noiseIntensity = noiseIntensity;
        feature.settings.glitchProbability = glitchProbability;

        // 预设
        feature.settings.subtleGlitchPreset = subtleGlitchPreset;
        feature.settings.intenseGlitchPreset = intenseGlitchPreset;

        // 波浪效果
        feature.settings.waveIntensity = waveIntensity;

        // 黑白效果
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
        Debug.Log($"渲染器数据列表包含 {rendererDataList.Length} 个渲染器");
        for (int i = 0; i < rendererDataList.Length; i++)
        {
            if (rendererDataList[i] != null)
            {
                Debug.Log($"渲染器 {i}: {rendererDataList[i].GetType().Name}");
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

            subtleGlitchPreset = feature.settings.subtleGlitchPreset;
            intenseGlitchPreset = feature.settings.intenseGlitchPreset;

            waveIntensity = feature.settings.waveIntensity;

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
        blackAndWhiteIntensity = 0.8f;

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
        blackAndWhiteIntensity = 0.0f;

        UpdateEffectSettings();
    }
}
