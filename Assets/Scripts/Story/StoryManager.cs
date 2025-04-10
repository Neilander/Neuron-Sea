using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    ActionMode,  // 动作模式，玩家可以自由移动
    StoryMode    // 剧情模式，玩家不能移动，进行对话
}

/// <summary>
/// 剧情管理器，负责管理游戏状态切换和对话系统
/// </summary>
public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    [Header("游戏状态设置")]
    [SerializeField] private GameState currentState = GameState.ActionMode;
    [SerializeField] private PlayerController playerController; // 玩家控制器引用
    private Rigidbody2D playerRigidbody; // 玩家刚体引用
    private Vector2 savedVelocity; // 保存玩家的速度
    private bool wasKinematic; // 保存刚体的运动学状态
    private bool isWaitingForLanding = false; // 是否正在等待玩家落地
    private StoryData pendingStoryData;

    [Header("对话系统设置")]
    [SerializeField] private GameObject dialoguePanel; // 对话面板
    [SerializeField] private TMPro.TextMeshProUGUI dialogueText; // 对话文本
    [SerializeField] private TMPro.TextMeshProUGUI speakerNameText; // 说话者名称文本
    [SerializeField] private GameObject continueIndicator; // 继续指示器

    [Header("事件")]
    public UnityEvent onEnterStoryMode; // 进入剧情模式时触发
    public UnityEvent onExitStoryMode; // 退出剧情模式时触发
    public UnityEvent onDialogueComplete; // 对话完成时触发

    private bool isDialogueActive = false; // 对话是否激活
    private StoryData currentStoryData; // 当前剧情数据
    private int currentDialogueIndex = 0; // 当前对话索引

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 初始化
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("无法找到 PlayerController！部分功能可能无法正常工作。");
            }
            else
            {
                // 获取玩家的Rigidbody2D组件
                playerRigidbody = playerController.GetComponent<Rigidbody2D>();
                if (playerRigidbody == null)
                {
                    Debug.LogError("玩家对象上未找到Rigidbody2D组件！");
                }
            }
        }

        // 初始化对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // 在剧情模式下，检测点击以继续对话
        if (currentState == GameState.StoryMode && isDialogueActive)
        {
            if (Input.GetMouseButtonDown(0))// || Input.GetKeyDown(KeyCode.Space)
            {
                ContinueDialogue();
            }
        }
    }

    /// <summary>
    /// 切换到剧情模式
    /// </summary>
    /// <param name="storyData">要播放的剧情数据</param>
    public void EnterStoryMode(StoryData storyData)
    {
        if (currentState == GameState.StoryMode)
        {
            Debug.LogWarning("已经在剧情模式中！");
            return;
        }

        // 保存当前剧情数据
        currentStoryData = storyData;
        currentDialogueIndex = 0;

        // 切换到剧情模式
        currentState = GameState.StoryMode;

        // 如果玩家在地面上，立即禁用移动
        // 如果玩家在空中，等待落地后再禁用移动
        if (playerController != null)
        {
            if (playerController.IsGrounded())
            {
                playerController.DisableMovement();
            }
            else
            {
                StartCoroutine(WaitForLandingToDisableMovement());
            }
        }

        // 触发进入剧情模式事件
        onEnterStoryMode?.Invoke();

        // 开始对话
        StartDialogue();
    }

    /// <summary>
    /// 等待玩家落地后禁用移动
    /// </summary>
    private IEnumerator WaitForLandingToDisableMovement()
    {
        while (playerController != null && !playerController.IsGrounded())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 等待一帧确保完全落地
        yield return null;

        if (playerController != null && currentState == GameState.StoryMode)
        {
            playerController.DisableMovement();
        }
    }

    /// <summary>
    /// 退出剧情模式，返回动作模式
    /// </summary>
    public void ExitStoryMode()
    {
        if (currentState != GameState.StoryMode)
        {
            Debug.LogWarning("不在剧情模式中！");
            return;
        }

        // 结束对话
        EndDialogue();

        // 切换到动作模式
        currentState = GameState.ActionMode;

        // 启用玩家移动
        if (playerController != null)
        {
            playerController.EnableMovement();
        }

        // 触发退出剧情模式事件
        onExitStoryMode?.Invoke();
    }

    /// <summary>
    /// 开始对话
    /// </summary>
    private void StartDialogue()
    {
        if (currentStoryData == null || currentStoryData.dialogues.Count == 0)
        {
            Debug.LogWarning("没有对话数据！");
            ExitStoryMode();
            return;
        }

        isDialogueActive = true;

        // 显示对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // 显示第一条对话
        DisplayCurrentDialogue();
    }

    /// <summary>
    /// 继续对话，显示下一条
    /// </summary>
    private void ContinueDialogue()
    {
        currentDialogueIndex++;

        // 检查是否还有更多对话
        if (currentDialogueIndex < currentStoryData.dialogues.Count)
        {
            DisplayCurrentDialogue();
        }
        else
        {
            // 对话结束
            EndDialogue();
            ExitStoryMode();

            // 触发对话完成事件
            onDialogueComplete?.Invoke();
        }
    }

    /// <summary>
    /// 显示当前对话
    /// </summary>
    private void DisplayCurrentDialogue()
    {
        if (currentDialogueIndex >= currentStoryData.dialogues.Count)
        {
            return;
        }

        DialogueData dialogue = currentStoryData.dialogues[currentDialogueIndex];

        // 更新对话文本
        if (dialogueText != null)
        {
            dialogueText.text = dialogue.text;
        }

        // 更新说话者名称
        if (speakerNameText != null)
        {
            speakerNameText.text = dialogue.speakerName;
        }

        // 如果有动画触发器，触发动画
        if (!string.IsNullOrEmpty(dialogue.animationTrigger))
        {
            // 这里可以添加触发动画的代码
            // 例如：animator.SetTrigger(dialogue.animationTrigger);
        }
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    private void EndDialogue()
    {
        isDialogueActive = false;

        // 隐藏对话面板
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// 等待玩家落地后进入剧情模式
    /// </summary>
    /// <param name="storyData">要执行的剧情数据</param>
    public void WaitForLandingThenEnterStoryMode(StoryData storyData)
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController 未找到，无法等待落地！");
            return;
        }

        if (isWaitingForLanding)
        {
            Debug.LogWarning("已经在等待玩家落地，请勿重复调用！");
            return;
        }

        pendingStoryData = storyData;
        isWaitingForLanding = true;
        StartCoroutine(CheckForLanding());
    }

    private IEnumerator CheckForLanding()
    {
        while (!playerController.IsGrounded())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 等待一帧确保完全落地
        yield return null;

        isWaitingForLanding = false;
        EnterStoryMode(pendingStoryData);
        pendingStoryData = null;
    }
}