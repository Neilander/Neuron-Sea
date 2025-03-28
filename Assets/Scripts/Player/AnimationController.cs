using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private float previousSpeed; // 用于保存上一帧的速度值
    private float currentSpeed;
    private Rigidbody2D rb;
    [SerializeField] private float speedChangeThreshold = 0.01f; // 速度变化阈值，避免微小波动
    void Start()
    {
        animator = GetComponent<Animator>();
        rb=GetComponent<Rigidbody2D>();
        previousSpeed = 0f;
        
    }

    void Update()
    {
        // 计算保留三位小数的速度
        Vector2 horizontalVelocity = new Vector2(rb.velocity.x, 0f);
        currentSpeed = Mathf.Round(horizontalVelocity.magnitude);

        // 判断速度方向（精确到三位小数）
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
}