using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game
{
    public class SimpleLevelGoal : MonoBehaviour
    {
        [Header("外观设置")]
        [SerializeField] private Color normalColor = new Color(0.5f, 0.8f, 1f, 0.5f);
        [SerializeField] private Color activeColor = new Color(0.5f, 1f, 0.5f, 0.8f);
        [SerializeField] private GameObject completionVFXPrefab;
        [SerializeField] private float pulseSpeed = 1f;
        [SerializeField] private float pulseAmount = 0.2f;

        [Header("检测设置")]
        [SerializeField] private float playerDetectionRadius = 1.5f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private bool requireAllCollectibles = true;

        // 组件引用
        private SpriteRenderer spriteRenderer;
        private TestLevelManager levelManager;

        // 状态
        private bool isPlayerNear = false;
        private bool isLevelComplete = false;

        // 事件
        public event Action OnPlayerReachedGoal;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            levelManager = FindObjectOfType<TestLevelManager>();

            // 如果没有设置玩家层，默认使用"Player"层
            if (playerLayer == 0)
            {
                playerLayer = LayerMask.GetMask("Player");
            }
        }

        void Start()
        {
            // 设置初始颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }

        void Update()
        {
            // 检测玩家是否在范围内
            CheckPlayerProximity();

            // 更新视觉效果
            UpdateVisualEffects();

            // 检查关卡完成条件
            CheckLevelCompletion();
        }

        void CheckPlayerProximity()
        {
            // 检查玩家是否在检测范围内
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, playerDetectionRadius, playerLayer);
            bool wasPlayerNear = isPlayerNear;
            isPlayerNear = playerCollider != null;

            // 如果玩家刚刚进入检测范围
            if (!wasPlayerNear && isPlayerNear)
            {
                OnPlayerEntered();
            }
        }

        void OnPlayerEntered()
        {
            // 检查是否可以完成关卡
            if (levelManager != null && (!requireAllCollectibles || levelManager.CanCompleteLevel()))
            {
                CompleteLevel();
            }
        }

        void UpdateVisualEffects()
        {
            if (spriteRenderer == null) return;

            // 设置颜色基于玩家是否在附近
            spriteRenderer.color = isPlayerNear ? activeColor : normalColor;

            // 脉冲效果
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = new Vector3(pulse, pulse, 1f);
        }

        void CheckLevelCompletion()
        {
            // 如果关卡已经完成，不再检查
            if (isLevelComplete) return;

            // 如果玩家在附近并且满足关卡条件，完成关卡
            if (isPlayerNear && levelManager != null && (!requireAllCollectibles || levelManager.CanCompleteLevel()))
            {
                CompleteLevel();
            }
        }

        void CompleteLevel()
        {
            if (isLevelComplete) return;

            // 设置为已完成
            isLevelComplete = true;

            // 触发事件
            OnPlayerReachedGoal?.Invoke();

            // 播放完成特效
            PlayCompletionVFX();
        }

        void PlayCompletionVFX()
        {
            if (completionVFXPrefab != null)
            {
                GameObject vfx = Instantiate(completionVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
            }
        }

        // 用于在编辑器中显示检测范围
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }

        // 获取关卡是否已完成
        public bool IsLevelComplete()
        {
            return isLevelComplete;
        }

        // 重置状态
        public void ResetState()
        {
            isLevelComplete = false;
        }
    }
}