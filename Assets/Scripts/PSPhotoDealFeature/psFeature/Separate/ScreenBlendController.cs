using UnityEngine;
using System.Collections;

public class ScreenBlendController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;    // 主图层
    [SerializeField] private SpriteRenderer screenLayer;  // 滤色图层

    [Header("混合设置")]
    [SerializeField] private float screenOpacity = 1f;    // 滤色不透明度
    [SerializeField] private float screenStrength = 1f;   // 滤色强度

    private Material blendMaterial;
    private Sprite lastMainSprite;
    private Sprite lastScreenSprite;

    private void Start()
    {
        // 创建材质实例
        blendMaterial = new Material(Shader.Find("Custom/ScreenBlend"));

        // 保存初始Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastScreenSprite = screenLayer.sprite;

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
            screenLayer.sprite != lastScreenSprite)
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
        if (screenLayer.sprite != null)
            blendMaterial.SetTexture("_ScreenTex", screenLayer.sprite.texture);

        // 更新Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastScreenSprite = screenLayer.sprite;
    }

    private void UpdateBlendSettings()
    {
        blendMaterial.SetFloat("_ScreenOpacity", screenOpacity);
        blendMaterial.SetFloat("_ScreenStrength", screenStrength);
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
    public void SetScreenOpacity(float value)
    {
        screenOpacity = Mathf.Clamp01(value);
    }

    public void SetScreenStrength(float value)
    {
        screenStrength = Mathf.Clamp(value, 0f, 2f);
    }

    // 动画控制方法
    public void AnimateScreenOpacity(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(screenOpacity, targetValue, duration, SetScreenOpacity));
    }

    public void AnimateScreenStrength(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(screenStrength, targetValue, duration, SetScreenStrength));
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