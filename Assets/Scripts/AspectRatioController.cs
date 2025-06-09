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
    private int lastWidth;
    private int lastHeight;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        cameraData = mainCamera.GetUniversalAdditionalCameraData();

        // 初始设置
        UpdateResolution();
    }

    void Update()
    {
        ResetAllOverlayCanvas();
        // 仅当分辨率变化时更新
        int currentWidth = Display.main.renderingWidth;
        int currentHeight = Display.main.renderingHeight;

        if (currentWidth != lastWidth || currentHeight != lastHeight)
        {
            UpdateResolution();
            lastWidth = currentWidth;
            lastHeight = currentHeight;
        }
    }

    void UpdateResolution()
    {
        // 使用实际渲染分辨率
        float currentAspect = (float)Display.main.renderingWidth / Display.main.renderingHeight;

        // 计算视口比例
        float scaleHeight = currentAspect / targetAspect;
        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1f)
        {
            rect.height = scaleHeight;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.x = (1f - scaleWidth) / 2f;
        }

        // 应用主相机视口
        mainCamera.rect = rect;
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

    // 编辑器中实时预览
#if UNITY_EDITOR
    void OnValidate()
    {
        if (mainCamera != null)
        {
            UpdateResolution();
        }
    }
#endif
}

