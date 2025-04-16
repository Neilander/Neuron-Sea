using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 控制CircleMask shader的参数，包括半径和位置
/// </summary>
public class CircleEffectController : MonoBehaviour
{
    [System.Serializable]
    public class CircleParams
    {
        [Tooltip("圆形半径")]
        [Range(0.01f, 1f)]
        public float radius = 0.5f;

        [Tooltip("圆形位置X坐标 (0-1)")]
        [Range(0f, 1f)]
        public float centerX = 0.5f;

        [Tooltip("圆形位置Y坐标 (0-1)")]
        [Range(0f, 1f)]
        public float centerY = 0.5f;

        [Tooltip("边缘柔和度")]
        [Range(0.01f, 0.5f)]
        public float softness = 0.1f;
    }

    [Header("目标设置")]
    [Tooltip("包含CircleMask shader的Image组件")]
    public Image targetImage;

    [Header("过渡设置")]
    [Tooltip("初始圆形参数")]
    public CircleParams startParams = new CircleParams { radius = 0.1f, centerX = 0.5f, centerY = 0.5f, softness = 0.1f };

    [Tooltip("目标圆形参数")]
    public CircleParams endParams = new CircleParams { radius = 1f, centerX = 0.5f, centerY = 0.5f, softness = 0.1f };

    [Tooltip("过渡时间(秒)")]
    [Range(0.1f, 10f)]
    public float transitionDuration = 1f;

    [Tooltip("半径过渡曲线")]
    public AnimationCurve radiusCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("位置过渡曲线")]
    public AnimationCurve positionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("控制选项")]
    [Tooltip("是否在启动时自动开始过渡")]
    public bool autoStartOnEnable = true;

    [Tooltip("过渡延迟时间(秒)")]
    [Range(0f, 5f)]
    public float startDelay = 0f;

    [Tooltip("过渡完成后是否禁用此组件")]
    public bool disableAfterTransition = true;

    [Tooltip("过渡完成后要禁用的面板")]
    public GameObject panelToDisable;

    // 保存对材质的引用
    private Material targetMaterial;
    
    // Shader中参数的名称
    private static readonly string CIRCLE_RADIUS_PROPERTY = "_CircleRadius";
    private static readonly string CIRCLE_SOFTNESS_PROPERTY = "_CircleSoftness";
    private static readonly string CIRCLE_CENTER_X_PROPERTY = "_CircleCenterX";
    private static readonly string CIRCLE_CENTER_Y_PROPERTY = "_CircleCenterY";
    
    // 是否正在过渡
    private bool isTransitioning = false;

    // 跟随目标
    public Transform followTarget;
    public bool followTargetPosition = false;
    public Camera mainCamera;
    public RectTransform canvasRectTransform;

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
            
            // 检查材质是否包含必要的属性
            if (!targetMaterial.HasProperty(CIRCLE_RADIUS_PROPERTY) ||
                !targetMaterial.HasProperty(CIRCLE_CENTER_X_PROPERTY) ||
                !targetMaterial.HasProperty(CIRCLE_CENTER_Y_PROPERTY))
            {
                Debug.LogError("目标材质缺少必要的属性！请确保使用了正确的CircleMask shader。");
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

        // 获取主摄像机（如果需要跟随目标）
        if (followTargetPosition && mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        if (autoStartOnEnable && targetMaterial != null)
        {
            // 在启用时设置初始参数
            SetCircleParams(startParams);
            
            // 启动过渡效果
            StartTransition();
        }
    }

    private void Update()
    {
        // 如果设置了跟随目标且没有处于过渡状态，则更新圆形位置
        if (followTargetPosition && followTarget != null && !isTransitioning && mainCamera != null && canvasRectTransform != null)
        {
            UpdateCirclePositionToTarget();
        }
    }

    /// <summary>
    /// 更新圆形位置到目标的屏幕位置
    /// </summary>
    private void UpdateCirclePositionToTarget()
    {
        // 将世界坐标转换为屏幕坐标
        Vector3 screenPos = mainCamera.WorldToViewportPoint(followTarget.position);
        
        // 设置shader中的位置参数
        targetMaterial.SetFloat(CIRCLE_CENTER_X_PROPERTY, screenPos.x);
        targetMaterial.SetFloat(CIRCLE_CENTER_Y_PROPERTY, screenPos.y);
    }

    /// <summary>
    /// 设置圆形参数到shader
    /// </summary>
    private void SetCircleParams(CircleParams parameters)
    {
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, parameters.radius);
            targetMaterial.SetFloat(CIRCLE_SOFTNESS_PROPERTY, parameters.softness);
            targetMaterial.SetFloat(CIRCLE_CENTER_X_PROPERTY, parameters.centerX);
            targetMaterial.SetFloat(CIRCLE_CENTER_Y_PROPERTY, parameters.centerY);
        }
    }

    /// <summary>
    /// 开始参数过渡动画
    /// </summary>
    public void StartTransition()
    {
        if (!isTransitioning && targetMaterial != null)
        {
            StartCoroutine(TransitionParameters());
        }
    }

    /// <summary>
    /// 重置参数到初始值
    /// </summary>
    public void ResetToStart()
    {
        if (targetMaterial != null)
        {
            SetCircleParams(startParams);
        }
    }

    /// <summary>
    /// 立即设置参数到目标值
    /// </summary>
    public void SetToEndParams()
    {
        if (targetMaterial != null)
        {
            SetCircleParams(endParams);
            
            // 如果设置了面板禁用，则禁用它
            if (panelToDisable != null)
            {
                panelToDisable.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 处理参数过渡的协程
    /// </summary>
    private IEnumerator TransitionParameters()
    {
        isTransitioning = true;
        
        // 应用延迟
        if (startDelay > 0)
        {
            yield return new WaitForSeconds(startDelay);
        }
        
        float elapsedTime = 0f;
        
        // 获取当前参数作为起点
        CircleParams currentParams = new CircleParams
        {
            radius = targetMaterial.GetFloat(CIRCLE_RADIUS_PROPERTY),
            centerX = targetMaterial.GetFloat(CIRCLE_CENTER_X_PROPERTY),
            centerY = targetMaterial.GetFloat(CIRCLE_CENTER_Y_PROPERTY),
            softness = targetMaterial.GetFloat(CIRCLE_SOFTNESS_PROPERTY)
        };
        
        // 过渡动画
        while (elapsedTime < transitionDuration)
        {
            // 计算过渡进度
            float progress = elapsedTime / transitionDuration;
            
            // 应用动画曲线
            float radiusProgress = radiusCurve.Evaluate(progress);
            float positionProgress = positionCurve.Evaluate(progress);
            
            // 计算当前参数
            float radius = Mathf.Lerp(currentParams.radius, endParams.radius, radiusProgress);
            float centerX = Mathf.Lerp(currentParams.centerX, endParams.centerX, positionProgress);
            float centerY = Mathf.Lerp(currentParams.centerY, endParams.centerY, positionProgress);
            float softness = Mathf.Lerp(currentParams.softness, endParams.softness, radiusProgress);
            
            // 设置Shader参数
            targetMaterial.SetFloat(CIRCLE_RADIUS_PROPERTY, radius);
            targetMaterial.SetFloat(CIRCLE_CENTER_X_PROPERTY, centerX);
            targetMaterial.SetFloat(CIRCLE_CENTER_Y_PROPERTY, centerY);
            targetMaterial.SetFloat(CIRCLE_SOFTNESS_PROPERTY, softness);
            
            // 更新时间
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }
        
        // 确保最终值正确
        SetCircleParams(endParams);
        
        isTransitioning = false;
        
        // 如果设置了面板禁用，则禁用它
        if (panelToDisable != null)
        {
            panelToDisable.SetActive(false);
        }
        
        // 如果需要，禁用组件
        if (disableAfterTransition)
        {
            enabled = false;
        }
    }
}