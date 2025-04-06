using UnityEngine;

public class RenderTextureToSprite : MonoBehaviour
{
    public Camera renderCamera; // 用于渲染的相机

    private RenderTexture renderTexture; // RenderTexture 对象

    private Texture2D texture2D; // 用于转换的 Texture2D

    private Sprite sprite; // 最终的 Sprite

    void Start(){
        // 创建 RenderTexture
        renderTexture = new RenderTexture(512, 512, 24, RenderTextureFormat.Default);
        renderCamera.targetTexture = renderTexture;

        // 创建 Texture2D
        texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);

        // 将 RenderTexture 转换为 Texture2D
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentRT;

        // 创建 Sprite
        sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero);

        // 将 Sprite 赋值给 SpriteRenderer
        GetComponent<SpriteRenderer>().sprite = sprite;
    }

    void OnDestroy(){
        // 释放 RenderTexture
        if (renderTexture != null) {
            renderTexture.Release();
        }
    }
}