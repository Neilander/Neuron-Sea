using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public Color highlightedColor = Color.red;
    public Color normalColor = Color.white;
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

        // 检查坐标是否在 RectTransform 的矩形范围内
        isMouseOver = rectTransform.rect.Contains(localMousePosition);

        if (isMouseOver)
        {
            HightlightColor();
        }
        else
        {
            NormalColor();
        }
    }

    public void HightlightColor()
    {
        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.color = highlightedColor;
        }
        foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>())
        {
            text.color = highlightedColor;
        }
    }

    public void NormalColor()
    {
        foreach (Image img in GetComponentsInChildren<Image>())
        {
            img.color = normalColor;
        }
        foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>())
        {
            text.color = normalColor;
        }
    }
}
//public class CustomButton : Button
//{
//    public Color highlightedColor = Color.yellow;

//    public Color pressedColor = Color.red;

//    protected override void DoStateTransition(SelectionState state, bool instant){
//        base.DoStateTransition(state, instant); // 保留原有的Sprite Swap逻辑

//        // 自定义颜色调整
//        Image targetImage = GetComponent<Image>();
//        if (targetImage == null) return;

//        switch (state) {
//            case SelectionState.Highlighted:
//                targetImage.color = highlightedColor;
//                break;
//            case SelectionState.Pressed:
//                targetImage.color = pressedColor;
//                break;
//            default:
//                targetImage.color = Color.white; // 恢复默认颜色
//                break;
//        }
//    }
//}
