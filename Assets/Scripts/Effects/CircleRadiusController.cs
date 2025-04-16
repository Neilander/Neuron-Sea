using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制CircleMask shader的圆形半径参数，使其在指定时间内从初始值过渡到目标值
/// </summary>
public class CircleRadiusController : MonoBehaviour
{
    [Header("Shader参数控制")]
    [Tooltip("包含CircleMask shader的Image组件")]
    public Image targetImage;

    [Tooltip("初始圆形半径值")]
    [Range(0.01f, 1f)]
    public float startRadius = 0.1f;

    [Tooltip("目标圆形半径值")]
    [Range(0.01f, 1f)]
    public float endRadius = 1f;

    [Tooltip("过渡时间(秒)")]
    [Range(0.1f, 10f)]
    public float transitionDuration = 1f;

    [Tooltip("动画曲线")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("控制选项")]
    [Tooltip("是否在启动时自动开始过渡")]
    public bool autoStartOnEnable = true;

    [Tooltip("过渡延迟时间(秒)")]
    [Range(0f, 5f)]
    public float startDelay = 0f;

    [Tooltip("过渡完成后是否禁用此组件")]
    public bool disableAfterTransition = true;

    // 保存对材质的引用
    private Material targetMaterial;
    
    // Shader中圆形半径参数的名称
    private static readonly string CIRCLE_RADIUS_PROPERTY = "_CircleRadius";
    
    // 是否正在过渡
    private bool isTransitioning = false;

    private void Awake()
    {
        // 获取目标图像的材质
        if (targetImage != null)
        {
            targetMaterial = targetImage.material;
            
            if (targetMaterial == null)
            {
                Debug.LogError("目标Image没有材质！请确保已设置材质且使用了CircleMask shader。");
                enabled = false;
                return;
            }
            
            // 检查材质是否包含CircleRadius属性
            if (!targetMaterial.HasProperty(CIRCLE_RADIUS_PROPERTY))
            {
                Debug.LogError("目标材质不包含 " + CIRCLE_RADIUS_PROPERTY + " 属性！请确保使用了正确的CircleMask shader。");
                enabled = false;
                return;
            }
        }
        else
        {
            Debug.LogError("未设置目标Image！请在Inspector中设置。");
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (autoStartOnEnable && targetMaterial != null)
        {
            // 在启用时设置初始半径
            targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, startRadius);
            
            // 启动过渡效果
            StartTransition();
        }
    }

    /// <summary>
    /// 开始半径过渡动画
    /// </summary>
    public void StartTransition()
    {
        if (!isTransitioning && targetMaterial != null)
        {
            StartCoroutine(TransitionRadius());
        }
    }

    /// <summary>
    /// 重置半径到初始值
    /// </summary>
    public void ResetRadius()
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, startRadius);
        }
    }

    /// <summary>
    /// 立即设置半径到目标值
    /// </summary>
    public void SetToEndRadius()
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, endRadius);
        }
    }

    /// <summary>
    /// 处理半径过渡的协程
    /// </summary>
    private IEnumerator TransitionRadius()
    {
        isTransitioning = true;
        
        // 应用延迟
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }
        
        float elapsedTime = 0f;
        
        // 获取当前半径值作为起点
        float currentRadius = targetMaterial.GetFloat(CIRCLE_RADIUS_PROPERTY);
        
        // 如果当前值与起始值不同，我们从当前值开始过渡
        float actualStartRadius = currentRadius;
        
        // 过渡动画
        while (elapsedTime < transitionDuration)
        {
            // 计算过渡进度
            float progress = elapsedTime / transitionDuration;
            
            // 应用动画曲线
            float curveProgress = transitionCurve.Evaluate(progress);
            
            // 计算当前半径
            float radius = Mathf.Lerp(actualStartRadius, endRadius, curveProgress);
            
            // 设置Shader参数
            targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, radius);
            
            // 更新时间
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }
        
        // 确保最终值正确
        targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, endRadius);
        
        isTransitioning = false;
        
        // 如果需要，禁用组件
        if (disableAfterTransition)
        {
            enabled = false;
        }
    }
}