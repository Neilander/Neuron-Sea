using UnityEngine;
using System.Collections;

public class SoftLightBlendController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;      // 主图层
    [SerializeField] private SpriteRenderer softLightLayer; // 柔光图层

    [Header("混合设置")]
    [SerializeField] private float softLightOpacity = 1f;   // 柔光不透明度
    [SerializeField] private float softLightStrength = 1f;  // 柔光强度

    private Material blendMaterial;
    private Sprite lastMainSprite;
    private Sprite lastSoftLightSprite;

    private void Start()
    {
        // 创建材质实例
        blendMaterial = new Material(Shader.Find("Custom/SoftLightBlend"));

        // 保存初始Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastSoftLightSprite = softLightLayer.sprite;

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
            softLightLayer.sprite != lastSoftLightSprite)
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
        if (softLightLayer.sprite != null)
            blendMaterial.SetTexture("_SoftLightTex", softLightLayer.sprite.texture);

        // 更新Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastSoftLightSprite = softLightLayer.sprite;
    }

    private void UpdateBlendSettings()
    {
        blendMaterial.SetFloat("_SoftLightOpacity", softLightOpacity);
        blendMaterial.SetFloat("_SoftLightStrength", softLightStrength);
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
    public void SetSoftLightOpacity(float value)
    {
        softLightOpacity = Mathf.Clamp01(value);
    }

    public void SetSoftLightStrength(float value)
    {
        softLightStrength = Mathf.Clamp(value, 0f, 2f);
    }

    // 动画控制方法
    public void AnimateSoftLightOpacity(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(softLightOpacity, targetValue, duration, SetSoftLightOpacity));
    }

    public void AnimateSoftLightStrength(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(softLightStrength, targetValue, duration, SetSoftLightStrength));
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