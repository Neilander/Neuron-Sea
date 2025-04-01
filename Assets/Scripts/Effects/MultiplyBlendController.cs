using UnityEngine;

/// <summary>
/// 正片叠底混合效果控制器
/// </summary>
public class MultiplyBlendController : MonoBehaviour
{
    [Header("混合设置")]
    [Tooltip("正片叠底纹理")]
    public Texture2D multiplyTexture;

    [Tooltip("混合不透明度 (0-1)")]
    [Range(0f, 1f)]
    public float opacity = 1f;

    [Tooltip("混合强度 (0-2)")]
    [Range(0f, 2f)]
    public float strength = 1f;

    [Tooltip("纹理缩放 (0.1-2)")]
    [Range(0.1f, 2f)]
    public float scale = 1f;

    [Tooltip("纹理偏移")]
    public Vector2 offset = Vector2.zero;

    [Header("运行时控制")]
    [Tooltip("是否在Start时自动设置")]
    public bool autoSetup = true;

    private Material materialInstance;
    private static readonly int MultiplyTex = Shader.PropertyToID("_MultiplyTex");
    private static readonly int MultiplyOpacity = Shader.PropertyToID("_MultiplyOpacity");
    private static readonly int MultiplyStrength = Shader.PropertyToID("_MultiplyStrength");
    private static readonly int MultiplyScale = Shader.PropertyToID("_MultiplyScale");
    private static readonly int MultiplyOffset = Shader.PropertyToID("_MultiplyOffset");

    private void Start()
    {
        if (autoSetup)
        {
            SetupEffect();
        }
    }

    /// <summary>
    /// 设置正片叠底效果
    /// </summary>
    public void SetupEffect()
    {
        // 获取SpriteRenderer组件
        var spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("未找到SpriteRenderer组件！");
            return;
        }

        // 创建材质实例
        if (materialInstance == null)
        {
            var shader = Shader.Find("Custom/SceneMultiplyBlend");
            if (shader == null)
            {
                Debug.LogError("未找到SceneMultiplyBlend shader！");
                return;
            }
            materialInstance = new Material(shader);
            spriteRenderer.material = materialInstance;
        }

        // 设置参数
        UpdateEffect();
    }

    /// <summary>
    /// 更新效果参数
    /// </summary>
    public void UpdateEffect()
    {
        if (materialInstance == null) return;

        // 设置纹理
        if (multiplyTexture != null)
        {
            materialInstance.SetTexture(MultiplyTex, multiplyTexture);
        }

        // 设置其他参数
        materialInstance.SetFloat(MultiplyOpacity, opacity);
        materialInstance.SetFloat(MultiplyStrength, strength);
        materialInstance.SetFloat(MultiplyScale, scale);
        materialInstance.SetVector(MultiplyOffset, new Vector4(offset.x, offset.y, 0, 0));
    }

    /// <summary>
    /// 设置正片叠底纹理
    /// </summary>
    public void SetMultiplyTexture(Texture2D texture)
    {
        multiplyTexture = texture;
        UpdateEffect();
    }

    /// <summary>
    /// 设置不透明度
    /// </summary>
    public void SetOpacity(float value)
    {
        opacity = Mathf.Clamp01(value);
        UpdateEffect();
    }

    /// <summary>
    /// 设置混合强度
    /// </summary>
    public void SetStrength(float value)
    {
        strength = Mathf.Clamp(value, 0f, 2f);
        UpdateEffect();
    }

    /// <summary>
    /// 设置纹理缩放
    /// </summary>
    public void SetScale(float value)
    {
        scale = Mathf.Clamp(value, 0.1f, 2f);
        UpdateEffect();
    }

    /// <summary>
    /// 设置纹理偏移
    /// </summary>
    public void SetOffset(Vector2 value)
    {
        offset = value;
        UpdateEffect();
    }

    private void OnDestroy()
    {
        // 清理材质实例
        if (materialInstance != null)
        {
            if (Application.isPlaying)
            {
                Destroy(materialInstance);
            }
            else
            {
                DestroyImmediate(materialInstance);
            }
        }
    }
}