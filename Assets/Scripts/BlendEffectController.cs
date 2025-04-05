using UnityEngine;
using UnityEngine.UI;

public class BlendEffectController : MonoBehaviour
{
    public enum BlendMode
    {
        Multiply,

        Screen,

        Overlay,

        SoftLight,

        ColorDodge,

        PinLight
    }

    [Header("混合模式设置")] public BlendMode currentBlendMode = BlendMode.SoftLight;

    [Header("Shader引用")] public Shader multiplyShader;

    public Shader screenShader;

    public Shader overlayShader;

    public Shader softLightShader;

    public Shader colorDodgeShader;

    public Shader pinLightShader;

    [Header("纹理设置")] public Texture2D mainTexture;

    public Texture2D blendTexture;

    [Header("混合参数")] [Range(0, 1)] public float blendAmount = 1.0f;

    [Range(0, 1)] public float opacity = 1.0f;

    private Material currentMaterial;

    private SpriteRenderer spriteRenderer;

    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private static readonly int BlendTex = Shader.PropertyToID("_BlendTex");

    private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");

    private static readonly int Opacity = Shader.PropertyToID("_Opacity");

    void Start(){
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            Debug.LogError("需要SpriteRenderer组件!");
            return;
        }

        // 初始化材质
        UpdateBlendMode();
    }

    void Update(){
        if (currentMaterial != null) {
            // 更新材质参数
            UpdateMaterialProperties();
        }
    }

    public void UpdateBlendMode(){
        // 根据当前混合模式选择shader
        Shader selectedShader = currentBlendMode switch
        {
            BlendMode.Multiply => multiplyShader,
            BlendMode.Screen => screenShader,
            BlendMode.Overlay => overlayShader,
            BlendMode.SoftLight => softLightShader,
            BlendMode.ColorDodge => colorDodgeShader,
            BlendMode.PinLight => pinLightShader,
            _ => softLightShader
        };

        // 创建新材质
        if (selectedShader != null) {
            if (currentMaterial != null) {
                Destroy(currentMaterial);
            }

            currentMaterial = new Material(selectedShader);
            spriteRenderer.material = currentMaterial;

            // 初始化材质属性
            UpdateMaterialProperties();
        }
        else {
            Debug.LogError($"未找到{currentBlendMode}对应的Shader!");
        }
    }

    private void UpdateMaterialProperties(){
        if (currentMaterial == null) return;

        // 更新纹理
        if (mainTexture != null &&
            currentBlendMode != BlendMode.ColorDodge &&
            currentBlendMode != BlendMode.PinLight) {
            if (currentMaterial.HasProperty(MainTex)) {
                currentMaterial.SetTexture(MainTex, mainTexture);
            }
        }

        // 设置混合纹理
        if (blendTexture != null) {
            currentMaterial.SetTexture(BlendTex, blendTexture);
        }

        // 更新混合参数
        currentMaterial.SetFloat(BlendAmount, blendAmount);
        currentMaterial.SetFloat(Opacity, opacity);
    }

    // 公共方法用于从外部控制
    public void SetBlendMode(BlendMode mode){
        currentBlendMode = mode;
        UpdateBlendMode();
    }

    public void SetBlendAmount(float amount){
        blendAmount = Mathf.Clamp01(amount);
        if (currentMaterial != null) {
            currentMaterial.SetFloat(BlendAmount, blendAmount);
        }
    }

    public void SetOpacity(float value){
        opacity = Mathf.Clamp01(value);
        if (currentMaterial != null) {
            currentMaterial.SetFloat(Opacity, opacity);
        }
    }

    public void SetMainTexture(Texture2D texture){
        mainTexture = texture;
        if (currentMaterial != null &&
            currentBlendMode != BlendMode.ColorDodge &&
            currentBlendMode != BlendMode.PinLight) {
            if (currentMaterial.HasProperty(MainTex)) {
                currentMaterial.SetTexture(MainTex, texture);
            }
        }
    }

    public void SetBlendTexture(Texture2D texture){
        blendTexture = texture;
        if (currentMaterial != null && texture != null) {
            currentMaterial.SetTexture(BlendTex, texture);
        }
    }

    void OnDestroy(){
        // 清理材质
        if (currentMaterial != null) {
            Destroy(currentMaterial);
        }
    }
}