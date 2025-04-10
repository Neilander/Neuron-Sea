using UnityEngine;

public class GlitchEffectController : MonoBehaviour
{
    [Header("参考")]
    [Tooltip("要控制的故障效果组件")]
    public ScanLineJitterEffect glitchEffect;

    [Header("触发设置")]
    [Tooltip("按下此键触发临时故障")]
    public KeyCode triggerKey = KeyCode.G;

    [Tooltip("按下此键切换故障效果开关")]
    public KeyCode toggleKey = KeyCode.T;

    [Tooltip("按下此键循环切换预设")]
    public KeyCode cyclePresetKey = KeyCode.Y;

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
    private float originalJitterIntensity;
    private float originalGlitchProbability;

    private void Start()
    {
        // 如果没有指定故障效果组件，尝试从摄像机获取
        if (glitchEffect == null)
        {
            glitchEffect = Camera.main.GetComponent<ScanLineJitterEffect>();

            if (glitchEffect == null)
            {
                Debug.LogWarning("未找到ScanLineJitterEffect组件。请手动指定或添加到主摄像机上。");
                enabled = false;
                return;
            }
        }

        // 保存原始参数值
        originalJitterIntensity = glitchEffect.jitterIntensity;
        originalGlitchProbability = glitchEffect.glitchProbability;
    }

    private void Update()
    {
        // 检查临时故障状态
        if (isTemporaryGlitchActive)
        {
            glitchTimer -= Time.deltaTime;

            if (glitchTimer <= 0)
            {
                // 恢复原始参数值
                glitchEffect.jitterIntensity = originalJitterIntensity;
                glitchEffect.glitchProbability = originalGlitchProbability;
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
            glitchEffect.enabled = !glitchEffect.enabled;
            Debug.Log("故障效果已" + (glitchEffect.enabled ? "启用" : "禁用"));
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
        if (glitchEffect == null) return;

        // 保存原始参数值（如果不是已经在临时故障中）
        if (!isTemporaryGlitchActive)
        {
            originalJitterIntensity = glitchEffect.jitterIntensity;
            originalGlitchProbability = glitchEffect.glitchProbability;
        }

        // 增强故障效果
        glitchEffect.jitterIntensity = temporaryGlitchIntensity;
        glitchEffect.glitchProbability = temporaryGlitchIntensity * 0.5f;

        // 设置定时器
        glitchTimer = glitchDuration;
        isTemporaryGlitchActive = true;

        Debug.Log("触发临时故障效果，持续" + glitchDuration + "秒");
    }

    // 循环切换预设
    public void CyclePreset()
    {
        if (glitchEffect == null) return;

        currentPreset = (currentPreset + 1) % 3;

        switch (currentPreset)
        {
            case 0:
                glitchEffect.subtleGlitchPreset = true;
                Debug.Log("应用轻微故障预设");
                break;
            case 1:
                glitchEffect.mediumGlitchPreset = true;
                Debug.Log("应用中等故障预设");
                break;
            case 2:
                glitchEffect.intenseGlitchPreset = true;
                Debug.Log("应用强烈故障预设");
                break;
        }
    }
}