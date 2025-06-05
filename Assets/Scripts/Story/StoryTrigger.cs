using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 增强版剧情触发器
/// </summary>
public class StoryTrigger : MonoBehaviour
{
    private string triggerID;
    [Header("跳过剧情设置")]
    [Tooltip("跳过剧情的UI按钮")]
    [SerializeField] private GameObject skipButton; // UI按钮对象
    private bool isStoryPlaying = false;



    [Header("检测设置")]
    [Tooltip("触发区域大小")]
    [SerializeField] private Vector2 triggerSize = new Vector2(2f, 2f);
    [Tooltip("触发区域偏移")]
    [SerializeField] private Vector2 triggerOffset = Vector2.zero;
    [Tooltip("检测频率（秒）")]
    [SerializeField] private float detectionInterval = 0.1f;

    // 当前碰撞区域的世界坐标
    private Vector2 TriggerWorldPosition => (Vector2)transform.position + triggerOffset;

    
    
    
    
    // [Header("事件")]
    public UnityEvent onEnterSpecificStory; // 进入剧情模式时触发
    public UnityEvent onExitSpecificStory; // 退出剧情模式时触发
    // public UnityEvent onSpecificDialogueComplete; // 对话完成时触发

    public enum StorySourceType
    {
        ScriptableObject,  // 使用StoryData ScriptableObject
        CSVResource        // 使用CSV文本资源
    }

    [Header("剧情来源设置")]
    [Tooltip("剧情数据来源类型")]
    [SerializeField] private StorySourceType storySourceType = StorySourceType.ScriptableObject;

    [Header("剧情资源")]
    [Tooltip("剧情数据资源路径，在Resources文件夹下的相对路径")]
    [SerializeField] private string storyResourcePath = "StoryData/IntroStory";

    [Header("CSV数据设置")]
    [Tooltip("CSV文件在Resources文件夹下的相对路径（仅当选择CSV来源时使用）")]
    [SerializeField] private string csvResourcePath = "StoryData/CSV/story01";

    [Header("触发设置")]
    [Tooltip("是否只触发一次")]
    [SerializeField] private bool triggerOnce = false;
    [Tooltip("是否需要按键触发")]
    [SerializeField] private bool requireButtonPress = false;
    [Tooltip("触发按键")]
    [SerializeField] private KeyCode triggerKey = KeyCode.E;
    [Tooltip("是否需要玩家在地面上才能触发")]
    [SerializeField] private bool requireGrounded = true;
    [Tooltip("是否不记忆")]
    [SerializeField] private bool noMemory = false;

    [Header("提示UI")]
    [Tooltip("触发提示文本")]
    [SerializeField] private string promptText = "按 E 键触发对话";
    [Tooltip("提示UI预制体")]
    [SerializeField] private GameObject promptPrefab;

    [Header("高级控制")]
    [Tooltip("剧情结束时是否阻止玩家解冻（用于连续剧情）")]
    [SerializeField] private bool preventPlayerUnfreeze = false;
    [Tooltip("剧情结束后是否自动触发下一段剧情")]
    [SerializeField] private bool autoTriggerNextStory = false;
    [Tooltip("下一段剧情的触发器")]
    [SerializeField] public StoryTrigger nextStoryTrigger;
    [Tooltip("下一段剧情触发前的延迟时间（秒）")]
    [SerializeField] private float nextStoryDelay = 0.5f;

    private bool hasTriggered = false;
    private bool playerInTriggerArea = false;
    private GameObject promptInstance;
    private PlayerController playerController;
    private bool isWaitingForStoryEnd = false;

    private void Awake(){
        triggerID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "_" + gameObject.name;
    }
    private void Start()
    {
        if (!string.IsNullOrEmpty(triggerID) && StoryGlobalLoadManager.instance.IsTriggerDisabled(triggerID)) {
            // 如果这个ID已经播放过，就禁用Collider
            GetComponent<Collider2D>().enabled = false;
        }
        // 向StoryManager注册剧情完成事件
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.onDialogueComplete+=OnDialogueComplete;
        }// 开始定期检测
        // StartCoroutine(DetectPlayerRoutine());
    }

    #region 检测玩家




    private void OnTriggerEnter2D(Collider2D other)
    {
        print("碰到我了");
        if (!other.CompareTag("Player")) return;
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

    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (!other.CompareTag("Player")) return;
    //     playerInTriggerArea = false;
    //     HidePrompt();
    // }







    // /// <summary>
    // /// 定期检测玩家是否在触发区域内
    // /// </summary>
    // private IEnumerator DetectPlayerRoutine(){
    //     while (true) {
    //         DetectPlayer();
    //         yield return new WaitForSeconds(detectionInterval);
    //     }
    // }

    // /// <summary>
    // /// 检测玩家是否在触发区域内
    // /// </summary>
    // private void DetectPlayer(){
    //     // 找到场景中的所有玩家
    //     PlayerController[] players = FindObjectsOfType<PlayerController>();
    //     bool foundPlayer = false;

    //     foreach (var player in players) {
    //         // 使用刚才添加的方法检查碰撞
    //         if (player.IsCollidingWithRect(TriggerWorldPosition, triggerSize)) {
    //             // 玩家进入区域
    //             if (!playerInTriggerArea) {
    //                 Debug.Log("检测到了玩家");
    //                 playerInTriggerArea = true;
    //                 playerController = player;

    //                 if (!requireButtonPress) {
    //                     TriggerStory();
    //                 }
    //                 else {
    //                     ShowPrompt();
    //                 }
    //             }

    //             foundPlayer = true;
    //             break;
    //         }
    //     }

    //     // 如果未检测到玩家，但之前检测到过
    //     if (!foundPlayer && playerInTriggerArea) {
    //         playerInTriggerArea = false;
    //         HidePrompt();
    //     }
    // }

// // 绘制触发区域的可视化表示（仅在编辑器中）
//     private void OnDrawGizmos(){
//         Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
//         Gizmos.DrawCube(TriggerWorldPosition, triggerSize);

//         Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
//         Gizmos.DrawWireCube(TriggerWorldPosition, triggerSize);
//     }
    

    #endregion
    
    private void OnDestroy()
    {
        // 取消注册事件
        if (StoryManager.Instance != null)
        {
            StoryManager.Instance.onDialogueComplete-=OnDialogueComplete;
        }
    }

    /// <summary>
    /// 剧情完成事件处理
    /// </summary>
    private void OnDialogueComplete()
    {
        // 只处理正在等待结束的剧情
        if (!isWaitingForStoryEnd) return;

        Debug.Log("剧情结束了！");
        // 重置等待标志
        isWaitingForStoryEnd = false;
            isStoryPlaying = false;
        // 隐藏跳过按钮
        HideSkipButton();
        // 触发退出事件
        onExitSpecificStory?.Invoke();
        if(transform.GetComponent<BoxCollider2D>() != null)
            transform.GetComponent<BoxCollider2D>().enabled = false;
        if(!noMemory)
            StoryGlobalLoadManager.instance.DisableTrigger(triggerID);
        // 如果需要自动触发下一段剧情
        if (autoTriggerNextStory && nextStoryTrigger != null)
        {
            StartCoroutine(TriggerNextStoryAfterDelay());
        }
    }

    /// <summary>
    /// 延迟触发下一段剧情
    /// </summary>
    private IEnumerator TriggerNextStoryAfterDelay()
    {
        //这里因为在Storymanger的退出之后，所以可以延续状态
        ActivityGateCenter.EnterState(ActivityState.Story);
        yield return new WaitForSeconds(nextStoryDelay);
        nextStoryTrigger.ForceStartStory();
    }

    /// <summary>
    /// 强制开始剧情，忽略触发条件
    /// </summary>
    public void ForceStartStory()
    {
        Debug.Log("[StoryTrigger] ForceStartStory called, this=" + this.name);
        Debug.Log($"[StoryTrigger] StartStoryInternal called, storySourceType={storySourceType}, storyResourcePath={storyResourcePath}, csvResourcePath={csvResourcePath}");
        StartStoryInternal();
    }

    
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         playerInTriggerArea = true;
    //         playerController = other.GetComponent<PlayerController>();
    //
    //         if (!requireButtonPress)
    //         {
    //             TriggerStory();
    //         }
    //         else
    //         {
    //             ShowPrompt();
    //         }
    //     }
    // }
    //
    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         playerInTriggerArea = false;
    //         HidePrompt();
    //     }
    // }

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
            Debug.Log("阻止了！");
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

        StartStoryInternal();
    }

    /// <summary>
    /// 内部启动剧情的方法
    /// </summary>
    private void StartStoryInternal()
    {
        bool success = false;

        // 设置阻止玩家解冻的标志（如果需要）
        if (preventPlayerUnfreeze && StoryManager.Instance != null)
        {
            StoryManager.Instance.SetPreventPlayerUnfreeze(preventPlayerUnfreeze);
        }

        // 触发进入事件
        onEnterSpecificStory?.Invoke();

        // 标记正在等待剧情结束
        isWaitingForStoryEnd = true;

        // 根据数据源类型选择加载方式
        if (storySourceType == StorySourceType.ScriptableObject)
        {
            // 获取剧情数据资源
            StoryData storyData = Resources.Load<StoryData>(storyResourcePath);

            if (storyData == null)
            {
                Debug.LogError("无法加载剧情数据: " + storyResourcePath);
                isWaitingForStoryEnd = false;
                return;
            }

            // 进入剧情模式
            StoryManager.Instance.EnterStoryMode(storyData);
            success = true;
        }
        else if (storySourceType == StorySourceType.CSVResource)
        {
            // 使用CSV资源加载剧情
            success = StoryManager.Instance.LoadStoryFromResourceCSV(csvResourcePath);

            if (!success)
            {
                Debug.LogError("无法从CSV加载剧情数据: " + csvResourcePath);
                isWaitingForStoryEnd = false;
                return;
            }
        }

        // 如果成功触发
        if (success)
        {
            isStoryPlaying = true;
            // 标记为已触发
            hasTriggered = true;

            // 隐藏提示
            HidePrompt();
            
            ShowSkipButton(); // 显示跳过按钮
        }
        else
        {
            isWaitingForStoryEnd = false;
        }
    }

    // 添加显示跳过按钮的方法
private void ShowSkipButton()
{
    if (skipButton != null)
    {
        skipButton.SetActive(true);
    }
}

// 添加隐藏跳过按钮的方法
private void HideSkipButton()
{
    if (skipButton != null)
    {
        skipButton.SetActive(false);
    }
}

// 添加给UI按钮调用的公共方法
public void OnSkipButtonClick()
{
    if (!isStoryPlaying) return;

    // 重置状态
    isStoryPlaying = false;
    isWaitingForStoryEnd = false;

    // 隐藏跳过按钮
    HideSkipButton();

    // 如果StoryManager存在，结束当前对话
    if (StoryManager.Instance != null)
    {
        StoryManager.Instance.ExitStoryMode();
    }

    // 触发退出事件
    onExitSpecificStory?.Invoke();

    // 如果需要自动触发下一段剧情
    if (autoTriggerNextStory && nextStoryTrigger != null)
    {
        StartCoroutine(TriggerNextStoryAfterDelay());
    }
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
            StartStoryInternal();
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

    public void SetStoryResourcePath(string path) {
        this.csvResourcePath = path;
    }

    public void SetTriggerId(string id) {
        this.triggerID = id;
    }
    
    public void SetStorySourceType(StorySourceType sourceType) {
        this.storySourceType = sourceType;
    }
}
