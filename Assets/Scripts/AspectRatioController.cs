using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    [Tooltip("Ŀ���߱ȣ����/�߶ȣ�")]
    public float targetAspect = 16f / 9f; // Ĭ��16:9

    private Camera mainCamera;
    private UniversalAdditionalCameraData cameraData;
    private int lastWidth;
    private int lastHeight;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        cameraData = mainCamera.GetUniversalAdditionalCameraData();

        // ��ʼ����
        UpdateResolution();
    }

    void Update()
    {
        ResetAllOverlayCanvas();
        // �����ֱ��ʱ仯ʱ����
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
        // ʹ��ʵ����Ⱦ�ֱ���
        float currentAspect = (float)Display.main.renderingWidth / Display.main.renderingHeight;

        // �����ӿڱ���
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

        // Ӧ��������ӿ�
        mainCamera.rect = rect;
    }
    void ResetAllOverlayCanvas()
    {
        // ��ȡ����Overlay Canvas
        var overlayCanvases = FindObjectsOfType<Canvas>();
        foreach (var canvas in overlayCanvases)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // ����Canvas����Ⱦ���
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = GameObject.Find("UICamera").GetComponent<Camera>();
                canvas.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            }
        }
    }

    // �༭����ʵʱԤ��
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

