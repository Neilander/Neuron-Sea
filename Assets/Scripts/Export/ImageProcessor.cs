using UnityEngine;
using System.IO;

public class ImageProcessor : MonoBehaviour
{
    public Texture2D sourceTexture;

    public Shader processingShader;

    void ProcessImage(){
        // 创建临时RenderTexture
        RenderTexture rt = new RenderTexture(
            sourceTexture.width,
            sourceTexture.height,
            0
        );

        // 应用Shader处理
        Material mat = new Material(processingShader);
        Graphics.Blit(sourceTexture, rt, mat);

        // 转换回Texture2D
        Texture2D result = new Texture2D(
            sourceTexture.width,
            sourceTexture.height,
            TextureFormat.ARGB32,
            false
        );

        RenderTexture.active = rt;
        result.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        result.Apply();

        // 保存文件
        byte[] bytes = result.EncodeToPNG();
        string path = Path.Combine(
            Application.persistentDataPath,
            "processed_image.png"
        );
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Saved to: {path}");

        // 清理资源
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(result);
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Space)) {
            ProcessImage();
        }
    }
}