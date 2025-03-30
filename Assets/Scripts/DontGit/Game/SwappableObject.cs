using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Collider2D))]
    public class SwappableObject : MonoBehaviour
    {
        [Header("交换属性")]
        [SerializeField] private bool canSwap = true;
        [SerializeField] private Transform anchorPoint;

        [Header("视觉效果")]
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
        [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f, 1f);
        [SerializeField] private float highlightWidth = 0.1f;
        [SerializeField] private float errorFlashSpeed = 2f;

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer highlightRenderer;
        private GameObject highlightObject;

        private bool isHighlighted = false;
        private bool isSelected = false;
        private bool isErrorState = false;

        private Vector3 originalPosition;
        private Vector3 originalScale;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            // 如果没有指定锚点，使用物体中心
            if (anchorPoint == null)
            {
                anchorPoint = transform;
            }

            // 创建高亮边框
            if (canSwap)
            {
                CreateHighlightBorder();
            }
        }

        private void Start()
        {
            // 保存原始值
            originalPosition = transform.position;
            originalScale = transform.localScale;
        }

        private void Update()
        {
            // 更新高亮/错误状态
            UpdateVisualState();
        }

        private void CreateHighlightBorder()
        {
            // 创建高亮边框对象
            highlightObject = new GameObject("Highlight");
            highlightObject.transform.SetParent(transform);
            highlightObject.transform.localPosition = Vector3.zero;
            highlightObject.transform.localRotation = Quaternion.identity;
            highlightObject.transform.localScale = Vector3.one;

            // 添加SpriteRenderer组件
            highlightRenderer = highlightObject.AddComponent<SpriteRenderer>();
            highlightRenderer.sprite = spriteRenderer.sprite;
            highlightRenderer.material = new Material(Shader.Find("Sprites/Default"));
            highlightRenderer.color = highlightColor;

            // 设置排序层级略高于本体
            highlightRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
            highlightRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

            // 初始状态
            UpdateHighlightScale();
            highlightRenderer.enabled = canSwap;
        }

        private void UpdateHighlightScale()
        {
            if (highlightRenderer != null && spriteRenderer != null)
            {
                // 边框稍大于精灵本身
                float scaleX = (spriteRenderer.bounds.size.x + highlightWidth * 2) / spriteRenderer.bounds.size.x;
                float scaleY = (spriteRenderer.bounds.size.y + highlightWidth * 2) / spriteRenderer.bounds.size.y;

                highlightObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
        }

        private void UpdateVisualState()
        {
            if (!canSwap || highlightRenderer == null) return;

            // 当处于错误状态时闪烁红色
            if (isErrorState)
            {
                float errorPulse = Mathf.PingPong(Time.unscaledTime * errorFlashSpeed, 1f);
                highlightRenderer.color = Color.Lerp(errorColor, new Color(errorColor.r, errorColor.g, errorColor.b, 0.3f), errorPulse);
            }
            // 选中状态使用高亮颜色
            else if (isSelected || isHighlighted)
            {
                highlightRenderer.color = highlightColor;
            }

            // 设置主精灵的透明度
            if (isSelected)
            {
                Color color = spriteRenderer.color;
                spriteRenderer.color = new Color(color.r, color.g, color.b, 0.7f);
            }
            else
            {
                Color color = spriteRenderer.color;
                spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
            }
        }

        // 设置高亮状态（鼠标悬停时）
        public void SetHighlighted(bool highlighted)
        {
            if (!canSwap) return;

            isHighlighted = highlighted;

            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = highlighted || isSelected || isErrorState;
            }
        }

        // 设置选中状态（鼠标拖动时）
        public void SetSelected(bool selected)
        {
            if (!canSwap) return;

            isSelected = selected;

            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = isHighlighted || selected || isErrorState;
            }
        }

        // 设置错误状态（不能交换时）
        public void SetErrorState(bool errorState)
        {
            if (!canSwap) return;

            isErrorState = errorState;

            if (highlightRenderer != null)
            {
                highlightRenderer.enabled = true;
            }
        }

        // 重置状态
        public void ResetState()
        {
            isHighlighted = false;
            isSelected = false;
            isErrorState = false;

            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                spriteRenderer.color = new Color(color.r, color.g, color.b, 1f);
            }

            if (highlightRenderer != null)
            {
                highlightRenderer.color = highlightColor;
                highlightRenderer.enabled = canSwap;
            }
        }

        // 交换位置
        public void SwapPosition(SwappableObject other)
        {
            if (!canSwap || !other.canSwap) return;

            // 获取两个对象的锚点位置差异
            Vector3 thisAnchorOffset = transform.position - anchorPoint.position;
            Vector3 otherAnchorOffset = other.transform.position - other.anchorPoint.position;

            // 交换位置（以锚点为基准）
            Vector3 thisPos = transform.position;
            transform.position = other.anchorPoint.position + thisAnchorOffset;
            other.transform.position = anchorPoint.position + otherAnchorOffset;

            // 重置状态
            ResetState();
            other.ResetState();
        }

        // 模拟交换位置（用于检查是否会发生碰撞）
        public (Vector3, Vector3) SimulateSwap(SwappableObject other)
        {
            // 获取两个对象的锚点位置差异
            Vector3 thisAnchorOffset = transform.position - anchorPoint.position;
            Vector3 otherAnchorOffset = other.transform.position - other.anchorPoint.position;

            // 计算交换后的位置
            Vector3 newThisPos = other.anchorPoint.position + thisAnchorOffset;
            Vector3 newOtherPos = anchorPoint.position + otherAnchorOffset;

            return (newThisPos, newOtherPos);
        }

        // 判断物体是否可交换
        public bool CanSwap()
        {
            return canSwap;
        }

        // 获取锚点
        public Transform GetAnchorPoint()
        {
            return anchorPoint;
        }

        // 重置到初始位置
        public void ResetToOriginalPosition()
        {
            transform.position = originalPosition;
            transform.localScale = originalScale;
            ResetState();
        }
    }
}