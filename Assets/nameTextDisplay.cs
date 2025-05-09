using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class nameTextDisplay : MonoBehaviour
{

    public TextMeshProUGUI targetText;

    private Image img;
    private Sprite currentSprite;

    void Start()
    {
        img = GetComponent<Image>();

        if (img == null)
        {
            Debug.LogWarning("nameTextDisplay: 没有找到 Image 组件！");
            enabled = false;
            return;
        }

        UpdateTextIfChanged();
    }

    void Update()
    {
        UpdateTextIfChanged();
    }

    private void UpdateTextIfChanged()
    {
        if (img.sprite != currentSprite)
        {
            currentSprite = img.sprite;

            if (targetText != null && currentSprite != null)
            {
                targetText.text = currentSprite.name;
            }
            else if (targetText != null)
            {
                targetText.text = "无图片";
            }
        }
    }

}
