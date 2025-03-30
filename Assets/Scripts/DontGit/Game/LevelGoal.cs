using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    public class LevelGoal : MonoBehaviour
    {
        [Header("视觉效果")]
        [SerializeField] private Color normalColor = new Color(0.3f, 0.8f, 0.3f, 1f);
        [SerializeField] private Color activatedColor = new Color(0.3f, 1f, 0.3f, 1f);
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseSize = 0.2f;

        private SpriteRenderer spriteRenderer;
        private bool isPlayerNear = false;
        private Vector3 originalScale;
        private bool isLevelCompleted = false;

        // 添加事件，与SimpleLevelGoal保持一致
        public event Action OnPlayerReachedGoal;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            // 设置Tag为Goal
            gameObject.tag = "Goal";
        }

        private void Start()
        {
            originalScale = transform.localScale;
        }

        private void Update()
        {
            // 呼吸/脉冲效果
            if (isPlayerNear)
            {
                float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseSize;
                transform.localScale = originalScale * pulse;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = activatedColor;
                }
            }
            else
            {
                transform.localScale = originalScale;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = normalColor;
                }
            }
        }

        // 检测玩家是否接近终点
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNear = true;

                // 防止重复触发
                if (!isLevelCompleted)
                {
                    // 触发关卡完成逻辑
                    CompleteLevel();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNear = false;
            }
        }

        // 获取玩家是否接近终点
        public bool IsPlayerNear()
        {
            return isPlayerNear;
        }

        // 完成关卡的方法
        private void CompleteLevel()
        {
            if (isLevelCompleted) return;

            isLevelCompleted = true;

            // 触发事件
            OnPlayerReachedGoal?.Invoke();

            // 通知GameManager（如果存在）
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteLevel();
            }
        }
    }
}