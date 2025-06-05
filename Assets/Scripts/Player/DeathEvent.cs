using System;

using UnityEngine;

public static class PlayerDeathEvent
{
    public static event Action<GameObject> OnDeathTriggered;
    public static event Func<bool> OnCheckInDeath;

    public static void Trigger(GameObject damageSource, DeathType deathType)
    {
        Debug.Log("Trigger death event : " + deathType);
        OnDeathTriggered?.Invoke(damageSource);
    }

    public static bool IsInDeath()
    {
        if (OnCheckInDeath != null)
        {
            foreach (Func<bool> handler in OnCheckInDeath.GetInvocationList())
            {
                if (handler.Invoke())
                {
                    return true; // 只要有一个返回true，就返回true
                }
            }
        }
        return false; // 如果没有任何返回true的，返回false
    }
    
}

public enum DeathType
{
    Fall,
    Explode,
    Spike,
    Squish
}