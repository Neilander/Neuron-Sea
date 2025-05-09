using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 相机序列播放器
/// 控制动画播放和PPU过渡的顺序，并提供前后可自定义事件
/// </summary>
public class CameraSequencePlayer : MonoBehaviour
{
    private IEnumerator ie;
    [System.Serializable]
    public class CameraTransition
    {
        [Header("PPU过渡设置")]
        public int fromPPU = 100;    // 起始PPU值
        public int toPPU = 32;       // 目标PPU值
        public float transitionDuration = 2.0f;  // 过渡持续时间(秒)
        public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 过渡动画曲线
    }

    [Header("组件引用")]
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;  // 像素完美相机组件
    [SerializeField] private Animator animator;  // 动画控制器，用于播放动画

    [Header("序列设置")]
    [SerializeField] private string animationTriggerName = "PlayIntro";  // 动画触发器名称
    [SerializeField] private float delayBeforeTransition = 0f;  // 动画播放后、开始过渡前的延迟
    [SerializeField] private CameraTransition cameraTransition;  // 相机过渡设置
    [SerializeField] private float delayAfterTransition = 0f;  // 过渡完成后的延迟

    [Header("事件")]
    [SerializeField] private UnityEvent onSequenceStart;  // 序列开始时触发
    [SerializeField] private UnityEvent beforeTransition;  // 过渡前触发
    [SerializeField] public UnityEvent afterTransition;   // 过渡后触发
    [SerializeField] private UnityEvent onSequenceComplete;  // 序列完成时触发

    [Header("自动播放设置")]
    [SerializeField] private bool playOnStart = false;  // 是否在Start时自动播放
    [SerializeField] private float autoPlayDelay = 0f;  // 自动播放的延迟时间

    private Coroutine currentSequence;  // 当前正在执行的序列
    private bool isPlaying = false;     // 是否正在播放序列

    private IlerpCompanion lerpCompanion;

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

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("未找到Animator组件！将只执行相机过渡而不播放动画。");
            }
        }

        // 确保在开始时设置初始PPU值
        if (pixelPerfectCamera != null &&!CameraControl.Instance.hasLoadOnce&& !(PlayerPrefs.GetInt("hasLoadOnce") == 1))
        {
            pixelPerfectCamera.assetsPPU = cameraTransition.fromPPU;
        }
        lerpCompanion = GetComponent<IlerpCompanion>();
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
    /// 播放完整序列：动画 -> 延迟 -> 相机过渡 -> 延迟
    /// </summary>
    public void PlaySequence(){
        if (!(PlayerPrefs.GetInt("hasLoadOnce") == 1)){
            if (isPlaying) {
                Debug.LogWarning("已有序列正在播放！");
                return;
            }

            if (pixelPerfectCamera == null) {
                Debug.LogWarning("未找到PixelPerfectCamera组件，无法执行序列！");
                return;
            }

            // 确保PPU是起始值
            pixelPerfectCamera.assetsPPU = cameraTransition.fromPPU;
            ie = PlayFullSequence();
            currentSequence = StartCoroutine(ie);
        }
    }

    // void OnDisable(){
    //     if (ie != null)
    //         StopCoroutine(ie);
    // }
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
        }
    }

    /// <summary>
    /// 完整序列的协程
    /// </summary>
    private IEnumerator PlayFullSequence()
    {
        isPlaying = true;

        // 触发序列开始事件
        onSequenceStart?.Invoke();

        // 1. 播放动画（如果有）
        if (animator != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            animator.SetTrigger(animationTriggerName);

            // 等待动画播放完成（通过AnimationClip长度或AnimatorState.length）
            if (animator.GetCurrentAnimatorClipInfo(0).Length > 0)
            {
                float animationLength = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length;
                yield return new WaitForSeconds(animationLength);
            }
            else
            {
                // 如果无法获取动画长度，给个默认等待时间
                yield return new WaitForSeconds(1.0f);
            }
        }

        // 过渡前延迟
        if (delayBeforeTransition > 0)
            yield return new WaitForSeconds(delayBeforeTransition);

        // 触发过渡前事件
        beforeTransition?.Invoke();

        // 2. 执行相机PPU过渡
        yield return StartCoroutine(TransitionPPU(
            cameraTransition.fromPPU,
            cameraTransition.toPPU,
            cameraTransition.transitionDuration,
            cameraTransition.transitionCurve,
            lerpCompanion
        ));

        // 触发过渡后事件
        afterTransition?.Invoke();

        // 过渡后延迟
        if (delayAfterTransition > 0)
            yield return new WaitForSeconds(delayAfterTransition);

        // 触发序列完成事件
        onSequenceComplete?.Invoke();

        isPlaying = false;
        currentSequence = null;
    }

    /// <summary>
    /// 平滑过渡PPU值的协程
    /// </summary>
    private IEnumerator TransitionPPU(int startPPU, int targetPPU, float duration, AnimationCurve curve,IlerpCompanion cp)
    {
        // 确保起始值正确
        pixelPerfectCamera.assetsPPU = startPPU;

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            Debug.Log("正在进行pputransition");
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // 使用动画曲线平滑过渡
            float curvedT = curve.Evaluate(t);

            // 计算当前PPU值
            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(startPPU, targetPPU, curvedT));

            // 应用到摄像机
            pixelPerfectCamera.assetsPPU = currentPPU;
            if (cp != null) cp.DoWhenLerp(t);
            yield return null;
        }

        // 确保最终设置为目标值
        pixelPerfectCamera.assetsPPU = targetPPU;
        if (cp != null) cp.DoWhenLerp(1);
    }

    /// <summary>
    /// 立即设置PPU值
    /// </summary>
    public void SetPPU(int ppu)
    {
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = ppu;
        }
    }

    /// <summary>
    /// 设置动画触发器名称
    /// </summary>
    public void SetAnimationTrigger(string triggerName)
    {
        animationTriggerName = triggerName;
    }

    /// <summary>
    /// 获取序列是否正在播放
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
}

public interface IlerpCompanion
{

    void DoWhenLerp(float t);
}