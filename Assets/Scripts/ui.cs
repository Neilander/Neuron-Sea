using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerBlendController : MonoBehaviour
{
    [Header("图层设置")]
    [SerializeField] private SpriteRenderer mainLayer;    // 主图层
    [SerializeField] private SpriteRenderer blendLayer;   // 混合图层
    [SerializeField] private SpriteRenderer gradientLayer; // 渐变图层

    [Header("混合设置")]
    [SerializeField] private float opacity = 1f;          // 不透明度
    [SerializeField] private float gradientStrength = 0.5f; // 渐变强度

    private Material blendMaterial;

    private void Start()
    {
        // 创建材质实例
        blendMaterial = new Material(Shader.Find("Custom/LayerBlend"));

        // 设置材质属性
        blendMaterial.SetTexture("_MainTex", mainLayer.sprite.texture);
        blendMaterial.SetTexture("_BlendTex", blendLayer.sprite.texture);
        blendMaterial.SetTexture("_GradientTex", gradientLayer.sprite.texture);
        blendMaterial.SetFloat("_Opacity", opacity);
        blendMaterial.SetFloat("_GradientStrength", gradientStrength);

        // 应用材质
        mainLayer.material = blendMaterial;
    }

    private void Update()
    {
        // 实时更新混合设置
        blendMaterial.SetFloat("_Opacity", opacity);
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
    public void SetOpacity(float value)
    {
        opacity = Mathf.Clamp01(value);
    }

    public void SetGradientStrength(float value)
    {
        gradientStrength = Mathf.Clamp01(value);
    }
}