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

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 将鼠标屏幕坐标转换为 UI 本地坐标
        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            Input.mousePosition,
            null, // 如果 Canvas 是 Screen Space - Overlay 模式，此处为 null
            out localMousePosition
        );

        // 检查坐标是否在 RectTransform 的矩形范围内
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
