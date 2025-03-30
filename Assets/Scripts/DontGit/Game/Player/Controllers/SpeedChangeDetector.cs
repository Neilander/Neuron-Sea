using UnityEngine;

public class SpeedChangeDetector : MonoBehaviour
{
    [SerializeField] private float speedChangeThreshold = 0.01f;

    private Rigidbody2D rb;
    private Animator animator;
    private float previousSpeed;
    private float currentSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void CheckSpeedChange()
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

        previousSpeed = currentSpeed;
    }
}