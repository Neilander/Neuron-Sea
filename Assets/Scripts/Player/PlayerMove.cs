using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private int maxJumps = 1; // 允许的最大跳跃次数

    [Header("Ground Check Settings")]
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f); // 地面检测盒大小
    [SerializeField] private Vector3 groundCheckOffset = new Vector3(0f, -0.1f, 0f); // 地面检测盒偏移
    [SerializeField] private LayerMask groundLayer;

    private Animator animator;
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        GroundCheck();
        Move();
        Rotate();
        Jump();
    }

    private void Move()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    private void Rotate()
    {
        float moveInput = Input.GetAxis("Horizontal");
        if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && (isGrounded || jumpCount < maxJumps))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetBool("isGrounded", false);
            jumpCount++;
        }
    }

    private void GroundCheck()
    {
        Vector3 checkPosition = transform.position + groundCheckOffset;
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.BoxCast(checkPosition, groundCheckSize, 0f, Vector2.down, 0.1f, groundLayer);
        animator.SetBool("isGrounded", isGrounded);

        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0; // 触地时重置跳跃次数
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + groundCheckOffset, groundCheckSize);
    }
}