using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ResetLevelEvent
{
    public static event Action OnResetTriggered;

    public static void Trigger()
    {
        OnResetTriggered?.Invoke();
    }
}