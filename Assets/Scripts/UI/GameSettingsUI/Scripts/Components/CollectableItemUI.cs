using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 收集物项目UI - 图鉴中的单个收集物显示项
/// </summary>
public class CollectableItemUI : MonoBehaviour
{
    [Header("UI元素")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private GameObject collectedIndicator;
    [SerializeField] private GameObject lockedOverlay;

    [Header("稀有度颜色")]
    [SerializeField] private Color normalColor = new Color(0.8f, 0.8f, 0.8f);
    [SerializeField] private Color rareColor = new Color(0.0f, 0.5f, 1.0f);
    [SerializeField] private Color epicColor = new Color(0.5f, 0.0f, 1.0f);
    [SerializeField] private Color legendaryColor = new Color(1.0f, 0.5f, 0.0f);
    [SerializeField] private Color mythicColor = new Color(1.0f, 0.0f, 0.0f);

    [Header("效果设置")]
    [SerializeField] private float uncollectedAlpha = 0.6f;
    [SerializeField] private bool useGrayscaleForUncollected = true;

    // 当前数据
    private CollectableManager.CollectableData data;

    // 灰度着色器材质
    private Material grayscaleMaterial;

    private void Awake()
    {
        // 创建灰度材质（如果需要）
        if (useGrayscaleForUncollected && iconImage != null)
        {
            Shader grayscaleShader = Shader.Find("UI/Grayscale");
            if (grayscaleShader != null)
            {
                grayscaleMaterial = new Material(grayscaleShader);
            }
            else
            {
                Debug.LogWarning("找不到灰度着色器，无法应用灰度效果。");
            }
        }
    }

    /// <summary>
    /// 设置收集物数据
    /// </summary>
    public void SetData(CollectableManager.CollectableData collectableData)
    {
        data = collectableData;

        // 设置图标
        if (iconImage != null)
        {
            iconImage.sprite = data.icon;

            // 对未收集的应用效果
            if (!data.isCollected)
            {
                // 设置半透明
                Color color = iconImage.color;
                color.a = uncollectedAlpha;
                iconImage.color = color;

                // 应用灰度效果（如果启用）
                if (useGrayscaleForUncollected && grayscaleMaterial != null)
                {
                    iconImage.material = grayscaleMaterial;
                }
            }
            else
            {
                // 恢复正常显示
                Color color = iconImage.color;
                color.a = 1.0f;
                iconImage.color = color;
                iconImage.material = null;
            }
        }

        // 设置名称
        if (nameText != null)
        {
            nameText.text = data.displayName;
        }

        // 设置稀有度文本和颜色
        if (rarityText != null)
        {
            string rarityString = GetRarityString(data.rarity);
            rarityText.text = rarityString;
            rarityText.color = GetRarityColor(data.rarity);
        }

        // 显示收集状态指示器
        if (collectedIndicator != null)
        {
            collectedIndicator.SetActive(data.isCollected);
        }

        // 显示锁定覆盖层
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!data.isCollected);
        }
    }

    /// <summary>
    /// 获取稀有度字符串
    /// </summary>
    private string GetRarityString(int rarity)
    {
        switch (rarity)
        {
            case 1: return "普通";
            case 2: return "稀有";
            case 3: return "史诗";
            case 4: return "传说";
            case 5: return "神话";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取稀有度对应的颜色
    /// </summary>
    private Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 1: return normalColor;
            case 2: return rareColor;
            case 3: return epicColor;
            case 4: return legendaryColor;
            case 5: return mythicColor;
            default: return Color.white;
        }
    }
}