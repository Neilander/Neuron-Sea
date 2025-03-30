using UnityEngine;

public class GroundCheckController : MonoBehaviour
{
    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheckLeft;
    [SerializeField] private Transform groundCheckRight;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Wall Check Settings")]
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private LayerMask wallLayer;

    private bool isGrounded;
    private bool isTouchingWallLeft;
    private bool isTouchingWallRight;

    public bool IsGrounded => isGrounded;
    public bool IsTouchingWallLeft => isTouchingWallLeft;
    public bool IsTouchingWallRight => isTouchingWallRight;

    public void CheckGround()
    {
        isTouchingWallLeft = Physics2D.OverlapCircle(wallCheckLeft.position, wallCheckRadius, wallLayer);
        isTouchingWallRight = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);

        isGrounded = Physics2D.OverlapCircle(groundCheckLeft.position, groundCheckRadius, groundLayer) ||
                    Physics2D.OverlapCircle(groundCheckRight.position, groundCheckRadius, groundLayer);
    }

    private void OnDrawGizmos()
    {
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
    }
}