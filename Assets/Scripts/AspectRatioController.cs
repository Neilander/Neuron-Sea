using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    [Tooltip("目标宽高比（宽度/高度）")]
    public float targetAspect = 16f / 9f; // 默认16:9

    private Camera mainCamera;
    private UniversalAdditionalCameraData cameraData;
    private float currentAspect;
    private Rect originalRect;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        cameraData = mainCamera.GetUniversalAdditionalCameraData();

        // 保存原始视口设置
        originalRect = mainCamera.rect;

        UpdateAspectRatio();
    }

    void Update()
    {
        // 当窗口大小改变时更新宽高比
        if (Screen.width != Screen.currentResolution.width ||
            Screen.height != Screen.currentResolution.height)
        {
            UpdateAspectRatio();
        }
        ResetAllOverlayCanvas();
    }

    void ResetAllOverlayCanvas()
    {
        // 获取所有Overlay Canvas
        var overlayCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in overlayCanvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // 重置Canvas的渲染相机
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
                canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            }
        }
    }

    void UpdateAspectRatio()
    {
        // 计算当前屏幕宽高比
        currentAspect = (float)Screen.width / Screen.height;

        // 计算目标宽高比与当前宽高比的比例
        float scaleHeight = currentAspect / targetAspect;
        float scaleWidth = 1f / scaleHeight;

        // 创建视口矩形
        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1f)
        {
            // 当前屏幕比目标更宽（上下黑边）
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            // 当前屏幕比目标更高（左右黑边）
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
        }

        // 应用调整后的视口
        mainCamera.rect = rect;
    }

    // 可选：在编辑器中实时更新
#if UNITY_EDITOR
    void OnValidate()
    {
        if (mainCamera != null)
        {
            UpdateAspectRatio();
        }
    }
#endif
}