using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 剧情记录管理器，负责存储和回放已播放的剧情
/// </summary>
public class StoryLogManager : MonoBehaviour
{
    public static StoryLogManager Instance { get; private set; }

    // 剧情记录列表，储存已播放的剧情
    private List<StoryLog> storyLogs = new List<StoryLog>();
    
    // 当前正在回放的剧情索引
    private int currentReplayingStoryIndex = -1;
    
    // 最大记录数量
    [SerializeField] private int maxLogEntries = 50;
    
    // 是否正在回放剧情
    public bool IsReplaying { get; private set; } = false;

    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        // 尝试从存档加载剧情记录
        LoadStoryLogs();
    }

    /// <summary>
    /// 记录一条新的剧情
    /// </summary>
    /// <param name="storyData">剧情数据</param>
    public void LogStory(StoryData storyData)
    {
        if (storyData == null) return;
        
        // 创建新的剧情记录
        StoryLog newLog = new StoryLog
        {
            storyName = storyData.storyName,
            timestamp = System.DateTime.Now,
            dialogues = new List<DialogueLog>()
        };
        
        // 复制对话内容
        foreach (var dialogue in storyData.dialogues)
        {
            DialogueLog dialogueLog = new DialogueLog
            {
                speakerName = dialogue.speakerName,
                text = dialogue.text,
                expression = dialogue.expression,
                portraitPosition = dialogue.portraitPosition
            };
            
            newLog.dialogues.Add(dialogueLog);
        }
        
        // 添加到记录列表
        storyLogs.Add(newLog);
        
        // 如果超过最大记录数，删除最旧的记录
        if (storyLogs.Count > maxLogEntries)
        {
            storyLogs.RemoveAt(0);
        }
        
        // 保存记录到存档
        SaveStoryLogs();
    }
    
    /// <summary>
    /// 获取所有剧情记录
    /// </summary>
    public List<StoryLog> GetAllStoryLogs()
    {
        return storyLogs;
    }
    
    /// <summary>
    /// 开始回放指定索引的剧情
    /// </summary>
    /// <param name="index">剧情索引</param>
    public void StartReplay(int index)
    {
        if (index < 0 || index >= storyLogs.Count) return;
        
        StoryLog logToReplay = storyLogs[index];
        
        // 创建临时StoryData用于回放
        StoryData tempStoryData = ScriptableObject.CreateInstance<StoryData>();
        tempStoryData.storyName = logToReplay.storyName + " (回放)";
        
        // 复制对话内容
        foreach (var dialogueLog in logToReplay.dialogues)
        {
            DialogueData dialogue = new DialogueData
            {
                speakerName = dialogueLog.speakerName,
                text = dialogueLog.text,
                expression = dialogueLog.expression,
                portraitPosition = dialogueLog.portraitPosition
            };
            
            tempStoryData.dialogues.Add(dialogue);
        }
        
        // 记录当前回放的索引
        currentReplayingStoryIndex = index;
        
        // 设置回放模式
        IsReplaying = true;
        
        // 使用StoryManager播放剧情
        StoryManager.Instance.EnterStoryMode(tempStoryData);
    }
    
    /// <summary>
    /// 结束回放
    /// </summary>
    public void EndReplay()
    {
        IsReplaying = false;
        currentReplayingStoryIndex = -1;
        
        // 退出剧情模式
        if (StoryManager.Instance.GetCurrentState() == GameState.StoryMode)
        {
            StoryManager.Instance.ExitStoryMode();
        }
    }
    
    /// <summary>
    /// 保存剧情记录到存档
    /// </summary>
    private void SaveStoryLogs()
    {
        // 创建可序列化的数据
        StoryLogSaveData saveData = new StoryLogSaveData
        {
            logs = storyLogs
        };
        
        // 将数据转换为JSON
        string json = JsonUtility.ToJson(saveData);
        
        // 保存到PlayerPrefs
        PlayerPrefs.SetString("StoryLogs", json);
        PlayerPrefs.Save();
        
        Debug.Log($"已保存 {storyLogs.Count} 条剧情记录");
    }
    
    /// <summary>
    /// 从存档加载剧情记录
    /// </summary>
    private void LoadStoryLogs()
    {
        if (PlayerPrefs.HasKey("StoryLogs"))
        {
            string json = PlayerPrefs.GetString("StoryLogs");
            
            try
            {
                StoryLogSaveData saveData = JsonUtility.FromJson<StoryLogSaveData>(json);
                storyLogs = saveData.logs;
                Debug.Log($"已加载 {storyLogs.Count} 条剧情记录");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"加载剧情记录时出错: {e.Message}");
                storyLogs = new List<StoryLog>();
            }
        }
    }
    
    /// <summary>
    /// 清除所有剧情记录
    /// </summary>
    public void ClearAllLogs()
    {
        storyLogs.Clear();
        SaveStoryLogs();
        Debug.Log("已清除所有剧情记录");
    }
}

/// <summary>
/// 剧情记录类，存储单个剧情的信息
/// </summary>
[System.Serializable]
public class StoryLog
{
    public string storyName; // 剧情名称
    public System.DateTime timestamp; // 记录时间
    public List<DialogueLog> dialogues = new List<DialogueLog>(); // 对话记录
}

/// <summary>
/// 对话记录类，存储单条对话的信息
/// </summary>
[System.Serializable]
public class DialogueLog
{
    public string speakerName; // 角色名称 
    public string text; // 对话内容
    public PortraitExpression expression; // 表情
    public PortraitPosition portraitPosition; // 立绘位置
}

/// <summary>
/// 剧情记录存档数据类，用于JSON序列化
/// </summary>
[System.Serializable]
public class StoryLogSaveData
{
    public List<StoryLog> logs = new List<StoryLog>();
}