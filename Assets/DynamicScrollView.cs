using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DynamicScrollView : MonoBehaviour
{
    [Header("绑定组件")] public Transform contentRoot; // ScrollView的Content对象

    public GameObject itemPrefab; // 图片项预制体

    [Header("测试图片")] public List<Sprite> demoImages; // 编辑器测试用图片

    void Start(){
        // 测试：加载编辑器中的图片
        LoadImages(demoImages);
    }

    // 动态加载图片方法（可外部调用）
    public void LoadImages(List<Sprite> images){
        ClearContent();

        foreach (var sprite in images) {
            GameObject newItem = Instantiate(itemPrefab, contentRoot);
            Image imgComponent = newItem.GetComponent<Image>();
            imgComponent.sprite = sprite;
            
            RectTransform rt = newItem.GetComponent<RectTransform>();
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 300);
            // 动态调整宽高比
            AspectRatioFitter arf = newItem.GetComponent<AspectRatioFitter>();
            if (arf != null) {
                arf.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
        }

        // 强制刷新布局（解决Content大小计算问题）
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot as RectTransform);
    }

    // 清空已有内容
    private void ClearContent(){
        foreach (Transform child in contentRoot) {
            Destroy(child.gameObject);
        }
    }
    //如果要从resource文件夹动态加载
    public void LoadFromResources(string folderPath){
        Sprite[] loadedSprites = Resources.LoadAll<Sprite>(folderPath);
        LoadImages(new List<Sprite>(loadedSprites));
    }
}
