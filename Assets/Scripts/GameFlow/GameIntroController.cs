using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// 游戏开场流程控制器
/// 管理游戏开场序列：相机过渡 -> 剧情播放 -> 显示提示图 -> 等待玩家跳跃
/// </summary>
public class GameIntroController : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] public CameraSequencePlayer cameraSequencer;  // 相机序列播放器
    [SerializeField] private PlayerController playerController;     // 玩家控制器
    [SerializeField] private StoryManager storyManager;            // 剧情管理器
    [SerializeField] private GameObject hintImage;                 // 提示图片（跳跃提示）
    [SerializeField] private Image transitionFadeImage;            // 过渡淡入淡出图片
    
    [Header("设置")]
    [SerializeField] private string firstStoryID = "intro_story";  // 第一段剧情的ID
    [SerializeField] private float fadeInOutDuration = 0.5f;       // 淡入淡出时间

    [Header("事件")]
    [SerializeField] private UnityEvent onGameIntroStart;          // 游戏开场开始事件
    [SerializeField] private UnityEvent onCameraTransitionComplete; // 相机过渡完成事件
    [SerializeField] private UnityEvent onStoryComplete;           // 剧情播放完成事件
    [SerializeField] private UnityEvent onFirstJump;               // 首次跳跃事件
    [SerializeField] private UnityEvent onIntroComplete;           // 整个开场完成事件

    private bool hasJumped = false;            // 是否已经跳跃
    private bool isCameraTransitionDone = false; // 相机过渡是否完成
    private bool isStoryDone = false;          // 剧情是否播放完成
    private bool isGameIntroActive = true;     // 游戏开场流程是否激活
    
    private void Awake()
    {
        // 查找组件
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
            
        if (storyManager == null)
            storyManager = FindObjectOfType<StoryManager>();
            
        if (cameraSequencer == null)
            cameraSequencer = FindObjectOfType<CameraSequencePlayer>();

        // 初始化状态
        DisablePlayerInput();
        
        // 隐藏提示图片
        if (hintImage != null)
            hintImage.SetActive(false);
            
        // 初始化淡入淡出图片
        if (transitionFadeImage != null)
        {
            Color c = transitionFadeImage.color;
            c.a = 0f;
            transitionFadeImage.color = c;
            transitionFadeImage.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        // 开始游戏开场流程
        StartGameIntro();
    }

    private void Update()
    {
        // 检测玩家首次跳跃
        if (isStoryDone && !hasJumped && playerController != null && IsPlayerJumping())
        {
            OnPlayerFirstJump();
        }
    }

    /// <summary>
    /// 开始游戏开场流程
    /// </summary>
    public void StartGameIntro()
    {
        // 触发开场开始事件
        onGameIntroStart?.Invoke();
        
        // 确保玩家不能输入
        DisablePlayerInput();
        
        // 开始相机序列
        if (cameraSequencer != null)
        {
            // 注册相机过渡完成事件
            cameraSequencer.afterTransition.AddListener(OnCameraTransitionComplete);
            
            // 开始播放相机序列
            cameraSequencer.PlaySequence();
        }
        else
        {
            // 如果没有相机序列播放器，直接进入下一步
            OnCameraTransitionComplete();
        }
    }

    /// <summary>
    /// 相机过渡完成后的回调
    /// </summary>
    public void OnCameraTransitionComplete()
    {
        isCameraTransitionDone = true;
        
        // 触发相机过渡完成事件
        onCameraTransitionComplete?.Invoke();
        
        // 开始播放第一段剧情
        StartCoroutine(PlayFirstStory());
    }

    /// <summary>
    /// 播放第一段剧情
    /// </summary>
    private IEnumerator PlayFirstStory()
    {
        // 等待短暂延迟，确保相机过渡完全结束
        yield return new WaitForSeconds(0.5f);
        
        // 如果有剧情管理器，播放指定剧情
        if (storyManager != null)
        {
            // 加载剧情数据
            StoryData storyData = LoadStoryData(firstStoryID);
            
            if (storyData != null)
            {
                // 注册剧情完成事件
                storyManager.onDialogueComplete += OnStoryComplete;
                
                // 开始播放剧情
                storyManager.EnterStoryMode(storyData);
            }
            else
            {
                // 如果没有找到剧情数据，直接完成剧情步骤
                OnStoryComplete();
            }
        }
        else
        {
            // 如果没有剧情管理器，直接完成剧情步骤
            OnStoryComplete();
        }
    }

    /// <summary>
    /// 剧情播放完成后的回调
    /// </summary>
    public void OnStoryComplete()
    {
        // 移除事件监听，避免重复调用
        if (storyManager != null)
        {
            storyManager.onDialogueComplete -= OnStoryComplete;
        }
        
        isStoryDone = true;
        
        // 触发剧情完成事件
        onStoryComplete?.Invoke();
        
        // 显示提示图片
        StartCoroutine(ShowHintImage());
        
        // 恢复玩家输入
        EnablePlayerInput();
    }

    /// <summary>
    /// 显示提示图片
    /// </summary>
    private IEnumerator ShowHintImage()
    {
        // 短暂延迟
        yield return new WaitForSeconds(0.5f);
        
        // 淡入显示提示图片
        if (hintImage != null)
        {
            hintImage.SetActive(true);
            
            // 如果有CanvasGroup组件，使用淡入效果
            CanvasGroup canvasGroup = hintImage.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                float elapsedTime = 0f;
                
                while (elapsedTime < fadeInOutDuration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInOutDuration);
                    yield return null;
                }
                
                canvasGroup.alpha = 1f;
            }
        }
    }

    /// <summary>
    /// 玩家首次跳跃的回调
    /// </summary>
    private void OnPlayerFirstJump()
    {
        hasJumped = true;
        
        // 触发首次跳跃事件
        onFirstJump?.Invoke();
        
        // 隐藏提示图片
        StartCoroutine(HideHintImage());
        
        // 完成整个开场流程
        CompleteGameIntro();
    }

    /// <summary>
    /// 隐藏提示图片
    /// </summary>
    private IEnumerator HideHintImage()
    {
        if (hintImage != null)
        {
            // 如果有CanvasGroup组件，使用淡出效果
            CanvasGroup canvasGroup = hintImage.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                float elapsedTime = 0f;
                float startAlpha = canvasGroup.alpha;
                
                while (elapsedTime < fadeInOutDuration)
                {
                    elapsedTime += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeInOutDuration);
                    yield return null;
                }
            }
            
            hintImage.SetActive(false);
        }
    }

    /// <summary>
    /// 完成整个游戏开场流程
    /// </summary>
    private void CompleteGameIntro()
    {
        isGameIntroActive = false;
        
        // 触发整个开场完成事件
        onIntroComplete?.Invoke();
    }

    /// <summary>
    /// 加载剧情数据
    /// </summary>
    private StoryData LoadStoryData(string storyID)
    {
        // 这里实现根据ID加载剧情数据的逻辑
        // 可以从Resources加载，或者从StoryManager中获取
        
        // 示例：从Resources加载
        StoryData storyData = Resources.Load<StoryData>("Stories/" + storyID);
        
        if (storyData == null)
        {
            Debug.LogWarning($"未能找到剧情数据: {storyID}");
            
            // 尝试从CSV加载
            if (storyManager != null)
            {
                // 尝试从Resources加载CSV
                bool loaded = storyManager.LoadStoryFromResourceCSV("Stories/CSV/" + storyID);
                if (!loaded)
                {
                    Debug.LogError($"未能从CSV加载剧情: {storyID}");
                }
                return null; // StoryManager会直接处理CSV加载的剧情
            }
        }
        
        return storyData;
    }

    /// <summary>
    /// 禁用玩家输入
    /// </summary>
    private void DisablePlayerInput()
    {
        if (playerController != null)
        {
            playerController.DisableMovement();
        }
    }

    /// <summary>
    /// 启用玩家输入
    /// </summary>
    private void EnablePlayerInput()
    {
        if (playerController != null)
        {
            playerController.EnableMovement();
        }
    }

    /// <summary>
    /// 检测玩家是否正在跳跃
    /// </summary>
    private bool IsPlayerJumping()
    {
        // 这里需要根据PlayerController的实际实现来判断
        // 示例实现，需要根据实际情况修改
        return Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space);
    }

    /// <summary>
    /// 提供给外部调用的自定义事件处理函数
    /// </summary>
    public void HandleCustomEvent(UnityAction action)
    {
        // 这个函数可以传入移动完摄像机的事件
        if (action != null)
        {
            // 添加到相机过渡完成事件
            onCameraTransitionComplete.AddListener(action);
        }
    }

    /// <summary>
    /// 重置游戏开场流程（用于测试）
    /// </summary>
    public void ResetGameIntro()
    {
        isGameIntroActive = true;
        isCameraTransitionDone = false;
        isStoryDone = false;
        hasJumped = false;
        
        if (hintImage != null)
            hintImage.SetActive(false);
            
        DisablePlayerInput();
    }
}