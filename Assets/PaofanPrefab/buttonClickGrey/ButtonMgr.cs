using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonMgr : MonoBehaviour
{
    [Header("按钮组")] public Button[] buttons;

    [Header("精灵设置")] public Sprite normalSprite; // 正常状态的精灵

    public Sprite graySprite; // 变灰状态的精灵

    // 或者使用精灵数组，为每个按钮设置不同的状态
    [System.Serializable]
    public class ButtonSprites
    {
        public Sprite normalSprite;

        public Sprite graySprite;
    }

    public ButtonSprites[] buttonSprites;

    private void Start(){
        for (int i = 0; i < buttons.Length; i++) {
            int buttonIndex = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(buttonIndex));
        }
    }

    private void OnButtonClick(int clickedIndex){
        for (int i = 0; i < buttons.Length; i++) {
            Image buttonImage = buttons[i].GetComponent<Image>();
            if (buttonImage != null) {
                if (buttonSprites != null && buttonSprites.Length > i) {
                    // 使用每个按钮独特的精灵
                    buttonImage.sprite = (i == clickedIndex)
                        ? buttonSprites[i].normalSprite
                        : buttonSprites[i].graySprite;
                }
                else {
                    // 使用通用的精灵
                    buttonImage.sprite = (i == clickedIndex) ? normalSprite : graySprite;
                }
            }
        }
    }
}
