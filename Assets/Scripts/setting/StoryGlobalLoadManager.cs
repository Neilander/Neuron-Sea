using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class StoryGlobalLoadManager : MonoBehaviour
{
    public static StoryGlobalLoadManager instance { get; private set; }
    public GameMode curMode;

    private HashSet<string> disabledTriggers = new HashSet<string>();
    private bool ifLoadedScene1Story;
    private bool ifLoadedScene2Story;
    private bool ifLoadedScene3Story;

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
        ifLoadedScene1Story = (PlayerPrefs.GetInt("SGLM_Scene1Loaded") == 1);
        ifLoadedScene2Story = (PlayerPrefs.GetInt("SGLM_Scene2Loaded") == 1);
        ifLoadedScene3Story = (PlayerPrefs.GetInt("SGLM_Scene3Loaded") == 1);
    }

    #region 是否已经触发过了？

    public void DisableTrigger(string id){
        if (disabledTriggers.Add(id)) // 如果是第一次添加
        {
            PlayerPrefs.SetInt("StoryTrigger_" + id, 1);
            PlayerPrefs.Save(); // 保存到磁盘
        }
    }

    public void ResetAll(){
        disabledTriggers.Clear();
    }
    public bool IsTriggerDisabled(string id){
        if (disabledTriggers.Contains(id))
            return true;

        // 若内存中没有，可以从 PlayerPrefs 检查
        return PlayerPrefs.GetInt("StoryTrigger_" + id, 0) == 1;
    }
    

    #endregion
    private bool HasLoadedSceneStory(int sceneIndex)
    {
        return sceneIndex switch
        {
            1 => ifLoadedScene1Story,
            2 => ifLoadedScene2Story,
            3 => ifLoadedScene3Story,
            _ => false
        };
    }

    public bool ShouldLoadSceneStory()
    {
        Debug.Log($"在场景{currentScene}关卡{currentLevel}检查是否该触发剧情");
        return (curMode == GameMode.Story) && !HasLoadedSceneStory(currentScene);
    }

    public bool ShouldLoadSpecificSceneStory(int scene)
    {
        return (curMode == GameMode.Story) && !HasLoadedSceneStory(currentScene) && (currentScene == scene);
    }

    public bool IfThisStartHasLevel()
    {
        return ifThisStartLevelHasStory;
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
    private bool ifThisStartLevelHasStory = false;
    public void StartLevel(int scene, int level)
    {
        ifThisStartLevelHasStory = false;
        currentScene = scene;
        currentLevel = level;
        if (ShouldLoadSceneStory())
        {
            ifThisStartLevelHasStory = true;
            OnStartWithStory?.Invoke(level);
            
        }
        else
        {
            OnStartWithOutStory?.Invoke(level);
        }
        OnGeneralStart?.Invoke(level);
        if (ifThisStartLevelHasStory)
        {
            switch (scene)
            {
                case 1:
                    ifLoadedScene1Story = true;
                    PlayerPrefs.SetInt("SGLM_Scene1Loaded", 1);
                    break;

                case 2:
                    ifLoadedScene2Story = true;
                    PlayerPrefs.SetInt("SGLM_Scene2Loaded", 1);
                    break;

                case 3:
                    ifLoadedScene3Story = true;
                    PlayerPrefs.SetInt("SGLM_Scene3Loaded", 1);
                    break;
            }
            PlayerPrefs.Save();
        }
        
    }

    public void ResetStory()
    {
        ifLoadedScene1Story = false; 
        ifLoadedScene2Story = false; 
        ifLoadedScene3Story = false;
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



