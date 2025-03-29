using UnityEngine;
using System.Collections;

public class OverlayBlendController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;     // 主图层
    [SerializeField] private SpriteRenderer overlayLayer;  // 叠加图层

    [Header("混合设置")]
    [SerializeField] private float overlayOpacity = 1f;    // 叠加不透明度
    [SerializeField] private float overlayStrength = 1f;   // 叠加强度

    private Material blendMaterial;
    private Sprite lastMainSprite;
    private Sprite lastOverlaySprite;

    private void Start()
    {
        // 创建材质实例
        blendMaterial = new Material(Shader.Find("Custom/OverlayBlend"));

        // 保存初始Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastOverlaySprite = overlayLayer.sprite;

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
            overlayLayer.sprite != lastOverlaySprite)
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
        if (overlayLayer.sprite != null)
            blendMaterial.SetTexture("_OverlayTex", overlayLayer.sprite.texture);

        // 更新Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastOverlaySprite = overlayLayer.sprite;
    }

    private void UpdateBlendSettings()
    {
        blendMaterial.SetFloat("_OverlayOpacity", overlayOpacity);
        blendMaterial.SetFloat("_OverlayStrength", overlayStrength);
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
    public void SetOverlayOpacity(float value)
    {
        overlayOpacity = Mathf.Clamp01(value);
    }

    public void SetOverlayStrength(float value)
    {
        overlayStrength = Mathf.Clamp(value, 0f, 2f);
    }

    // 动画控制方法
    public void AnimateOverlayOpacity(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(overlayOpacity, targetValue, duration, SetOverlayOpacity));
    }

    public void AnimateOverlayStrength(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(overlayStrength, targetValue, duration, SetOverlayStrength));
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