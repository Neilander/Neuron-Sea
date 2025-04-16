using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 立绘位置枚举
/// </summary>
public enum PortraitPosition
{
    Left,   // 左侧
    Center, // 中间
    Right   // 右侧
}

/// <summary>
/// 立绘表情枚举
/// </summary>
public enum PortraitExpression
{
    Normal,     // 普通表情
    Happy,      // 开心
    Sad,        // 悲伤
    Angry,      // 生气
    Surprised,  // 惊讶
    Custom      // 自定义表情(使用自定义sprite)
}

/// <summary>
/// 立绘特效类型
/// </summary>
public enum PortraitEffect
{
    None,       // 无特效
    Shake,      // 震动
    Bounce,     // 弹跳
    Spin,       // 旋转
    Flash       // 闪烁
}

/// <summary>
/// 对话数据类，用于存储单条对话的信息
/// </summary>
[System.Serializable]
public class DialogueData
{
    [Header("基本信息")]
    public string speakerName; // 说话者名称
    [TextArea(3, 10)]
    public string text; // 对话内容

    [Header("肖像设置")]
    public Sprite portrait; // 立绘图像
    public Sprite avatar; // 头像图像
    public PortraitPosition portraitPosition = PortraitPosition.Left; // 立绘位置
    public bool showPortrait = true; // 是否显示立绘

    [Header("表情和特效")]
    public PortraitExpression expression = PortraitExpression.Normal; // 表情
    public PortraitEffect portraitEffect = PortraitEffect.None; // 立绘特效
    public float effectIntensity = 1.0f; // 特效强度
    public float effectDuration = 1.0f; // 特效持续时间

    [Header("高级选项")]
    public string animationTrigger; // 动画触发器名称（可选）
    public Color textColor = Color.white; // 文本颜色
    public Color nameColor = Color.white; // 名称颜色
    public AudioClip voiceClip; // 角色语音片段（可选）
    public bool hideOtherPortraits = false; // 是否隐藏其他立绘
}

/// <summary>
/// 角色配置，可以存储角色的不同表情立绘
/// </summary>
[System.Serializable]
public class CharacterConfig
{
    public string characterName; // 角色名称
    public Dictionary<PortraitExpression, Sprite> expressionSprites = new Dictionary<PortraitExpression, Sprite>(); // 表情对应的立绘
    public Sprite defaultAvatar; // 默认头像
}

/// <summary>
/// 剧情数据类，用于存储一组对话
/// </summary>
[CreateAssetMenu(fileName = "NewStoryData", menuName = "Story/Story Data")]
public class StoryData : ScriptableObject
{
    public string storyName; // 剧情名称
    [Header("背景设置")]
    public Sprite backgroundImage; // 对话背景图像

    [Header("对话内容")]
    public List<DialogueData> dialogues = new List<DialogueData>(); // 对话列表

    [Header("角色配置")]
    public List<CharacterConfig> characters = new List<CharacterConfig>(); // 角色配置列表
}