using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayGallaryMusicButton : MonoBehaviour
{
    public static PlayGallaryMusicButton instance;
    public Sprite NormalSprite;
    public Sprite StopSprite;

    public Image img;
    private void Awake()
    {
        instance = this;
    }

    public void SetSprite(bool ifPlay)
    {
        img.sprite = ifPlay ? StopSprite : NormalSprite;
    }
}
