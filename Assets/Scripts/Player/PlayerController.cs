using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour, IMovementController
{
    #region 移动速度,跳跃速度
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 7f;

    [SerializeField]
    private float jumpForce = 5f;

    [SerializeField]
    private float maxFallSpeed = -10f;

    [Header("土狼时间(开始时刷新)")]
    [SerializeField] private float extraJumpAllowTime = 0.1f;
    #endregion
    #region 地面检测点
    [Header("Ground Check Settings")]
    [SerializeField]
    private new BoxCollider2D collider; // 角色碰撞体

    [SerializeField]
    private float deviation = 0.02f; // 检测误差，为unity物理误差的2倍

    [SerializeField]
    private LayerMask groundLayer; // 只检测地面层
    #endregion
    #region 墙壁检测点
    // [Header("Wall Check Settings")]
    // [SerializeField] private Transform wallCheckLeft; // 左侧墙壁检测点
    //
    // [SerializeField] private Transform wallCheckRight; // 右侧墙壁检测点
    // [SerializeField]
    // private Transform wallCheck; // 角色脚前方的检测点
    //
    //
    //
    // [SerializeField]
    // private float wallCheckRadius = 0.2f; // 圆形检测的半径
    //
    // [SerializeField]
    // private LayerMask wallLayer; // 只检测墙壁层
    #endregion
    #region 角色身上脚本
    private Animator animator;
    private Rigidbody2D rb;
    #endregion
    #region 布尔值
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;
    #endregion
    #region Timer
    float minJumpTime = 0.08f;
    float minJumpTimer;
    #endregion

    #region 包装属性
    private BoolRefresher ifJustGround;
    private BoolRefresher ifGetControlledOutside;
    #endregion

    #region 判断加速度正负
    private float previousSpeed; // 用于保存上一帧的速度值
    private float currentSpeed;

    [SerializeField] private float speedChangeThreshold = 0.01f; // 速度变化阈值，避免微小波

    private float CurrentYSpeed;
    #endregion

    private bool canMove = true; // 控制是否可以移动的状态
    private bool canInput = true; // 控制是否接受输入的状态

    [HideInInspector] public UnityEvent OnTabSelected = new UnityEvent();

    private Vector3 lockedPosition; // 存储锁定的位置
    private bool isPositionLocked = false; // 位置是否被锁定
    private MovementComparison movementBounds;


    private bool dropped = false;
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        ifJustGround = new BoolRefresher(extraJumpAllowTime, watchExtraJumpAllowTime);
        if (ifGetControlledOutside == null) ifGetControlledOutside = new BoolRefresher(1);
    }

    private void Update()
    {
        //只有在可以输入时才处理输入
        if (canInput)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

            if (Input.GetKeyDown(KeyCode.J))
            {
                levelManager.instance.SwitchToBeforeLevel_Direct();
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                levelManager.instance.SwitchToNextLevel_Direct();
            }
        }

        if (movementBounds.IsAtRightEdge())
        {
            FindAnyObjectByType<levelManager>().SwitchToNextLevel();
        }
        else if (movementBounds.ShouldDrop() && !dropped)
        {
            dropped = true;
            PlayerDeathEvent.Trigger(gameObject, DeathType.Fall);
        }
        /*
        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        if (rb.velocity.y < maxFallSpeed)
        {
            Vector2 newVelocity = rb.velocity;
            newVelocity.y = maxFallSpeed;
            rb.velocity = newVelocity;
        }
        CurrentYSpeed = rb.velocity.y;
        if (CurrentYSpeed > -1 && CurrentYSpeed <= 1)
        {
            CurrentYSpeed = 0;
        }
        animator.SetFloat("VerticalSpeed", CurrentYSpeed);
        GetSpeedChange();

        if (!canMove) // 如果不能移动，直接停止所有移动
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("Speed", 0);
            return;
        }

        if (ifGetControlledOutside.Get())
        {
            MoveInControl();
            RotateInControl();
        }
        else if (canInput) // 只有在可以输入时才处理移动和旋转
        {
            Move();
            Rotate();
            CheckJump();
        }
        ifGetControlledOutside.Update(Time.deltaTime);
        ifJustGround.Update(Time.deltaTime);
        */
    }

    private void FixedUpdate()
    {

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        if (rb.velocity.y < maxFallSpeed)
        {
            Vector2 newVelocity = rb.velocity;
            newVelocity.y = maxFallSpeed;
            rb.velocity = newVelocity;
        }
        CurrentYSpeed = rb.velocity.y;
        if (CurrentYSpeed > -1 && CurrentYSpeed <= 1)
        {
            CurrentYSpeed = 0;
        }
        animator.SetFloat("VerticalSpeed", CurrentYSpeed);
        GetSpeedChange();

        if (!canMove) // 如果不能移动，直接停止所有移动
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("Speed", 0);
            return;
        }

        if (ifGetControlledOutside.Get())
        {
            MoveInControl();
            RotateInControl();
        }
        else if (canInput) // 只有在可以输入时才处理移动和旋转
        {
            Move();
            Rotate();
            CheckJump();
        }
        ifGetControlledOutside.Update(Time.deltaTime);
        ifJustGround.Update(Time.deltaTime);

    }

    private float watchExtraJumpAllowTime() { return extraJumpAllowTime; }

    #region 判断地面
    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        // 检测角色是否接触地面
        isGrounded = false;
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(collider.bounds.center - new Vector3(0, collider.bounds.size.y / 2, 0), new Vector2(collider.bounds.size.x + deviation * 0.8f, deviation), 0f, Vector2.down, deviation / 2, groundLayer))
        {
            if (hit.normal.y > 0.9f)
            {
                isGrounded = true;
                ifJustGround.Refresh();
                break;
            }
        }

        // 添加地面状态变化的调试
        if (!wasGrounded && isGrounded)
        {
            Debug.Log($"玩家落地 - 位置: {transform.position}");
        }
        else if (wasGrounded && !isGrounded)
        {
            Debug.Log($"玩家离开地面 - 位置: {transform.position}");
        }

        // 检测脚前方是否接触墙壁
        // Debug.Log("isTouchingWall: " + isTouchingWall);
        // isTouchingWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);
        // isTouchingWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);
        // isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }
    #endregion
    #region 角色水平移动
    private void Move()
    {
        // 只有在可以输入时才获取输入
        float moveInput = canInput ? Input.GetAxis("Horizontal") : 0f;

        // 在地面上应用摩擦力
        if (isGrounded)
        {
            // 类似图片中在蹲下和地面上的处理，应用水平方向的摩擦力
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, 0, Constants.DuckFriction * Time.deltaTime),
                rb.velocity.y);
        }

        // 获取移动方向乘数
        float mult = isGrounded ? 1 : Constants.AirMult;

        // 当前最大速度
        float max = Constants.MaxRun;

        // 如果有输入
        if (moveInput != 0)
        {
            // 如果速度超过最大值且方向相同
            if (Mathf.Abs(rb.velocity.x) > max && Mathf.Sign(rb.velocity.x) == Mathf.Sign(moveInput))
            {
                // 减速到最大速度
                rb.velocity = new Vector2(
                    Mathf.MoveTowards(rb.velocity.x, max * Mathf.Sign(moveInput), Constants.RunReduce * mult * Time.deltaTime),
                    rb.velocity.y);
            }
            else
            {
                // 加速到目标速度
                rb.velocity = new Vector2(
                    Mathf.MoveTowards(rb.velocity.x, max * moveInput, Constants.RunAccel * mult * Time.deltaTime),
                    rb.velocity.y);
            }
        }
        else
        {
            // 无输入时，向0减速
            rb.velocity = new Vector2(
                Mathf.MoveTowards(rb.velocity.x, 0, Constants.RunReduce * Time.deltaTime),
                rb.velocity.y);
        }

        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
    }
    #endregion
    #region 角色身体转向
    private void Rotate()
    {
        // 只有在可以输入时才获取输入
        float moveInput = canInput ? Input.GetAxis("Horizontal") : 0f;

        if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }
    #endregion
    #region 角色跳跃
    void Jump()
    {
        ifJustGround.Take();
        // 触发跳跃动画
        animator.SetTrigger("Jump");
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        JumpInput.Jump.OnTrigger();
        Debug.Log($"玩家跳跃 - 位置: {transform.position}, 速度: {rb.velocity}, 跳跃力: {jumpForce}");
    }
    void CheckJump()
    {
        // 只有在可以输入时才检查跳跃
        if (canInput)
        {
            CheckJumpStart();
            CheckJumpInterrupt();
        }
    }
    void CheckJumpInterrupt()
    {
        if (!JumpInput.Jump.Checked() && !isGrounded && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            Debug.Log("跳跃中断");
        }
    }
    void CheckJumpStart()
    {
        bool jumpCondition = isGrounded || ifJustGround.Get();
        if (JumpInput.Jump.Pressed() && jumpCondition)
        {
            Debug.Log($"检测到跳跃输入 - 地面状态: {isGrounded}, 土狼时间: {ifJustGround.Get()}");
            Jump();
        }
        else if (JumpInput.Jump.Pressed() && !jumpCondition)
        {
            Debug.Log($"跳跃条件不满足 - 地面状态: {isGrounded}, 土狼时间: {ifJustGround.Get()}");
        }
    }
    #endregion

    #region 限制控制
    private float controlInput;
    public void StartControl(float controlInput, float time)
    {
        if (ifGetControlledOutside == null) ifGetControlledOutside = new BoolRefresher(1);
        ifGetControlledOutside.Refresh(time);
        this.controlInput = Mathf.Clamp(controlInput, 0, 1);
    }

    private void MoveInControl()
    {
        rb.velocity = new Vector2(controlInput * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(controlInput));
    }

    private void RotateInControl()
    {
        float moveInput = controlInput;
        if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }
    #endregion

    #region 角色偏移


    // private void HandleWallCollision()
    // {
    //     // 获取角色的宽度
    //     float halfWidth = rb.GetComponent<Collider2D>().bounds.size.x / 2;

    //     // 如果左射线检测到物体，右射线没有检测到物体
    //     if (isTouchingWallLeft && !isTouchingWallRight)
    //     {
    //         // 向右偏移
    //         rb.position = new Vector2(rb.position.x + 0.002f, rb.position.y); //+ halfWidth 
    //     }
    //     // 如果右射线检测到物体，左射线没有检测到物体
    //     else if (isTouchingWallRight && !isTouchingWallLeft)
    //     {
    //         // 向左偏移
    //         rb.position = new Vector2(rb.position.x - 0.002f, rb.position.y);//- halfWidth
    //     }


    // }
    #endregion
    #region 绘制测试射线
    private void OnDrawGizmos()
    {
        //可视化碰撞体
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube((Vector2)transform.position + collider.offset, collider.size);

        // 可视化脚前方的墙壁检测
        // if (wallCheck != null)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        // }
    }
    #endregion
    #region 判断加速度正负
    private void GetSpeedChange()
    {
        Vector2 horizontalVelocity = new Vector2(rb.velocity.x, 0f);
        currentSpeed = Mathf.Round(horizontalVelocity.magnitude);

        if (currentSpeed > previousSpeed + speedChangeThreshold)
        {
            animator.SetBool("IsIncreasing", true);
        }
        else if (currentSpeed < previousSpeed - speedChangeThreshold)
        {
            animator.SetBool("IsIncreasing", false);
        }

        // // 更新 Animator 参数（直接传递原始值，无需四舍五入）
        // animator.SetFloat("Speed", horizontalVelocity.magnitude);

        previousSpeed = currentSpeed;
    }
    #endregion

    #region 区域限制
    public void SetMovementBounds(Rect rect)
    {
        movementBounds = new MovementComparison(rect, transform);
    }

    #endregion

    public void ResetMovement()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            animator?.SetFloat("Speed", 0);
            animator?.SetFloat("VerticalSpeed", 0);
        }
        Debug.Log("已重置玩家移动状态");
    }

    public void DisableMovement()
    {
        canMove = false;
        canInput = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            animator?.SetFloat("Speed", 0);
            // 锁定位置
            LockPosition();
        }
        Debug.Log("已禁用玩家移动并锁定位置");
    }

    public void EnableMovement()
    {
        canMove = true;
        canInput = true;
        // 解锁位置
        UnlockPosition();
        Debug.Log("已启用玩家移动并解锁位置");
    }

    // 新增：单独控制输入的方法
    public void DisableInput()
    {
        canInput = false;
    }

    public void EnableInput()
    {
        canInput = true;
        Debug.Log("已启用玩家输入 - 可以接收跳跃指令");
    }

    // 新增：获取是否在地面上的公共方法
    public bool IsGrounded()
    {
        return isGrounded;
    }

    private void LateUpdate()
    {
        // 如果位置被锁定，强制保持在锁定位置
        if (isPositionLocked)
        {
            transform.position = lockedPosition;
        }
    }

    // 锁定位置的方法
    public void LockPosition()
    {
        lockedPosition = transform.position;
        isPositionLocked = true;

        // 完全锁定刚体的所有移动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            Debug.Log($"玩家位置已锁定 - 位置: {lockedPosition}, 刚体约束: 全部冻结");
        }
    }

    // 解锁位置的方法
    public void UnlockPosition()
    {
        isPositionLocked = false;

        // 恢复刚体的正常约束
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"玩家位置已解锁 - 刚体约束: 仅冻结旋转");
        }
    }
}


public class BoolRefresher
{
    private bool value = false;
    private float timer = 0f;
    private float duration;

    private Func<float> durationWatcher;
    private float lastWatchedValue;

    public BoolRefresher(float duration, Func<float> externalDurationGetter = null)
    {
        this.duration = duration;
        this.durationWatcher = externalDurationGetter;
        this.lastWatchedValue = externalDurationGetter?.Invoke() ?? duration;
    }

    /// <summary>
    /// 设置为 true 并重置计时
    /// </summary>
    public void Refresh()
    {
        value = true;
        timer = duration;
    }

    public void Refresh(float t)
    {
        value = true;
        timer = t;
    }

    /// <summary>
    /// 在 Update 中调用，刷新内部状态
    /// </summary>
    public void Update(float deltaTime)
    {
        // 检查外部 duration 是否改变
        if (durationWatcher != null)
        {
            float current = durationWatcher.Invoke();
            lastWatchedValue = current;
            duration = current;
        }

        // 正常倒计时逻辑
        if (!value) return;

        timer -= deltaTime;
        if (timer <= 0f)
        {
            value = false;
            timer = 0f;
        }
    }

    /// <summary>
    /// 获取当前状态（true = 刷新中）
    /// </summary>
    public bool Get()
    {
        return value;
    }

    public void Take()
    {
        value = false;
        timer = 0f;
    }
}

public class MovementComparison
{
    private float leftX;
    private float rightX;
    private float topY;
    private float bottomY;

    private Transform target;

    public MovementComparison(Rect rect, Transform targetTransform)
    {
        leftX = rect.xMin;
        rightX = rect.xMax;
        bottomY = rect.yMin;
        topY = rect.yMax;

        target = targetTransform;
    }

    // ✅ 是否到达右边（例如可以前进）
    public bool IsAtRightEdge()
    {
        return target.position.x >= rightX;
    }

    // ✅ 是否到达左边（例如不能再后退）
    public bool IsAtLeftEdge()
    {
        return target.position.x <= leftX;
    }

    // ✅ 是否该掉落（例如到达下边）
    public bool ShouldDrop()
    {
        return target.position.y <= bottomY;
    }

    // ✅ 是否到达顶部（例如可以跳的限制）
    public bool IsAtTopEdge()
    {
        return target.position.y >= topY;
    }

    // ✅ 你可以添加更多判断方法
}

// 添加常量类
public static class Constants
{
    public static float DuckFriction = 10.0f;     // 地面摩擦力
    public static float AirMult = 1f;          // 空中移动乘数
    public static float MaxRun = 7.0f;           // 最大奔跑速度
    public static float RunReduce = 20.0f;       // 减速系数
    public static float RunAccel = 25.0f;        // 加速系数
}