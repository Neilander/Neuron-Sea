using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 聚光灯显示效果：开始时屏幕全黑，只有角色周围的小圈可见，然后小圈逐渐扩大显示整个场景
/// </summary>
public class SpotlightReveal : MonoBehaviour
{
    [Header("基本设置")]
    [Tooltip("跟随的目标(通常是玩家)")]
    public Transform target;

    [Tooltip("起始圆圈半径")]
    [Range(0.1f, 5f)]
    public float startRadius = 1f;

    [Tooltip("最终圆圈半径")]
    [Range(5f, 50f)]
    public float endRadius = 20f;

    [Tooltip("扩散时间(秒)")]
    [Range(0.5f, 10f)]
    public float expandDuration = 3f;

    [Tooltip("扩散开始的延迟时间(秒)")]
    [Range(0f, 5f)]
    public float startDelay = 0.5f;

    [Tooltip("扩散完成后是否自动销毁")]
    public bool destroyAfterReveal = true;

    [Header("扩散曲线设置")]
    [Tooltip("扩散的动画曲线")]
    public AnimationCurve expandCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("UI引用")]
    [Tooltip("黑色遮罩图像")]
    public Image blackMaskImage;

    [Tooltip("圆形遮罩图像")]
    public Image circleMaskImage;

    [Header("完成时禁用的面板")]
    [Tooltip("效果完成后需要禁用的面板")]
    public GameObject panelToDisable;

    private RectTransform circleMaskRect;
    private Material circleMaskMaterial;
    private Vector3 initialScale;
    private bool isRevealing = false;
    private Camera mainCamera;

    private void Awake()
    {
        // 获取主摄像机
        mainCamera = Camera.main;

        // 获取圆形遮罩的RectTransform
        if (circleMaskImage != null)
        {
            circleMaskRect = circleMaskImage.rectTransform;
            initialScale = circleMaskRect.localScale;

            // 获取材质
            circleMaskMaterial = circleMaskImage.material;
            if (circleMaskMaterial == null)
            {
                Debug.LogError("圆形遮罩图像必须使用支持Shader的材质，如UI/Default或自定义Shader");
                enabled = false;
                return;
            }
        }
        else
        {
            Debug.LogError("未设置圆形遮罩图像");
            enabled = false;
            return;
        }

        // 检查黑色遮罩图像
        if (blackMaskImage == null)
        {
            Debug.LogError("未设置黑色遮罩图像");
            enabled = false;
            return;
        }

        // 初始化遮罩状态
        SetupInitialState();
    }

    private void Start()
    {
        // 自动寻找玩家目标
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("自动找到玩家目标: " + target.name);
            }
            else
            {
                Debug.LogWarning("未找到玩家目标，将使用摄像机位置");
            }
        }

        // 开始聚光灯扩散
        StartCoroutine(RevealSequence());
    }

    private void Update()
    {
        if (isRevealing && target != null)
        {
            // 更新圆形遮罩位置为目标在屏幕上的位置
            UpdateMaskPosition();
        }
    }

    private void SetupInitialState()
    {
        // 设置初始半径
        if (circleMaskRect != null)
        {
            // 设置初始缩放，使得圆圈大小为startRadius
            float scaleFactor = startRadius / 100f; // 假设基础图像大小为100x100
            circleMaskRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
        }

        // 确保黑色遮罩和圆形遮罩是可见的
        if (blackMaskImage != null)
        {
            blackMaskImage.enabled = true;
            Color color = blackMaskImage.color;
            color.a = 1f; // 完全不透明
            blackMaskImage.color = color;
        }

        if (circleMaskImage != null)
        {
            circleMaskImage.enabled = true;
        }
    }

    private void UpdateMaskPosition()
    {
        if (circleMaskRect != null && target != null && mainCamera != null)
        {
            // 将目标的世界坐标转换为屏幕坐标
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target.position);

            // 将屏幕坐标转换为UI坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                circleMaskRect.parent as RectTransform,
                screenPos,
                null, // 如果使用Canvas的世界空间模式，这里应该是mainCamera
                out Vector2 localPos
            );

            // 更新圆形遮罩的位置
            circleMaskRect.localPosition = localPos;
        }
    }

    private IEnumerator RevealSequence()
    {
        // 等待设定的延迟时间
        yield return new WaitForSeconds(startDelay);

        // 开始扩散
        isRevealing = true;
        float elapsedTime = 0f;

        while (elapsedTime < expandDuration)
        {
            // 计算扩散进度
            float progress = elapsedTime / expandDuration;
            
            // 应用曲线
            float curveValue = expandCurve.Evaluate(progress);
            
            // 计算当前半径
            float currentRadius = Mathf.Lerp(startRadius, endRadius, curveValue);
            
            // 更新圆形遮罩大小
            float scaleFactor = currentRadius / 100f;
            circleMaskRect.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
            
            // 更新圆形遮罩位置
            UpdateMaskPosition();
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保最终半径正确
        float finalScaleFactor = endRadius / 100f;
        circleMaskRect.localScale = new Vector3(finalScaleFactor, finalScaleFactor, 1f);
        
        // 完成扩散
        isRevealing = false;
        
        // 禁用面板
        if (panelToDisable != null)
        {
            panelToDisable.SetActive(false);
            Debug.Log("效果完成，禁用面板: " + panelToDisable.name);
        }
        
        // 可选：扩散完成后销毁组件或游戏对象
        if (destroyAfterReveal)
        {
            // 延迟一帧后销毁，确保面板已经被禁用
            yield return null;
            Destroy(gameObject);
        }
    }
}