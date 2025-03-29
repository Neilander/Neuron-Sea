using UnityEngine;
using System.Collections;

public class MultiplyBlendController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;    // 主图层
    [SerializeField] private SpriteRenderer multiplyLayer; // 正片叠底图层

    [Header("混合设置")]
    [SerializeField] private float multiplyOpacity = 1f;  // 正片叠底不透明度

    private Material blendMaterial;
    private Sprite lastMainSprite;
    private Sprite lastMultiplySprite;

    private void Start()
    {
        // 创建材质实例
        blendMaterial = new Material(Shader.Find("Custom/MultiplyBlend"));

        // 保存初始Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastMultiplySprite = multiplyLayer.sprite;

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
            multiplyLayer.sprite != lastMultiplySprite)
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
        if (multiplyLayer.sprite != null)
            blendMaterial.SetTexture("_MultiplyTex", multiplyLayer.sprite.texture);

        // 更新Sprite引用
        lastMainSprite = mainLayer.sprite;
        lastMultiplySprite = multiplyLayer.sprite;
    }

    private void UpdateBlendSettings()
    {
        blendMaterial.SetFloat("_MultiplyOpacity", multiplyOpacity);
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
    public void SetMultiplyOpacity(float value)
    {
        multiplyOpacity = Mathf.Clamp01(value);
    }

    // 动画控制方法
    public void AnimateMultiplyOpacity(float targetValue, float duration)
    {
        StartCoroutine(AnimateValue(multiplyOpacity, targetValue, duration, SetMultiplyOpacity));
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