using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PauseEvent
{
    public static event Action OnPauseTriggered;

    public static event Action OnPauseResumed;

    private static bool isPaused = false;

    public static void Pause(){
        if (!isPaused) {
            Time.timeScale = 0f; // 暂停游戏时间
            isPaused = true;
            OnPauseTriggered?.Invoke();
        }
    }

    public static void Resume(){
        if (isPaused) {
            Time.timeScale = 1f; // 恢复游戏时间
            isPaused = false;
            OnPauseResumed?.Invoke();
        }
    }

    public static bool IsPaused(){
        return isPaused;
    }
}