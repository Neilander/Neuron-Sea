using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Puzzle : MonoBehaviour
{
    public Sprite puzzlePhoto { get; private set; }

    private void Update(){
        //得到拼图图片
        puzzlePhoto=GetComponent<Image>().sprite;
    }

    // 可选：提供一个方法动态设置图片
    public void SetPuzzlePhoto(Sprite sprite)
    {
        puzzlePhoto = sprite;
        var img = GetComponent<Image>();
        if (img != null)
            img.sprite = sprite;
    }
}
