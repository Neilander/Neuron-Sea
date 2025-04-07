using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScanLineJitterFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class ScanLineJitterSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("效果参数")]
        [Range(0, 1)]
        public float jitterIntensity = 0.1f;

        [Range(0, 100)]
        public float jitterFrequency = 10f;

        [Range(0, 10)]
        public float scanLineThickness = 2f;

        [Range(0, 10)]
        public float scanLineSpeed = 1f;

        [Range(0, 1)]
        public float colorShiftIntensity = 0.05f;

        [Range(0, 1)]
        public float noiseIntensity = 0.1f;

        [Range(0, 1)]
        public float glitchProbability = 0.05f;

        [Header("波浪效果")]
        [Range(0, 1)]
        public float waveIntensity = 0.2f;

        [Range(0, 50)]
        public float waveFrequency = 10f;

        [Range(0, 10)]
        public float waveSpeed = 2f;

        [Header("黑白效果")]
        [Range(0, 1)]
        public float bwEffect = 0f;

        [Range(1, 50)]
        public float bwNoiseScale = 10f;

        [Range(0, 1)]
        public float bwNoiseIntensity = 0.2f;

        [Range(0, 20)]
        public float bwFlickerSpeed = 5f;

        [Header("颜色校正")]
        [Range(0, 1)]
        public float colorCorrection = 0f;

        [Range(-180, 180)]
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

        [Header("预设")]
        public bool subtleGlitchPreset = false;
        public bool mediumGlitchPreset = false;
        public bool intenseGlitchPreset = false;
        public bool waveOnlyPreset = false;
        public bool blackAndWhitePreset = false;
        public bool waveBlackAndWhitePreset = false;
        public bool fixPinkColorPreset = false;
    }

    public ScanLineJitterSettings settings = new ScanLineJitterSettings();
    private ScanLineJitterPass scanLineJitterPass;
    private Material scanLineMaterial;

    public override void Create()
    {
        // 加载着色器并创建材质
        Shader shader = Shader.Find("Custom/ScanLineJitterEffectURP");
        if (shader == null)
        {
            Debug.LogError("找不到扫描线抖动故障效果着色器！请确保ScanLineJitterEffectURP.shader文件存在于项目中。");
            return;
        }

        scanLineMaterial = new Material(shader);
        scanLineJitterPass = new ScanLineJitterPass(settings, scanLineMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (scanLineMaterial == null)
        {
            Debug.LogWarning("扫描线抖动故障效果材质不可用！");
            return;
        }

        // 检查预设设置
        if (settings.subtleGlitchPreset)
        {
            ApplySubtlePreset();
            settings.subtleGlitchPreset = false;
        }
        else if (settings.mediumGlitchPreset)
        {
            ApplyMediumPreset();
            settings.mediumGlitchPreset = false;
        }
        else if (settings.intenseGlitchPreset)
        {
            ApplyIntensePreset();
            settings.intenseGlitchPreset = false;
        }
        else if (settings.waveOnlyPreset)
        {
            ApplyWaveOnlyPreset();
            settings.waveOnlyPreset = false;
        }
        else if (settings.blackAndWhitePreset)
        {
            ApplyBlackAndWhitePreset();
            settings.blackAndWhitePreset = false;
        }
        else if (settings.waveBlackAndWhitePreset)
        {
            ApplyWaveBlackAndWhitePreset();
            settings.waveBlackAndWhitePreset = false;
        }
        else if (settings.fixPinkColorPreset)
        {
            ApplyFixPinkColorPreset();
            settings.fixPinkColorPreset = false;
        }

        // 设置材质属性
        scanLineMaterial.SetFloat("_JitterIntensity", settings.jitterIntensity);
        scanLineMaterial.SetFloat("_JitterFrequency", settings.jitterFrequency);
        scanLineMaterial.SetFloat("_ScanLineThickness", settings.scanLineThickness);
        scanLineMaterial.SetFloat("_ScanLineSpeed", settings.scanLineSpeed);
        scanLineMaterial.SetFloat("_ColorShiftIntensity", settings.colorShiftIntensity);
        scanLineMaterial.SetFloat("_NoiseIntensity", settings.noiseIntensity);
        scanLineMaterial.SetFloat("_GlitchProbability", settings.glitchProbability);
        scanLineMaterial.SetFloat("_WaveIntensity", settings.waveIntensity);
        scanLineMaterial.SetFloat("_WaveFrequency", settings.waveFrequency);
        scanLineMaterial.SetFloat("_WaveSpeed", settings.waveSpeed);
        scanLineMaterial.SetFloat("_BWEffect", settings.bwEffect);
        scanLineMaterial.SetFloat("_BWNoiseScale", settings.bwNoiseScale);
        scanLineMaterial.SetFloat("_BWNoiseIntensity", settings.bwNoiseIntensity);
        scanLineMaterial.SetFloat("_BWFlickerSpeed", settings.bwFlickerSpeed);
        scanLineMaterial.SetFloat("_ColorCorrection", settings.colorCorrection);
        scanLineMaterial.SetFloat("_HueShift", settings.hueShift);
        scanLineMaterial.SetFloat("_Saturation", settings.saturation);
        scanLineMaterial.SetFloat("_Brightness", settings.brightness);
        scanLineMaterial.SetFloat("_Contrast", settings.contrast);
        scanLineMaterial.SetFloat("_RedOffset", settings.redOffset);
        scanLineMaterial.SetFloat("_GreenOffset", settings.greenOffset);
        scanLineMaterial.SetFloat("_BlueOffset", settings.blueOffset);

        // 获取相机目标颜色
        scanLineJitterPass.ConfigureInput(ScriptableRenderPassInput.Color);
        scanLineJitterPass.renderPassEvent = settings.renderPassEvent;
        renderer.EnqueuePass(scanLineJitterPass);
    }

    // 应用轻微故障预设
    private void ApplySubtlePreset()
    {
        settings.jitterIntensity = 0.05f;
        settings.jitterFrequency = 5f;
        settings.scanLineThickness = 1f;
        settings.scanLineSpeed = 0.5f;
        settings.colorShiftIntensity = 0.02f;
        settings.noiseIntensity = 0.05f;
        settings.glitchProbability = 0.02f;
        settings.waveIntensity = 0.05f;
        settings.waveFrequency = 5f;
        settings.waveSpeed = 1f;
        settings.bwEffect = 0f;
        settings.colorCorrection = 0f;
    }

    // 应用中等故障预设
    private void ApplyMediumPreset()
    {
        settings.jitterIntensity = 0.1f;
        settings.jitterFrequency = 10f;
        settings.scanLineThickness = 2f;
        settings.scanLineSpeed = 1f;
        settings.colorShiftIntensity = 0.05f;
        settings.noiseIntensity = 0.1f;
        settings.glitchProbability = 0.05f;
        settings.waveIntensity = 0.1f;
        settings.waveFrequency = 10f;
        settings.waveSpeed = 2f;
        settings.bwEffect = 0f;
        settings.colorCorrection = 0f;
    }

    // 应用强烈故障预设
    private void ApplyIntensePreset()
    {
        settings.jitterIntensity = 0.2f;
        settings.jitterFrequency = 20f;
        settings.scanLineThickness = 4f;
        settings.scanLineSpeed = 2f;
        settings.colorShiftIntensity = 0.1f;
        settings.noiseIntensity = 0.2f;
        settings.glitchProbability = 0.1f;
        settings.waveIntensity = 0.25f;
        settings.waveFrequency = 15f;
        settings.waveSpeed = 3f;
        settings.bwEffect = 0f;
        settings.colorCorrection = 0f;
    }

    // 应用仅波浪效果预设
    private void ApplyWaveOnlyPreset()
    {
        settings.jitterIntensity = 0.0f;
        settings.jitterFrequency = 0f;
        settings.scanLineThickness = 0f;
        settings.scanLineSpeed = 0f;
        settings.colorShiftIntensity = 0.05f;
        settings.noiseIntensity = 0.02f;
        settings.glitchProbability = 0.0f;
        settings.waveIntensity = 0.3f;
        settings.waveFrequency = 12f;
        settings.waveSpeed = 2f;
        settings.bwEffect = 0f;
        settings.colorCorrection = 0f;
    }

    // 应用黑白效果预设
    private void ApplyBlackAndWhitePreset()
    {
        settings.jitterIntensity = 0.05f;
        settings.jitterFrequency = 5f;
        settings.scanLineThickness = 1f;
        settings.scanLineSpeed = 0.5f;
        settings.colorShiftIntensity = 0f;
        settings.noiseIntensity = 0.05f;
        settings.glitchProbability = 0.02f;
        settings.waveIntensity = 0f;
        settings.bwEffect = 1.0f;
        settings.bwNoiseScale = 20f;
        settings.bwNoiseIntensity = 0.3f;
        settings.bwFlickerSpeed = 10f;
        settings.colorCorrection = 0f;
    }

    // 应用波浪+黑白效果预设
    private void ApplyWaveBlackAndWhitePreset()
    {
        settings.jitterIntensity = 0.0f;
        settings.jitterFrequency = 0f;
        settings.scanLineThickness = 0f;
        settings.scanLineSpeed = 0f;
        settings.colorShiftIntensity = 0.02f;
        settings.noiseIntensity = 0.05f;
        settings.glitchProbability = 0.0f;
        settings.waveIntensity = 0.25f;
        settings.waveFrequency = 10f;
        settings.waveSpeed = 1.5f;
        settings.bwEffect = 0.8f;
        settings.bwNoiseScale = 15f;
        settings.bwNoiseIntensity = 0.2f;
        settings.bwFlickerSpeed = 8f;
        settings.colorCorrection = 0f;
    }

    // 应用修复粉色问题的预设
    private void ApplyFixPinkColorPreset()
    {
        // 保持其他效果设置不变
        settings.colorCorrection = 1.0f;
        settings.hueShift = -60f; // 将粉色偏移到其他色调
        settings.saturation = 0.8f; // 稍微降低饱和度
        settings.brightness = 1.0f;
        settings.contrast = 1.1f;
        settings.redOffset = -0.2f; // 减少红色
        settings.greenOffset = 0.1f; // 增加绿色
        settings.blueOffset = 0.1f; // 增加蓝色
    }

    protected override void Dispose(bool disposing)
    {
        if (scanLineJitterPass != null)
        {
            scanLineJitterPass.Cleanup();
        }

        if (scanLineMaterial != null)
        {
            CoreUtils.Destroy(scanLineMaterial);
        }
    }

    // 定义渲染通道
    class ScanLineJitterPass : ScriptableRenderPass
    {
        private Material material;
        private ScanLineJitterSettings settings;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;

        public ScanLineJitterPass(ScanLineJitterSettings settings, Material material)
        {
            this.settings = settings;
            this.material = material;
            tempTexture.Init("_TempScanLineJitterTexture");
        }

        public void Cleanup()
        {
            // 清理资源
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("ScanLineJitterEffect");

            // 获取屏幕大小
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;

            // 创建临时渲染纹理
            cmd.GetTemporaryRT(tempTexture.id, descriptor, FilterMode.Bilinear);

            // 渲染效果
            cmd.Blit(source, tempTexture.Identifier(), material, 0);
            cmd.Blit(tempTexture.Identifier(), source);

            // 执行命令
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}