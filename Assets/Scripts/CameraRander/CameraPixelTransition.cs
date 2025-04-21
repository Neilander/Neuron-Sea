using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

/// <summary>
/// 控制像素完美摄像机的像素到单位比例平滑过渡
/// </summary>
public class CameraPixelTransition : MonoBehaviour
{
    [Header("过渡设置")]
    [Tooltip("过渡的开始值")]
    [SerializeField] private int startAssetsPPU = 100;
    
    [Tooltip("过渡的结束值")]
    [SerializeField] private int targetAssetsPPU = 32;
    
    [Tooltip("过渡时间（秒）")]
    [SerializeField] private float transitionDuration = 2.0f;
    
    [Tooltip("过渡曲线")]
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("是否在场景加载后自动开始过渡")]
    [SerializeField] private bool autoTransitionOnStart = true;
    
    // 参考到场景中的PixelPerfectCamera组件
    private PixelPerfectCamera pixelPerfectCamera;
    private Coroutine transitionCoroutine;
    
    private void Awake()
    {
        // 获取PixelPerfectCamera组件
        pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
        if (pixelPerfectCamera == null)
        {
            Debug.LogError("未找到PixelPerfectCamera组件！");
            enabled = false;
            return;
        }
    }
    
    private void Start()
    {
        if (autoTransitionOnStart)
        {
            // 立即设置初始值
            pixelPerfectCamera.assetsPPU = startAssetsPPU;
            
            // 开始过渡
            StartTransition();
        }
    }
    
    /// <summary>
    /// 开始过渡效果
    /// </summary>
    public void StartTransition()
    {
        // 如果已经有过渡在进行，先停止它
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        // 开始新的过渡
        transitionCoroutine = StartCoroutine(TransitionCoroutine());
    }
    
    /// <summary>
    /// 过渡协程
    /// </summary>
    private IEnumerator TransitionCoroutine()
    {
        float startTime = Time.time;
        float elapsedTime = 0f;
        
        // 记录初始值（以防StartTransition被多次调用时当前值不是startAssetsPPU）
        int currentAssetsPPU = pixelPerfectCamera.assetsPPU;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime = Time.time - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / transitionDuration);
            
            // 使用动画曲线计算当前进度
            float curveValue = transitionCurve.Evaluate(normalizedTime);
            
            // 计算当前AssetsPPU值
            int newValue = Mathf.RoundToInt(Mathf.Lerp(currentAssetsPPU, targetAssetsPPU, curveValue));
            
            // 应用到摄像机
            pixelPerfectCamera.assetsPPU = newValue;
            
            yield return null;
        }
        
        // 确保最终值精确
        pixelPerfectCamera.assetsPPU = targetAssetsPPU;
        
        transitionCoroutine = null;
    }
    
    /// <summary>
    /// 直接设置摄像机的AssetsPPU值
    /// </summary>
    public void SetAssetsPPU(int value)
    {
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = value;
        }
    }
    
    /// <summary>
    /// 重置为开始值
    /// </summary>
    public void ResetToStartValue()
    {
        SetAssetsPPU(startAssetsPPU);
    }
    
    /// <summary>
    /// 设置为目标值
    /// </summary>
    public void SetToTargetValue()
    {
        SetAssetsPPU(targetAssetsPPU);
    }
}