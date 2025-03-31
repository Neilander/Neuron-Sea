using UnityEngine;

public class PlayerController : MonoBehaviour
{
//1

    #region 移动速度,跳跃速度
    [Header("Movement Settings")]
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private float jumpForce = 5f;
    #endregion
    #region 地面检测点
    [Header("Ground Check Settings")]
    [SerializeField]
    private Transform groundCheckLeft; // 左侧地面检测点
    
    [SerializeField]
    private Transform groundCheckRight; // 右侧地面检测点
    
    [SerializeField]
    private float groundCheckRadius = 0.2f; // 地面检测半径
    
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
    #region 判断加速度正负
    private float previousSpeed; // 用于保存上一帧的速度值
    private float currentSpeed;

    [SerializeField] private float speedChangeThreshold = 0.01f; // 速度变化阈值，避免微小波
    #endregion
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {

        GroundCheck();
        animator.SetBool("isGrounded", isGrounded);
        GetSpeedChange();
        Move();
        Rotate();
        Jump();
        // HandleWallCollision();


    }

    #region 用两个射线判断地面
    private void GroundCheck()
    {
        // Debug.Log("isTouchingWall: " + isTouchingWall);
        // isTouchingWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);
        // isTouchingWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);
        // 检测角色是否接触左侧或右侧的地面
        isGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, groundCheckRadius, groundLayer) ||
                     Physics2D.OverlapCircle(groundCheckRight.position, groundCheckRadius, groundLayer);

        // 检测脚前方是否接触墙壁
        // isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }
    #endregion
    #region 角色水平移动
    private void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");

        if (isGrounded) //|| isTouchingWall
        {
            // 如果角色在地面上或者接触墙壁，正常移动
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
        // else
        // {
        //     // 如果角色在空中并接触到墙壁，仍然允许水平移动
        //     if (isTouchingWall)
        //     {
        //         rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        //     }
        // }

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
    #region 角色跳跃,处理跳跃之后是落地还是降落
    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {

            // 触发跳跃动画
            animator.SetTrigger("Jump");
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetBool("isGrounded", false);

        }
        else if (Input.GetButtonUp("Jump") && rb.velocity.y > 0) {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }
    #endregion


    #region 角色偏移


    private void HandleWallCollision()
    {
        // 获取角色的宽度
        float halfWidth = rb.GetComponent<Collider2D>().bounds.size.x / 2;

        // 如果左射线检测到物体，右射线没有检测到物体
        if (isTouchingWallLeft && !isTouchingWallRight)
        {
            // 向右偏移
            rb.position = new Vector2(rb.position.x + 0.002f, rb.position.y); //+ halfWidth 
        }
        // 如果右射线检测到物体，左射线没有检测到物体
        else if (isTouchingWallRight && !isTouchingWallLeft)
        {
            // 向左偏移
            rb.position = new Vector2(rb.position.x - 0.002f, rb.position.y);//- halfWidth
        }


    }
    #endregion
    #region 绘制测试射线
    private void OnDrawGizmos()
    {
        //可视化左侧和右侧的地面检测
        if (groundCheckLeft != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckLeft.position, groundCheckRadius);
        }
        if (groundCheckRight != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckRight.position, groundCheckRadius);
        }

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