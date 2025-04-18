using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏状态枚举
/// </summary>
public enum GameState
{
    ActionMode,  // 动作模式，玩家可以自由移动111
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
    [SerializeField] private float typingSpeed = 0.05f; // 打字速度，每个字符间隔时间
    [SerializeField] private AudioSource typingSoundEffect; // 打字声音效果（可选）
    [SerializeField] private float typingSoundInterval = 0.1f; // 打字声音播放间隔（可选）

    [Header("事件")]
    public UnityEvent onEnterStoryMode; // 进入剧情模式时触发
    public UnityEvent onExitStoryMode; // 退出剧情模式时触发
    public UnityEvent onDialogueComplete; // 对话完成时触发

    private bool isDialogueActive = false; // 对话是否激活
    private StoryData currentStoryData; // 当前剧情数据
    private int currentDialogueIndex = 0; // 当前对话索引
    private bool isTyping = false; // 是否正在显示打字效果
    private Coroutine typingCoroutine; // 打字协程

    [Header("立绘和头像设置")]
    [SerializeField] private GameObject portraitPanel; // 立绘面板
    [SerializeField] private Image leftPortraitImage; // 左侧立绘图像组件
    [SerializeField] private Image centerPortraitImage; // 中间立绘图像组件
    [SerializeField] private Image rightPortraitImage; // 右侧立绘图像组件
    [SerializeField] private Image avatarImage; // 头像图像组件
    [SerializeField] private Image backgroundImage; // 背景图像组件
    [SerializeField] private float portraitFadeDuration = 0.5f; // 立绘淡入淡出时间

    // 已激活的立绘位置跟踪
    private Dictionary<PortraitPosition, bool> activePortraits = new Dictionary<PortraitPosition, bool>()
    {
        { PortraitPosition.Left, false },
        { PortraitPosition.Center, false },
        { PortraitPosition.Right, false }
    };

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

        // 隐藏对话面板
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // 隐藏立绘面板
        if (portraitPanel != null)
            portraitPanel.SetActive(false);

        // 初始化立绘组件
        InitializePortraitComponents();
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
    }

    /// <summary>
    /// 初始化立绘相关组件
    /// </summary>
    private void InitializePortraitComponents()
    {
        // 初始化所有立绘图像
        if (leftPortraitImage != null)
        {
            leftPortraitImage.gameObject.SetActive(false);
            Color c = leftPortraitImage.color;
            c.a = 0;
            leftPortraitImage.color = c;
        }

        if (centerPortraitImage != null)
        {
            centerPortraitImage.gameObject.SetActive(false);
            Color c = centerPortraitImage.color;
            c.a = 0;
            centerPortraitImage.color = c;
        }

        if (rightPortraitImage != null)
        {
            rightPortraitImage.gameObject.SetActive(false);
            Color c = rightPortraitImage.color;
            c.a = 0;
            rightPortraitImage.color = c;
        }

        // 初始化头像图像
        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(false);
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

        // 重置所有立绘状态
        ResetAllPortraits();

        // 清除当前剧情数据
        currentStoryData = null;
        currentDialogueIndex = 0;
        isTyping = false;
    }

    /// <summary>
    /// 重置所有立绘
    /// </summary>
    private void ResetAllPortraits()
    {
        if (leftPortraitImage != null)
        {
            leftPortraitImage.gameObject.SetActive(false);
            Color c = leftPortraitImage.color;
            c.a = 0;
            leftPortraitImage.color = c;
        }

        if (centerPortraitImage != null)
        {
            centerPortraitImage.gameObject.SetActive(false);
            Color c = centerPortraitImage.color;
            c.a = 0;
            centerPortraitImage.color = c;
        }

        if (rightPortraitImage != null)
        {
            rightPortraitImage.gameObject.SetActive(false);
            Color c = rightPortraitImage.color;
            c.a = 0;
            rightPortraitImage.color = c;
        }

        if (avatarImage != null)
        {
            avatarImage.gameObject.SetActive(false);
        }

        // 重置活动立绘跟踪
        activePortraits[PortraitPosition.Left] = false;
        activePortraits[PortraitPosition.Center] = false;
        activePortraits[PortraitPosition.Right] = false;
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
        // 如果正在打字，则直接显示全部文本
        if (isTyping)
        {
            StopTypingEffect();
            return;
        }

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
    /// 停止打字效果，直接显示完整文本
    /// </summary>
    private void StopTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentDialogueIndex < currentStoryData.dialogues.Count)
        {
            // 立即显示完整文本
            DialogueData dialogue = currentStoryData.dialogues[currentDialogueIndex];
            if (dialogueText != null)
            {
                dialogueText.text = dialogue.text;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = dialogue.speakerName;
            }

            // 显示继续指示器
            if (continueIndicator != null)
            {
                continueIndicator.SetActive(true);
            }
        }

        isTyping = false;
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

        // 更新说话者名称
        if (speakerNameText != null)
        {
            speakerNameText.text = dialogue.speakerName;
        }

        // 隐藏继续指示器
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }

        // 启动打字机效果
        if (dialogueText != null)
        {
            // 清空文本框
            dialogueText.text = "";

            // 启动打字机效果
            typingCoroutine = StartCoroutine(TypeDialogue(dialogue.text));
        }

        // 处理立绘和头像
        HandlePortraitAndAvatar(dialogue);
    }

    /// <summary>
    /// 处理立绘和头像的显示
    /// </summary>
    private void HandlePortraitAndAvatar(DialogueData dialogue)
    {
        // 处理头像
        if (avatarImage != null)
        {
            if (dialogue.avatar != null)
            {
                avatarImage.sprite = dialogue.avatar;
                avatarImage.gameObject.SetActive(true);
            }
            else
            {
                avatarImage.gameObject.SetActive(false);
            }
        }

        // 处理立绘
        if (!dialogue.showPortrait || dialogue.portrait == null)
        {
            return;
        }

        // 如果需要隐藏其他立绘
        if (dialogue.hideOtherPortraits)
        {
            HideOtherPortraits(dialogue.portraitPosition);
        }

        // 获取对应位置的立绘组件
        Image targetPortraitImage = GetPortraitImageByPosition(dialogue.portraitPosition);
        if (targetPortraitImage == null) return;

        // 显示立绘面板
        if (portraitPanel != null && !portraitPanel.activeSelf)
        {
            portraitPanel.SetActive(true);
        }

        // 设置并显示立绘
        targetPortraitImage.sprite = dialogue.portrait;
        targetPortraitImage.gameObject.SetActive(true);

        // 淡入新的立绘
        StartCoroutine(FadeInPortrait(targetPortraitImage));

        // 应用立绘特效
        if (dialogue.portraitEffect != PortraitEffect.None)
        {
            ApplyPortraitEffect(targetPortraitImage, dialogue.portraitEffect, dialogue.effectIntensity, dialogue.effectDuration);
        }

        // 标记此位置的立绘为活动状态
        activePortraits[dialogue.portraitPosition] = true;
    }

    /// <summary>
    /// 隐藏除了指定位置外的所有立绘
    /// </summary>
    private void HideOtherPortraits(PortraitPosition exceptPosition)
    {
        foreach (PortraitPosition position in System.Enum.GetValues(typeof(PortraitPosition)))
        {
            if (position != exceptPosition && activePortraits[position])
            {
                Image portraitImage = GetPortraitImageByPosition(position);
                if (portraitImage != null && portraitImage.gameObject.activeSelf)
                {
                    StartCoroutine(FadeOutPortrait(portraitImage));
                    activePortraits[position] = false;
                }
            }
        }
    }

    /// <summary>
    /// 应用立绘特效
    /// </summary>
    private void ApplyPortraitEffect(Image targetImage, PortraitEffect effect, float intensity, float duration)
    {
        switch (effect)
        {
            case PortraitEffect.Shake:
                StartCoroutine(ShakeEffect(targetImage.transform, intensity, duration));
                break;
            case PortraitEffect.Bounce:
                StartCoroutine(BounceEffect(targetImage.transform, intensity, duration));
                break;
            case PortraitEffect.Spin:
                StartCoroutine(SpinEffect(targetImage.transform, intensity, duration));
                break;
            case PortraitEffect.Flash:
                StartCoroutine(FlashEffect(targetImage, intensity, duration));
                break;
        }
    }

    /// <summary>
    /// 震动特效
    /// </summary>
    private IEnumerator ShakeEffect(Transform target, float intensity, float duration)
    {
        Vector3 originalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-1f, 1f) * intensity * 0.1f;
            float y = originalPosition.y + Random.Range(-1f, 1f) * intensity * 0.1f;

            target.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPosition;
    }

    /// <summary>
    /// 弹跳特效
    /// </summary>
    private IEnumerator BounceEffect(Transform target, float intensity, float duration)
    {
        Vector3 originalPosition = target.localPosition;
        Vector3 originalScale = target.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float value = Mathf.Sin(progress * Mathf.PI * 8) * intensity * 0.1f;

            target.localPosition = new Vector3(originalPosition.x, originalPosition.y + value, originalPosition.z);
            target.localScale = new Vector3(
                originalScale.x * (1 + value * 0.1f),
                originalScale.y * (1 - value * 0.1f),
                originalScale.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPosition;
        target.localScale = originalScale;
    }

    /// <summary>
    /// 旋转特效
    /// </summary>
    private IEnumerator SpinEffect(Transform target, float intensity, float duration)
    {
        Quaternion originalRotation = target.localRotation;
        float elapsed = 0f;
        float maxAngle = 10f * intensity;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float angle = Mathf.Sin(progress * Mathf.PI * 4) * maxAngle;

            target.localRotation = Quaternion.Euler(0, 0, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localRotation = originalRotation;
    }

    /// <summary>
    /// 闪烁特效
    /// </summary>
    private IEnumerator FlashEffect(Image target, float intensity, float duration)
    {
        Color originalColor = target.color;
        float elapsed = 0f;
        float flashSpeed = 8f * intensity;

        while (elapsed < duration)
        {
            float value = (Mathf.Sin(elapsed * flashSpeed) + 1) / 2;
            target.color = new Color(
                originalColor.r,
                originalColor.g,
                originalColor.b,
                Mathf.Lerp(originalColor.a * 0.5f, originalColor.a, value)
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.color = originalColor;
    }

    /// <summary>
    /// 根据位置获取对应的立绘Image组件
    /// </summary>
    private Image GetPortraitImageByPosition(PortraitPosition position)
    {
        switch (position)
        {
            case PortraitPosition.Left:
                return leftPortraitImage;
            case PortraitPosition.Center:
                return centerPortraitImage;
            case PortraitPosition.Right:
                return rightPortraitImage;
            default:
                return null;
        }
    }

    /// <summary>
    /// 淡入立绘效果协程
    /// </summary>
    private IEnumerator FadeInPortrait(Image portraitImage)
    {
        float elapsedTime = 0;
        Color startColor = portraitImage.color;
        startColor.a = 0;
        portraitImage.color = startColor;

        Color targetColor = startColor;
        targetColor.a = 1;

        while (elapsedTime < portraitFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / portraitFadeDuration);

            Color newColor = Color.Lerp(startColor, targetColor, normalizedTime);
            portraitImage.color = newColor;

            yield return null;
        }

        portraitImage.color = targetColor;
    }

    /// <summary>
    /// 淡出立绘效果协程
    /// </summary>
    private IEnumerator FadeOutPortrait(Image portraitImage)
    {
        float elapsedTime = 0;
        Color startColor = portraitImage.color;
        Color targetColor = startColor;
        targetColor.a = 0;

        while (elapsedTime < portraitFadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / portraitFadeDuration);

            Color newColor = Color.Lerp(startColor, targetColor, normalizedTime);
            portraitImage.color = newColor;

            yield return null;
        }

        portraitImage.color = targetColor;
        portraitImage.gameObject.SetActive(false);
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

    /// <summary>
    /// 打字机效果协程
    /// </summary>
    private IEnumerator TypeDialogue(string text)
    {
        isTyping = true;
        float timeSinceLastSound = 0f;

        // 逐字显示文本
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];

            // 播放打字声音（如果有）
            if (typingSoundEffect != null && timeSinceLastSound >= typingSoundInterval)
            {
                typingSoundEffect.pitch = Random.Range(0.9f, 1.1f); // 稍微变化音调，增加真实感
                typingSoundEffect.Play();
                timeSinceLastSound = 0f;
            }
            timeSinceLastSound += typingSpeed;

            yield return new WaitForSeconds(typingSpeed);
        }

        // 显示完成后，显示继续指示器
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }

        isTyping = false;
        typingCoroutine = null;
    }
}