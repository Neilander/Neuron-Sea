using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game
{
    public class CollectibleItem : MonoBehaviour
    {
        [Header("视觉设置")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float hoverHeight = 0.5f;
        [SerializeField] private float hoverSpeed = 1f;
        [SerializeField] private float rotationSpeed = 30f;

        [Header("文本设置")]
        [SerializeField] private TextMeshPro swapCountText;
        [SerializeField] private TextMeshPro targetCountText;

        private int currentSwapCount;
        private int targetSwapCount;
        private bool isActive = false;
        private bool isCollected = false;

        private Vector3 startPosition;

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (swapCountText == null)
            {
                swapCountText = transform.Find("SwapCountText")?.GetComponent<TextMeshPro>();
            }

            if (targetCountText == null)
            {
                targetCountText = transform.Find("TargetCountText")?.GetComponent<TextMeshPro>();
            }
        }

        private void Start()
        {
            startPosition = transform.position;
            UpdateVisuals();
        }

        private void Update()
        {
            // 悬浮效果
            float newY = startPosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // 旋转效果
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }

        // 设置交换次数和目标次数
        public void SetSwapCount(int current, int target)
        {
            currentSwapCount = current;
            targetSwapCount = target;

            // 如果当前交换次数小于等于目标次数，则收集品处于激活状态
            isActive = currentSwapCount <= targetSwapCount;

            UpdateVisuals();
        }

        // 更新视觉效果
        private void UpdateVisuals()
        {
            // 更新颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isActive ? activeColor : inactiveColor;
            }

            // 更新文本
            if (swapCountText != null)
            {
                swapCountText.text = currentSwapCount.ToString();
            }

            if (targetCountText != null)
            {
                targetCountText.text = "目标: " + targetSwapCount.ToString();
            }
        }

        // 当玩家触碰收集品
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag("Player") && isActive)
            {
                CollectItem();
            }
        }

        // 收集物品
        private void CollectItem()
        {
            isCollected = true;

            // 播放收集动画
            StartCoroutine(CollectAnimation());

            // 通知游戏管理器收集成功
            // 这里可以添加收集奖励逻辑
        }

        // 收集动画
        private IEnumerator CollectAnimation()
        {
            float duration = 0.5f;
            float elapsed = 0f;

            Vector3 startScale = transform.localScale;
            Vector3 endScale = Vector3.zero;

            Color startColor = spriteRenderer.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                // 缩小效果
                transform.localScale = Vector3.Lerp(startScale, endScale, t);

                // 淡出效果
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(startColor, endColor, t);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 动画结束后销毁对象
            Destroy(gameObject);
        }

        // 获取收集状态
        public bool IsCollected()
        {
            return isCollected;
        }

        // 获取激活状态
        public bool IsActive()
        {
            return isActive;
        }
    }
}