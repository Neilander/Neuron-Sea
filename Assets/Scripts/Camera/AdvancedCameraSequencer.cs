using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 高级相机序列控制器
/// 支持多段序列、复杂的相机变换和自定义事件
/// </summary>
public class AdvancedCameraSequencer : MonoBehaviour
{
    [System.Serializable]
    public class SequenceStep
    {
        public enum StepType
        {
            Animation,      // 播放动画
            CameraTransition,  // 相机过渡
            Delay,          // 延时
            Event           // 触发事件
        }

        [Header("步骤基本设置")]
        public string stepName = "Step";  // 步骤名称
        public StepType type;            // 步骤类型

        [Header("动画设置")]
        [Tooltip("仅当类型为Animation时使用")]
        public Animator targetAnimator;   // 目标动画控制器
        public string animationTrigger;   // 动画触发器名称
        public float animationDuration = 1f;  // 动画持续时间
        public bool waitForAnimationComplete = true;  // 是否等待动画完成

        [Header("相机过渡设置")]
        [Tooltip("仅当类型为CameraTransition时使用")]
        public int fromPPU = 100;        // 起始PPU值
        public int toPPU = 32;           // 目标PPU值
        public float transitionDuration = 2.0f;  // 过渡持续时间
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 过渡曲线

        [Header("延时设置")]
        [Tooltip("仅当类型为Delay时使用")]
        public float delayDuration = 1.0f;  // 延时时间

        [Header("事件")]
        [Tooltip("仅当类型为Event时使用")]
        public UnityEvent customEvent;   // 自定义事件

        [Header("步骤事件")]
        public UnityEvent onStepBegin;   // 步骤开始时触发
        public UnityEvent onStepComplete;  // 步骤完成时触发
    }

    [Header("组件引用")]
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;  // 像素完美相机组件

    [Header("序列设置")]
    [SerializeField] private List<SequenceStep> sequence = new List<SequenceStep>();  // 序列步骤列表
    [SerializeField] private bool playOnStart = false;  // 是否在Start时自动播放
    [SerializeField] private float autoPlayDelay = 0f;  // 自动播放的延迟时间
    [SerializeField] private bool loopSequence = false;  // 是否循环播放序列

    [Header("序列事件")]
    [SerializeField] private UnityEvent onSequenceStart;     // 序列开始时触发
    [SerializeField] private UnityEvent onSequenceComplete;  // 序列完成时触发

    private Coroutine currentSequence;  // 当前正在执行的序列
    private bool isPlaying = false;     // 是否正在播放序列
    private int currentStepIndex = -1;  // 当前步骤索引

    private void Awake()
    {
        // 组件检查
        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();
            if (pixelPerfectCamera == null)
            {
                Debug.LogError("未找到PixelPerfectCamera组件！请确保主相机上有该组件或手动指定。");
            }
        }

        // 查找序列中的初始PPU值（如果有相机过渡步骤）
        SetInitialPPUValue();
    }

    /// <summary>
    /// 设置初始PPU值
    /// </summary>
    private void SetInitialPPUValue()
    {
        if (pixelPerfectCamera == null || sequence.Count == 0)
            return;

        // 查找第一个相机过渡步骤
        SequenceStep cameraStep = sequence.Find(step => step.type == SequenceStep.StepType.CameraTransition);
        if (cameraStep != null)
        {
            // 使用该步骤的初始PPU值
            pixelPerfectCamera.assetsPPU = cameraStep.fromPPU;
        }
    }

    private void Start()
    {
        // 自动播放
        if (playOnStart)
        {
            if (autoPlayDelay > 0)
                Invoke("PlaySequence", autoPlayDelay);
            else
                PlaySequence();
        }
    }

    /// <summary>
    /// 播放完整序列
    /// </summary>
    public void PlaySequence()
    {
        if (isPlaying)
        {
            Debug.LogWarning("已有序列正在播放！");
            return;
        }

        if (sequence.Count == 0)
        {
            Debug.LogWarning("序列为空，没有步骤可执行！");
            return;
        }

        // 确保设置了初始PPU值
        SetInitialPPUValue();

        currentSequence = StartCoroutine(PlayFullSequence());
    }

    /// <summary>
    /// 从指定步骤开始播放序列
    /// </summary>
    public void PlayFromStep(int stepIndex)
    {
        if (isPlaying)
        {
            StopSequence();
        }

        if (stepIndex < 0 || stepIndex >= sequence.Count)
        {
            Debug.LogError($"无效的步骤索引：{stepIndex}，可用范围: 0-{sequence.Count - 1}");
            return;
        }

        currentSequence = StartCoroutine(PlayFullSequence(stepIndex));
    }

    /// <summary>
    /// 从指定步骤名称开始播放序列
    /// </summary>
    public void PlayFromStepName(string stepName)
    {
        int stepIndex = sequence.FindIndex(step => step.stepName == stepName);
        if (stepIndex >= 0)
        {
            PlayFromStep(stepIndex);
        }
        else
        {
            Debug.LogError($"未找到名为 '{stepName}' 的步骤！");
        }
    }

    /// <summary>
    /// 停止当前播放的序列
    /// </summary>
    public void StopSequence()
    {
        if (currentSequence != null)
        {
            StopCoroutine(currentSequence);
            currentSequence = null;
            isPlaying = false;
            currentStepIndex = -1;
        }
    }

    /// <summary>
    /// 暂停序列播放
    /// </summary>
    public void PauseSequence()
    {
        // TODO: 实现暂停功能
        Debug.LogWarning("暂停功能尚未实现");
    }

    /// <summary>
    /// 恢复序列播放
    /// </summary>
    public void ResumeSequence()
    {
        // TODO: 实现恢复功能
        Debug.LogWarning("恢复功能尚未实现");
    }

    /// <summary>
    /// 完整序列的协程
    /// </summary>
    private IEnumerator PlayFullSequence(int startStepIndex = 0)
    {
        isPlaying = true;
        currentStepIndex = startStepIndex;

        // 触发序列开始事件
        onSequenceStart?.Invoke();

        do
        {
            // 执行序列中的所有步骤
            for (int i = startStepIndex; i < sequence.Count; i++)
            {
                currentStepIndex = i;
                SequenceStep step = sequence[i];

                // 步骤开始事件
                step.onStepBegin?.Invoke();

                // 根据步骤类型执行不同操作
                switch (step.type)
                {
                    case SequenceStep.StepType.Animation:
                        yield return ExecuteAnimationStep(step);
                        break;

                    case SequenceStep.StepType.CameraTransition:
                        yield return ExecuteCameraTransitionStep(step);
                        break;

                    case SequenceStep.StepType.Delay:
                        yield return new WaitForSeconds(step.delayDuration);
                        break;

                    case SequenceStep.StepType.Event:
                        step.customEvent?.Invoke();
                        break;
                }

                // 步骤完成事件
                step.onStepComplete?.Invoke();
            }

            // 如果循环，则重置起始步骤索引为0
            startStepIndex = 0;
        }
        while (loopSequence);

        // 触发序列完成事件
        onSequenceComplete?.Invoke();

        isPlaying = false;
        currentStepIndex = -1;
        currentSequence = null;
    }

    /// <summary>
    /// 执行动画步骤
    /// </summary>
    private IEnumerator ExecuteAnimationStep(SequenceStep step)
    {
        if (step.targetAnimator == null)
        {
            Debug.LogWarning($"步骤 '{step.stepName}' 未指定动画控制器！");
            yield break;
        }

        if (string.IsNullOrEmpty(step.animationTrigger))
        {
            Debug.LogWarning($"步骤 '{step.stepName}' 未指定动画触发器！");
            yield break;
        }

        // 触发动画
        step.targetAnimator.SetTrigger(step.animationTrigger);

        // 等待动画完成
        if (step.waitForAnimationComplete)
        {
            yield return new WaitForSeconds(step.animationDuration);
        }
    }

    /// <summary>
    /// 执行相机过渡步骤
    /// </summary>
    private IEnumerator ExecuteCameraTransitionStep(SequenceStep step)
    {
        if (pixelPerfectCamera == null)
        {
            Debug.LogError("未找到PixelPerfectCamera组件，无法执行相机过渡！");
            yield break;
        }

        // 设置初始值
        pixelPerfectCamera.assetsPPU = step.fromPPU;

        float elapsedTime = 0f;

        while (elapsedTime < step.transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / step.transitionDuration);

            // 使用动画曲线平滑过渡
            float curvedT = step.transitionCurve.Evaluate(t);

            // 计算当前PPU值
            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(step.fromPPU, step.toPPU, curvedT));

            // 应用到摄像机
            pixelPerfectCamera.assetsPPU = currentPPU;

            yield return null;
        }

        // 确保最终设置为目标值
        pixelPerfectCamera.assetsPPU = step.toPPU;
    }

    /// <summary>
    /// 获取当前步骤索引
    /// </summary>
    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }

    /// <summary>
    /// 获取序列是否正在播放
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }

    /// <summary>
    /// 添加步骤到序列
    /// </summary>
    public void AddStep(SequenceStep step)
    {
        sequence.Add(step);
    }

    /// <summary>
    /// 清除所有步骤
    /// </summary>
    public void ClearSequence()
    {
        if (isPlaying)
        {
            StopSequence();
        }
        sequence.Clear();
    }
}