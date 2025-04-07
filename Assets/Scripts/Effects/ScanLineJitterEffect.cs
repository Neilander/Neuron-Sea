using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("Image Effects/Scan Line Jitter Effect")]
[RequireComponent(typeof(Camera))]
public class ScanLineJitterEffect : MonoBehaviour
{
    [Header("效果设置")]
    [Tooltip("故障效果的着色器")]
    public Shader glitchShader;

    [Range(0, 1)]
    [Tooltip("抖动强度")]
    public float jitterIntensity = 0.1f;

    [Range(0, 100)]
    [Tooltip("抖动频率")]
    public float jitterFrequency = 10f;

    [Range(0, 10)]
    [Tooltip("扫描线厚度")]
    public float scanLineThickness = 2f;

    [Range(0, 10)]
    [Tooltip("扫描线速度")]
    public float scanLineSpeed = 1f;

    [Range(0, 1)]
    [Tooltip("颜色偏移强度")]
    public float colorShiftIntensity = 0.05f;

    [Range(0, 1)]
    [Tooltip("噪点强度")]
    public float noiseIntensity = 0.1f;

    [Range(0, 1)]
    [Tooltip("故障出现概率")]
    public float glitchProbability = 0.05f;

    [Header("常用预设")]
    [Tooltip("是否使用轻微故障预设")]
    public bool subtleGlitchPreset = false;

    [Tooltip("是否使用中等故障预设")]
    public bool mediumGlitchPreset = false;

    [Tooltip("是否使用强烈故障预设")]
    public bool intenseGlitchPreset = false;

    // 材质对象
    private Material material;

    // 获取材质
    private Material Material
    {
        get
        {
            if (material == null)
            {
                material = new Material(glitchShader);
                material.hideFlags = HideFlags.HideAndDontSave;
            }
            return material;
        }
    }

    private void OnEnable()
    {
        // 检查着色器是否可用
        if (glitchShader == null)
        {
            glitchShader = Shader.Find("Custom/ScanLineJitterEffect");
            if (glitchShader == null)
            {
                Debug.LogError("找不到扫描线抖动故障效果着色器！请确保ScanLineJitterEffect.shader文件存在于项目中。");
                enabled = false;
                return;
            }
        }

        // 检查是否支持图像效果
        if (!SystemInfo.supportsImageEffects)
        {
            Debug.LogError("当前系统不支持图像效果！");
            enabled = false;
            return;
        }
    }

    private void OnDisable()
    {
        if (material)
        {
            DestroyImmediate(material);
        }
    }

    // 检查并应用预设
    private void Update()
    {
        if (subtleGlitchPreset)
        {
            ApplySubtlePreset();
            subtleGlitchPreset = false;
        }
        else if (mediumGlitchPreset)
        {
            ApplyMediumPreset();
            mediumGlitchPreset = false;
        }
        else if (intenseGlitchPreset)
        {
            ApplyIntensePreset();
            intenseGlitchPreset = false;
        }
    }

    // 应用轻微故障预设
    private void ApplySubtlePreset()
    {
        jitterIntensity = 0.05f;
        jitterFrequency = 5f;
        scanLineThickness = 1f;
        scanLineSpeed = 0.5f;
        colorShiftIntensity = 0.02f;
        noiseIntensity = 0.05f;
        glitchProbability = 0.02f;
    }

    // 应用中等故障预设
    private void ApplyMediumPreset()
    {
        jitterIntensity = 0.1f;
        jitterFrequency = 10f;
        scanLineThickness = 2f;
        scanLineSpeed = 1f;
        colorShiftIntensity = 0.05f;
        noiseIntensity = 0.1f;
        glitchProbability = 0.05f;
    }

    // 应用强烈故障预设
    private void ApplyIntensePreset()
    {
        jitterIntensity = 0.2f;
        jitterFrequency = 20f;
        scanLineThickness = 4f;
        scanLineSpeed = 2f;
        colorShiftIntensity = 0.1f;
        noiseIntensity = 0.2f;
        glitchProbability = 0.1f;
    }

    // 渲染图像效果
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (glitchShader != null && material != null)
        {
            // 设置各个参数
            material.SetFloat("_JitterIntensity", jitterIntensity);
            material.SetFloat("_JitterFrequency", jitterFrequency);
            material.SetFloat("_ScanLineThickness", scanLineThickness);
            material.SetFloat("_ScanLineSpeed", scanLineSpeed);
            material.SetFloat("_ColorShiftIntensity", colorShiftIntensity);
            material.SetFloat("_NoiseIntensity", noiseIntensity);
            material.SetFloat("_GlitchProbability", glitchProbability);

            // 应用效果
            Graphics.Blit(source, destination, material);
        }
        else
        {
            // 如果着色器不可用，直接复制
            Graphics.Blit(source, destination);
        }
    }
}