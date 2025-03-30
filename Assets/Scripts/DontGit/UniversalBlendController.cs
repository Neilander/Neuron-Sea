using UnityEngine;
using System.Collections;

public class UniversalBlendController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;      // 主图层
    [SerializeField] private SpriteRenderer middleLayer;    // 中间层
    [SerializeField] private SpriteRenderer effectLayer;    // 效果层

    [Header("混合设置")]
    [SerializeField] private BlendMode blendMode = BlendMode.Multiply;  // 混合模式
    [SerializeField] private float effectOpacity = 1f;      // 效果层不透明度
    [SerializeField] private float effectStrength = 1f;     // 效果层强度
    [SerializeField] private float middleBlend = 0.5f;      // 中间层混合度
    [SerializeField] private bool useMiddleLayer = false;   // 是否使用中间层
    [SerializeField] private bool showMiddleLayer = false;  // 是否显示中间层
    [SerializeField] private bool showEffectLayer = true;   // 是否显示效果层

    [Header("图层位置")]
    [SerializeField] private Vector2 mainOffset = Vector2.zero;    // 主图层偏移
    [SerializeField] private Vector2 middleOffset = Vector2.zero;  // 中间层偏移
    [SerializeField] private Vector2 effectOffset = Vector2.zero;  // 效果层偏移
    [SerializeField] private Vector2 mainScale = Vector2.one;      // 主图层缩放
    [SerializeField] private Vector2 middleScale = Vector2.one;    // 中间层缩放
    [SerializeField] private Vector2 effectScale = Vector2.one;    // 效果层缩放

    private Material blendMaterial;
    private Sprite lastMainSprite;
    private Sprite lastMiddleSprite;
    private Sprite lastEffectSprite;
    private BlendMode lastBlendMode;

    private void Start()
    {
        // 创建材质实例
        UpdateMaterial();

        // 保存初始Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastMiddleSprite = middleLayer.sprite;
        lastEffectSprite = effectLayer.sprite;
        lastBlendMode = blendMode;

        // 设置材质属性
        UpdateTextures();
        UpdateBlendSettings();

        // 应用材质
        mainLayer.material = blendMaterial;
    }

    private void Update()
    {
        // 检查混合模式是否改变
        if (blendMode != lastBlendMode)
        {
            UpdateMaterial();
            lastBlendMode = blendMode;
        }

        // 检查Sprite是否发生变化
        if (mainLayer.sprite != lastMainSprite ||
            middleLayer.sprite != lastMiddleSprite ||
            effectLayer.sprite != lastEffectSprite)
        {
            UpdateTextures();
        }

        // 实时更新混合设置
        UpdateBlendSettings();
    }

    private void UpdateMaterial()
    {
        // 清理旧材质
        if (blendMaterial != null)
        {
            Destroy(blendMaterial);
        }

        // 根据混合模式创建新材质
        string shaderName = GetShaderName();
        blendMaterial = new Material(Shader.Find(shaderName));
    }

    private string GetShaderName()
    {
        switch (blendMode)
        {
            case BlendMode.Multiply:
                return "Custom/MultiplyBlend";
            case BlendMode.Screen:
                return "Custom/ScreenBlend";
            case BlendMode.Overlay:
                return "Custom/OverlayBlend";
            case BlendMode.SoftLight:
                return "Custom/SoftLightBlend";
            case BlendMode.GradientMap:
                return "Custom/GradientMap";
            default:
                return "Custom/MultiplyBlend";
        }
    }

    private void UpdateTextures()
    {
        if (mainLayer.sprite != null)
            blendMaterial.SetTexture("_MainTex", mainLayer.sprite.texture);
        if (middleLayer.sprite != null)
            blendMaterial.SetTexture("_MiddleTex", middleLayer.sprite.texture);
        if (effectLayer.sprite != null)
            blendMaterial.SetTexture(GetEffectTextureName(), effectLayer.sprite.texture);

        // 更新Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastMiddleSprite = middleLayer.sprite;
        lastEffectSprite = effectLayer.sprite;
    }

    private string GetEffectTextureName()
    {
        switch (blendMode)
        {
            case BlendMode.Multiply:
                return "_MultiplyTex";
            case BlendMode.Screen:
                return "_ScreenTex";
            case BlendMode.Overlay:
                return "_OverlayTex";
            case BlendMode.SoftLight:
                return "_SoftLightTex";
            case BlendMode.GradientMap:
                return "_GradientTex";
            default:
                return "_MultiplyTex";
        }
    }

    private void UpdateBlendSettings()
    {
        // 设置通用参数
        blendMaterial.SetFloat("_UseMiddleLayer", useMiddleLayer ? 1 : 0);
        blendMaterial.SetFloat("_ShowMiddleLayer", showMiddleLayer ? 1 : 0);
        blendMaterial.SetFloat("_ShowEffectLayer", showEffectLayer ? 1 : 0);
        blendMaterial.SetFloat("_MiddleBlend", middleBlend);

        // 设置效果层参数
        blendMaterial.SetFloat(GetOpacityPropertyName(), effectOpacity);
        blendMaterial.SetFloat(GetStrengthPropertyName(), effectStrength);

        // 设置图层位置和缩放
        blendMaterial.SetVector("_MainOffset", mainOffset);
        blendMaterial.SetVector("_MiddleOffset", middleOffset);
        blendMaterial.SetVector("_EffectOffset", effectOffset);
        blendMaterial.SetVector("_MainScale", mainScale);
        blendMaterial.SetVector("_MiddleScale", middleScale);
        blendMaterial.SetVector("_EffectScale", effectScale);
    }

    private string GetOpacityPropertyName()
    {
        switch (blendMode)
        {
            case BlendMode.Multiply:
                return "_MultiplyOpacity";
            case BlendMode.Screen:
                return "_ScreenOpacity";
            case BlendMode.Overlay:
                return "_OverlayOpacity";
            case BlendMode.SoftLight:
                return "_SoftLightOpacity";
            case BlendMode.GradientMap:
                return "_GradientOpacity";
            default:
                return "_MultiplyOpacity";
        }
    }

    private string GetStrengthPropertyName()
    {
        switch (blendMode)
        {
            case BlendMode.Multiply:
                return "_MultiplyStrength";
            case BlendMode.Screen:
                return "_ScreenStrength";
            case BlendMode.Overlay:
                return "_OverlayStrength";
            case BlendMode.SoftLight:
                return "_SoftLightStrength";
            case BlendMode.GradientMap:
                return "_GradientStrength";
            default:
                return "_MultiplyStrength";
        }
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
    public void SetBlendMode(BlendMode mode)
    {
        blendMode = mode;
    }

    public void SetEffectOpacity(float value)
    {
        effectOpacity = Mathf.Clamp01(value);
    }

    public void SetEffectStrength(float value)
    {
        effectStrength = Mathf.Clamp(value, 0f, 2f);
    }

    public void SetMiddleBlend(float value)
    {
        middleBlend = Mathf.Clamp01(value);
    }

    public void SetUseMiddleLayer(bool value)
    {
        useMiddleLayer = value;
    }

    public void SetShowMiddleLayer(bool value)
    {
        showMiddleLayer = value;
    }

    public void SetShowEffectLayer(bool value)
    {
        showEffectLayer = value;
    }

    // 添加新的公共方法用于控制图层位置和缩放
    public void SetMainOffset(Vector2 offset)
    {
        mainOffset = offset;
    }

    public void SetMiddleOffset(Vector2 offset)
    {
        middleOffset = offset;
    }

    public void SetEffectOffset(Vector2 offset)
    {
        effectOffset = offset;
    }

    public void SetMainScale(Vector2 scale)
    {
        mainScale = scale;
    }

    public void SetMiddleScale(Vector2 scale)
    {
        middleScale = scale;
    }

    public void SetEffectScale(Vector2 scale)
    {
        effectScale = scale;
    }

    // 动画控制方法
    public void AnimateEffectOpacity(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(effectOpacity, targetValue, duration, SetEffectOpacity));
    }

    public void AnimateEffectStrength(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(effectStrength, targetValue, duration, SetEffectStrength));
    }

    public void AnimateMiddleBlend(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(middleBlend, targetValue, duration, SetMiddleBlend));
    }

    public void AnimateMainOffset(Vector2 targetOffset, float duration)
    {
        StartCoroutine(AnimateVector2(mainOffset, targetOffset, duration, SetMainOffset));
    }

    public void AnimateMiddleOffset(Vector2 targetOffset, float duration)
    {
        StartCoroutine(AnimateVector2(middleOffset, targetOffset, duration, SetMiddleOffset));
    }

    public void AnimateEffectOffset(Vector2 targetOffset, float duration)
    {
        StartCoroutine(AnimateVector2(effectOffset, targetOffset, duration, SetEffectOffset));
    }

    public void AnimateMainScale(Vector2 targetScale, float duration)
    {
        StartCoroutine(AnimateVector2(mainScale, targetScale, duration, SetMainScale));
    }

    public void AnimateMiddleScale(Vector2 targetScale, float duration)
    {
        StartCoroutine(AnimateVector2(middleScale, targetScale, duration, SetMiddleScale));
    }

    public void AnimateEffectScale(Vector2 targetScale, float duration)
    {
        StartCoroutine(AnimateVector2(effectScale, targetScale, duration, SetEffectScale));
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

    private System.Collections.IEnumerator AnimateVector2(Vector2 startValue, Vector2 targetValue, float duration, System.Action<Vector2> setValue)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Vector2 currentValue = Vector2.Lerp(startValue, targetValue, t);
            setValue(currentValue);
            yield return null;
        }
        setValue(targetValue);
    }
}