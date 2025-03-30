using UnityEngine;

/// <summary>
/// 角色控制器，处理角色的状态和行为
/// </summary>
public class CharacterDead : MonoBehaviour
{
    [Header("角色设置")]
    public bool isPlayer = true;
    public float respawnDelay = 2f;

    [Header("死亡特效")]
    public GameObject deathEffectPrefab;

    private bool isDead = false;
    private Vector3 respawnPosition;

    private void Start()
    {
        // 记录初始位置作为重生点
        respawnPosition = transform.position;

        // 订阅死亡事件
        DeathEventSystem.OnCharacterDeath += HandleDeath;
    }

    private void OnDestroy()
    {
        // 取消订阅，避免内存泄漏
        DeathEventSystem.OnCharacterDeath -= HandleDeath;

    }

    /// <summary>
    /// 处理死亡事件
    /// </summary>
    private void HandleDeath(GameObject victim, DeathEventSystem.DeathType deathType)
    {
        // 检查死亡对象是否为自己
        if (victim != gameObject) return;

        // 防止重复处理
        if (isDead) return;

        isDead = true;

        // 根据死亡类型执行不同的死亡表现
        switch (deathType)
        {
            case DeathEventSystem.DeathType.Trap:
                Die("被陷阱杀死了！");
                break;
            case DeathEventSystem.DeathType.Fall:
                Die("掉落致死！");
                break;
            case DeathEventSystem.DeathType.Enemy:
                Die("被敌人击败！");
                break;
            case DeathEventSystem.DeathType.Drowning:
                Die("溺水身亡！");
                break;
            default:
                Die("意外死亡！");
                break;
        }
    }

    /// <summary>
    /// 角色死亡逻辑
    /// </summary>
    private void Die(string deathMessage)
    {
        Debug.Log($"角色{gameObject.name}: {deathMessage}");

        // 播放死亡动画或特效
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // 如果是玩家角色，触发重生流程
        if (isPlayer)
        {
            // 临时禁用角色渲染或碰撞
            SetCharacterActive(false);

            // 延迟后重生
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            // 如果是NPC，可能直接销毁
            Destroy(gameObject, 0.5f);
        }
    }

    /// <summary>
    /// 设置角色激活状态
    /// </summary>
    private void SetCharacterActive(bool active)
    {
        // 禁用渲染器
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = active;
        }

        // 禁用碰撞器
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = active;
        }

        // 禁用2D碰撞器
        Collider2D[] colliders2D = GetComponentsInChildren<Collider2D>();
        foreach (var collider in colliders2D)
        {
            collider.enabled = active;
        }
    }

    /// <summary>
    /// 角色重生
    /// </summary>
    private void Respawn()
    {
        // 重置状态
        isDead = false;

        // 回到重生点
        transform.position = respawnPosition;

        // 重新激活角色
        SetCharacterActive(true);

        Debug.Log($"角色{gameObject.name}重生了");
    }

    /// <summary>
    /// 设置新的重生点
    /// </summary>
    public void SetRespawnPoint(Vector3 position)
    {
        respawnPosition = position;
        Debug.Log($"设置新的重生点: {position}");
    }
}