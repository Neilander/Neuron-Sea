using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConceptArt : MonoBehaviour
{
    [SerializeField] private Transform imgParent; // 父物体
    [SerializeField] private Animator anim; // 背景动效

    private void Start()
    {
        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // 设置动画更新模式为不受时间缩放影响
    }
    public void ShowPic(int index)
    {
        Debug.Log("ShowPic called with index: " + index);
        gameObject.SetActive(true);
        // Hide all images
        foreach (Transform child in imgParent)
        {
            child.gameObject.SetActive(false);
        }
        // Show the selected image
        if (index >= 0 && index < imgParent.childCount)
        {
            imgParent.GetChild(index).gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (GameInput.Back.Pressed(false))
        {
            GetComponent<ClickAndExit>().Exit();
        }
    }
}
