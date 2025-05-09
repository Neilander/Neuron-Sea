using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Portal : MonoBehaviour
{
    [Header("传送门设置")]
    [Tooltip("传送门的目标位置")]
    public Transform targetPortal;

    [Tooltip("传送门激活时的特效")]
    public GameObject portalEffect;

    [Tooltip("传送门是否激活")]
    public bool isActive = true;

    [Tooltip("传送冷却时间(秒)")]
    public float cooldownTime = 1f;

    [Tooltip("传送时是否立即更新摄像机")]
    public bool updateCameraImmediately = true;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    // 添加静态变量跟踪传送状态，防止无限传送循环
    private static bool isPlayerBeingTeleported = false;

    private void Start()
    {
        if (portalEffect != null)
        {
            portalEffect.SetActive(isActive);
        }
    }

    private void Update()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("准备传送");
            SetClosestPortalAsTarget();
        }
        

        // 检查是否可以传送
        if (!isActive || isOnCooldown || targetPortal == null || isPlayerBeingTeleported)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("碰到传送门了！准备传送到: " + targetPortal.position);
            TeleportPlayer(other.gameObject);
        }
    }

    private void SetClosestPortalAsTarget()
    {
        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (var portal in FindObjectsOfType<Portal>())
        {
            if (portal == this) continue; // 不选自己

            float dist = Vector2.Distance(transform.position, portal.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = portal.transform;
            }
        }

        targetPortal = closest;
    }

    private void TeleportPlayer(GameObject player)
    {
        // 防止传送循环
        isPlayerBeingTeleported = true;

        // 设置冷却
        isOnCooldown = true;
        cooldownTimer = cooldownTime;

        // 目标传送门也设置冷却，防止立即传送回来
        Portal targetPortalComponent = targetPortal.GetComponent<Portal>();
        if (targetPortalComponent != null)
        {
            targetPortalComponent.isOnCooldown = true;
            targetPortalComponent.cooldownTimer = cooldownTime;
        }

        /*修改前
        // 获取玩家的Rigidbody2D组件
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        // 原来的位置和目标位置，用于计算偏移量
        Vector2 originalPosition = player.transform.position;
        Vector2 targetPosition = targetPortal.position;
        Vector2 positionDelta = targetPosition - originalPosition;

        if (rb != null)
        {
            // 保存当前速度
            Vector2 currentVelocity = rb.velocity;

            // 使用MovePosition移动玩家 (更可靠)
            rb.position = targetPortal.position;

            // 保持玩家速度
            rb.velocity = currentVelocity;

            Debug.Log("玩家已传送到: " + targetPortal.position);
        }
        else
        {
            // 备用方法：如果没有Rigidbody2D，直接设置位置
            player.transform.position = targetPortal.position;
            Debug.Log("使用备用方法传送玩家到: " + targetPortal.position);
        }
        */

        AudioManager.Instance.Play(SFXClip.Teleport);

        // 原来的位置和目标位置，用于计算偏移量
        Vector2 originalPosition = player.transform.position;
        Vector2 targetPosition = (Vector2)targetPortal.position - (Vector2)transform.position + originalPosition;
        Vector2 positionDelta = targetPosition - originalPosition;
        // 直接设置玩家位置
        player.GetComponent<PlayerController>().MovePosition((Vector2)targetPortal.position - player.GetComponent<BoxCollider2D>().offset * Mathf.Abs(player.transform.localScale.x));
        player.GetComponent<PlayerController>().AdjustPosition(targetPosition - (Vector2)player.transform.position);

        // 立即同步摄像机位置
        //if (updateCameraImmediately)
        //{
        //    UpdateCameraPosition(positionDelta);
        //}
        Camera.main.transform.GetComponent<CameraControl>().isTransitioning = true;

        // 播放传送特效
        if (portalEffect != null)
        {
            portalEffect.SetActive(false);
            Invoke(nameof(ReactivateEffect), 0.5f);
        }

        // 延迟一帧后重置传送状态
        Invoke(nameof(ResetTeleportationState), 0.1f);
    }

    // 更新摄像机位置
    private void UpdateCameraPosition(Vector2 positionDelta)
    {
        // 查找主摄像机及其控制脚本
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 尝试查找CameraControl脚本
            CameraControl cameraControl = mainCamera.GetComponent<CameraControl>();

            if (cameraControl != null)
            {
                // 摄像机有CameraControl控制脚本，直接让其跟随下一帧更新
                // 不需要做任何事，摄像机会在下一帧LateUpdate中自动更新位置
                Debug.Log("找到CameraControl脚本，摄像机会自动更新");
            }
            else
            {
                // 如果没有找到CameraControl脚本，则直接移动摄像机
                Vector3 newCamPos = mainCamera.transform.position;
                newCamPos.x += positionDelta.x;
                newCamPos.y += positionDelta.y;
                mainCamera.transform.position = newCamPos;
                Debug.Log("直接更新摄像机位置: " + newCamPos);
            }
        }
        else
        {
            Debug.LogWarning("未找到主摄像机，无法同步位置");
        }
    }

    private void ResetTeleportationState()
    {
        isPlayerBeingTeleported = false;
    }

    private void ReactivateEffect()
    {
        if (portalEffect != null)
        {
            portalEffect.SetActive(true);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        if (portalEffect != null)
        {
            portalEffect.SetActive(active);
        }
    }
}