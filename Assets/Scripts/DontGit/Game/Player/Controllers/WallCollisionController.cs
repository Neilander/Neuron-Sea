using UnityEngine;

public class WallCollisionController : MonoBehaviour
{
    private Rigidbody2D rb;
    private GroundCheckController groundCheckController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundCheckController = GetComponent<GroundCheckController>();
    }

    public void HandleWallCollision()
    {
        if (groundCheckController.IsTouchingWallLeft && !groundCheckController.IsTouchingWallRight)
        {
            rb.position = new Vector2(rb.position.x + 0.002f, rb.position.y);
        }
        else if (groundCheckController.IsTouchingWallRight && !groundCheckController.IsTouchingWallLeft)
        {
            rb.position = new Vector2(rb.position.x - 0.002f, rb.position.y);
        }
    }
}