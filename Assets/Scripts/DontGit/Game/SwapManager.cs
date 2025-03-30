using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    public class SwapManager : MonoBehaviour
    {
        [Header("交换设置")]
        [SerializeField] private LayerMask swappableLayer;
        [SerializeField] private float swapCollisionCheckRadius = 0.1f;
        [SerializeField] private LayerMask collisionCheckLayer;
        [SerializeField] private int maxSwaps = 5; // 添加最大交换次数

        [Header("移动端支持")]
        [SerializeField] private bool enableMobileInput = false;
        [SerializeField] private float touchRadius = 0.5f;

        private bool isDragging = false;
        private SwappableObject selectedObject = null;
        private SwappableObject targetObject = null;

        private Camera mainCamera;
        private int swapCount = 0;

        // 游戏暂停状态管理
        private bool isPaused = false;
        private bool isFakePaused = false; // 假暂停（交换模式下的暂停）
        private float timeScaleBeforePause;

        // 粘连机制相关
        private Dictionary<SwappableObject, SwappableObject> stuckObjects = new Dictionary<SwappableObject, SwappableObject>();

        // 添加事件
        public event Action OnSwapPerformed;

        // 添加接口兼容方法
        public void OnObjectHover(SimpleSwappableObject obj)
        {
            // 简化版的悬停处理方法
            // 这里可以不做实际操作，仅提供接口兼容
        }

        public void OnObjectExit(SimpleSwappableObject obj)
        {
            // 简化版的鼠标离开处理方法
            // 这里可以不做实际操作，仅提供接口兼容
        }

        public void OnObjectSelected(SimpleSwappableObject obj)
        {
            // 简化版的选择处理方法
            // 这里可以不做实际操作，仅提供接口兼容
        }

        // 设置最大交换次数
        public void SetMaxSwaps(int max)
        {
            maxSwaps = max;
        }

        // 获取当前已使用的交换次数
        public int GetCurrentSwaps()
        {
            return swapCount;
        }

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Start()
        {
            // 游戏开始时重置交换次数
            ResetSwapCount();
        }

        private void Update()
        {
            // 如果真暂停状态，除了暂停/继续，其他逻辑不执行
            if (isPaused && !Input.GetKeyDown(KeyCode.Escape) && !Input.GetMouseButtonDown(1))
            {
                return;
            }

            // 处理暂停/继续输入
            HandlePauseInput();

            // 处理物体交换逻辑
            HandleSwapInput();

            // 处理重开本关的输入
            if (Input.GetKeyDown(KeyCode.R))
            {
                GameManager.Instance.RestartLevel();
            }
        }

        private void HandlePauseInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }

        private void HandleSwapInput()
        {
            // 处理PC输入
            if (!enableMobileInput)
            {
                // 获取鼠标位置并转为世界坐标
                Vector3 mousePosition = Input.mousePosition;
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));

                // 检测鼠标下方物体
                Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition, swappableLayer);
                SwappableObject hoveredObject = hitCollider ? hitCollider.GetComponent<SwappableObject>() : null;

                // 更新鼠标悬停效果
                UpdateHoverState(hoveredObject);

                // 处理拖拽开始
                if (Input.GetMouseButtonDown(0) && hoveredObject != null && hoveredObject.CanSwap())
                {
                    StartDragging(hoveredObject);
                }

                // 处理拖拽中
                if (isDragging && selectedObject != null)
                {
                    // 设置选中物体跟随鼠标
                    UpdateDragging(worldPosition);

                    // 检测目标物体
                    if (hoveredObject != null && hoveredObject != selectedObject && hoveredObject.CanSwap())
                    {
                        SetTargetObject(hoveredObject);
                    }
                    else if (targetObject != null && (hoveredObject == null || hoveredObject == selectedObject))
                    {
                        ClearTargetObject();
                    }
                }

                // 处理拖拽结束
                if (isDragging && Input.GetMouseButtonUp(0))
                {
                    EndDragging();
                }
            }
            // 处理移动端输入
            else
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    Vector3 touchPosition = mainCamera.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, -mainCamera.transform.position.z));

                    // 检测触摸位置的物体
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(touchPosition, touchRadius, swappableLayer);
                    SwappableObject touchedObject = null;

                    if (hitColliders.Length > 0)
                    {
                        touchedObject = hitColliders[0].GetComponent<SwappableObject>();
                    }

                    // 更新悬停效果
                    UpdateHoverState(touchedObject);

                    // 处理触摸开始
                    if (touch.phase == TouchPhase.Began && touchedObject != null && touchedObject.CanSwap())
                    {
                        StartDragging(touchedObject);
                    }

                    // 处理触摸移动
                    if (isDragging && selectedObject != null && (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary))
                    {
                        UpdateDragging(touchPosition);

                        if (touchedObject != null && touchedObject != selectedObject && touchedObject.CanSwap())
                        {
                            SetTargetObject(touchedObject);
                        }
                        else if (targetObject != null && (touchedObject == null || touchedObject == selectedObject))
                        {
                            ClearTargetObject();
                        }
                    }

                    // 处理触摸结束
                    if (isDragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                    {
                        EndDragging();
                    }
                }
            }
        }

        private void UpdateHoverState(SwappableObject hoveredObject)
        {
            // 清除之前的所有高亮状态
            SwappableObject[] swappables = FindObjectsOfType<SwappableObject>();
            foreach (SwappableObject obj in swappables)
            {
                if (obj != selectedObject && obj != targetObject)
                {
                    obj.SetHighlighted(false);
                }
            }

            // 设置当前悬停物体的高亮状态
            if (hoveredObject != null && hoveredObject != selectedObject && hoveredObject != targetObject)
            {
                hoveredObject.SetHighlighted(true);
            }
        }

        private void StartDragging(SwappableObject obj)
        {
            isDragging = true;
            selectedObject = obj;
            selectedObject.SetSelected(true);

            // 激活假暂停
            if (!isPaused)
            {
                FakePause();
            }
        }

        private void UpdateDragging(Vector3 position)
        {
            if (selectedObject == null) return;

            // 使选中物体跟随鼠标/触摸位置
            Vector3 originalZ = new Vector3(0, 0, selectedObject.transform.position.z);
            selectedObject.transform.position = new Vector3(position.x, position.y, 0) + originalZ;
        }

        private void SetTargetObject(SwappableObject obj)
        {
            // 清除之前的目标
            if (targetObject != null && targetObject != obj)
            {
                targetObject.ResetState();
            }

            targetObject = obj;

            // 检查是否可以交换（是否会发生碰撞）
            if (IsSwapValid(selectedObject, targetObject))
            {
                targetObject.SetSelected(true);
                targetObject.SetErrorState(false);
                selectedObject.SetErrorState(false);
            }
            else
            {
                targetObject.SetErrorState(true);
                selectedObject.SetErrorState(true);
            }
        }

        private void ClearTargetObject()
        {
            if (targetObject != null)
            {
                targetObject.ResetState();
                targetObject = null;
            }

            if (selectedObject != null)
            {
                selectedObject.SetErrorState(false);
            }
        }

        private void EndDragging()
        {
            isDragging = false;

            // 如果有目标对象，检查是否可以交换
            if (targetObject != null && IsSwapValid(selectedObject, targetObject))
            {
                // 执行交换
                SwapObjects(selectedObject, targetObject);
            }
            else
            {
                // 如果没有目标对象或无法交换，重置选择对象的位置和状态
                if (selectedObject != null)
                {
                    selectedObject.ResetState();
                }

                // 恢复游戏
                if (isFakePaused)
                {
                    FakeResume();
                }
            }

            // 清除选择和目标
            selectedObject = null;
            targetObject = null;
        }

        private bool IsSwapValid(SwappableObject obj1, SwappableObject obj2)
        {
            // 如果达到最大交换次数，不允许再交换
            if (swapCount >= maxSwaps)
            {
                return false;
            }

            // 检查是否已经是粘连状态
            SwappableObject obj1Root = GetRootObject(obj1);
            SwappableObject obj2Root = GetRootObject(obj2);
            if (obj1Root == obj2Root)
            {
                return false;
            }

            // 检查交换后是否会与其他物体或玩家重叠
            (Vector3 newPos1, Vector3 newPos2) = obj1.SimulateSwap(obj2);

            // 获取两个物体的碰撞体
            Collider2D collider1 = obj1.GetComponent<Collider2D>();
            Collider2D collider2 = obj2.GetComponent<Collider2D>();

            // 临时禁用这两个物体的碰撞体，防止检测到自身
            if (collider1 != null) collider1.enabled = false;
            if (collider2 != null) collider2.enabled = false;

            // 检查obj1移动到新位置是否会与其他物体重叠
            bool obj1Valid = !Physics2D.OverlapCircle(newPos1, swapCollisionCheckRadius, collisionCheckLayer);

            // 检查obj2移动到新位置是否会与其他物体重叠
            bool obj2Valid = !Physics2D.OverlapCircle(newPos2, swapCollisionCheckRadius, collisionCheckLayer);

            // 重新启用碰撞体
            if (collider1 != null) collider1.enabled = true;
            if (collider2 != null) collider2.enabled = true;

            return obj1Valid && obj2Valid;
        }

        private void SwapObjects(SwappableObject obj1, SwappableObject obj2)
        {
            // 检查是否是粘连物体
            SwappableObject obj1Root = GetRootObject(obj1);
            SwappableObject obj2Root = GetRootObject(obj2);

            // 如果两者根物体相同，则它们已经粘连，不能交换
            if (obj1Root == obj2Root) return;

            // 获取所有与 obj1 粘连的物体
            List<SwappableObject> obj1Group = GetStuckGroup(obj1);

            // 获取所有与 obj2 粘连的物体
            List<SwappableObject> obj2Group = GetStuckGroup(obj2);

            // 交换每个组中物体的位置
            foreach (var objInGroup1 in obj1Group)
            {
                foreach (var objInGroup2 in obj2Group)
                {
                    objInGroup1.SwapPosition(objInGroup2);
                    break; // 只需要交换一次
                }
            }

            // 增加交换次数
            swapCount++;

            // 触发交换完成事件
            OnSwapPerformed?.Invoke();

            // 检查粘连机制
            CheckSticking(obj1, obj2);

            // 如果交换发生在假暂停状态，恢复游戏
            if (isFakePaused)
            {
                FakeResume();
            }
        }

        private void CheckSticking(SwappableObject obj1, SwappableObject obj2)
        {
            // 获取交换后的位置
            Vector3 pos1 = obj1.transform.position;
            Vector3 pos2 = obj2.transform.position;

            // 检查两个物体交换后是否紧贴
            float distance = Vector3.Distance(pos1, pos2);
            if (distance <= swapCollisionCheckRadius * 2)
            {
                // 确定哪个物体移动了（哪个是根物体）
                SwappableObject rootObj = obj1;
                SwappableObject stickObj = obj2;

                // 将两个物体粘连
                StickObjects(rootObj, stickObj);
            }
        }

        private void StickObjects(SwappableObject root, SwappableObject stick)
        {
            // 如果已经存在粘连关系，先解除
            if (stuckObjects.ContainsKey(stick))
            {
                stuckObjects.Remove(stick);
            }

            // 建立新的粘连关系
            stuckObjects[stick] = root;

            // 视觉效果可以在这里添加，如颜色变化等
            Debug.Log($"物体 {stick.name} 已与 {root.name} 粘连");
        }

        private SwappableObject GetRootObject(SwappableObject obj)
        {
            // 查找物体的根物体（粘连组的根）
            if (stuckObjects.TryGetValue(obj, out SwappableObject root))
            {
                // 递归查找最终的根
                return GetRootObject(root);
            }
            return obj; // 自己就是根
        }

        private List<SwappableObject> GetStuckGroup(SwappableObject obj)
        {
            // 获取与指定物体粘连的所有物体
            List<SwappableObject> group = new List<SwappableObject>();
            SwappableObject rootObj = GetRootObject(obj);
            group.Add(rootObj);

            // 查找所有以这个根物体为根的物体
            foreach (var pair in stuckObjects)
            {
                if (GetRootObject(pair.Key) == rootObj)
                {
                    group.Add(pair.Key);
                }
            }

            return group;
        }

        private void PauseGame()
        {
            if (isPaused) return;

            isPaused = true;
            timeScaleBeforePause = Time.timeScale;
            Time.timeScale = 0f;

            // 通知其他系统游戏暂停
            // ...
        }

        private void ResumeGame()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = timeScaleBeforePause;

            // 通知其他系统游戏继续
            // ...
        }

        private void FakePause()
        {
            if (isFakePaused) return;

            isFakePaused = true;
            // 进入交换模式，可以暂停一些系统但不影响时间
            // ...
        }

        private void FakeResume()
        {
            if (!isFakePaused) return;

            isFakePaused = false;
            // 退出交换模式
            // ...
        }

        public void ResetSwapCount()
        {
            swapCount = 0;
        }

        public int GetSwapCount()
        {
            return swapCount;
        }

        public bool IsPaused()
        {
            return isPaused || isFakePaused;
        }
    }
}