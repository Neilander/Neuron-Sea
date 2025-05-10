using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public Color highlightedColor = Color.red;
    public Color normalColor = Color.white;

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
