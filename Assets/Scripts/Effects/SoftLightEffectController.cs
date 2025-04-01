using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SoftLightEffectController : MonoBehaviour
{
    [Header("柔光设置")]
    [Range(0, 1)]
    public float blendAmount = 1f;
    [Range(0, 1)]
    public float opacity = 1f;

    private SpriteRenderer spriteRenderer;
    private Material material;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // 创建材质实例以避免影响其他使用相同材质的对象
        material = new Material(spriteRenderer.material);
        spriteRenderer.material = material;
    }

    private void OnValidate()
    {
        if (material != null)
        {
            UpdateEffect();
        }
    }

    private void UpdateEffect()
    {
        material.SetFloat("_BlendAmount", blendAmount);
        material.SetFloat("_Opacity", opacity);
    }

    private void OnDestroy()
    {
        // 清理材质实例
        if (material != null)
        {
            Destroy(material);
        }
    }
}