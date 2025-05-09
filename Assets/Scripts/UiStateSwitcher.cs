using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiStateSwitcher : MonoBehaviour
{
    [Range(0, 1)]
    public float progress;

    public Image img;

    public Vector2 anchoredPoint1;
    public float alpha1;
    public Vector3 localScale1;

    public Vector2 anchoredPoint2;
    public float alpha2;
    public Vector3 localScale2;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ApplyUiState();
    }

    public void ApplyUiState()
    {
        // 插值位置
        RectTransform rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.Lerp(anchoredPoint1, anchoredPoint2, progress);

        // 插值缩放
        rect.localScale = Vector3.Lerp(localScale1, localScale2, progress);

        // 插值 alpha
        if (img != null)
        {
            Color color = img.color;
            color.a = Mathf.Lerp(alpha1, alpha2, progress);
            img.color = color;
        }
    }
}
