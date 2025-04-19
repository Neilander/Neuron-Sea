using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 剧情记录UI，用于显示和回放剧情记录
/// </summary>
public class StoryLogUI : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject logPanel; // 记录面板
    [SerializeField] private RectTransform logEntriesContainer; // 记录条目容器
    [SerializeField] private Button closeButton; // 关闭按钮
    [SerializeField] private Button clearAllButton; // 清除全部按钮
    [SerializeField] private TextMeshProUGUI noLogsText; // 无记录提示文本

    [Header("记录条目设置")]
    [SerializeField] private GameObject logEntryPrefab; // 记录条目预制体
    [SerializeField] private int maxDisplayEntries = 20; // 最大显示条目数量

    // 当前显示的记录条目列表
    private List<GameObject> currentEntries = new List<GameObject>();

    // 是否已初始化
    private bool isInitialized = false;

    private void Start()
    {
        // 初始化UI
        InitializeUI();
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    private void InitializeUI()
    {
        if (isInitialized) return;

        // 隐藏面板
        if (logPanel != null)
        {
            logPanel.SetActive(false);
        }

        // 添加关闭按钮事件
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(HideLogPanel);
        }

        // 添加清除全部按钮事件
        if (clearAllButton != null)
        {
            clearAllButton.onClick.AddListener(ClearAllLogs);
        }

        isInitialized = true;
    }

    /// <summary>
    /// 显示记录面板
    /// </summary>
    public void ShowLogPanel()
    {
        if (!isInitialized)
        {
            InitializeUI();
        }

        if (logPanel != null)
        {
            logPanel.SetActive(true);

            // 刷新记录列表
            RefreshLogEntries();
        }
    }

    /// <summary>
    /// 隐藏记录面板
    /// </summary>
    public void HideLogPanel()
    {
        if (logPanel != null)
        {
            logPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新记录条目列表
    /// </summary>
    private void RefreshLogEntries()
    {
        // 清除现有条目
        ClearCurrentEntries();

        // 获取记录列表
        if (StoryLogManager.Instance == null)
        {
            Debug.LogWarning("StoryLogManager实例不存在！");
            return;
        }

        List<StoryLog> logs = StoryLogManager.Instance.GetAllStoryLogs();

        // 如果没有记录，显示提示
        if (logs.Count == 0)
        {
            if (noLogsText != null)
            {
                noLogsText.gameObject.SetActive(true);
            }
            return;
        }

        // 隐藏提示
        if (noLogsText != null)
        {
            noLogsText.gameObject.SetActive(false);
        }

        // 反向遍历记录（最新的在最上面）
        int count = 0;
        for (int i = logs.Count - 1; i >= 0 && count < maxDisplayEntries; i--)
        {
            StoryLog log = logs[i];

            // 创建条目
            GameObject entry = Instantiate(logEntryPrefab, logEntriesContainer);

            // 设置条目内容
            SetupLogEntry(entry, log, logs.Count - 1 - i);

            // 添加到列表
            currentEntries.Add(entry);

            count++;
        }
    }

    /// <summary>
    /// 设置记录条目的内容
    /// </summary>
    private void SetupLogEntry(GameObject entry, StoryLog log, int index)
    {
        // 设置标题
        TextMeshProUGUI titleText = entry.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
        {
            titleText.text = log.storyName;
        }

        // 设置时间
        TextMeshProUGUI timeText = entry.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
        if (timeText != null)
        {
            timeText.text = log.timestamp.ToString("yyyy-MM-dd HH:mm");
        }

        // 设置对话计数
        TextMeshProUGUI countText = entry.transform.Find("CountText")?.GetComponent<TextMeshProUGUI>();
        if (countText != null)
        {
            countText.text = $"对话数量: {log.dialogues.Count}";
        }

        // 设置回放按钮
        Button replayButton = entry.transform.Find("ReplayButton")?.GetComponent<Button>();
        if (replayButton != null)
        {
            int capturedIndex = index; // 捕获当前索引
            replayButton.onClick.RemoveAllListeners();
            replayButton.onClick.AddListener(() =>
            {
                OnReplayButtonClicked(capturedIndex);
            });
        }
    }

    /// <summary>
    /// 回放按钮点击处理
    /// </summary>
    private void OnReplayButtonClicked(int index)
    {
        if (StoryLogManager.Instance == null) return;

        // 获取记录列表
        List<StoryLog> logs = StoryLogManager.Instance.GetAllStoryLogs();

        // 计算真实索引（因为显示时是反向的）
        int realIndex = logs.Count - 1 - index;

        // 隐藏面板
        HideLogPanel();

        // 开始回放
        StoryLogManager.Instance.StartReplay(realIndex);
    }

    /// <summary>
    /// 清除当前显示的条目
    /// </summary>
    private void ClearCurrentEntries()
    {
        foreach (var entry in currentEntries)
        {
            Destroy(entry);
        }

        currentEntries.Clear();
    }

    /// <summary>
    /// 清除所有记录
    /// </summary>
    private void ClearAllLogs()
    {
        if (StoryLogManager.Instance == null) return;

        // 由于Unity的对话框API不统一，这里直接清除
        // 在实际项目中，应添加自定义对话框UI来确认
        // 例如: 
        // DialogManager.ShowConfirmDialog("是否清除所有剧情记录？此操作不可撤销。", 
        //     () => { 
        //         StoryLogManager.Instance.ClearAllLogs(); 
        //         RefreshLogEntries();
        //     });

        // 直接清除所有记录
        StoryLogManager.Instance.ClearAllLogs();
        RefreshLogEntries();
    }

    /// <summary>
    /// 切换记录面板的显示状态
    /// </summary>
    public void ToggleLogPanel()
    {
        if (logPanel != null)
        {
            if (logPanel.activeSelf)
            {
                HideLogPanel();
            }
            else
            {
                ShowLogPanel();
            }
        }
    }
}