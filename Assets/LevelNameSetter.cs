using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelNameSetter : MonoBehaviour
{
    [TextArea(3, 10)]
    public string fullText; // 输入的长文字，使用 / 分隔

    // 调用此方法来解析并设置文本
    public void ParseAndSetTexts()
    {
        string[] parts = fullText.Split(' ');
        int index = 0;

        // 找出所有子物体中激活的 TextMeshProUGUI
        var tmps = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: false);

        foreach (var tmp in tmps)
        {
            if (index < parts.Length)
            {
                tmp.text = parts[index].Trim();
                index++;
            }
            else
            {
                break; // 超出文本数量
            }
        }

        Debug.Log($"已设置 {index} 个文本");
    }
}
