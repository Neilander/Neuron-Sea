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
    private float currentAspect;
    private Rect originalRect;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        cameraData = mainCamera.GetUniversalAdditionalCameraData();

        // ����ԭʼ�ӿ�����
        originalRect = mainCamera.rect;

        UpdateAspectRatio();
    }

    void Update()
    {
        // �����ڴ�С�ı�ʱ���¿�߱�
        if (Screen.width != Screen.currentResolution.width ||
            Screen.height != Screen.currentResolution.height)
        {
            UpdateAspectRatio();
        }
        ResetAllOverlayCanvas();
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

    void UpdateAspectRatio()
    {
        // ���㵱ǰ��Ļ��߱�
        currentAspect = (float)Screen.width / Screen.height;

        // ����Ŀ���߱��뵱ǰ��߱ȵı���
        float scaleHeight = currentAspect / targetAspect;
        float scaleWidth = 1f / scaleHeight;

        // �����ӿھ���
        Rect rect = new Rect(0, 0, 1, 1);

        if (scaleHeight < 1f)
        {
            // ��ǰ��Ļ��Ŀ��������ºڱߣ�
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            // ��ǰ��Ļ��Ŀ����ߣ����Һڱߣ�
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0;
        }

        // Ӧ�õ�������ӿ�
        mainCamera.rect = rect;
    }

    // ��ѡ���ڱ༭����ʵʱ����
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