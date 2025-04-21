using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;
#if UNITY_2020_2_OR_NEWER
// using Unity.RenderPipelines.Universal.Runtime;
#endif

/// <summary>
/// 摄像机像素完美值过渡效果
/// 在场景加载时将PixelPerfectCamera组件的AssetsPPU从起始值平滑过渡到目标值
/// </summary>
public class PPUTransitionEffect : MonoBehaviour
{
    [Header("过渡设置")]
    [SerializeField] private int startPPU = 100;  // 起始PPU值
    [SerializeField] private int targetPPU = 32;  // 目标PPU值
    [SerializeField] private float transitionDuration = 2.0f;  // 过渡持续时间(秒)
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 过渡动画曲线

    [Header("组件引用")]
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;  // 像素完美摄像机组件引用

    private void Awake()
    {
        // 如果没有手动指定PixelPerfectCamera组件，尝试获取挂载在同一游戏对象上的组件
        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = GetComponent<PixelPerfectCamera>();

            if (pixelPerfectCamera == null)
            {
                Debug.LogError("未找到PixelPerfectCamera组件！请确保组件存在或正确指定。");
                enabled = false;  // 禁用此脚本
                return;
            }
        }

        // 在开始时设置初始值
        pixelPerfectCamera.assetsPPU = startPPU;
    }

    private void Start()
    {
        // 启动过渡协程
        StartCoroutine(TransitionPPU());
    }

    /// <summary>
    /// 平滑过渡PPU值的协程
    /// </summary>
    private IEnumerator TransitionPPU()
    {
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
            pixelPerfectCamera.assetsPPU = currentPPU;

            yield return null;
        }

        // 确保最终设置为目标值
        pixelPerfectCamera.assetsPPU = targetPPU;

        Debug.Log($"PPU过渡完成: {startPPU} -> {targetPPU}");
    }
}