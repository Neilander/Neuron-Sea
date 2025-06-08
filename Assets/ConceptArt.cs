using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConceptArt : MonoBehaviour
{
    [SerializeField] private Transform imgParent; // ������
    [SerializeField] private Animator anim; // ������Ч
    private int recordIndex;
    private void Start()
    {
        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // ���ö�������ģʽΪ����ʱ������Ӱ��
    }
    public void ShowPic(int index)
    {
        Debug.Log("ShowPic called with index: " + index);
        recordIndex = index;
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
        
        //AudioManager.Instance.PlayBGMForGallary(Factory(index));
    }

    private BGMClip Factory(int index)
    {
        return index switch
        {
            0=>BGMClip.Scene1,
            1=>BGMClip.Scene1,
            2=>BGMClip.Scene2,
            3=>BGMClip.Scene3,
            _ =>BGMClip.Scene1
        };
    }

    void Update()
    {
        if (GameInput.Back.Pressed(false))
        {
            GetComponent<ClickAndExit>().Exit();
        }
    }

    private bool inPlaying = false;
    public void PlayMusic()
    {
        if (inPlaying)
        {
            AudioManager.Instance.StopGallaryMusic(Factory(recordIndex));
            inPlaying = false;
        }
        else
        {
            AudioManager.Instance.PlayBGMForGallary(Factory(recordIndex));
            inPlaying = true;
        }
        
    }
}
