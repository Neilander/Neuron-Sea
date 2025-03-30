using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        public enum MovementType
        {
            Auto,       // 自动移动
            Triggered,  // 触发后移动
            TimedReturn // 触发后移动并定时返回
        }

        [Header("移动设置")]
        [SerializeField] private MovementType movementType = MovementType.Auto;
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float waitTime = 0.5f;
        [SerializeField] private bool cyclic = true;

        [Header("触发设置")]
        [SerializeField] private float returnDelay = 3f; // 对于TimedReturn类型的平台，在此时间后返回

        private Rigidbody2D rb;
        private int currentWaypointIndex = 0;
        private float waitCounter = 0f;
        private bool isMoving = true;
        private bool isMovingForward = true;
        private Vector3 originalPosition;
        private bool playerOnPlatform = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            // 如果是触发类型，初始状态不移动
            if (movementType != MovementType.Auto)
            {
                isMoving = false;
            }

            originalPosition = transform.position;
        }

        private void Start()
        {
            // 如果没有设置路径点，使用当前位置作为唯一路径点
            if (waypoints == null || waypoints.Length == 0)
            {
                waypoints = new Transform[1];
                GameObject waypointObj = new GameObject("Waypoint");
                waypointObj.transform.position = transform.position;
                waypointObj.transform.parent = transform.parent;
                waypoints[0] = waypointObj.transform;
            }
        }

        private void FixedUpdate()
        {
            if (!isMoving || waypoints.Length == 0) return;

            // 处理等待时间
            if (waitCounter > 0)
            {
                waitCounter -= Time.fixedDeltaTime;
                return;
            }

            // 移动平台
            MovePlatform();
        }

        private void MovePlatform()
        {
            // 获取目标路径点
            Transform targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint == null)
            {
                // 如果路径点被销毁，跳过它
                IncrementWaypointIndex();
                return;
            }

            // 移动平台
            Vector3 targetPosition = targetWaypoint.position;
            Vector3 moveDirection = (targetPosition - transform.position).normalized;

            rb.velocity = moveDirection * moveSpeed;

            // 检查是否到达路径点
            float distance = Vector3.Distance(transform.position, targetPosition);
            if (distance < 0.1f)
            {
                rb.velocity = Vector2.zero;

                // 增加路径点索引
                IncrementWaypointIndex();

                // 设置等待时间
                waitCounter = waitTime;
            }
        }

        private void IncrementWaypointIndex()
        {
            if (isMovingForward)
            {
                currentWaypointIndex++;

                // 如果到达末尾
                if (currentWaypointIndex >= waypoints.Length)
                {
                    if (cyclic)
                    {
                        // 循环从头开始
                        currentWaypointIndex = 0;
                    }
                    else
                    {
                        // 非循环模式，反向移动
                        currentWaypointIndex = waypoints.Length - 2;
                        isMovingForward = false;

                        // 对于TimedReturn类型，停止移动并设置定时器返回
                        if (movementType == MovementType.TimedReturn)
                        {
                            isMoving = false;
                            Invoke("ReturnToOriginalPosition", returnDelay);
                        }
                    }
                }
            }
            else
            {
                currentWaypointIndex--;

                // 如果到达起点
                if (currentWaypointIndex < 0)
                {
                    if (cyclic)
                    {
                        // 循环到末尾
                        currentWaypointIndex = waypoints.Length - 1;
                    }
                    else
                    {
                        // 非循环模式，正向移动
                        currentWaypointIndex = 1;
                        isMovingForward = true;

                        // 对于TimedReturn类型，停止移动
                        if (movementType == MovementType.TimedReturn)
                        {
                            isMoving = false;
                        }
                    }
                }
            }
        }

        // 触发移动
        public void TriggerMovement()
        {
            if (movementType == MovementType.Auto) return;

            isMoving = true;
            isMovingForward = true;
            currentWaypointIndex = 0;
        }

        // 返回原始位置
        private void ReturnToOriginalPosition()
        {
            // 将平台移回原始位置
            transform.position = originalPosition;
            currentWaypointIndex = 0;
            isMovingForward = true;
        }

        // 检测玩家是否站在平台上
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerOnPlatform = true;

                // 如果是触发类型的平台，开始移动
                if (movementType != MovementType.Auto && !isMoving)
                {
                    TriggerMovement();
                }

                // 让玩家成为平台的子物体，这样玩家会跟随平台移动
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                playerOnPlatform = false;

                // 当玩家离开平台时，解除父子关系
                collision.transform.SetParent(null);
            }
        }

        // 绘制路径点，便于在编辑器中查看
        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.blue;

            // 绘制路径点
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;

                Vector3 position = waypoints[i].position;
                Gizmos.DrawSphere(position, 0.2f);

                // 绘制连接线
                if (i < waypoints.Length - 1 && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(position, waypoints[i + 1].position);
                }
                else if (cyclic && i == waypoints.Length - 1 && waypoints[0] != null)
                {
                    // 如果是循环模式，连接最后一个点和第一个点
                    Gizmos.DrawLine(position, waypoints[0].position);
                }
            }
        }
    }
}