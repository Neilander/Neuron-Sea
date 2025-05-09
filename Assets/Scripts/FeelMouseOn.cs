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
        bool hovering = RectTransformUtility.RectangleContainsScreenPoint(rect, mousePos, uiCam);
                        //&& !EventSystem.current.IsPointerOverGameObject(); // 可选：屏蔽被其他 UI 挡住的情况

        float delta = speed * Time.deltaTime;
        if (hovering)
            switcher.progress = Mathf.Clamp01(switcher.progress + delta);
        else
            switcher.progress = Mathf.Clamp01(switcher.progress - delta);
    }
}
