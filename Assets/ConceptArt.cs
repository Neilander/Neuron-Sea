using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConceptArt : MonoBehaviour
{
    [SerializeField] private Transform imgParent; // ������
    [SerializeField] private Animator anim; // ������Ч

    private void Start()
    {
        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // ���ö�������ģʽΪ����ʱ������Ӱ��
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
