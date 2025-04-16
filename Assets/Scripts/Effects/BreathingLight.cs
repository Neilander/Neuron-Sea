using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 呼吸灯效果脚本
/// 将此脚本挂载到带有Light2D组件的游戏对象上，可以实现灯光强度的周期性变化，形成呼吸效果
/// </summary>
public class BreathingLight : MonoBehaviour
{
    public enum BreathingMode
    {
        Simple,     // 简单的线性呼吸
        SmoothStep, // 平滑的渐进呼吸
        Curve       // 曲线控制的呼吸
    }

    [Header("呼吸效果设置")]
    [Tooltip("最小亮度值")]
    [Range(0f, 10f)]
    public float minIntensity = 0.5f;

    [Tooltip("最大亮度值")]
    [Range(0f, 10f)]
    public float maxIntensity = 1.5f;

    [Tooltip("完成一次呼吸周期的时间(秒)")]
    [Range(0.1f, 10f)]
    public float breathingCycle = 2.0f;

    [Tooltip("呼吸模式")]
    public BreathingMode breathingMode = BreathingMode.SmoothStep;

    [Tooltip("呼吸曲线，控制亮度变化速率（仅在Curve模式下使用）")]
    public AnimationCurve breathingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("亮度上升时间占比（0-1之间，仅在Simple和SmoothStep模式下使用）")]
    [Range(0.1f, 0.9f)]
    public float riseDuration = 0.5f;

    [Tooltip("亮度下降时间占比（0-1之间，仅在Simple和SmoothStep模式下使用）")]
    [Range(0.1f, 0.9f)]
    public float fallDuration = 0.5f;

    [Header("颜色变化设置")]
    [Tooltip("是否启用颜色变化")]
    public bool enableColorChange = false;

    [Tooltip("呼吸过程中的起始颜色")]
    public Color startColor = Color.white;

    [Tooltip("呼吸过程中的结束颜色")]
    public Color endColor = new Color(1f, 0.8f, 0.6f, 1f);

    [Header("平滑恢复设置")]
    [Tooltip("恢复到原始状态时的过渡时间(秒)")]
    [Range(0.1f, 5f)]
    public float resetTransitionTime = 1.0f;

    // Light2D组件引用
    private Light2D lightComponent;

    // 原始强度值
    private float originalIntensity;
    // 原始颜色
    private Color originalColor;

    // 计时器
    private float timer = 0f;

    // 是否正在平滑过渡
    private bool isTransitioning = false;
    // 过渡计时器
    private float transitionTimer = 0f;
    // 过渡起始强度
    private float transitionStartIntensity;
    // 过渡起始颜色
    private Color transitionStartColor;
    // 过渡目标强度
    private float transitionTargetIntensity;
    // 过渡目标颜色
    private Color transitionTargetColor;

    private void Awake()
    {
        // 获取Light2D组件
        lightComponent = GetComponent<Light2D>();

        if (lightComponent == null)
        {
            Debug.LogError("未找到Light2D组件！请将此脚本挂载到带有Light2D组件的游戏对象上。");
            enabled = false;
            return;
        }

        // 保存原始值
        originalIntensity = lightComponent.intensity;
        originalColor = lightComponent.color;
    }

    private void OnEnable()
    {
        // 重置计时器
        timer = 0f;
    }

    private void Update()
    {
        // 如果正在执行过渡
        if (isTransitioning)
        {
            HandleTransition();
            return;
        }

        if (lightComponent == null) return;

        // 更新计时器
        timer += Time.deltaTime;
        if (timer > breathingCycle)
        {
            timer %= breathingCycle;
        }

        // 计算当前周期的进度(0-1)
        float progress = timer / breathingCycle;

        // 根据不同的呼吸模式计算强度
        float intensityValue = 0;

        switch (breathingMode)
        {
            case BreathingMode.Simple:
                // 简单线性变化，分为上升和下降两个阶段
                if (progress < riseDuration)
                {
                    // 上升阶段
                    intensityValue = Mathf.Lerp(minIntensity, maxIntensity, progress / riseDuration);
                }
                else
                {
                    // 下降阶段
                    float fallProgress = (progress - riseDuration) / fallDuration;
                    intensityValue = Mathf.Lerp(maxIntensity, minIntensity, Mathf.Clamp01(fallProgress));
                }
                break;

            case BreathingMode.SmoothStep:
                // 使用SmoothStep实现平滑渐进的变化
                if (progress < riseDuration)
                {
                    // 上升阶段（平滑）
                    float riseProgress = progress / riseDuration;
                    intensityValue = Mathf.Lerp(minIntensity, maxIntensity, Mathf.SmoothStep(0, 1, riseProgress));
                }
                else
                {
                    // 下降阶段（平滑）
                    float fallProgress = (progress - riseDuration) / fallDuration;
                    intensityValue = Mathf.Lerp(maxIntensity, minIntensity, Mathf.SmoothStep(0, 1, Mathf.Clamp01(fallProgress)));
                }
                break;

            case BreathingMode.Curve:
                // 使用自定义曲线
                intensityValue = Mathf.Lerp(minIntensity, maxIntensity, breathingCurve.Evaluate(progress));
                break;
        }

        // 应用强度
        lightComponent.intensity = intensityValue;

        // 如果启用了颜色变化，则同时变化颜色
        if (enableColorChange)
        {
            // 根据强度的变化比例计算颜色
            float colorProgress = (intensityValue - minIntensity) / (maxIntensity - minIntensity);
            lightComponent.color = Color.Lerp(startColor, endColor, colorProgress);
        }
    }

    // 处理平滑过渡
    private void HandleTransition()
    {
        if (lightComponent == null)
        {
            isTransitioning = false;
            return;
        }

        // 更新过渡计时器
        transitionTimer += Time.deltaTime;

        // 计算过渡进度
        float progress = Mathf.Clamp01(transitionTimer / resetTransitionTime);

        // 使用平滑曲线
        float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

        // 平滑过渡强度
        lightComponent.intensity = Mathf.Lerp(transitionStartIntensity, transitionTargetIntensity, smoothProgress);

        // 平滑过渡颜色
        lightComponent.color = Color.Lerp(transitionStartColor, transitionTargetColor, smoothProgress);

        // 过渡完成
        if (progress >= 1f)
        {
            isTransitioning = false;

            // 如果要回到呼吸状态，需要重新启用呼吸效果
            timer = 0f;
            enabled = true;
        }
    }

    // 恢复到原始状态(平滑过渡)
    public void ResetToOriginal()
    {
        if (lightComponent == null) return;

        // 开始平滑过渡
        isTransitioning = true;
        transitionTimer = 0f;

        // 记录当前值作为起点
        transitionStartIntensity = lightComponent.intensity;
        transitionStartColor = lightComponent.color;

        // 设置目标值
        transitionTargetIntensity = originalIntensity;
        transitionTargetColor = originalColor;

        // 确保组件启用以处理过渡
        this.enabled = true;
    }

    // 暂停呼吸效果
    public void PauseBreathing()
    {
        if (!isTransitioning)
        {
            enabled = false;
        }
    }

    // 恢复呼吸效果(平滑过渡)
    public void ResumeBreathing()
    {
        if (isTransitioning)
        {
            // 如果正在过渡，等待过渡完成
            return;
        }

        // 如果当前不在呼吸状态，开始平滑过渡到呼吸状态
        if (!enabled)
        {
            // 开始平滑过渡
            isTransitioning = true;
            transitionTimer = 0f;

            // 记录当前值作为起点
            transitionStartIntensity = lightComponent.intensity;
            transitionStartColor = lightComponent.color;

            // 计算呼吸周期中点的强度作为目标
            float midCycleIntensity = (minIntensity + maxIntensity) / 2f;
            transitionTargetIntensity = midCycleIntensity;

            // 计算呼吸周期中点的颜色作为目标
            if (enableColorChange)
            {
                transitionTargetColor = Color.Lerp(startColor, endColor, 0.5f);
            }
            else
            {
                transitionTargetColor = lightComponent.color;
            }

            // 启用Update来处理过渡
            this.enabled = true;
        }
        else
        {
            // 已经在呼吸状态，只需重置计时器
            timer = 0f;
        }
    }

    private void OnValidate()
    {
        // 确保上升和下降时间总和不超过1
        float total = riseDuration + fallDuration;
        if (total > 1.0f)
        {
            // 按比例缩减
            riseDuration = riseDuration / total;
            fallDuration = fallDuration / total;
        }
    }
}