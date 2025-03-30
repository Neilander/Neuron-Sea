using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class SimpleMovingPlatform : MonoBehaviour
    {
        [Header("移动设置")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waitTime = 0.5f;

        [Header("触发设置")]
        [SerializeField] private bool autoStart = true;
        [SerializeField] private bool isTriggered = false;
        [SerializeField] private float triggerDistance = 3f;
        [SerializeField] private bool resetWhenPlayerLeaves = false;

        // 组件引用
        private Rigidbody2D rb;
        private SimpleSwapManager swapManager;

        // 状态
        private int currentWaypointIndex = 0;
        private bool isWaiting = false;
        private bool isMoving = false;
        private Vector3 initialPosition;
        private GameObject player;

        // 设置平台的运动方式
        public enum MovementType
        {
            LoopForward,       // 循环向前
            PingPong,          // 往返移动
            OnceForward,       // 单次向前
            OnceBackward       // 单次向后
        }
        [SerializeField] private MovementType movementType = MovementType.PingPong;

        // 运动状态，用于支持玩家站立
        private enum PlatformState
        {
            Idle,
            Moving,
            Waiting
        }
        private PlatformState currentState = PlatformState.Idle;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            swapManager = FindObjectOfType<SimpleSwapManager>();

            // 记录初始位置
            initialPosition = transform.position;

            // 如果没有设置刚体，则添加一个
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            }

            // 确保刚体是Kinematic，这样平台可以移动而不受物理影响
            rb.bodyType = RigidbodyType2D.Kinematic;

            // 寻找玩家
            player = GameObject.FindGameObjectWithTag("Player");
        }

        void Start()
        {
            // 验证路径点设置
            if (waypoints.Length == 0)
            {
                Debug.LogWarning("移动平台没有设置路径点。添加默认路径点。");
                // 创建两个默认路径点
                waypoints = new Transform[2];

                GameObject wp1 = new GameObject("Waypoint1");
                wp1.transform.parent = transform.parent;
                wp1.transform.position = transform.position;
                waypoints[0] = wp1.transform;

                GameObject wp2 = new GameObject("Waypoint2");
                wp2.transform.parent = transform.parent;
                wp2.transform.position = transform.position + new Vector3(3f, 0, 0);
                waypoints[1] = wp2.transform;
            }

            // 如果设置为自动开始，则开始移动
            if (autoStart && !isTriggered)
            {
                StartMoving();
            }
        }

        void Update()
        {
            // 如果处于交换暂停状态，则不移动
            if (swapManager != null && swapManager.IsPaused())
            {
                return;
            }

            // 检测触发
            if (isTriggered && !isMoving)
            {
                CheckTrigger();
            }

            // 如果设置了自动重置并且玩家离开
            if (resetWhenPlayerLeaves && isMoving)
            {
                CheckReset();
            }
        }

        void FixedUpdate()
        {
            // 如果正在移动，则更新平台位置
            if (isMoving && !isWaiting)
            {
                MovePlatform();
            }
        }

        void MovePlatform()
        {
            // 获取当前目标
            Transform targetPoint = waypoints[currentWaypointIndex];

            // 计算移动方向和距离
            Vector2 targetPosition = targetPoint.position;
            Vector2 currentPosition = rb.position;
            Vector2 direction = (targetPosition - currentPosition).normalized;
            float distance = Vector2.Distance(currentPosition, targetPosition);

            // 根据距离决定速度，确保不会超过目标点
            float speedThisFrame = moveSpeed;
            if (distance < moveSpeed * Time.fixedDeltaTime)
            {
                speedThisFrame = distance / Time.fixedDeltaTime;
            }

            // 移动平台
            Vector2 movement = direction * speedThisFrame * Time.fixedDeltaTime;
            rb.MovePosition(currentPosition + movement);

            // 如果接近目标点，则前往下一个路径点
            if (distance < 0.05f)
            {
                // 到达一个路径点，等待一段时间
                StartCoroutine(WaitAtWaypoint());

                // 更新下一个路径点索引
                UpdateWaypointIndex();
            }

            // 更新状态
            currentState = PlatformState.Moving;
        }

        void UpdateWaypointIndex()
        {
            // 根据移动类型更新下一个路径点
            switch (movementType)
            {
                case MovementType.LoopForward:
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    break;

                case MovementType.PingPong:
                    if (currentWaypointIndex == waypoints.Length - 1)
                    {
                        // 到达终点，开始返回
                        System.Array.Reverse(waypoints);
                        currentWaypointIndex = 1;
                    }
                    else
                    {
                        currentWaypointIndex++;
                    }
                    break;

                case MovementType.OnceForward:
                    currentWaypointIndex++;
                    if (currentWaypointIndex >= waypoints.Length)
                    {
                        // 完成单次向前移动
                        StopMoving();
                    }
                    break;

                case MovementType.OnceBackward:
                    currentWaypointIndex--;
                    if (currentWaypointIndex < 0)
                    {
                        // 完成单次向后移动
                        StopMoving();
                    }
                    break;
            }
        }

        IEnumerator WaitAtWaypoint()
        {
            // 等待一段时间
            isWaiting = true;
            currentState = PlatformState.Waiting;

            yield return new WaitForSeconds(waitTime);

            isWaiting = false;
        }

        void CheckTrigger()
        {
            // 如果玩家靠近并且平台被触发，开始移动
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= triggerDistance)
                {
                    StartMoving();
                }
            }
        }

        void CheckReset()
        {
            // 如果玩家远离，重置平台
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance > triggerDistance * 2)
                {
                    ResetPlatform();
                }
            }
        }

        public void StartMoving()
        {
            isMoving = true;
        }

        public void StopMoving()
        {
            isMoving = false;
        }

        public void ResetPlatform()
        {
            // 停止移动
            StopMoving();

            // 重置位置
            transform.position = initialPosition;

            // 重置路径点索引
            currentWaypointIndex = 0;
        }

        // 碰撞检测，玩家站在平台上时跟随平台移动
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // 找到玩家的刚体
                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    // 设置玩家为平台的子物体，使其跟随平台移动
                    collision.transform.SetParent(transform);
                }
            }
        }

        void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // 取消父子关系
                collision.transform.SetParent(null);
            }
        }

        // 用于编辑器中显示路径
        void OnDrawGizmos()
        {
            // 如果没有设置路径点，退出
            if (waypoints == null || waypoints.Length == 0)
                return;

            // 绘制路径线
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }

            // 绘制路径点
            Gizmos.color = Color.red;
            foreach (Transform waypoint in waypoints)
            {
                if (waypoint != null)
                {
                    Gizmos.DrawSphere(waypoint.position, 0.2f);
                }
            }

            // 如果设置为触发式，绘制触发范围
            if (isTriggered)
            {
                Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
                Gizmos.DrawSphere(transform.position, triggerDistance);
            }
        }
    }
}