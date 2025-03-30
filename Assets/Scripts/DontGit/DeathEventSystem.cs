using UnityEngine;
using System;

/// <summary>
/// 死亡事件系统，实现陷阱与角色之间的解耦
/// </summary>
public static class DeathEventSystem
{
    // 静态事件，可以传递触发者信息和死亡类型等参数
    public static event Action<GameObject, DeathType> OnCharacterDeath;

    // 死亡类型枚举
    public enum DeathType
    {
        Trap,       // 陷阱导致的死亡
        Fall,       // 跌落导致的死亡
        Enemy,      // 敌人导致的死亡
        Drowning,   // 溺水导致的死亡
        Other       // 其他原因
    }

    /// <summary>
    /// 触发角色死亡事件
    /// </summary>
    /// <param name="victim">死亡的游戏对象</param>
    /// <param name="deathType">死亡类型</param>
    public static void TriggerDeath(GameObject victim, DeathType deathType)
    {
        OnCharacterDeath?.Invoke(victim, deathType);
    }
}