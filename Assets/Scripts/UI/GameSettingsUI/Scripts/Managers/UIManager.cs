using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// UI管理器 - 负责管理游戏中所有UI面板和导航
/// </summary>
public class UIManager : MonoBehaviour
{
    #region 单例实现
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 在Awake中验证必要组件
        ValidateComponents();
    }
    #endregion

    [Header("UI面板引用")]
    [SerializeField] private GameObject mainSettingsPanel;
    [SerializeField] private GameObject collectablesPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject volumePanel;
    [SerializeField] private GameObject achievementsPanel;
    [SerializeField] private GameObject levelCompletePanel;

    [Header("标签页按钮")]
    [SerializeField] private TabButton collectablesTabButton;
    [SerializeField] private TabButton levelSelectTabButton;
    [SerializeField] private TabButton volumeTabButton;
    [SerializeField] private TabButton achievementsTabButton;

    // 当前活跃的面板
    private GameObject currentActivePanel;

    // 是否已经初始化
    private bool isInitialized = false;

    // 定义委托和事件
    [System.Serializable]
    public class PanelEvent : UnityEvent { }

    private PanelEvent onCollectablesPanelShow = new PanelEvent();
    private PanelEvent onLevelSelectPanelShow = new PanelEvent();
    private PanelEvent onVolumePanelShow = new PanelEvent();
    private PanelEvent onAchievementsPanelShow = new PanelEvent();

    /// <summary>
    /// 验证必要组件
    /// </summary>
    private void ValidateComponents()
    {
        if (mainSettingsPanel == null)
        {
            Debug.LogError("UIManager: 主设置面板未分配！");
        }

        // 验证所有面板引用
        ValidatePanelReference(collectablesPanel, "收集品面板");
        ValidatePanelReference(levelSelectPanel, "关卡选择面板");
        ValidatePanelReference(volumePanel, "音量控制面板");
        ValidatePanelReference(achievementsPanel, "成就面板");
        ValidatePanelReference(levelCompletePanel, "关卡完成面板");

        // 验证所有按钮引用
        ValidateButtonReference(collectablesTabButton, "收集品标签页按钮");
        ValidateButtonReference(levelSelectTabButton, "关卡选择标签页按钮");
        ValidateButtonReference(volumeTabButton, "音量控制标签页按钮");
        ValidateButtonReference(achievementsTabButton, "成就标签页按钮");
    }

    /// <summary>
    /// 验证面板引用
    /// </summary>
    private void ValidatePanelReference(GameObject panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"UIManager: {panelName}未分配，相关功能可能无法使用。");
        }
    }

    /// <summary>
    /// 验证按钮引用
    /// </summary>
    private void ValidateButtonReference(TabButton button, string buttonName)
    {
        if (button == null)
        {
            Debug.LogWarning($"UIManager: {buttonName}未分配，相关功能可能无法使用。");
            return;
        }

        if (button.GetComponent<UnityEngine.UI.Button>() == null)
        {
            Debug.LogError($"UIManager: {buttonName}缺少Button组件！");
        }
    }

    /// <summary>
    /// 初始化UI管理器
    /// </summary>
    public void Initialize()
    {
        if (isInitialized)
        {
            Debug.Log("UIManager已经初始化过，跳过重复初始化。");
            return;
        }

        try
        {
            // 确保主设置面板在开始时是隐藏的
            if (mainSettingsPanel != null)
            {
                mainSettingsPanel.SetActive(false);
            }

            // 隐藏所有内容面板
            HideAllPanels();

            // 设置标签页按钮的点击事件
            SetupButtonListeners();

            isInitialized = true;
            Debug.Log("UIManager初始化完成");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UIManager初始化失败: {e.Message}");
            isInitialized = false;
        }
    }

    /// <summary>
    /// 设置按钮监听器
    /// </summary>
    private void SetupButtonListeners()
    {
        if (collectablesTabButton != null)
        {
            var button = collectablesTabButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    Debug.Log("点击了收集品标签页按钮");
                    ShowCollectablesPanel();
                });
            }
        }

        if (levelSelectTabButton != null)
        {
            var button = levelSelectTabButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    Debug.Log("点击了关卡选择标签页按钮");
                    ShowLevelSelectPanel();
                });
            }
        }

        if (volumeTabButton != null)
        {
            var button = volumeTabButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    Debug.Log("点击了音量控制标签页按钮");
                    ShowVolumePanel();
                });
            }
        }

        if (achievementsTabButton != null)
        {
            var button = achievementsTabButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.AddListener(() =>
                {
                    Debug.Log("点击了成就标签页按钮");
                    ShowAchievementsPanel();
                });
            }
        }
    }

    /// <summary>
    /// 打开设置UI
    /// </summary>
    public void OpenSettingsUI()
    {
        if (!isInitialized)
        {
            Initialize();
        }

        mainSettingsPanel.SetActive(true);

        // 默认显示第一个标签页
        if (collectablesPanel != null && collectablesTabButton != null)
        {
            ShowCollectablesPanel();
        }
        else
        {
            // 如果没有收集品面板，则显示下一个可用面板
            if (levelSelectPanel != null && levelSelectTabButton != null)
            {
                ShowLevelSelectPanel();
            }
            else if (volumePanel != null && volumeTabButton != null)
            {
                ShowVolumePanel();
            }
            else if (achievementsPanel != null && achievementsTabButton != null)
            {
                ShowAchievementsPanel();
            }
        }
    }

    /// <summary>
    /// 关闭设置UI
    /// </summary>
    public void CloseSettingsUI()
    {
        mainSettingsPanel.SetActive(false);
    }

    /// <summary>
    /// 切换设置UI的显示状态
    /// </summary>
    public void ToggleSettingsUI()
    {
        if (mainSettingsPanel.activeSelf)
        {
            CloseSettingsUI();
        }
        else
        {
            OpenSettingsUI();
        }
    }

    /// <summary>
    /// 显示收集物面板
    /// </summary>
    public void ShowCollectablesPanel()
    {
        SwitchPanel(collectablesPanel);
        onCollectablesPanelShow.Invoke();
    }

    /// <summary>
    /// 显示关卡选择面板
    /// </summary>
    public void ShowLevelSelectPanel()
    {
        SwitchPanel(levelSelectPanel);
        onLevelSelectPanelShow.Invoke();
    }

    /// <summary>
    /// 显示音量控制面板
    /// </summary>
    public void ShowVolumePanel()
    {
        SwitchPanel(volumePanel);
        onVolumePanelShow.Invoke();
    }

    /// <summary>
    /// 显示成就面板
    /// </summary>
    public void ShowAchievementsPanel()
    {
        SwitchPanel(achievementsPanel);
        onAchievementsPanelShow.Invoke();
    }

    /// <summary>
    /// 显示关卡完成界面
    /// </summary>
    /// <param name="score">关卡得分</param>
    public void ShowLevelCompleteUI(int score)
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);

            // 获取面板组件并设置分数
            ILevelCompletePanel panel = levelCompletePanel.GetComponent<ILevelCompletePanel>();
            if (panel != null)
            {
                panel.SetScore(score);
            }
        }
    }

    /// <summary>
    /// 隐藏关卡完成界面
    /// </summary>
    public void HideLevelCompleteUI()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 切换到指定面板
    /// </summary>
    /// <param name="targetPanel">目标面板</param>
    private void SwitchPanel(GameObject targetPanel)
    {
        // 隐藏当前面板
        if (currentActivePanel != null)
        {
            currentActivePanel.SetActive(false);
        }

        // 显示目标面板
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            currentActivePanel = targetPanel;
        }
    }

    /// <summary>
    /// 隐藏所有面板
    /// </summary>
    private void HideAllPanels()
    {
        if (collectablesPanel != null) collectablesPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (volumePanel != null) volumePanel.SetActive(false);
        if (achievementsPanel != null) achievementsPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);

        currentActivePanel = null;
    }
}

/// <summary>
/// 关卡完成面板接口
/// </summary>
public interface ILevelCompletePanel
{
    void SetScore(int score);
}