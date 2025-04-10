using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 收集物面板 - 显示游戏中收集物的图鉴和状态
/// </summary>
public class CollectablesPanel : MonoBehaviour
{
    [Header("内容区域")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject collectableItemPrefab;
    [SerializeField] private GameObject noItemsMessage;

    [Header("筛选选项")]
    [SerializeField] private TMP_Dropdown filterDropdown;
    [SerializeField] private Toggle showCollectedToggle;
    [SerializeField] private Toggle showUncollectedToggle;

    [Header("统计信息")]
    [SerializeField] private TMP_Text collectibleCountText;
    [SerializeField] private Slider collectProgressSlider;

    [Header("详情面板")]
    [SerializeField] private GameObject detailPanel;
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text detailNameText;
    [SerializeField] private TMP_Text detailDescriptionText;
    [SerializeField] private TMP_Text detailCollectionTimeText;
    [SerializeField] private GameObject collectedStatus;
    [SerializeField] private GameObject uncollectedStatus;

    // 当前显示的收集物项
    private List<GameObject> spawnedItems = new List<GameObject>();

    // 当前选中的收集物
    private CollectableManager.CollectableData selectedCollectable;

    private void OnEnable()
    {
        // 每次面板显示时刷新收集物列表
        RefreshCollectablesView();

        // 隐藏详情面板
        if (detailPanel != null)
            detailPanel.SetActive(false);

        // 注册事件
        if (CollectableManager.Instance != null)
        {
            CollectableManager.Instance.OnCollectablesUpdated += OnCollectablesUpdated;
        }
    }

    private void OnDisable()
    {
        // 注销事件
        if (CollectableManager.Instance != null)
        {
            CollectableManager.Instance.OnCollectablesUpdated -= OnCollectablesUpdated;
        }
    }

    private void Start()
    {
        // 初始化下拉菜单
        if (filterDropdown != null)
        {
            filterDropdown.ClearOptions();
            List<string> options = new List<string>
            {
                "全部",
                "按稀有度: 普通",
                "按稀有度: 稀有",
                "按稀有度: 史诗",
                "按稀有度: 传说",
                "按稀有度: 神话"
            };
            filterDropdown.AddOptions(options);
            filterDropdown.onValueChanged.AddListener(OnFilterChanged);
        }

        // 初始化复选框
        if (showCollectedToggle != null)
            showCollectedToggle.onValueChanged.AddListener(OnToggleChanged);

        if (showUncollectedToggle != null)
            showUncollectedToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    /// <summary>
    /// 收集物数据更新时的回调
    /// </summary>
    private void OnCollectablesUpdated()
    {
        RefreshCollectablesView();
    }

    /// <summary>
    /// 筛选选项改变时的回调
    /// </summary>
    private void OnFilterChanged(int value)
    {
        RefreshCollectablesView();
    }

    /// <summary>
    /// 显示选项改变时的回调
    /// </summary>
    private void OnToggleChanged(bool value)
    {
        RefreshCollectablesView();
    }

    /// <summary>
    /// 刷新收集物视图
    /// </summary>
    public void RefreshCollectablesView()
    {
        // 清除现有项目
        ClearItems();

        // 获取收集物数据
        if (CollectableManager.Instance != null)
        {
            List<CollectableManager.CollectableData> collectables = GetFilteredCollectables();

            // 显示或隐藏"没有收集物"的消息
            if (noItemsMessage != null)
                noItemsMessage.SetActive(collectables.Count == 0);

            // 生成收集物项
            foreach (var collectableData in collectables)
            {
                CreateCollectableItem(collectableData);
            }

            // 更新进度文本和进度条
            UpdateProgressInfo();
        }
    }

    /// <summary>
    /// 根据筛选条件获取收集物
    /// </summary>
    private List<CollectableManager.CollectableData> GetFilteredCollectables()
    {
        List<CollectableManager.CollectableData> result = new List<CollectableManager.CollectableData>();

        // 先获取所有收集物
        List<CollectableManager.CollectableData> allCollectables = CollectableManager.Instance.GetAllCollectables();

        // 应用稀有度过滤
        if (filterDropdown != null && filterDropdown.value > 0)
        {
            int rarityFilter = filterDropdown.value;
            foreach (var data in allCollectables)
            {
                if (data.rarity == rarityFilter)
                    result.Add(data);
            }
        }
        else
        {
            result.AddRange(allCollectables);
        }

        // 应用收集状态过滤
        bool showCollected = showCollectedToggle == null || showCollectedToggle.isOn;
        bool showUncollected = showUncollectedToggle == null || showUncollectedToggle.isOn;

        if (!showCollected || !showUncollected)
        {
            List<CollectableManager.CollectableData> filteredByStatus = new List<CollectableManager.CollectableData>();

            foreach (var data in result)
            {
                if ((showCollected && data.isCollected) || (showUncollected && !data.isCollected))
                {
                    filteredByStatus.Add(data);
                }
            }

            result = filteredByStatus;
        }

        return result;
    }

    /// <summary>
    /// 创建收集物项目
    /// </summary>
    private void CreateCollectableItem(CollectableManager.CollectableData data)
    {
        if (contentContainer == null || collectableItemPrefab == null)
            return;

        GameObject item = Instantiate(collectableItemPrefab, contentContainer);
        spawnedItems.Add(item);

        // 设置项目数据
        CollectableItemUI itemUI = item.GetComponent<CollectableItemUI>();
        if (itemUI != null)
        {
            itemUI.SetData(data);

            // 添加点击事件
            Button button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => ShowCollectableDetails(data));
            }
        }
    }

    /// <summary>
    /// 显示收集物详细信息
    /// </summary>
    private void ShowCollectableDetails(CollectableManager.CollectableData data)
    {
        if (detailPanel == null)
            return;

        selectedCollectable = data;

        // 显示详情面板
        detailPanel.SetActive(true);

        // 更新详情内容
        if (detailIcon != null)
            detailIcon.sprite = data.icon;

        if (detailNameText != null)
            detailNameText.text = data.displayName;

        if (detailDescriptionText != null)
            detailDescriptionText.text = data.description;

        // 显示收集状态
        if (collectedStatus != null)
            collectedStatus.SetActive(data.isCollected);

        if (uncollectedStatus != null)
            uncollectedStatus.SetActive(!data.isCollected);

        // 显示收集时间
        if (detailCollectionTimeText != null)
        {
            if (data.isCollected && data.collectedTime != default)
            {
                detailCollectionTimeText.text = "收集于: " + data.collectedTime.ToString("yyyy-MM-dd HH:mm:ss");
                detailCollectionTimeText.gameObject.SetActive(true);
            }
            else
            {
                detailCollectionTimeText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 关闭详情面板
    /// </summary>
    public void CloseDetailPanel()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        selectedCollectable = null;
    }

    /// <summary>
    /// 更新进度信息
    /// </summary>
    private void UpdateProgressInfo()
    {
        if (CollectableManager.Instance == null)
            return;

        int total = CollectableManager.Instance.GetAllCollectables().Count;
        int collected = CollectableManager.Instance.GetCollectedCollectables().Count;

        if (collectibleCountText != null)
        {
            collectibleCountText.text = string.Format("收集进度: {0}/{1} ({2:P0})",
                collected, total, total > 0 ? (float)collected / total : 0);
        }

        if (collectProgressSlider != null)
        {
            collectProgressSlider.maxValue = total;
            collectProgressSlider.value = collected;
        }
    }

    /// <summary>
    /// 清除所有项目
    /// </summary>
    private void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();
    }
}