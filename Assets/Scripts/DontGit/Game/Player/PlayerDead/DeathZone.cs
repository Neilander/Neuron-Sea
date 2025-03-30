using UnityEngine;

/// <summary>
/// 死亡区域，当角色进入该区域时会触发特定类型的死亡事件
/// </summary>
public class DeathZone : MonoBehaviour
{
    [Header("死亡区域设置")]
    public DeathEventSystem.DeathType deathType = DeathEventSystem.DeathType.Fall;

    [Header("延迟设置")]
    [Tooltip("是否延迟触发死亡事件")]
    public bool delayDeath = false;
    public float deathDelay = 0.5f;

    [Header("标签设置")]
    [Tooltip("哪些标签的游戏对象会受到此死亡区域的影响")]
    public string[] targetTags = { "Player", "Enemy" };

    // 延迟死亡的对象引用
    private class DelayedDeath
    {
        public GameObject victim;
        public float timeToKill;

        public DelayedDeath(GameObject victim, float delay)
        {
            this.victim = victim;
            this.timeToKill = Time.time + delay;
        }
    }

    private System.Collections.Generic.List<DelayedDeath> delayedDeaths = new System.Collections.Generic.List<DelayedDeath>();

    private void Update()
    {
        // 处理延迟死亡
        if (delayedDeaths.Count > 0)
        {
            for (int i = delayedDeaths.Count - 1; i >= 0; i--)
            {
                if (Time.time >= delayedDeaths[i].timeToKill)
                {
                    TriggerDeathEvent(delayedDeaths[i].victim);
                    delayedDeaths.RemoveAt(i);
                }
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckDeathZone(other.gameObject);
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        // 某些情况下可能需要持续检测，例如水区域
        if (deathType == DeathEventSystem.DeathType.Drowning)
        {
            CheckDeathZone(other.gameObject);
        }
    }

    /// <summary>
    /// 检查是否应触发死亡事件
    /// </summary>
    private void CheckDeathZone(GameObject other)
    {
        // 检查目标对象是否具有我们关心的标签
        bool isValidTarget = false;
        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                isValidTarget = true;
                break;
            }
        }

        if (!isValidTarget) return;

        // 检查是否已经在延迟死亡列表中
        if (delayDeath)
        {
            foreach (var death in delayedDeaths)
            {
                if (death.victim == other)
                {
                    return; // 已经在处理中，跳过
                }
            }

            // 添加到延迟列表
            delayedDeaths.Add(new DelayedDeath(other, deathDelay));
        }
        else
        {
            // 立即触发死亡
            TriggerDeathEvent(other);
        }
    }

    /// <summary>
    /// 触发死亡事件
    /// </summary>
    private void TriggerDeathEvent(GameObject victim)
    {
            DeathEventSystem.TriggerDeath(victim, deathType);
    }
}