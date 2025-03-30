using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class SimpleCollectible : MonoBehaviour
    {
        [Header("外观设置")]
        [SerializeField] private Color normalColor = new Color(1f, 0.8f, 0.2f, 0.8f);
        [SerializeField] private Color highlightColor = new Color(1f, 1f, 0.5f, 1f);
        [SerializeField] private GameObject collectVFXPrefab;
        [SerializeField] private float rotateSpeed = 90f;
        [SerializeField] private float hoverAmount = 0.2f;
        [SerializeField] private float hoverSpeed = 1f;

        [Header("文本设置")]
        [SerializeField] private string collectibleName = "交换点";
        [SerializeField] private int additionalSwaps = 1;
        [SerializeField] private Text tooltipText;

        // 组件引用
        private SpriteRenderer spriteRenderer;
        private SimpleSwapManager swapManager;
        private TestLevelManager levelManager;

        // 状态
        private bool isCollected = false;
        private bool isPlayerNear = false;
        private Vector3 startPosition;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            swapManager = FindObjectOfType<SimpleSwapManager>();
            levelManager = FindObjectOfType<TestLevelManager>();

            // 记录初始位置
            startPosition = transform.position;
        }

        void Start()
        {
            // 设置默认颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            // 隐藏文本提示
            if (tooltipText != null)
            {
                tooltipText.gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (isCollected) return;

            // 旋转效果
            transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);

            // 悬浮效果
            float hover = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
            transform.position = startPosition + new Vector3(0, hover, 0);

            // 检测玩家是否在附近
            CheckPlayerProximity();

            // 更新提示文本
            UpdateTooltip();
        }

        void CheckPlayerProximity()
        {
            // 获取玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) return;

            // 检查距离
            float distance = Vector3.Distance(transform.position, player.transform.position);
            bool wasPlayerNear = isPlayerNear;
            isPlayerNear = distance < 2f;

            // 如果玩家刚刚靠近
            if (!wasPlayerNear && isPlayerNear)
            {
                OnPlayerEnter();
            }
            // 如果玩家刚刚离开
            else if (wasPlayerNear && !isPlayerNear)
            {
                OnPlayerExit();
            }

            // 如果玩家在附近并按下互动键
            if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                Collect();
            }
        }

        void OnPlayerEnter()
        {
            // 高亮显示
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }

            // 显示提示文本
            if (tooltipText != null)
            {
                tooltipText.gameObject.SetActive(true);
                tooltipText.text = $"按E收集 {collectibleName} (+{additionalSwaps}次交换)";
            }
        }

        void OnPlayerExit()
        {
            // 恢复正常颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }

            // 隐藏提示文本
            if (tooltipText != null)
            {
                tooltipText.gameObject.SetActive(false);
            }
        }

        void UpdateTooltip()
        {
            if (tooltipText == null || !isPlayerNear) return;

            // 更新提示文本位置跟随对象
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1f, 0));
            tooltipText.transform.position = screenPos;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            // 检查是否为玩家
            if (other.CompareTag("Player"))
            {
                Collect();
            }
        }

        public void Collect()
        {
            if (isCollected) return;

            // 设置为已收集
            isCollected = true;

            // 增加交换次数
            if (swapManager != null)
            {
                int maxSwaps = swapManager.GetCurrentSwaps() + additionalSwaps;
                swapManager.SetMaxSwaps(maxSwaps);
            }

            // 播放收集特效
            PlayCollectVFX();

            // 隐藏提示文本
            if (tooltipText != null)
            {
                tooltipText.gameObject.SetActive(false);
            }

            // 隐藏对象
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // 禁用碰撞器
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            // 延迟销毁对象
            Destroy(gameObject, 2f);
        }

        void PlayCollectVFX()
        {
            if (collectVFXPrefab != null)
            {
                GameObject vfx = Instantiate(collectVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 1f);
            }

            // 播放收集音效（如果有）
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.enabled)
            {
                audioSource.Play();
            }
        }

        // 是否已被收集
        public bool IsCollected()
        {
            return isCollected;
        }

        // 重置状态
        public void ResetState()
        {
            if (isCollected)
            {
                isCollected = false;

                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    spriteRenderer.color = normalColor;
                }

                Collider2D collider = GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }

                isPlayerNear = false;
            }
        }
    }
}