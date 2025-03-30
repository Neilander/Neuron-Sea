using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class SimpleSwappableObject : MonoBehaviour
    {
        [Header("交换设置")]
        [SerializeField] private bool canBeSwapped = true;
        [SerializeField] private float swapCooldown = 0.5f;
        [SerializeField] private GameObject swapVFXPrefab;

        [Header("外观设置")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightedColor = Color.yellow;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color errorColor = Color.red;

        // 组件引用
        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private Collider2D col;

        // 状态
        private bool isHighlighted = false;
        private bool isSelected = false;
        private bool isError = false;
        private bool isCoolingDown = false;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        // 交换相关
        private SwapManager swapManager;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            // 记录初始位置和旋转
            originalPosition = transform.position;
            originalRotation = transform.rotation;

            // 获取交换管理器
            swapManager = FindObjectOfType<SwapManager>();
        }

        void Start()
        {
            // 确保有默认的碰撞器
            if (col == null)
            {
                col = gameObject.AddComponent<BoxCollider2D>();
            }

            // 确保有默认的Rigidbody
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }

            // 设置默认颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        void Update()
        {
            // 更新视觉状态
            UpdateVisualState();
        }

        void UpdateVisualState()
        {
            if (spriteRenderer == null) return;

            // 根据状态设置颜色
            if (isError)
            {
                spriteRenderer.color = errorColor;
            }
            else if (isSelected)
            {
                spriteRenderer.color = selectedColor;
            }
            else if (isHighlighted)
            {
                spriteRenderer.color = highlightedColor;
            }
            else
            {
                spriteRenderer.color = normalColor;
            }
        }

        // 当鼠标悬停在对象上时
        void OnMouseEnter()
        {
            if (!canBeSwapped || isCoolingDown) return;

            isHighlighted = true;

            // 通知交换管理器此对象可以被选中
            if (swapManager != null)
            {
                swapManager.OnObjectHover(this);
            }
        }

        // 当鼠标离开对象时
        void OnMouseExit()
        {
            if (!canBeSwapped) return;

            isHighlighted = false;

            // 通知交换管理器此对象不再被悬停
            if (swapManager != null)
            {
                swapManager.OnObjectExit(this);
            }
        }

        // 当鼠标点击对象时
        void OnMouseDown()
        {
            if (!canBeSwapped || isCoolingDown) return;

            // 通知交换管理器此对象被选中
            if (swapManager != null)
            {
                swapManager.OnObjectSelected(this);
            }
        }

        // 设置选中状态
        public void SetSelected(bool selected)
        {
            isSelected = selected;
        }

        // 设置错误状态
        public void SetError(bool error)
        {
            isError = error;

            if (error)
            {
                // 如果设置为错误，启动协程来重置状态
                StartCoroutine(ResetErrorState(0.5f));
            }
        }

        // 重置错误状态
        IEnumerator ResetErrorState(float delay)
        {
            yield return new WaitForSeconds(delay);
            isError = false;
        }

        // 执行交换操作
        public void PerformSwap(SimpleSwappableObject other)
        {
            if (other == null || isCoolingDown || !canBeSwapped) return;

            // 交换位置
            Vector3 tempPosition = transform.position;
            transform.position = other.transform.position;
            other.transform.position = tempPosition;

            // 播放交换特效
            PlaySwapVFX();

            // 设置冷却
            StartCoroutine(SetCooldown());
        }

        // 播放交换特效
        void PlaySwapVFX()
        {
            if (swapVFXPrefab != null)
            {
                GameObject vfx = Instantiate(swapVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 1f);
            }
        }

        // 设置冷却状态
        IEnumerator SetCooldown()
        {
            isCoolingDown = true;
            yield return new WaitForSeconds(swapCooldown);
            isCoolingDown = false;
        }

        // 重置对象状态
        public void ResetState()
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            isHighlighted = false;
            isSelected = false;
            isError = false;
            isCoolingDown = false;
        }

        // 是否可以被交换
        public bool CanBeSwapped()
        {
            return canBeSwapped && !isCoolingDown;
        }

        // 获取Rigidbody2D
        public Rigidbody2D GetRigidbody()
        {
            return rb;
        }
    }
}