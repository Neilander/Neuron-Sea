using UnityEngine;
using System.IO;

public class ShaderExporter : MonoBehaviour
{
    public Camera exportCamera; // 绑定渲染用的相机

    public RenderTexture exportRT; // 绑定创建的RenderTexture

    public string savePath = "ExportedImages"; // 保存路径（相对项目根目录）

    void Start(){
        // 确保文件夹存在
        Directory.CreateDirectory(Application.dataPath + "/" + savePath);
    }

    // 触发导出操作
    public void ExportImage(){
        // 强制渲染一帧到RenderTexture
        exportCamera.Render();

        // 创建Texture2D并读取RenderTexture数据
        Texture2D tex = new Texture2D(exportRT.width, exportRT.height, TextureFormat.RGB24, false);
        RenderTexture.active = exportRT;
        tex.ReadPixels(new Rect(0, 0, exportRT.width, exportRT.height), 0, 0);
        tex.Apply();
        RenderTexture.active = null;

        // 将Texture2D编码为PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        // 保存文件
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string filePath = Application.dataPath + "/" + savePath + "/Exported_" + timestamp + ".png";
        File.WriteAllBytes(filePath, bytes);
        Debug.Log("图片已保存至：" + filePath);
    }
}