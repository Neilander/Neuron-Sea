using UnityEngine;

public class CompanionController : MonoBehaviour
{
    [Header("跟随设置")]
    [SerializeField] private Transform target; // 跟随目标（玩家）
    [SerializeField] private float followSpeed = 5f; // 跟随速度
    [SerializeField] private Vector3 offset = new Vector3(1f, 1f, 0f); // 相对于目标的偏移量
    [SerializeField] private float smoothTime = 0.3f; // 平滑时间
    [SerializeField] private bool autoAdjustPosition = true; // 是否自动根据玩家朝向调整位置

    [Header("动画设置")]
    [SerializeField] private Animator animator; // 动画控制器
    [SerializeField] private string isMovingParam = "IsMoving"; // 移动状态参数名
    [SerializeField] private string directionParam = "Direction"; // 方向参数名
    [SerializeField] private float moveThreshold = 0.1f; // 移动检测阈值

    [Header("位置预设")]
    [SerializeField]
    private Vector3[] positionPresets = new Vector3[]
    {
        new Vector3(1f, 1f, 0f),  // 右上角
        new Vector3(-1f, 1f, 0f), // 左上角
        new Vector3(0f, 1.5f, 0f), // 正上方
        new Vector3(1f, 0.5f, 0f)  // 右侧
    };

    private Vector3 velocity = Vector3.zero;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer targetSpriteRenderer;
    private int currentPresetIndex = 0;
    private Vector3 currentOffset;
    private Vector3 lastPosition;
    private bool isMoving;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("未找到SpriteRenderer组件！");
        }

        if (target != null)
        {
            targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
                Debug.LogWarning("未找到Animator组件！");
            
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (target == null) return;

        // 检测是否在移动
        float distance = Vector3.Distance(transform.position, lastPosition);
        isMoving = distance > moveThreshold;

        // 更新动画状态
        if (animator != null)
        {
            animator.SetBool(isMovingParam, isMoving);

            // 根据目标位置设置方向参数
            float direction = transform.position.x < target.position.x ? -1f : 1f;
            animator.SetFloat(directionParam, direction);
        }

        // 根据玩家scale.x自动调整位置
        if (autoAdjustPosition)
        {
            // 如果玩家朝左（scale.x = -1），跟随物在右上角
            if (target.localScale.x < 0)
            {
                currentOffset = new Vector3(1f, 1f, 0f);
            }
            // 如果玩家朝右（scale.x = 1），跟随物在左上角
            else
            {
                currentOffset = new Vector3(-1f, 1f, 0f);
            }
        }
        else
        {
            currentOffset = offset;
        }

        // 计算目标位置（目标位置 + 偏移量）
        Vector3 targetPosition = target.position + currentOffset;

        // 使用Mathf.Lerp实现平滑跟随
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime,
            followSpeed
        );

        lastPosition = transform.position;
    }

    // 设置跟随目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        }
    }

    // 切换到下一个位置预设
    public void SwitchToNextPosition()
    {
        currentPresetIndex = (currentPresetIndex + 1) % positionPresets.Length;
        offset = positionPresets[currentPresetIndex];
    }

    // 切换到指定位置预设
    public void SwitchToPosition(int index)
    {
        if (index >= 0 && index < positionPresets.Length)
        {
            currentPresetIndex = index;
            offset = positionPresets[index];
        }
        else
        {
            Debug.LogWarning($"无效的位置预设索引: {index}");
        }
    }

    // 设置自定义偏移量
    public void SetCustomOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    // 设置是否自动调整位置
    public void SetAutoAdjustPosition(bool value)
    {
        autoAdjustPosition = value;
    }
}