using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImgBreath : MonoBehaviour
{
    public Image image; // 要控制透明度的Image组件
    public AnimationCurve transparencyCurve; // 控制透明度的曲线
    public float duration = 2f; // 一次呼吸的持续时间

    private float timeElapsed = 0f;
    private bool isFadingOut = false; // 判断当前是淡入还是淡出

    // Start is called before the first frame update
    void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>(); // 如果没有手动赋值，尝试获取当前对象上的Image组件
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 计算时间进度，循环归零
        timeElapsed += Time.deltaTime;
        if (timeElapsed > duration)
        {
            timeElapsed = 0f; // 每次呼吸完成后归零，重新开始
            isFadingOut = !isFadingOut; // 淡入和淡出之间切换
        }

        // 根据时间进度控制透明度
        float alpha = isFadingOut ? 1 - transparencyCurve.Evaluate(timeElapsed / duration) : transparencyCurve.Evaluate(timeElapsed / duration);

        // 设置图片的透明度
        Color color = image.color;
        color.a = alpha; // 修改透明度
        image.color = color;
    }
}
