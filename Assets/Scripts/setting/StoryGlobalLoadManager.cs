using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StoryGlobalLoadManager : MonoBehaviour
{
    public static StoryGlobalLoadManager instance { get; private set; }
    public GameMode curMode;


    private bool ifLoadScene1Story;
    private bool ifLoadScene2Story;
    private bool ifLoadScene3Story;

    private event Action<int> OnStartWithStory;
    private event Action<int> OnStartWithOutStory;
    private event Action<int> OnGeneralStart;

    [Header("是否启用debug信息")]
    public bool ifDebug = false;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        ifLoadScene1Story = !(PlayerPrefs.GetInt("SGLM_Scene1Loaded") == 1);
        ifLoadScene2Story = !(PlayerPrefs.GetInt("SGLM_Scene2Loaded") == 1);
        ifLoadScene3Story = !(PlayerPrefs.GetInt("SGLM_Scene3Loaded") == 1);
    }

    private bool HasLoadedSceneStory(int sceneIndex)
    {
        return sceneIndex switch
        {
            1 => ifLoadScene1Story,
            2 => ifLoadScene2Story,
            3 => ifLoadScene3Story,
            _ => false
        };
    }

    public bool ShouldLoadSceneStory()
    {
        Debug.Log($"在场景{currentScene}关卡{currentLevel}检查是否该触发剧情");
        return (curMode == GameMode.Story) && !HasLoadedSceneStory(currentScene);
    }

    public void RegisterOnStartWithStory(Action<int> callback)
    {
        OnStartWithStory += callback;

        if (ifDebug)
        {
            Debug.Log($"[注册成功] 注册了一个 OnStartWithStory 回调：{callback.Method.Name} 来自 {callback.Target}");
        }
    }

    public void RegisterOnStartWithoutStory(Action<int> callback)
    {
        OnStartWithOutStory += callback;
        if (ifDebug)
        {
            Debug.Log($"[注册成功] 注册了一个 OnStartWithOutStory 回调：{callback.Method.Name} 来自 {callback.Target}");
        }
    }

    public void RegisterGeneralStart(Action<int> callback)
    {
        OnGeneralStart += callback;
        if (ifDebug)
        {
            Debug.Log($"[注册成功] 注册了一个 OnStartWithOutStory 回调：{callback.Method.Name} 来自 {callback.Target}");
        }
    }

    public void UnregisterOnStartWithStory(Action<int> callback)
    {
        OnStartWithStory -= callback;

        if (ifDebug)
        {
            Debug.Log($"[取消注册] 移除了一个 OnStartWithStory 回调：{callback.Method.Name} 来自 {callback.Target}");
        }
    }

    public void UnregisterOnStartWithoutStory(Action<int> callback)
    {
        OnStartWithOutStory -= callback;

        if (ifDebug)
        {
            Debug.Log($"[取消注册] 移除了一个 OnStartWithOutStory 回调：{callback.Method.Name} 来自 {callback.Target}");
        }
    }

    public void UnregisterGeneralStart(Action<int> callback)
    {
        OnGeneralStart -= callback;

        if (ifDebug)
        {
            Debug.Log($"[取消注册] 移除了一个 OnGeneralStart 回调：{callback.Method.Name} 来自 {callback.Target}");
        }
    }


    private int currentScene;
    private int currentLevel;
    public void StartLevel(int scene, int level)
    {
        
        if (ShouldLoadSceneStory())
        {
            OnStartWithStory?.Invoke(level);
            switch (scene)
            {
                case 1:
                    ifLoadScene1Story = false;
                    PlayerPrefs.SetInt("SGLM_Scene1Loaded",1);
                    break;

                case 2:
                    ifLoadScene2Story = false;
                    PlayerPrefs.SetInt("SGLM_Scene2Loaded", 1);
                    break;

                case 3:
                    ifLoadScene3Story = false;
                    PlayerPrefs.SetInt("SGLM_Scene3Loaded", 1);
                    break;
            }
        }
        else
        {
            OnStartWithOutStory?.Invoke(level);
        }
        OnGeneralStart?.Invoke(level);
        currentScene = scene;
        currentLevel = level;
    }

    private void OnApplicationQuit()
    {
#if UNITY_EDITOR
    Debug.Log("清除剧情加载记录（编辑器模式退出时）");
    PlayerPrefs.SetInt("SGLM_Scene1Loaded", 0);
    PlayerPrefs.SetInt("SGLM_Scene2Loaded", 0);
    PlayerPrefs.SetInt("SGLM_Scene3Loaded", 0);
    PlayerPrefs.Save();
#endif
    }
}

public enum GameMode
{
    Story,
    OnlyLevel
}



