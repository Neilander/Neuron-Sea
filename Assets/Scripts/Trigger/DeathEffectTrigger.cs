using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffectTrigger : MonoBehaviour
{
    private ControlEffects controlEffects;
    private bool isEffectActive = false;

    [Header("效果设置")]
    [SerializeField] private float colorCorrectionPreDelay = 0.3f; // 颜色校正先变化的时间
    [SerializeField] private float transitionDuration = 0.5f; // 过渡时间
    [SerializeField] private float effectDuration = 1.0f; // 效果持续时间
    [SerializeField] private float colorCorrectionRecoveryDelay = 3.0f; // 颜色校正和饱和度恢复前的等待时间

    [System.Serializable]
    public class EffectParameters
    {
        [Header("扫描线参数")]
        public float jitterIntensity = 0f;
        public float jitterFrequency = 0f;
        public float scanLineThickness = 0f;
        public float scanLineSpeed = 0f;

        [Header("颜色和噪点参数")]
        public float colorShiftIntensity = 0f;
        public float noiseIntensity = 0f;
        public float glitchProbability = 0f;

        [Header("波浪效果参数")]
        public float waveIntensity = 0f;
        public float waveFrequency = 0f;
        public float waveSpeed = 0f;

        [Header("黑白效果参数")]
        public float bwEffect = 0f;
        public float bwNoiseScale = 0f;
        public float bwNoiseIntensity = 0f;
        public float bwFlickerSpeed = 0f;

        [Header("颜色校正参数")]
        public float colorCorrection = 0f;
        public float hueShift = 0f;
        public float saturation = 1f; // 默认值为1，避免变绿
        public float brightness = 1f;
        public float contrast = 1f;
        public float redOffset = 0f;
        public float greenOffset = 0f;
        public float blueOffset = 0f;

        [Header("效果开关")]
        public bool enableScanLineJitter = false;
        public bool enableColorShift = false;
        public bool enableNoise = false;
        public bool enableGlitch = false;
        public bool enableWaveEffect = false;
        public bool enableBlackAndWhite = false;
    }

    // 保存原始值的备份
    private EffectParameters originalValues;

    [Header("初始效果参数")]
    [SerializeField]
    private EffectParameters initialValues = new EffectParameters
    {
        // 确保默认饱和度为1
        saturation = 1f,
        brightness = 1f,
        contrast = 0f
    };

    [Header("结束效果参数")]
    [SerializeField]
    private EffectParameters targetValues = new EffectParameters
    {
        jitterIntensity = 0.195f,
        jitterFrequency = 64.5f,
        scanLineThickness = 0f,
        scanLineSpeed = 4.2f,
        colorShiftIntensity = 0f,
        noiseIntensity = 0.172f,
        glitchProbability = 1f,
        waveIntensity = 0f,
        waveFrequency = 27f,
        waveSpeed = 10f,
        bwEffect = 0f,
        bwNoiseScale = 15f,
        bwNoiseIntensity = 0.2f,
        bwFlickerSpeed = 8f,
        colorCorrection = 1f,
        hueShift = 0f,
        saturation = 0f,
        brightness = 1f,
        contrast = 1f,
        redOffset = 0f,
        greenOffset = 0f,
        blueOffset = 0f,
        enableScanLineJitter = true,
        enableColorShift = true,
        enableNoise = true,
        enableGlitch = true,
        enableWaveEffect = true,
        enableBlackAndWhite = true
    };

    private void Start()
    {
        // 获取ControlEffects组件
        controlEffects = FindObjectOfType<ControlEffects>();
        if (controlEffects == null)
        {
            Debug.LogError("场景中没有找到ControlEffects组件！");
        }
        else
        {
            // 创建原始值的备份
            originalValues = new EffectParameters();
            BackupOriginalValues();

            // 确保初始状态是正确的
            ApplyParameters(initialValues);

            // 确保ScanLineJitterFeature是禁用状态
            controlEffects.DisableScanLineJitterFeature();
        }
    }

    // 备份原始值
    private void BackupOriginalValues()
    {
        if (controlEffects == null) return;

        originalValues.jitterIntensity = controlEffects.jitterIntensity;
        originalValues.jitterFrequency = controlEffects.jitterFrequency;
        originalValues.scanLineThickness = controlEffects.scanLineThickness;
        originalValues.scanLineSpeed = controlEffects.scanLineSpeed;
        originalValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        originalValues.noiseIntensity = controlEffects.noiseIntensity;
        originalValues.glitchProbability = controlEffects.glitchProbability;
        originalValues.waveIntensity = controlEffects.waveIntensity;
        originalValues.waveFrequency = controlEffects.waveFrequency;
        originalValues.waveSpeed = controlEffects.waveSpeed;
        originalValues.bwEffect = controlEffects.bwEffect;
        originalValues.bwNoiseScale = controlEffects.bwNoiseScale;
        originalValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        originalValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        originalValues.colorCorrection = controlEffects.colorCorrection;
        originalValues.hueShift = controlEffects.hueShift;
        originalValues.saturation = controlEffects.saturation;
        originalValues.brightness = controlEffects.brightness;
        originalValues.contrast = controlEffects.contrast;
        originalValues.redOffset = controlEffects.redOffset;
        originalValues.greenOffset = controlEffects.greenOffset;
        originalValues.blueOffset = controlEffects.blueOffset;
        originalValues.enableScanLineJitter = controlEffects.enableScanLineJitter;
        originalValues.enableColorShift = controlEffects.enableColorShift;
        originalValues.enableNoise = controlEffects.enableNoise;
        originalValues.enableGlitch = controlEffects.enableGlitch;
        originalValues.enableWaveEffect = controlEffects.enableWaveEffect;
        originalValues.enableBlackAndWhite = controlEffects.enableBlackAndWhite;

        // 确保饱和度不会变成0(这会导致图像变绿)
        if (originalValues.saturation <= 0.01f)
            originalValues.saturation = 1.0f;

        Debug.Log($"备份的原始饱和度值: {originalValues.saturation}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("碰到死亡效果触发器了！");
            if (!isEffectActive && controlEffects != null)
            {
                isEffectActive = true;
                StartCoroutine(ApplyDeathEffectWithTransition());
            }
        }
    }

    private void ApplyParameters(EffectParameters parameters)
    {
        if (controlEffects == null) return;

        // 设置所有参数
        controlEffects.jitterIntensity = parameters.jitterIntensity;
        controlEffects.jitterFrequency = parameters.jitterFrequency;
        controlEffects.scanLineThickness = parameters.scanLineThickness;
        controlEffects.scanLineSpeed = parameters.scanLineSpeed;
        controlEffects.colorShiftIntensity = parameters.colorShiftIntensity;
        controlEffects.noiseIntensity = parameters.noiseIntensity;
        controlEffects.glitchProbability = parameters.glitchProbability;

        controlEffects.waveIntensity = parameters.waveIntensity;
        controlEffects.waveFrequency = parameters.waveFrequency;
        controlEffects.waveSpeed = parameters.waveSpeed;

        controlEffects.bwEffect = parameters.bwEffect;
        controlEffects.bwNoiseScale = parameters.bwNoiseScale;
        controlEffects.bwNoiseIntensity = parameters.bwNoiseIntensity;
        controlEffects.bwFlickerSpeed = parameters.bwFlickerSpeed;

        controlEffects.colorCorrection = parameters.colorCorrection;
        controlEffects.hueShift = parameters.hueShift;
        controlEffects.saturation = parameters.saturation;
        controlEffects.brightness = parameters.brightness;
        controlEffects.contrast = parameters.contrast;
        controlEffects.redOffset = parameters.redOffset;
        controlEffects.greenOffset = parameters.greenOffset;
        controlEffects.blueOffset = parameters.blueOffset;

        // 设置所有开关
        controlEffects.enableScanLineJitter = parameters.enableScanLineJitter;
        controlEffects.enableColorShift = parameters.enableColorShift;
        controlEffects.enableNoise = parameters.enableNoise;
        controlEffects.enableGlitch = parameters.enableGlitch;
        controlEffects.enableWaveEffect = parameters.enableWaveEffect;
        controlEffects.enableBlackAndWhite = parameters.enableBlackAndWhite;
    }

    private IEnumerator ApplyDeathEffectWithTransition()
    {
        Debug.Log("开始应用死亡特效...");

        // 先启用ScanLineJitterFeature特性
        controlEffects.EnableScanLineJitterFeature();

        // 检查特性是否成功启用
        if (!controlEffects.IsFeatureActive())
        {
            Debug.LogError("无法启用ScanLineJitterFeature特性！特效可能无法正常显示");
        }
        else
        {
            Debug.Log("ScanLineJitterFeature特性已启用");
        }

        // 强制立即更新一次特效参数
        controlEffects.ForceUpdateEffects();

        // 记录开始时的参数
        EffectParameters startValues = new EffectParameters();
        startValues.jitterIntensity = controlEffects.jitterIntensity;
        startValues.jitterFrequency = controlEffects.jitterFrequency;
        startValues.scanLineThickness = controlEffects.scanLineThickness;
        startValues.scanLineSpeed = controlEffects.scanLineSpeed;
        startValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        startValues.noiseIntensity = controlEffects.noiseIntensity;
        startValues.glitchProbability = controlEffects.glitchProbability;
        startValues.waveIntensity = controlEffects.waveIntensity;
        startValues.waveFrequency = controlEffects.waveFrequency;
        startValues.waveSpeed = controlEffects.waveSpeed;
        startValues.bwEffect = controlEffects.bwEffect;
        startValues.bwNoiseScale = controlEffects.bwNoiseScale;
        startValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        startValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        startValues.colorCorrection = controlEffects.colorCorrection;
        startValues.hueShift = controlEffects.hueShift;
        startValues.saturation = controlEffects.saturation;
        startValues.brightness = controlEffects.brightness;
        startValues.contrast = controlEffects.contrast;
        startValues.redOffset = controlEffects.redOffset;
        startValues.greenOffset = controlEffects.greenOffset;
        startValues.blueOffset = controlEffects.blueOffset;

        // 启用所有需要的效果
        controlEffects.enableScanLineJitter = targetValues.enableScanLineJitter;
        controlEffects.enableColorShift = targetValues.enableColorShift;
        controlEffects.enableNoise = targetValues.enableNoise;
        controlEffects.enableGlitch = targetValues.enableGlitch;
        controlEffects.enableWaveEffect = targetValues.enableWaveEffect;
        controlEffects.enableBlackAndWhite = targetValues.enableBlackAndWhite;

        // 立即设置glitchProbability为目标值
        controlEffects.glitchProbability = targetValues.glitchProbability;

        // 第一步：先只变化颜色校正参数和饱和度
        float elapsedTime = 0;
        float initialColorCorrection = startValues.colorCorrection;
        float initialSaturation = startValues.saturation;

        while (elapsedTime < colorCorrectionPreDelay)
        {
            float t = elapsedTime / colorCorrectionPreDelay; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 只变化颜色校正参数和饱和度
            controlEffects.colorCorrection = Mathf.Lerp(initialColorCorrection, targetValues.colorCorrection, smoothT);
            controlEffects.saturation = Mathf.Lerp(initialSaturation, targetValues.saturation, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保颜色校正和饱和度达到目标值
        controlEffects.colorCorrection = targetValues.colorCorrection;
        controlEffects.saturation = targetValues.saturation;
        Debug.Log($"第一阶段完成：颜色校正={controlEffects.colorCorrection}, 饱和度={controlEffects.saturation}");

        // 第二步：开始其他所有参数的过渡
        elapsedTime = 0;
        Debug.Log("开始第二阶段：其他参数的过渡");

        // 保存颜色校正已经完成后的当前状态
        EffectParameters currentValues = new EffectParameters();
        currentValues.jitterIntensity = controlEffects.jitterIntensity;
        currentValues.jitterFrequency = controlEffects.jitterFrequency;
        currentValues.scanLineThickness = controlEffects.scanLineThickness;
        currentValues.scanLineSpeed = controlEffects.scanLineSpeed;
        currentValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        currentValues.noiseIntensity = controlEffects.noiseIntensity;
        currentValues.glitchProbability = controlEffects.glitchProbability;
        currentValues.waveIntensity = controlEffects.waveIntensity;
        currentValues.waveFrequency = controlEffects.waveFrequency;
        currentValues.waveSpeed = controlEffects.waveSpeed;
        currentValues.bwEffect = controlEffects.bwEffect;
        currentValues.bwNoiseScale = controlEffects.bwNoiseScale;
        currentValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        currentValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        currentValues.colorCorrection = controlEffects.colorCorrection; // 已经是目标值
        currentValues.hueShift = controlEffects.hueShift;
        currentValues.saturation = controlEffects.saturation;
        currentValues.brightness = controlEffects.brightness;
        currentValues.contrast = controlEffects.contrast;
        currentValues.redOffset = controlEffects.redOffset;
        currentValues.greenOffset = controlEffects.greenOffset;
        currentValues.blueOffset = controlEffects.blueOffset;

        // 开始其他效果的过渡
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 平滑插值所有其他参数，保持颜色校正不变
            controlEffects.jitterIntensity = Mathf.Lerp(currentValues.jitterIntensity, targetValues.jitterIntensity, smoothT);
            controlEffects.jitterFrequency = Mathf.Lerp(currentValues.jitterFrequency, targetValues.jitterFrequency, smoothT);
            controlEffects.scanLineThickness = Mathf.Lerp(currentValues.scanLineThickness, targetValues.scanLineThickness, smoothT);
            controlEffects.scanLineSpeed = Mathf.Lerp(currentValues.scanLineSpeed, targetValues.scanLineSpeed, smoothT);
            controlEffects.colorShiftIntensity = Mathf.Lerp(currentValues.colorShiftIntensity, targetValues.colorShiftIntensity, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(currentValues.noiseIntensity, targetValues.noiseIntensity, smoothT);
            // glitchProbability直接设置为目标值，不进行平滑过渡
            controlEffects.glitchProbability = targetValues.glitchProbability;
            controlEffects.waveIntensity = Mathf.Lerp(currentValues.waveIntensity, targetValues.waveIntensity, smoothT);
            controlEffects.waveFrequency = Mathf.Lerp(currentValues.waveFrequency, targetValues.waveFrequency, smoothT);
            controlEffects.waveSpeed = Mathf.Lerp(currentValues.waveSpeed, targetValues.waveSpeed, smoothT);
            controlEffects.bwEffect = Mathf.Lerp(currentValues.bwEffect, targetValues.bwEffect, smoothT);
            controlEffects.bwNoiseScale = Mathf.Lerp(currentValues.bwNoiseScale, targetValues.bwNoiseScale, smoothT);
            controlEffects.bwNoiseIntensity = Mathf.Lerp(currentValues.bwNoiseIntensity, targetValues.bwNoiseIntensity, smoothT);
            controlEffects.bwFlickerSpeed = Mathf.Lerp(currentValues.bwFlickerSpeed, targetValues.bwFlickerSpeed, smoothT);
            // 颜色校正已经设置好了，不需要再变化
            controlEffects.hueShift = Mathf.Lerp(currentValues.hueShift, targetValues.hueShift, smoothT);
            controlEffects.saturation = Mathf.Lerp(currentValues.saturation, targetValues.saturation, smoothT);
            controlEffects.brightness = Mathf.Lerp(currentValues.brightness, targetValues.brightness, smoothT);
            controlEffects.contrast = Mathf.Lerp(currentValues.contrast, targetValues.contrast, smoothT);
            controlEffects.redOffset = Mathf.Lerp(currentValues.redOffset, targetValues.redOffset, smoothT);
            controlEffects.greenOffset = Mathf.Lerp(currentValues.greenOffset, targetValues.greenOffset, smoothT);
            controlEffects.blueOffset = Mathf.Lerp(currentValues.blueOffset, targetValues.blueOffset, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保达到精确的目标值
        ApplyParameters(targetValues);
        Debug.Log("已应用目标值，所有参数过渡完成");

        // 保持效果一段时间
        Debug.Log($"效果将保持 {effectDuration} 秒");
        yield return new WaitForSeconds(effectDuration);
        Debug.Log("开始恢复参数");

        // 平滑过渡回初始值
        // 先恢复除颜色校正外的所有参数
        elapsedTime = 0;

        // 记录当前参数(目标效果值)
        EffectParameters endValues = new EffectParameters();
        endValues.jitterIntensity = controlEffects.jitterIntensity;
        endValues.jitterFrequency = controlEffects.jitterFrequency;
        endValues.scanLineThickness = controlEffects.scanLineThickness;
        endValues.scanLineSpeed = controlEffects.scanLineSpeed;
        endValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        endValues.noiseIntensity = controlEffects.noiseIntensity;
        endValues.glitchProbability = controlEffects.glitchProbability;
        endValues.waveIntensity = controlEffects.waveIntensity;
        endValues.waveFrequency = controlEffects.waveFrequency;
        endValues.waveSpeed = controlEffects.waveSpeed;
        endValues.bwEffect = controlEffects.bwEffect;
        endValues.bwNoiseScale = controlEffects.bwNoiseScale;
        endValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        endValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        endValues.colorCorrection = controlEffects.colorCorrection;
        endValues.hueShift = controlEffects.hueShift;
        endValues.saturation = controlEffects.saturation;
        endValues.brightness = controlEffects.brightness;
        endValues.contrast = controlEffects.contrast;
        endValues.redOffset = controlEffects.redOffset;
        endValues.greenOffset = controlEffects.greenOffset;
        endValues.blueOffset = controlEffects.blueOffset;

        // 使用备份的原始值而不是初始值
        while (elapsedTime < transitionDuration)
        {
            float t = elapsedTime / transitionDuration; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 平滑插值所有参数回到原始值，但保持颜色校正和饱和度不变
            controlEffects.jitterIntensity = Mathf.Lerp(endValues.jitterIntensity, originalValues.jitterIntensity, smoothT);
            controlEffects.jitterFrequency = Mathf.Lerp(endValues.jitterFrequency, originalValues.jitterFrequency, smoothT);
            controlEffects.scanLineThickness = Mathf.Lerp(endValues.scanLineThickness, originalValues.scanLineThickness, smoothT);
            controlEffects.scanLineSpeed = Mathf.Lerp(endValues.scanLineSpeed, originalValues.scanLineSpeed, smoothT);
            controlEffects.colorShiftIntensity = Mathf.Lerp(endValues.colorShiftIntensity, originalValues.colorShiftIntensity, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(endValues.noiseIntensity, originalValues.noiseIntensity, smoothT);
            // glitchProbability直接恢复为原始值，不进行平滑过渡
            controlEffects.glitchProbability = originalValues.glitchProbability;
            controlEffects.waveIntensity = Mathf.Lerp(endValues.waveIntensity, originalValues.waveIntensity, smoothT);
            controlEffects.waveFrequency = Mathf.Lerp(endValues.waveFrequency, originalValues.waveFrequency, smoothT);
            controlEffects.waveSpeed = Mathf.Lerp(endValues.waveSpeed, originalValues.waveSpeed, smoothT);
            controlEffects.bwEffect = Mathf.Lerp(endValues.bwEffect, originalValues.bwEffect, smoothT);
            controlEffects.bwNoiseScale = Mathf.Lerp(endValues.bwNoiseScale, originalValues.bwNoiseScale, smoothT);
            controlEffects.bwNoiseIntensity = Mathf.Lerp(endValues.bwNoiseIntensity, originalValues.bwNoiseIntensity, smoothT);
            controlEffects.bwFlickerSpeed = Mathf.Lerp(endValues.bwFlickerSpeed, originalValues.bwFlickerSpeed, smoothT);
            // 颜色校正和饱和度保持不变
            controlEffects.hueShift = Mathf.Lerp(endValues.hueShift, originalValues.hueShift, smoothT);
            controlEffects.brightness = Mathf.Lerp(endValues.brightness, originalValues.brightness, smoothT);
            controlEffects.contrast = Mathf.Lerp(endValues.contrast, originalValues.contrast, smoothT);
            controlEffects.redOffset = Mathf.Lerp(endValues.redOffset, originalValues.redOffset, smoothT);
            controlEffects.greenOffset = Mathf.Lerp(endValues.greenOffset, originalValues.greenOffset, smoothT);
            controlEffects.blueOffset = Mathf.Lerp(endValues.blueOffset, originalValues.blueOffset, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 恢复其他参数后，等待指定时间
        Debug.Log($"所有其他参数已恢复，颜色校正和饱和度将等待 {colorCorrectionRecoveryDelay} 秒后恢复");
        yield return new WaitForSeconds(colorCorrectionRecoveryDelay);
        Debug.Log("开始恢复颜色校正和饱和度");

        // 最后再平滑恢复颜色校正和饱和度
        elapsedTime = 0;
        float finalColorCorrection = controlEffects.colorCorrection;
        float finalSaturation = controlEffects.saturation;

        // 确保目标饱和度至少为1，避免绿色效果
        float targetSaturation = originalValues.saturation <= 0.01f ? 1.0f : originalValues.saturation;

        while (elapsedTime < colorCorrectionPreDelay)
        {
            float t = elapsedTime / colorCorrectionPreDelay; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 同时平滑变化颜色校正和饱和度参数
            float targetCorrection = originalValues.colorCorrection;
            controlEffects.colorCorrection = Mathf.Lerp(finalColorCorrection, targetCorrection, smoothT);
            controlEffects.saturation = Mathf.Lerp(finalSaturation, targetSaturation, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保颜色校正和饱和度达到目标值
        controlEffects.colorCorrection = originalValues.colorCorrection;
        controlEffects.saturation = targetSaturation;

        // 完全恢复到原始状态（开关设置）
        controlEffects.enableScanLineJitter = originalValues.enableScanLineJitter;
        controlEffects.enableColorShift = originalValues.enableColorShift;
        controlEffects.enableNoise = originalValues.enableNoise;
        controlEffects.enableGlitch = originalValues.enableGlitch;
        controlEffects.enableWaveEffect = originalValues.enableWaveEffect;
        controlEffects.enableBlackAndWhite = originalValues.enableBlackAndWhite;

        Debug.Log($"恢复后的饱和度值: {controlEffects.saturation}");

        // 禁用ScanLineJitterFeature特性
        controlEffects.DisableScanLineJitterFeature();
        Debug.Log("特效已完全禁用，效果结束");

        isEffectActive = false;
    }
}