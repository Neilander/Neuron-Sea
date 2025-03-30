using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GridManager : MonoBehaviour
    {
        [Header("网格设置")]
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private Vector2 gridOffset = Vector2.zero;
        [SerializeField] private bool showGridInEditor = true;
        [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);

        [Header("自动对齐")]
        [SerializeField] private bool alignOnStart = true;
        [SerializeField] private bool alignSwappableObjects = true;
        [SerializeField] private LayerMask objectsToAlign;

        // 单例实例
        public static GridManager Instance { get; private set; }

        private void Awake()
        {
            // 单例模式设置
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (alignOnStart)
            {
                AlignAllObjectsToGrid();
            }
        }

        // 将所有对象对齐到网格
        public void AlignAllObjectsToGrid()
        {
            if (alignSwappableObjects)
            {
                SwappableObject[] swappableObjects = FindObjectsOfType<SwappableObject>();
                foreach (SwappableObject obj in swappableObjects)
                {
                    AlignObjectToGrid(obj.gameObject);
                }
            }

            // 如果有其他需要对齐的对象，可以在这里添加
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(1000, 1000), 0, objectsToAlign);
            foreach (Collider2D collider in colliders)
            {
                // 避免重复对齐已经检查过的SwappableObject
                if (!alignSwappableObjects || collider.GetComponent<SwappableObject>() == null)
                {
                    AlignObjectToGrid(collider.gameObject);
                }
            }
        }

        // 将单个对象对齐到网格
        public void AlignObjectToGrid(GameObject obj)
        {
            if (obj == null) return;

            // 获取当前位置
            Vector3 position = obj.transform.position;

            // 计算网格对齐后的位置
            Vector3 alignedPosition = new Vector3(
                Mathf.Round((position.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x,
                Mathf.Round((position.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y,
                position.z
            );

            // 设置新位置
            obj.transform.position = alignedPosition;
        }

        // 将位置对齐到网格
        public Vector3 AlignPositionToGrid(Vector3 position)
        {
            // 计算网格对齐后的位置
            Vector3 alignedPosition = new Vector3(
                Mathf.Round((position.x - gridOffset.x) / gridSize) * gridSize + gridOffset.x,
                Mathf.Round((position.y - gridOffset.y) / gridSize) * gridSize + gridOffset.y,
                position.z
            );

            return alignedPosition;
        }

        // 获取最近的网格点
        public Vector3 GetNearestGridPoint(Vector3 position)
        {
            return AlignPositionToGrid(position);
        }

        // 检查位置是否在网格点上
        public bool IsPositionOnGridPoint(Vector3 position, float tolerance = 0.01f)
        {
            Vector3 alignedPosition = AlignPositionToGrid(position);
            return Vector3.Distance(position, alignedPosition) < tolerance;
        }

        // 获取网格大小
        public float GetGridSize()
        {
            return gridSize;
        }

        // 获取网格偏移
        public Vector2 GetGridOffset()
        {
            return gridOffset;
        }

        // 在编辑器中绘制网格
        private void OnDrawGizmos()
        {
            if (!showGridInEditor) return;

            Gizmos.color = gridColor;

            // 获取视图大小
            float viewSize = 50f; // 固定大小

            // 绘制网格线
            for (float x = -viewSize; x <= viewSize; x += gridSize)
            {
                float alignedX = Mathf.Round((x - gridOffset.x) / gridSize) * gridSize + gridOffset.x;
                Gizmos.DrawLine(
                    new Vector3(alignedX, -viewSize + gridOffset.y, 0),
                    new Vector3(alignedX, viewSize + gridOffset.y, 0)
                );
            }

            for (float y = -viewSize; y <= viewSize; y += gridSize)
            {
                float alignedY = Mathf.Round((y - gridOffset.y) / gridSize) * gridSize + gridOffset.y;
                Gizmos.DrawLine(
                    new Vector3(-viewSize + gridOffset.x, alignedY, 0),
                    new Vector3(viewSize + gridOffset.x, alignedY, 0)
                );
            }

            // 绘制坐标系原点
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(new Vector3(gridOffset.x, gridOffset.y, 0), 0.1f);
        }
    }
}