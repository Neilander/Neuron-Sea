using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 收集物观察者 - 附加到现有的收集物上，监听销毁事件并报告给收集物管理器
/// </summary>
public class CollectableObserver : MonoBehaviour
{
    [Tooltip("收集物的唯一ID，如果留空则会自动生成")]
    [SerializeField] private string collectableId;

    [Tooltip("收集物的显示名称")]
    [SerializeField] private string displayName;

    [Tooltip("收集物的描述")]
    [SerializeField] private string description;

    [Tooltip("收集物的稀有度 (1-5)")]
    [SerializeField] private int rarity = 1;

    [Tooltip("收集物的图标")]
    [SerializeField] private Sprite icon;

    private bool hasRegistered = false;

    private void Start()
    {
        // 如果没有指定ID，生成一个唯一ID
        if (string.IsNullOrEmpty(collectableId))
        {
            collectableId = System.Guid.NewGuid().ToString();
        }

        // 如果没有指定图标，尝试使用SpriteRenderer的图标
        if (icon == null)
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                icon = spriteRenderer.sprite;
            }
        }

        // 如果没有指定显示名称，使用游戏对象的名称
        if (string.IsNullOrEmpty(displayName))
        {
            displayName = gameObject.name;
        }

        // 注册到收集物管理器
        RegisterToManager();
    }

    private void OnDestroy()
    {
        // 如果是由于游戏对象被销毁（比如被收集），则报告给管理器
        if (this.isActiveAndEnabled && Time.frameCount > 1 && hasRegistered)
        {
            ReportCollected();
        }
    }

    /// <summary>
    /// 注册到收集物管理器
    /// </summary>
    private void RegisterToManager()
    {
        if (CollectableManager.Instance != null)
        {
            CollectableManager.Instance.RegisterCollectable(collectableId);
            hasRegistered = true;
        }
        else
        {
            Debug.LogWarning("收集物管理器未初始化，无法注册收集物: " + displayName);
        }
    }

    /// <summary>
    /// 报告收集物已被收集
    /// </summary>
    private void ReportCollected()
    {
        if (CollectableManager.Instance != null)
        {
            // 补充收集物数据
            CollectableManager.CollectableData collectableData = new CollectableManager.CollectableData
            {
                id = collectableId,
                displayName = displayName,
                description = description,
                icon = icon,
                rarity = rarity
            };

            // 标记为已收集
            CollectableManager.Instance.MarkAsCollected(collectableId);

            Debug.Log("收集物已收集: " + displayName);
        }
    }

    /// <summary>
    /// 手动触发收集事件（用于测试）
    /// </summary>
    public void ForceCollect()
    {
        ReportCollected();
    }
}