using System;

using UnityEngine;

public static class PlayerDeathEvent
{
    public static event Action<GameObject> OnDeathTriggered;

    public static void Trigger(GameObject damageSource, DeathType deathType)
    {
        Debug.Log("Trigger death event : " + deathType);
        OnDeathTriggered?.Invoke(damageSource);
    }

    
}

public enum DeathType
{
    Fall,
    Explode,
    Spike,
    Squish
}