using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 玩家跳跃检测器
/// 专门用于检测玩家的跳跃动作并触发事件
/// </summary>
public class PlayerJumpDetector : MonoBehaviour
{
    [Header("事件")]
    public UnityEvent onFirstJump;       // 首次跳跃事件
    public UnityEvent onJump;            // 每次跳跃事件
    
    [Header("设置")]
    [SerializeField] private bool detectOnlyFirstJump = true;  // 是否只检测首次跳跃
    [SerializeField] private PlayerController playerController;  // 玩家控制器引用
    
    private bool hasJumped = false;  // 是否已经跳跃过
    private bool isListening = true;  // 是否正在监听跳跃
    
    // 用于比较状态的变量
    private bool wasGrounded = true;
    private bool wasJumping = false;
    
    private void Awake()
    {
        // 如果没有指定玩家控制器，尝试自动查找
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController == null)
            {
                Debug.LogWarning("未找到PlayerController组件，跳跃检测可能无法正常工作。");
            }
        }
    }
    
    private void Start()
    {
        // 初始状态
        if (playerController != null)
        {
            wasGrounded = playerController.IsGrounded();
        }
    }
    
    private void Update()
    {
        if (!isListening) return;
        
        // 如果只检测首次跳跃且已经跳跃过，则不再检测
        if (detectOnlyFirstJump && hasJumped) return;
        
        // 检测跳跃
        CheckForJump();
    }
    
    /// <summary>
    /// 检测跳跃动作
    /// </summary>
    private void CheckForJump()
    {
        if (playerController == null) return;
        
        // 方法1：通过Input检测跳跃键按下
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpDetected();
            return;
        }
        
        // 方法2：通过玩家状态变化检测跳跃
        bool isGrounded = playerController.IsGrounded();
        bool isJumping = IsPlayerJumping();
        
        // 检测从地面到跳跃的状态变化
        if (wasGrounded && !isGrounded && isJumping && !wasJumping)
        {
            OnJumpDetected();
        }
        
        // 更新状态
        wasGrounded = isGrounded;
        wasJumping = isJumping;
    }
    
    /// <summary>
    /// 检测玩家是否正在跳跃
    /// </summary>
    private bool IsPlayerJumping()
    {
        // 这里需要根据实际的PlayerController实现
        // 示例实现，需要根据实际情况修改
        
        // 1. 如果PlayerController有IsJumping方法，直接调用
        // return playerController.IsJumping();
        
        // 2. 如果没有可以根据垂直速度判断
        Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            return rb.velocity.y > 0.5f;
        }
        
        return false;
    }
    
    /// <summary>
    /// 跳跃检测到时调用
    /// </summary>
    private void OnJumpDetected()
    {
        // 触发每次跳跃事件
        onJump?.Invoke();
        
        // 如果是首次跳跃
        if (!hasJumped)
        {
            hasJumped = true;
            onFirstJump?.Invoke();
            
            // 如果只检测首次跳跃，则取消监听
            if (detectOnlyFirstJump)
            {
                isListening = false;
            }
        }
    }
    
    /// <summary>
    /// 重置检测器状态
    /// </summary>
    public void Reset()
    {
        hasJumped = false;
        isListening = true;
    }
    
    /// <summary>
    /// 暂停检测
    /// </summary>
    public void PauseDetection()
    {
        isListening = false;
    }
    
    /// <summary>
    /// 恢复检测
    /// </summary>
    public void ResumeDetection()
    {
        isListening = true;
    }
}