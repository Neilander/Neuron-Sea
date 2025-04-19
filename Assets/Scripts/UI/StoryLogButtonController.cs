using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 剧情记录按钮控制器，用于控制剧情记录面板的显示
/// </summary>
public class StoryLogButtonController : MonoBehaviour
{
    [SerializeField] private Button storyLogButton; // 剧情记录按钮
    [SerializeField] private StoryLogUI storyLogUI; // 剧情记录UI
    
    private void Start()
    {
        // 初始化按钮事件
        if (storyLogButton != null)
        {
            storyLogButton.onClick.AddListener(OnStoryLogButtonClicked);
        }
        
        // 如果没有指定StoryLogUI，尝试查找
        if (storyLogUI == null)
        {
            storyLogUI = FindObjectOfType<StoryLogUI>();
        }
    }
    
    /// <summary>
    /// 剧情记录按钮点击事件
    /// </summary>
    private void OnStoryLogButtonClicked()
    {
        if (storyLogUI != null)
        {
            // 检查当前是否在剧情模式，如果是则不打开记录面板
            if (StoryManager.Instance != null && 
                StoryManager.Instance.GetCurrentState() == GameState.StoryMode)
            {
                Debug.Log("当前在剧情模式中，无法打开剧情记录！");
                return;
            }
            
            storyLogUI.ToggleLogPanel();
        }
        else
        {
            Debug.LogWarning("StoryLogUI未找到！");
        }
    }
}