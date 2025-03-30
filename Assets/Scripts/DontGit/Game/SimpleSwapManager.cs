using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    public class SimpleSwapManager : MonoBehaviour
    {
        [Header("交换设置")]
        [SerializeField] private int maxSwaps = 5;
        [SerializeField] private float swapValidDistance = 5f;
        [SerializeField] private GameObject swapLinePrefab;
        [SerializeField] private Color validSwapColor = Color.green;
        [SerializeField] private Color invalidSwapColor = Color.red;

        [Header("移动平台设置")]
        [SerializeField] private bool pauseWhileSwapping = true;

        [Header("触摸输入设置")]
        [SerializeField] private bool enableTouchInput = true;
        [SerializeField] private float touchRadius = 0.5f;

        // 状态
        private SimpleSwappableObject selectedObject = null;
        private SimpleSwappableObject hoveredObject = null;
        private bool isPaused = false;
        private int currentSwaps = 0;
        private bool canSwap = true;

        // 交换线
        private LineRenderer swapLine;

        // 事件
        public event Action OnSwapPerformed;

        void Awake()
        {
            // 创建交换线
            if (swapLinePrefab != null)
            {
                GameObject lineObj = Instantiate(swapLinePrefab, transform);
                swapLine = lineObj.GetComponent<LineRenderer>();
            }
            else
            {
                // 创建默认的LineRenderer
                GameObject lineObj = new GameObject("SwapLine");
                lineObj.transform.SetParent(transform);
                swapLine = lineObj.AddComponent<LineRenderer>();
                swapLine.startWidth = 0.1f;
                swapLine.endWidth = 0.1f;
                swapLine.positionCount = 2;
                swapLine.material = new Material(Shader.Find("Sprites/Default"));
            }

            // 禁用交换线，直到需要时再启用
            if (swapLine != null)
            {
                swapLine.enabled = false;
            }
        }

        void Update()
        {
            // 处理输入
            HandleInput();

            // 更新交换线
            UpdateSwapLine();
        }

        void HandleInput()
        {
            // 检查是否可以进行交换操作
            if (!canSwap) return;

            // 鼠标输入处理
            if (Input.GetMouseButtonDown(0))
            {
                // 检查是否点击了UI元素
                if (UnityEngine.EventSystems.EventSystem.current != null &&
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }

                // 如果启用了触摸输入，处理触摸
                if (enableTouchInput && Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        HandleTouchInput(touch.position);
                    }
                }
            }

            // 取消选择
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelSelection();
            }
        }

        void HandleTouchInput(Vector2 touchPosition)
        {
            // 转换触摸位置为世界坐标
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, 10f));

            // 检测触摸位置是否有可交换对象
            Collider2D[] colliders = Physics2D.OverlapCircleAll(new Vector2(worldPos.x, worldPos.y), touchRadius);
            SimpleSwappableObject touchedObject = null;

            foreach (Collider2D collider in colliders)
            {
                SimpleSwappableObject swappable = collider.GetComponent<SimpleSwappableObject>();
                if (swappable != null && swappable.CanBeSwapped())
                {
                    touchedObject = swappable;
                    break;
                }
            }

            if (touchedObject != null)
            {
                // 如果已经有选择的对象，尝试交换
                if (selectedObject != null && selectedObject != touchedObject)
                {
                    TrySwapObjects(selectedObject, touchedObject);
                }
                else
                {
                    // 选择新对象
                    SelectObject(touchedObject);
                }
            }
            else
            {
                // 在空白处点击，取消选择
                CancelSelection();
            }
        }

        public void OnObjectHover(SimpleSwappableObject obj)
        {
            hoveredObject = obj;
        }

        public void OnObjectExit(SimpleSwappableObject obj)
        {
            if (hoveredObject == obj)
            {
                hoveredObject = null;
            }
        }

        public void OnObjectSelected(SimpleSwappableObject obj)
        {
            // 如果已经有选择的对象，尝试交换
            if (selectedObject != null && selectedObject != obj)
            {
                TrySwapObjects(selectedObject, obj);
            }
            else
            {
                // 选择新对象
                SelectObject(obj);
            }
        }

        void SelectObject(SimpleSwappableObject obj)
        {
            // 取消之前的选择
            if (selectedObject != null)
            {
                selectedObject.SetSelected(false);
            }

            // 设置新选择
            selectedObject = obj;
            if (selectedObject != null)
            {
                selectedObject.SetSelected(true);

                // 如果需要，暂停移动平台
                if (pauseWhileSwapping)
                {
                    SetPaused(true);
                }
            }
        }

        void CancelSelection()
        {
            if (selectedObject != null)
            {
                selectedObject.SetSelected(false);
                selectedObject = null;

                // 取消暂停状态
                if (pauseWhileSwapping)
                {
                    SetPaused(false);
                }
            }

            // 隐藏交换线
            if (swapLine != null)
            {
                swapLine.enabled = false;
            }
        }

        void TrySwapObjects(SimpleSwappableObject obj1, SimpleSwappableObject obj2)
        {
            // 检查是否可以交换
            if (CanSwapObjects(obj1, obj2))
            {
                // 执行交换
                obj1.PerformSwap(obj2);

                // 增加交换次数
                currentSwaps++;

                // 触发事件
                OnSwapPerformed?.Invoke();

                // 重置选择状态
                CancelSelection();

                // 如果达到最大交换次数，禁用进一步的交换
                if (currentSwaps >= maxSwaps)
                {
                    canSwap = false;
                }
            }
            else
            {
                // 显示错误状态
                obj1.SetError(true);
                obj2.SetError(true);
            }
        }

        bool CanSwapObjects(SimpleSwappableObject obj1, SimpleSwappableObject obj2)
        {
            // 检查是否达到最大交换次数
            if (currentSwaps >= maxSwaps)
            {
                return false;
            }

            // 检查距离
            float distance = Vector3.Distance(obj1.transform.position, obj2.transform.position);
            return distance <= swapValidDistance;
        }

        void UpdateSwapLine()
        {
            if (swapLine == null) return;

            // 如果有选择的对象，显示交换线
            if (selectedObject != null)
            {
                swapLine.enabled = true;

                // 设置起始点为选择的对象位置
                Vector3 startPos = selectedObject.transform.position;
                startPos.z = -1; // 确保线在前面
                swapLine.SetPosition(0, startPos);

                // 设置终点
                Vector3 endPos;
                if (hoveredObject != null)
                {
                    // 如果鼠标悬停在另一个对象上，指向那个对象
                    endPos = hoveredObject.transform.position;

                    // 设置线的颜色
                    bool canSwap = CanSwapObjects(selectedObject, hoveredObject);
                    swapLine.startColor = swapLine.endColor = canSwap ? validSwapColor : invalidSwapColor;
                }
                else
                {
                    // 否则指向鼠标位置
                    Vector3 mousePos = Input.mousePosition;
                    mousePos.z = 10; // 设置深度，保证正确计算世界坐标
                    endPos = Camera.main.ScreenToWorldPoint(mousePos);

                    // 设置默认颜色
                    swapLine.startColor = swapLine.endColor = validSwapColor;
                }

                endPos.z = -1; // 确保线在前面
                swapLine.SetPosition(1, endPos);
            }
            else
            {
                // 隐藏交换线
                swapLine.enabled = false;
            }
        }

        // 设置暂停状态
        public void SetPaused(bool paused)
        {
            isPaused = paused;
        }

        // 获取是否暂停
        public bool IsPaused()
        {
            return isPaused;
        }

        // 设置最大交换次数
        public void SetMaxSwaps(int max)
        {
            maxSwaps = max;
        }

        // 获取当前交换次数
        public int GetCurrentSwaps()
        {
            return currentSwaps;
        }

        // 重置交换管理器状态
        public void ResetState()
        {
            currentSwaps = 0;
            canSwap = true;
            CancelSelection();
        }
    }
}