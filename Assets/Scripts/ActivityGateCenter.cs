using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivityGateCenter
{
    private static HashSet<ActivityState> activeStates = new HashSet<ActivityState>();

    public static void EnterState(ActivityState state)
    {
        activeStates.Add(state);
    }

    public static void ExitState(ActivityState state)
    {
        activeStates.Remove(state);
    }

    //单个状态
    public static bool IsStateActive(ActivityState state)
    {
        return activeStates.Contains(state);
    }

    //任意一个
    public static bool IsStateActiveAny(params ActivityState[] states)
    {
        foreach (var state in states)
        {
            if (activeStates.Contains(state))
            {
                return true;
            }
                
        }
        return false;
    }

    //全部都在
    public static bool IsStateActiveAll(params ActivityState[] states)
    {
        foreach (var state in states)
        {
            if (!activeStates.Contains(state))
                return false;
        }
        return true;
    }
}

public enum ActivityState
{
    IntroScreen,
    StartEffectMove,
    Story,
    Pause,
}
