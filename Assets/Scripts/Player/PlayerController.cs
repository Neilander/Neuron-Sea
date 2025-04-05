using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region 移动速度,跳跃速度
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 10f;

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
    #endregion

    #region 判断加速度正负
    private float previousSpeed; // 用于保存上一帧的速度值
    private float currentSpeed;

    [SerializeField] private float speedChangeThreshold = 0.01f; // 速度变化阈值，避免微小波

    private float CurrentYSpeed;
    #endregion
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        ifJustGround = new BoolRefresher(extraJumpAllowTime, watchExtraJumpAllowTime);
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
        if (CurrentYSpeed > -1&&CurrentYSpeed<=1) {
            CurrentYSpeed = 0;
        }
        // print(CurrentYSpeed);
        animator.SetFloat("VerticalSpeed", CurrentYSpeed);
        GetSpeedChange();
        
        Move();
        Rotate();
        CheckJump();
        ifJustGround.Update(Time.deltaTime);
        // HandleWallCollision();
    }

    private float watchExtraJumpAllowTime() { return extraJumpAllowTime; }

    #region 判断地面
    private void GroundCheck()
    {
        //bool wasGrounded = isGrounded;
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
        //if(!wasGrounded && isGrounded)
        //{
        //    Debug.Log("Land");
        //}
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
        float moveInput = Input.GetAxis("Horizontal");

        /*防穿墙
        if (Physics2D.BoxCast(collider.bounds.center + deviation * Vector3.left, new Vector2(collider.bounds.size.x, collider.bounds.size.y - deviation), 0f, Vector2.zero, 0, groundLayer))
        {
            transform.position += Vector3.right * deviation;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else if (Physics2D.BoxCast(collider.bounds.center + deviation * Vector3.right, new Vector2(collider.bounds.size.x, collider.bounds.size.y - deviation), 0f, Vector2.zero, 0, groundLayer))
        {
            transform.position += Vector3.left * deviation;
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
        */
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }
    #endregion
    #region 角色身体转向
    private void Rotate()
    {
        float moveInput = Input.GetAxis("Horizontal");
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
    }
    void CheckJump()
    {
        CheckJumpStart();
        CheckJumpInterrupt();
    }
    void CheckJumpInterrupt()
    {
        if (!JumpInput.Jump.Checked() && !isGrounded && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }
    void CheckJumpStart()
    {
        if (JumpInput.Jump.Pressed() &&( isGrounded|| ifJustGround.Get()))
        {
            Jump();
        }
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