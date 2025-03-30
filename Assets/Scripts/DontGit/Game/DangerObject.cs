using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class DangerObject : MonoBehaviour
    {
        public enum DangerType
        {
            Spikes,     // 静止的尖刺
            MovingSpikes, // 移动的尖刺
            Explosion,   // 爆炸物
        }

        [Header("基本设置")]
        [SerializeField] private DangerType dangerType = DangerType.Spikes;
        [SerializeField] private float damage = 1f;

        [Header("爆炸设置")]
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float explosionForce = 500f;
        [SerializeField] private float explosionDelay = 1.5f;
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private LayerMask explosionAffectedLayers;

        private bool isTriggered = false;
        private bool hasExploded = false;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (dangerType == DangerType.Explosion && !isTriggered)
                {
                    // 如果是爆炸物，触发爆炸倒计时
                    TriggerExplosion();
                }
                else
                {
                    // 其他危险类型，直接对玩家造成伤害
                    DamagePlayer(other.gameObject);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (dangerType == DangerType.Explosion && !isTriggered)
                {
                    // 如果是爆炸物，触发爆炸倒计时
                    TriggerExplosion();
                }
                else
                {
                    // 其他危险类型，直接对玩家造成伤害
                    DamagePlayer(collision.gameObject);
                }
            }
        }

        // 伤害玩家
        private void DamagePlayer(GameObject player)
        {
            // 触发玩家死亡/受伤逻辑
            // 这里可以根据需要接入具体的生命值系统

            // 简单实现：直接重启关卡
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartLevel();
            }
        }

        // 触发爆炸倒计时
        private void TriggerExplosion()
        {
            isTriggered = true;

            // 开始爆炸闪烁效果
            StartCoroutine(ExplosionCountdown());
        }

        // 爆炸倒计时
        private IEnumerator ExplosionCountdown()
        {
            float countdown = explosionDelay;
            float blinkSpeed = 0.1f;

            // 闪烁警告效果
            while (countdown > 0)
            {
                // 快速在原始颜色和红色之间闪烁
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.red;
                }

                yield return new WaitForSeconds(blinkSpeed);

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }

                yield return new WaitForSeconds(blinkSpeed);

                countdown -= blinkSpeed * 2;

                // 随着倒计时减少，闪烁速度加快
                blinkSpeed = Mathf.Max(0.05f, blinkSpeed * 0.9f);
            }

            // 爆炸
            Explode();
        }

        // 爆炸逻辑
        private void Explode()
        {
            if (hasExploded) return;

            hasExploded = true;

            // 播放爆炸特效
            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            // 检测爆炸范围内的物体
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionAffectedLayers);

            foreach (Collider2D hit in colliders)
            {
                // 如果是玩家，造成伤害
                if (hit.CompareTag("Player"))
                {
                    DamagePlayer(hit.gameObject);
                }

                // 如果是刚体，施加爆炸力
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = hit.transform.position - transform.position;
                    float distance = Mathf.Max(0.1f, direction.magnitude); // 防止除零错误
                    float forceMagnitude = explosionForce / distance; // 力的大小随距离衰减

                    rb.AddForce(direction.normalized * forceMagnitude, ForceMode2D.Impulse);
                }

                // 如果是可交换物体，可以在这里添加额外处理
                SwappableObject swappableObj = hit.GetComponent<SwappableObject>();
                if (swappableObj != null)
                {
                    // 可以在这里添加被炸毁的效果
                }

                // 如果是其他爆炸物，引爆它
                DangerObject otherDanger = hit.GetComponent<DangerObject>();
                if (otherDanger != null && otherDanger.dangerType == DangerType.Explosion && !otherDanger.isTriggered)
                {
                    otherDanger.TriggerExplosion();
                }
            }

            // 销毁自身（延迟一点，让爆炸特效有时间播放）
            Destroy(gameObject, 0.1f);
        }

        // 在编辑器中绘制爆炸范围
        private void OnDrawGizmosSelected()
        {
            if (dangerType == DangerType.Explosion)
            {
                Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
                Gizmos.DrawSphere(transform.position, explosionRadius);
            }
        }
    }
}