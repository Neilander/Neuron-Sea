using System.Collections;
using UnityEngine;

/// <summary>
/// 通用PPU过渡效果 - 兼容不同Unity版本的PixelPerfectCamera
/// 在场景加载时将PixelPerfectCamera的AssetsPPU从起始值平滑过渡到目标值
/// </summary>
public class UniversalPPUTransition : MonoBehaviour
{
    [Header("过渡设置")]
    [SerializeField] private int startPPU = 100;  // 起始PPU值
    [SerializeField] private int targetPPU = 32;  // 目标PPU值
    [SerializeField] private float transitionDuration = 2.0f;  // 过渡持续时间(秒)
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 过渡动画曲线
    [SerializeField] private bool autoStartTransition = true;  // 是否自动开始过渡

    [Header("组件引用")]
    [SerializeField] private Camera targetCamera;  // 目标相机
    
    // 像素完美相机适配器
    private PixelPerfectAdaptor pixelPerfectAdaptor;
    private Coroutine currentTransition;

    private void Awake()
    {
        // 如果未指定相机，使用主相机
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("未找到相机！请手动指定目标相机。");
                enabled = false;
                return;
            }
        }

        // 获取或创建适配器
        pixelPerfectAdaptor = PixelPerfectAdaptor.CreateAdaptor(targetCamera);
        
        if (pixelPerfectAdaptor == null || !pixelPerfectAdaptor.IsValid())
        {
            Debug.LogError("无法创建或初始化PixelPerfectAdaptor！请确保相机上有PixelPerfectCamera组件。");
            enabled = false;
            return;
        }

        // 设置初始PPU值
        pixelPerfectAdaptor.SetAssetsPPU(startPPU);
    }

    private void Start()
    {
        // 如果设置为自动开始，启动过渡
        if (autoStartTransition)
        {
            StartTransition();
        }
    }

    /// <summary>
    /// 开始PPU过渡
    /// </summary>
    public void StartTransition()
    {
        if (pixelPerfectAdaptor == null || !pixelPerfectAdaptor.IsValid())
        {
            Debug.LogError("PixelPerfectAdaptor无效，无法开始过渡！");
            return;
        }

        // 如果已经有过渡在进行，先停止
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // 开始新的过渡
        currentTransition = StartCoroutine(TransitionPPU());
    }

    /// <summary>
    /// 平滑过渡PPU值的协程
    /// </summary>
    private IEnumerator TransitionPPU()
    {
        if (pixelPerfectAdaptor == null) yield break;

        float elapsedTime = 0f;
        float startValue = startPPU;
        float targetValue = targetPPU;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);
            
            // 使用动画曲线平滑过渡
            float curvedT = transitionCurve.Evaluate(t);
            
            // 计算当前PPU值（四舍五入到整数，因为PPU必须是整数）
            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, curvedT));
            
            // 应用到摄像机
            pixelPerfectAdaptor.SetAssetsPPU(currentPPU);

            yield return null;
        }

        // 确保最终设置为目标值
        pixelPerfectAdaptor.SetAssetsPPU(targetPPU);

        Debug.Log($"PPU过渡完成: {startPPU} -> {targetPPU}");
        currentTransition = null;
    }

    /// <summary>
    /// 立即设置PPU值
    /// </summary>
    public void SetPPUImmediate(int ppu)
    {
        if (pixelPerfectAdaptor == null || !pixelPerfectAdaptor.IsValid())
            return;

        // 如果有过渡在进行，停止它
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        pixelPerfectAdaptor.SetAssetsPPU(ppu);
    }

    /// <summary>
    /// 自定义过渡
    /// </summary>
    public void CustomTransition(int fromPPU, int toPPU, float duration)
    {
        if (pixelPerfectAdaptor == null || !pixelPerfectAdaptor.IsValid())
            return;

        // 更新参数
        startPPU = fromPPU;
        targetPPU = toPPU;
        transitionDuration = duration;

        // 如果当前有过渡，停止它
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // 开始新过渡
        currentTransition = StartCoroutine(TransitionPPU());
    }
}