using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 实现场景进入时的聚光灯展示效果：
/// 开始时只有人物周围有一个小亮圈，然后圈慢慢变大直到整个屏幕都可见
/// </summary>
public class SpotlightReveal : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("聚光灯跟随的目标（通常是玩家）")]
    public Transform targetToFollow;

    [Tooltip("目标位置的偏移量（世界空间）")]
    public Vector3 targetOffset = Vector3.zero;

    [Tooltip("是否使用屏幕空间的偏移量")]
    public bool useScreenSpaceOffset = false;

    [Tooltip("屏幕空间的偏移量（0-1范围，仅当useScreenSpaceOffset为true时使用）")]
    public Vector2 screenSpaceOffset = Vector2.zero;

    [Header("效果设置")]
    [Tooltip("初始亮圈大小")]
    public float initialRadius = 1.0f;

    [Tooltip("最终亮圈大小")]
    public float finalRadius = 15.0f;

    [Tooltip("展开速度")]
    public float expandSpeed = 2.0f;

    [Tooltip("平滑程度")]
    [Range(0.1f, 10f)]
    public float smoothness = 3.0f;

    [Header("遮罩设置")]
    [Tooltip("黑色遮罩的透明度")]
    [Range(0f, 1f)]
    public float maskAlpha = 0.95f;

    [Header("完成后处理")]
    [Tooltip("效果完成后要禁用的面板")]
    public GameObject panelToDisable;

    [Tooltip("完成效果后的延迟（秒）")]
    public float disableDelay = 0.5f;

    // 遮罩材质
    private Material maskMaterial;
    private bool isExpanding = false;
    private float currentRadius;

    // UI元素
    private RawImage maskImage;

    private void Awake()
    {
        // 创建一个RawImage作为遮罩
        GameObject maskObject = new GameObject("SpotlightMask");
        maskObject.transform.SetParent(transform);
        maskObject.transform.localPosition = Vector3.zero;
        maskObject.transform.localScale = Vector3.one;

        // 添加Canvas组件，确保遮罩显示在最上层
        Canvas canvas = maskObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 确保显示在最上层

        // 添加CanvasScaler组件以适应不同分辨率
        CanvasScaler scaler = maskObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 创建RawImage组件
        maskImage = maskObject.AddComponent<RawImage>();
        
        // 创建一个材质，使用自定义shader
        maskMaterial = new Material(Shader.Find("Custom/SpotlightMask"));
        if (maskMaterial == null)
        {
            Debug.LogError("找不到'Custom/SpotlightMask'着色器。请确保添加了正确的着色器。");
            
            // 退路：使用基本材质
            maskMaterial = new Material(Shader.Find("UI/Default"));
            maskImage.color = new Color(0, 0, 0, maskAlpha);
        }
        else
        {
            // 设置初始参数
            maskMaterial.SetFloat("_Radius", initialRadius);
            maskMaterial.SetFloat("_Smoothness", smoothness);
            maskMaterial.SetColor("_MaskColor", new Color(0, 0, 0, maskAlpha));
            
            // 设置RawImage的材质
            maskImage.material = maskMaterial;
        }

        // 设置遮罩填满整个屏幕
        maskImage.rectTransform.anchorMin = Vector2.zero;
        maskImage.rectTransform.anchorMax = Vector2.one;
        maskImage.rectTransform.sizeDelta = Vector2.zero;

        // 初始化当前半径
        currentRadius = initialRadius;
    }

    private void Start()
    {
        // 确保有目标跟随
        if (targetToFollow == null)
        {
            Debug.LogWarning("未设置跟随目标，将尝试查找玩家对象");
            targetToFollow = FindObjectOfType<PlayerController>()?.transform;
            
            if (targetToFollow == null)
            {
                Debug.LogError("无法找到玩家对象，聚光灯效果将居中显示");
            }
        }

        // 开始扩展效果
        StartCoroutine(ExpandSpotlight());
    }

    private void Update()
    {
        // 如果有目标且遮罩材质存在，更新聚光灯位置
        if (targetToFollow != null && maskMaterial != null)
        {
            Vector3 targetPosition;
            
            // 应用世界空间偏移
            targetPosition = targetToFollow.position + targetOffset;
            
            // 将目标的世界坐标转换为屏幕坐标
            Vector3 screenPos = Camera.main.WorldToScreenPoint(targetPosition);
            
            // 转换为归一化坐标(0-1范围)
            Vector2 normalizedPos = new Vector2(
                screenPos.x / Screen.width,
                screenPos.y / Screen.height
            );
            
            // 应用屏幕空间偏移（如果启用）
            if (useScreenSpaceOffset)
            {
                normalizedPos += screenSpaceOffset;
                
                // 确保坐标在0-1范围内
                normalizedPos.x = Mathf.Clamp01(normalizedPos.x);
                normalizedPos.y = Mathf.Clamp01(normalizedPos.y);
            }
            
            // 设置材质中的聚光灯中心位置
            maskMaterial.SetVector("_Center", normalizedPos);
            
            // 调试位置信息
            // Debug.Log($"聚光灯位置：世界({targetPosition})，屏幕({screenPos})，归一化({normalizedPos})");
        }
    }

    private IEnumerator ExpandSpotlight()
    {
        isExpanding = true;
        
        // 等待一帧确保所有组件都已初始化
        yield return null;
        
        // 逐渐扩大亮圈
        while (currentRadius < finalRadius)
        {
            currentRadius += expandSpeed * Time.deltaTime;
            
            // 确保不超过最大半径
            currentRadius = Mathf.Min(currentRadius, finalRadius);
            
            // 更新材质中的半径值
            if (maskMaterial != null)
            {
                maskMaterial.SetFloat("_Radius", currentRadius);
            }
            
            yield return null;
        }
        
        // 等待指定的延迟
        yield return new WaitForSeconds(disableDelay);
        
        // 禁用面板（如果已设置）
        if (panelToDisable != null)
        {
            panelToDisable.SetActive(false);
        }
        
        // 使遮罩渐渐消失
        float alpha = maskAlpha;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime;
            if (maskMaterial != null)
            {
                maskMaterial.SetColor("_MaskColor", new Color(0, 0, 0, alpha));
            }
            else if (maskImage != null)
            {
                maskImage.color = new Color(0, 0, 0, alpha);
            }
            
            yield return null;
        }
        
        // 完成后禁用自身
        gameObject.SetActive(false);
        
        isExpanding = false;
    }
}