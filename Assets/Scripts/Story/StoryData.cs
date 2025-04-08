using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对话数据类，用于存储单条对话的信息
/// </summary>
[System.Serializable]
public class DialogueData
{
    public string speakerName; // 说话者名称
    public string text; // 对话内容
    public string animationTrigger; // 动画触发器名称（可选）
}

/// <summary>
/// 剧情数据类，用于存储一组对话
/// </summary>
[CreateAssetMenu(fileName = "NewStoryData", menuName = "Story/Story Data")]
public class StoryData : ScriptableObject
{
    public string storyName; // 剧情名称
    public List<DialogueData> dialogues = new List<DialogueData>(); // 对话列表
}