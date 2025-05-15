using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FeelMouseOn : MonoBehaviour
{
    private UiStateSwitcher switcher;
    private RectTransform rect;
    [SerializeField] private float speed = 2f;

    void Start()
    {
        switcher = GetComponent<UiStateSwitcher>();
        rect = GetComponent<RectTransform>();

        if (switcher == null)
            Debug.LogWarning("缺少 UiStateSwitcher");
        if (rect == null)
            Debug.LogWarning("缺少 RectTransform");
    }

    void Update()
    {
        if (switcher == null || rect == null) return;

        Vector2 mousePos = Input.mousePosition;
        Camera uiCam = GetComponentInParent<Canvas>().worldCamera;

        // 检查鼠标是否在这个 UI 元素上
        //bool hovering = RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, uiCam);
        //                //&& !EventSystem.current.IsPointerOverGameObject(); // 可选：屏蔽被其他 UI 挡住的情况
        bool hovering = false;
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject == switcher.img.gameObject)
            {
                RectTransform rectTransform = switcher.img.rectTransform;
                Vector2 localPoint;
                // 将屏幕坐标转换为Image的本地坐标
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    eventData.position,
                    null, // 对于Overlay模式，相机为null
                    out localPoint))
                {
                    // 获取Raycast Padding值（左、右、上、下）
                    Vector4 padding = switcher.img.raycastPadding;
                    Rect rect = rectTransform.rect;

                    // 计算有效区域边界
                    float leftBound = -rect.width / 2 + padding.x;
                    float rightBound = rect.width / 2 - padding.z;
                    float bottomBound = -rect.height / 2 + padding.y;
                    float topBound = rect.height / 2 - padding.w;

                    // 判断是否在有效区域内
                    hovering = localPoint.x >= leftBound &&
                                   localPoint.x <= rightBound &&
                                   localPoint.y >= bottomBound &&
                                   localPoint.y <= topBound;
                }
                break; // 找到目标后退出循环
            }
        }

        float delta = speed * Time.unscaledDeltaTime;
        if (hovering)
            switcher.progress = Mathf.Clamp01(switcher.progress + delta);
        else
            switcher.progress = Mathf.Clamp01(switcher.progress - delta);

    }
}
