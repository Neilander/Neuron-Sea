using UnityEngine;

/// <summary>
/// 陷阱脚本，当角色触碰时触发死亡事件
/// </summary>
public class Trap : MonoBehaviour
{
    [Header("陷阱设置")]
    public DeathEventSystem.DeathType deathType = DeathEventSystem.DeathType.Trap;

    [Header("特效设置")]
    public GameObject trapTriggerEffectPrefab;
    public float effectDestroyDelay = 2f;

    [Header("标签设置")]
    [Tooltip("哪些标签的游戏对象会被此陷阱影响")]
    public string[] targetTags = { "Player", "Enemy" };

    private void OnTriggerEnter(Collider other)
    {
        CheckTrapTrigger(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckTrapTrigger(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckTrapTrigger(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckTrapTrigger(collision.gameObject);
    }

    /// <summary>
    /// 检查是否触发陷阱
    /// </summary>
    private void CheckTrapTrigger(GameObject other)
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

        // 播放陷阱触发特效
        if (trapTriggerEffectPrefab != null)
        {
            GameObject effect = Instantiate(trapTriggerEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDestroyDelay);
        }

        // 触发死亡事件
        
        DeathEventSystem.TriggerDeath(other, deathType);
        
    }
}