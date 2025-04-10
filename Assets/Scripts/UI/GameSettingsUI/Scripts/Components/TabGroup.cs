using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 标签组 - 用于管理多个标签按钮及其对应的内容面板
/// </summary>
public class TabGroup : MonoBehaviour
{
    [Header("引用")]
    [SerializeField] private List<TabButton> tabButtons;
    [SerializeField] private List<GameObject> contentPanels;
    [SerializeField] private TabButton selectedTab;

    [Header("设置")]
    [SerializeField] private bool selectFirstTabOnStart = true;

    private List<TabButton> registeredTabs = new List<TabButton>();

    private void Start()
    {
        // 初始化时，如果配置了自动选择第一个标签
        if (selectFirstTabOnStart && registeredTabs.Count > 0 && selectedTab == null)
        {
            OnTabSelected(registeredTabs[0]);
        }
        else if (selectedTab != null)
        {
            // 如果已经有预选定的标签，则选中它
            OnTabSelected(selectedTab);
        }
        else
        {
            // 默认隐藏所有内容面板
            UpdateContentPanels();
        }
    }

    /// <summary>
    /// 注册标签按钮到此标签组
    /// </summary>
    public void Subscribe(TabButton button)
    {
        if (!registeredTabs.Contains(button))
        {
            registeredTabs.Add(button);
        }

        // 如果已经在Inspector中预设了按钮列表，确保所有预设的按钮都被注册
        if (tabButtons != null && !tabButtons.Contains(button) && button != null)
        {
            tabButtons.Add(button);
        }
    }

    /// <summary>
    /// 从标签组中注销标签按钮
    /// </summary>
    public void Unsubscribe(TabButton button)
    {
        if (registeredTabs.Contains(button))
        {
            registeredTabs.Remove(button);
        }

        if (tabButtons != null && tabButtons.Contains(button))
        {
            tabButtons.Remove(button);
        }

        // 如果移除的是当前选中的标签
        if (selectedTab == button)
        {
            selectedTab = null;
            // 如果还有其他标签，选择第一个
            if (registeredTabs.Count > 0)
            {
                OnTabSelected(registeredTabs[0]);
            }
            else
            {
                UpdateContentPanels(); // 隐藏所有面板
            }
        }
    }

    /// <summary>
    /// 当鼠标悬停在标签上时调用
    /// </summary>
    public void OnTabEnter(TabButton button)
    {
        if (button != selectedTab)
        {
            button.SetHover();
        }
    }

    /// <summary>
    /// 当鼠标离开标签时调用
    /// </summary>
    public void OnTabExit(TabButton button)
    {
        if (button != selectedTab)
        {
            button.SetInactive();
        }
    }

    /// <summary>
    /// 当标签被选中时调用
    /// </summary>
    public void OnTabSelected(TabButton button)
    {
        // 如果当前已经选中了这个标签，则不做任何事
        if (selectedTab == button)
            return;

        // 取消选择之前的标签
        if (selectedTab != null)
        {
            selectedTab.SetInactive();
        }

        // 选择新标签
        selectedTab = button;
        selectedTab.SetActive();

        // 更新面板显示
        UpdateContentPanels();
    }

    /// <summary>
    /// 更新内容面板的显示/隐藏状态
    /// </summary>
    private void UpdateContentPanels()
    {
        // 如果没有内容面板，则直接返回
        if (contentPanels == null || contentPanels.Count == 0)
            return;

        // 隐藏所有面板
        foreach (var panel in contentPanels)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        // 如果没有选中的标签，则所有面板都保持隐藏
        if (selectedTab == null)
            return;

        // 获取选中标签的索引
        int index = tabButtons.IndexOf(selectedTab);
        if (index >= 0 && index < contentPanels.Count && contentPanels[index] != null)
        {
            // 显示对应的内容面板
            contentPanels[index].SetActive(true);
        }
    }

    /// <summary>
    /// 按索引选择标签
    /// </summary>
    public void SelectTabByIndex(int index)
    {
        if (tabButtons != null && index >= 0 && index < tabButtons.Count && tabButtons[index] != null)
        {
            OnTabSelected(tabButtons[index]);
        }
    }
}