using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")] [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Check Settings")] [SerializeField]
    private Transform groundCheck; // 地面检测点

    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private LayerMask groundLayer; // 只检测地面层

    private Animator animator;

    private Rigidbody2D rb;

    private bool isGrounded;

    private void Start(){
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update(){
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
        Move();
        Rotate();
        Jump();
    }

    private void Move(){
        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
    }

    private void Rotate(){
        float moveInput = Input.GetAxis("Horizontal");
        if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
    }

    private void Jump(){
        if (Input.GetButtonDown("Jump")&& isGrounded) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            animator.SetBool("isGrounded", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (collision.gameObject.CompareTag("Ground")) {
            isGrounded = true;
            animator.SetBool("isGrounded", true);
        }
    }
}