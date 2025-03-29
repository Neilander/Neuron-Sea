using UnityEngine;
using System.Collections;

public class GradientMapController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;      // 主图层
    [SerializeField] private SpriteRenderer gradientLayer;  // 渐变映射层

    [Header("混合设置")]
    [SerializeField] private float gradientOpacity = 1f;    // 渐变不透明度
    [SerializeField] private float gradientStrength = 1f;   // 渐变强度

    private Material blendMaterial;
    private Sprite lastMainSprite;
    private Sprite lastGradientSprite;

    private void Start()
    {
        // 创建材质实例
        blendMaterial = new Material(Shader.Find("Custom/GradientMap"));

        // 保存初始Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastGradientSprite = gradientLayer.sprite;

        // 设置材质属性
        UpdateTextures();
        UpdateBlendSettings();

        // 应用材质
        mainLayer.material = blendMaterial;
    }

    private void Update()
    {
        // 检查Sprite是否发生变化
        if (mainLayer.sprite != lastMainSprite ||
            gradientLayer.sprite != lastGradientSprite)
        {
            UpdateTextures();
        }

        // 实时更新混合设置
        UpdateBlendSettings();
    }

    private void UpdateTextures()
    {
        if (mainLayer.sprite != null)
            blendMaterial.SetTexture("_MainTex", mainLayer.sprite.texture);
        if (gradientLayer.sprite != null)
            blendMaterial.SetTexture("_GradientTex", gradientLayer.sprite.texture);

        // 更新Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastGradientSprite = gradientLayer.sprite;
    }

    private void UpdateBlendSettings()
    {
        blendMaterial.SetFloat("_GradientOpacity", gradientOpacity);
        blendMaterial.SetFloat("_GradientStrength", gradientStrength);
    }

    private void OnDestroy()
    {
        // 清理材质
        if (blendMaterial != null)
        {
            Destroy(blendMaterial);
        }
    }

    // 公共方法用于外部控制
    public void SetGradientOpacity(float value)
    {
        gradientOpacity = Mathf.Clamp01(value);
    }

    public void SetGradientStrength(float value)
    {
        gradientStrength = Mathf.Clamp(value, 0f, 2f);
    }

    // 动画控制方法
    public void AnimateGradientOpacity(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(gradientOpacity, targetValue, duration, SetGradientOpacity));
    }

    public void AnimateGradientStrength(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(gradientStrength, targetValue, duration, SetGradientStrength));
    }

    private System.Collections.IEnumerator AnimateValue(float startValue, float targetValue, float duration, System.Action<float> setValue)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float currentValue = Mathf.Lerp(startValue, targetValue, t);
            setValue(currentValue);
            yield return null;
        }
        setValue(targetValue);
    }
}