using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 增强版剧情触发器
/// </summary>
public class StoryTrigger : MonoBehaviour
{
    [Header("剧情资源")]
    [Tooltip("剧情数据资源路径，在Resources文件夹下的相对路径")]
    [SerializeField] private string storyResourcePath = "StoryData/IntroStory";

    [Header("触发设置")]
    [Tooltip("是否只触发一次")]
    [SerializeField] private bool triggerOnce = false;
    [Tooltip("是否需要按键触发")]
    [SerializeField] private bool requireButtonPress = false;
    [Tooltip("触发按键")]
    [SerializeField] private KeyCode triggerKey = KeyCode.E;
    [Tooltip("是否需要玩家在地面上才能触发")]
    [SerializeField] private bool requireGrounded = true;

    [Header("提示UI")]
    [Tooltip("触发提示文本")]
    [SerializeField] private string promptText = "按 E 键触发对话";
    [Tooltip("提示UI预制体")]
    [SerializeField] private GameObject promptPrefab;

    private bool hasTriggered = false;
    private bool playerInTriggerArea = false;
    private GameObject promptInstance;
    private PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerArea = true;
            playerController = other.GetComponent<PlayerController>();

            if (!requireButtonPress)
            {
                TriggerStory();
            }
            else
            {
                ShowPrompt();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTriggerArea = false;
            HidePrompt();
        }
    }

    private void Update()
    {
        if (playerInTriggerArea && requireButtonPress && Input.GetKeyDown(triggerKey))
        {
            TriggerStory();
        }
    }

    /// <summary>
    /// 触发剧情
    /// </summary>
    private void TriggerStory()
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        // 如果需要地面检测
        if (requireGrounded && playerController != null)
        {
            if (!playerController.IsGrounded())
            {
                // 玩家不在地面上，等待落地后再触发
                StartCoroutine(WaitForPlayerLanding());
                return;
            }
        }

        // 获取剧情数据资源
        StoryData storyData = Resources.Load<StoryData>(storyResourcePath);

        if (storyData == null)
        {
            Debug.LogError("无法加载剧情数据: " + storyResourcePath);
            return;
        }

        // 进入剧情模式
        StoryManager.Instance.EnterStoryMode(storyData);

        // 标记为已触发
        hasTriggered = true;

        // 隐藏提示
        HidePrompt();
    }

    /// <summary>
    /// 等待玩家落地后触发剧情
    /// </summary>
    private IEnumerator WaitForPlayerLanding()
    {
        // 显示等待提示
        if (promptInstance != null)
        {
            // 可以更新提示文本为"等待玩家落地..."
        }

        while (playerController != null && !playerController.IsGrounded())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 确保玩家仍在触发区域内
        if (playerInTriggerArea)
        {
            // 获取剧情数据资源
            StoryData storyData = Resources.Load<StoryData>(storyResourcePath);

            if (storyData != null)
            {
                // 进入剧情模式
                StoryManager.Instance.EnterStoryMode(storyData);

                // 标记为已触发
                hasTriggered = true;

                // 隐藏提示
                HidePrompt();
            }
        }
    }

    /// <summary>
    /// 显示交互提示
    /// </summary>
    private void ShowPrompt()
    {
        if (promptPrefab != null && promptInstance == null)
        {
            promptInstance = Instantiate(promptPrefab, transform.position + Vector3.up, Quaternion.identity);

            // 如果提示预制体有文本组件，设置提示文本
            TMPro.TextMeshProUGUI textComponent = promptInstance.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = promptText;
            }
        }
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    private void HidePrompt()
    {
        if (promptInstance != null)
        {
            Destroy(promptInstance);
            promptInstance = null;
        }
    }

    /// <summary>
    /// 重置触发器状态
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
