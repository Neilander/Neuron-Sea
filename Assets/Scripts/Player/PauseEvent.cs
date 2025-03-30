using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseEvent
{
    public static event Action OnPauseTriggered;
    public static event Action OnPauseResumed;

    public static void Pause()
    {
        OnPauseTriggered?.Invoke();
    }

    public static void Resume()
    {
        OnPauseResumed?.Invoke();
    }
}