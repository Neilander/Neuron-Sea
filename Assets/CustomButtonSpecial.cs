using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButtonSpecial : MonoBehaviour
{
    [Header("鼠标放上面时激活物体")]
    [SerializeField]
    private GameObject highlightedObj;
    [Header("鼠标不在上面时激活物体")]
    [SerializeField]
    private GameObject normalObj;
    private RectTransform rectTransform;
    private bool isMouseOver = false;
    private Canvas parentCanvas; // 新增：获取父级Canvas
    private Camera eventCamera;  // 新增：存储事件相机

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // 新增：获取父Canvas及其渲染相机
        parentCanvas = GetComponentInParent<Canvas>();
        eventCamera = parentCanvas.worldCamera; // 关键修改：获取Canvas使用的相机
    }

    void Update()
    {
        // 检查相机是否存在（Camera模式需要）
        if (eventCamera == null)
        {
            // 尝试重新获取相机
            eventCamera = parentCanvas.worldCamera;
            if (eventCamera == null) return; // 仍然获取不到则跳过
        }

        // 将鼠标屏幕坐标转换为UI本地坐标
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            Input.mousePosition,
            eventCamera, // 关键修改：传入Canvas的渲染相机
            out localMousePosition
        );

        // 检查坐标是否在RectTransform的矩形范围内
        isMouseOver = rectTransform.rect.Contains(localMousePosition);

        if (isMouseOver)
        {
            HightlightObj();
        }
        else
        {
            NormalObj();
        }
    }

    public void HightlightObj()
    {
        highlightedObj.SetActive(true);
        normalObj.SetActive(false);
    }

    public void NormalObj()
    {
        highlightedObj.SetActive(false);
        normalObj.SetActive(true);
    }
}