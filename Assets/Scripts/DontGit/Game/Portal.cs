using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Portal : MonoBehaviour
    {
        [Header("传送门设置")]
        [SerializeField] private Portal linkedPortal;
        [SerializeField] private Color portalColor = new Color(0.3f, 0.7f, 1f, 1f);
        [SerializeField] private float cooldownTime = 1f;
        [SerializeField] private bool isActive = true;

        [Header("视觉效果")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.2f;
        [SerializeField] private ParticleSystem portalParticles;

        private SpriteRenderer spriteRenderer;
        private bool isCoolingDown = false;
        private Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = portalColor;
            }
        }

        private void Update()
        {
            if (!isActive) return;

            // 旋转效果
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

            // 脉冲缩放效果
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = new Vector3(pulse, pulse, 1f);

            // 更新传送冷却时间
            List<GameObject> toRemove = new List<GameObject>();
            foreach (var pair in cooldowns)
            {
                cooldowns[pair.Key] -= Time.deltaTime;
                if (cooldowns[pair.Key] <= 0)
                {
                    toRemove.Add(pair.Key);
                }
            }

            foreach (var obj in toRemove)
            {
                cooldowns.Remove(obj);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // 如果是玩家或可交换物体，进行传送
            if ((other.CompareTag("Player") || other.GetComponent<SwappableObject>() != null))
            {
                TryTeleport(other.gameObject);
            }
        }

        // 尝试传送物体
        private void TryTeleport(GameObject obj)
        {
            // 检查是否激活、是否有链接的传送门、是否在冷却中
            if (!isActive || linkedPortal == null || !linkedPortal.isActive || isCoolingDown)
            {
                return;
            }

            // 检查传送对象是否在冷却中
            if (cooldowns.ContainsKey(obj) && cooldowns[obj] > 0)
            {
                return;
            }

            // 传送物体
            Teleport(obj);

            // 设置冷却
            StartCoroutine(CooldownRoutine());

            // 对传送的物体设置冷却，防止来回传送
            cooldowns[obj] = cooldownTime;
        }

        // 执行传送
        private void Teleport(GameObject obj)
        {
            // 获取传送前的速度
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            Vector2 velocity = Vector2.zero;

            if (rb != null)
            {
                velocity = rb.velocity;
                // 临时停止物理模拟
                rb.simulated = false;
            }

            // 传送到目标位置
            obj.transform.position = linkedPortal.transform.position;

            // 恢复物理模拟和速度
            if (rb != null)
            {
                rb.simulated = true;
                rb.velocity = velocity;
            }

            // 播放特效
            PlayTeleportEffect();
            linkedPortal.PlayTeleportEffect();
        }

        // 播放传送特效
        private void PlayTeleportEffect()
        {
            if (portalParticles != null)
            {
                portalParticles.Play();
            }

            // 可以在这里添加声音或其他视觉效果
        }

        // 冷却协程
        private IEnumerator CooldownRoutine()
        {
            isCoolingDown = true;

            // 在冷却期间可以改变颜色
            if (spriteRenderer != null)
            {
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = new Color(originalColor.r * 0.7f, originalColor.g * 0.7f, originalColor.b * 0.7f, originalColor.a);

                yield return new WaitForSeconds(cooldownTime);

                spriteRenderer.color = originalColor;
            }
            else
            {
                yield return new WaitForSeconds(cooldownTime);
            }

            isCoolingDown = false;
        }

        // 设置传送门是否激活
        public void SetActive(bool active)
        {
            isActive = active;

            // 更新视觉效果
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = active;
            }

            if (portalParticles != null)
            {
                if (active)
                {
                    portalParticles.Play();
                }
                else
                {
                    portalParticles.Stop();
                }
            }
        }

        // 设置链接的传送门
        public void SetLinkedPortal(Portal portal)
        {
            linkedPortal = portal;
        }

        // 获取当前状态
        public bool IsActive()
        {
            return isActive && !isCoolingDown;
        }
    }
}