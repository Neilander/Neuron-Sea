using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private MovementController movementController;
    private JumpController jumpController;
    private GroundCheckController groundCheckController;
    private WallCollisionController wallCollisionController;
    private SpeedChangeDetector speedChangeDetector;

    private void Awake()
    {
        movementController = GetComponent<MovementController>();
        jumpController = GetComponent<JumpController>();
        groundCheckController = GetComponent<GroundCheckController>();
        // wallCollisionController = GetComponent<WallCollisionController>();
        speedChangeDetector = GetComponent<SpeedChangeDetector>();
    }

    private void Update()
    {
        groundCheckController.CheckGround();
        jumpController.SetGrounded(groundCheckController.IsGrounded);

        float moveInput = Input.GetAxis("Horizontal");

        movementController.Move(moveInput);
        movementController.Rotate(moveInput);
        jumpController.HandleJump();
        // wallCollisionController.HandleWallCollision();
        speedChangeDetector.CheckSpeedChange();
    }
}