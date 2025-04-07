using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchEffectControllerURP : MonoBehaviour
{
    [Header("参考")]
    [Tooltip("渲染管线资产，如果留空将自动获取当前管线")]
    public UniversalRenderPipelineAsset urpAsset;

    [Tooltip("渲染器索引（通常为0）")]
    public int rendererIndex = 0;

    [Header("触发设置")]
    [Tooltip("按下此键触发临时故障")]
    public KeyCode triggerKey = KeyCode.G;

    [Tooltip("按下此键切换故障效果开关")]
    public KeyCode toggleKey = KeyCode.T;

    [Tooltip("按下此键循环切换预设")]
    public KeyCode cyclePresetKey = KeyCode.P;

    [Header("临时故障设置")]
    [Tooltip("临时故障持续时间")]
    public float glitchDuration = 2.0f;

    [Tooltip("临时故障强度")]
    [Range(0, 1)]
    public float temporaryGlitchIntensity = 0.2f;

    // 内部变量
    private bool isTemporaryGlitchActive = false;
    private float glitchTimer = 0f;
    private int currentPreset = 0;
    private ScanLineJitterFeature glitchFeature;
    private bool isEnabled = true;

    // 原始参数备份
    private float originalJitterIntensity;
    private float originalGlitchProbability;

    private void Start()
    {
        // 如果没有指定URP资产，尝试获取当前的
        if (urpAsset == null)
        {
            urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("未找到URP渲染管线资产！请确保项目已设置URP。");
                enabled = false;
                return;
            }
        }

        // 尝试获取故障效果渲染特性
        GetGlitchFeature();

        if (glitchFeature == null)
        {
            Debug.LogWarning("未找到ScanLineJitterFeature！请先将其添加到渲染管线中。");
            enabled = false;
            return;
        }

        // 保存原始参数值
        originalJitterIntensity = glitchFeature.settings.jitterIntensity;
        originalGlitchProbability = glitchFeature.settings.glitchProbability;
    }

    private void GetGlitchFeature()
    {
        // 通过反射获取渲染器
        var rendererData = GetRendererData(urpAsset, rendererIndex);
        if (rendererData != null)
        {
            // 获取渲染特性
            var rendererFeatures = GetRendererFeatures(rendererData);
            if (rendererFeatures != null)
            {
                foreach (var feature in rendererFeatures)
                {
                    if (feature is ScanLineJitterFeature)
                    {
                        glitchFeature = feature as ScanLineJitterFeature;
                        break;
                    }
                }
            }
        }
    }

    // 通过反射获取渲染器数据
    private ScriptableRendererData GetRendererData(UniversalRenderPipelineAsset urpAsset, int index)
    {
        var propertyInfo = urpAsset.GetType().GetProperty("m_RendererDataList",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (propertyInfo != null)
        {
            var rendererDataList = propertyInfo.GetValue(urpAsset) as ScriptableRendererData[];
            if (rendererDataList != null && rendererDataList.Length > index)
            {
                return rendererDataList[index];
            }
        }

        return null;
    }

    // 通过反射获取渲染特性列表
    private System.Collections.Generic.List<ScriptableRendererFeature> GetRendererFeatures(ScriptableRendererData rendererData)
    {
        var fieldInfo = rendererData.GetType().GetField("m_RendererFeatures",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(rendererData) as System.Collections.Generic.List<ScriptableRendererFeature>;
        }

        return null;
    }

    private void Update()
    {
        if (glitchFeature == null) return;

        // 检查临时故障状态
        if (isTemporaryGlitchActive)
        {
            glitchTimer -= Time.deltaTime;

            if (glitchTimer <= 0)
            {
                // 恢复原始参数值
                glitchFeature.settings.jitterIntensity = originalJitterIntensity;
                glitchFeature.settings.glitchProbability = originalGlitchProbability;
                isTemporaryGlitchActive = false;
            }
        }

        // 触发临时故障
        if (Input.GetKeyDown(triggerKey))
        {
            TriggerTemporaryGlitch();
        }

        // 切换故障效果开关
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleGlitchEffect();
        }

        // 循环切换预设
        if (Input.GetKeyDown(cyclePresetKey))
        {
            CyclePreset();
        }
    }

    // 触发临时故障
    public void TriggerTemporaryGlitch()
    {
        if (glitchFeature == null) return;

        // 保存原始参数值（如果不是已经在临时故障中）
        if (!isTemporaryGlitchActive)
        {
            originalJitterIntensity = glitchFeature.settings.jitterIntensity;
            originalGlitchProbability = glitchFeature.settings.glitchProbability;
        }

        // 增强故障效果
        glitchFeature.settings.jitterIntensity = temporaryGlitchIntensity;
        glitchFeature.settings.glitchProbability = temporaryGlitchIntensity * 0.5f;

        // 设置定时器
        glitchTimer = glitchDuration;
        isTemporaryGlitchActive = true;

        Debug.Log("触发临时故障效果，持续" + glitchDuration + "秒");
    }

    // 切换故障效果开关
    public void ToggleGlitchEffect()
    {
        if (glitchFeature == null) return;

        isEnabled = !isEnabled;

        if (isEnabled)
        {
            // 恢复参数值
            glitchFeature.settings.jitterIntensity = originalJitterIntensity;
            glitchFeature.settings.glitchProbability = originalGlitchProbability;
        }
        else
        {
            // 将所有参数设为0来禁用效果
            originalJitterIntensity = glitchFeature.settings.jitterIntensity;
            originalGlitchProbability = glitchFeature.settings.glitchProbability;

            glitchFeature.settings.jitterIntensity = 0;
            glitchFeature.settings.noiseIntensity = 0;
            glitchFeature.settings.colorShiftIntensity = 0;
            glitchFeature.settings.glitchProbability = 0;
        }

        Debug.Log("故障效果已" + (isEnabled ? "启用" : "禁用"));
    }

    // 循环切换预设
    public void CyclePreset()
    {
        if (glitchFeature == null) return;

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
    }
}