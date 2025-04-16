using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 故事测试器，用于开发和调试对话系统
/// </summary>
public class StoryTester : MonoBehaviour
{
    [SerializeField] private Sprite testPortrait1; // 测试立绘1
    [SerializeField] private Sprite testPortrait2; // 测试立绘2
    [SerializeField] private Sprite testAvatar1; // 测试头像1
    [SerializeField] private Sprite testAvatar2; // 测试头像2
    [SerializeField] private Sprite backgroundImage; // 背景图像
    [SerializeField] private AudioClip testVoiceClip; // 测试语音片段
    [SerializeField] private KeyCode testKey = KeyCode.T; // 触发测试的按键
    [SerializeField] private KeyCode testEffectsKey = KeyCode.Y; // 触发测试特效的按键

    private void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestStoryWithPortraits();
        }

        if (Input.GetKeyDown(testEffectsKey))
        {
            TestStoryWithEffects();
        }
    }

    /// <summary>
    /// 测试带有立绘和头像的对话
    /// </summary>
    private void TestStoryWithPortraits()
    {
        // 创建测试故事数据
        StoryData testStory = new StoryData();
        testStory.backgroundImage = backgroundImage;

        // 创建对话数据
        List<DialogueData> dialogues = new List<DialogueData>();

        // 对话1：左侧立绘
        DialogueData dialogue1 = new DialogueData
        {
            speakerName = "角色A",
            text = "你好！我是站在左侧的角色A。这是一段测试对话，用于展示立绘和头像功能。",
            portrait = testPortrait1,
            avatar = testAvatar1,
            portraitPosition = PortraitPosition.Left,
            showPortrait = true,
            nameColor = Color.blue,
            textColor = Color.black,
            voiceClip = testVoiceClip
        };
        dialogues.Add(dialogue1);

        // 对话2：右侧立绘
        DialogueData dialogue2 = new DialogueData
        {
            speakerName = "角色B",
            text = "你好A！我是站在右侧的角色B。对话系统支持不同位置的立绘显示。",
            portrait = testPortrait2,
            avatar = testAvatar2,
            portraitPosition = PortraitPosition.Right,
            showPortrait = true,
            nameColor = Color.red,
            textColor = Color.black
        };
        dialogues.Add(dialogue2);

        // 对话3：中间立绘
        DialogueData dialogue3 = new DialogueData
        {
            speakerName = "角色A",
            text = "现在我移动到了中间位置！系统会自动处理立绘的淡入淡出效果。",
            portrait = testPortrait1,
            avatar = testAvatar1,
            portraitPosition = PortraitPosition.Center,
            showPortrait = true,
            nameColor = Color.blue,
            textColor = Color.black
        };
        dialogues.Add(dialogue3);

        // 对话4：无立绘，只有头像
        DialogueData dialogue4 = new DialogueData
        {
            speakerName = "角色B",
            text = "这个对话只显示头像，没有立绘。对话系统可以灵活配置显示方式。",
            avatar = testAvatar2,
            showPortrait = false,
            nameColor = Color.red,
            textColor = Color.black
        };
        dialogues.Add(dialogue4);

        // 对话5：结束测试
        DialogueData dialogue5 = new DialogueData
        {
            speakerName = "系统",
            text = "测试结束！按空格键或点击鼠标可以继续对话。",
            nameColor = Color.green,
            textColor = Color.black
        };
        dialogues.Add(dialogue5);

        // 设置故事数据
        testStory.dialogues = dialogues;

        // 启动故事
        StoryManager.Instance.EnterStoryMode(testStory);
    }

    /// <summary>
    /// 测试立绘特效和表情
    /// </summary>
    private void TestStoryWithEffects()
    {
        // 创建测试故事数据
        StoryData testStory = new StoryData();
        testStory.backgroundImage = backgroundImage;
        testStory.storyName = "立绘特效测试";

        // 创建对话数据
        List<DialogueData> dialogues = new List<DialogueData>();

        // 对话1：震动特效测试
        DialogueData dialogue1 = new DialogueData
        {
            speakerName = "角色A",
            text = "这是震动特效的测试！可以表现角色的惊慌或紧张情绪。",
            portrait = testPortrait1,
            avatar = testAvatar1,
            portraitPosition = PortraitPosition.Left,
            showPortrait = true,
            portraitEffect = PortraitEffect.Shake,
            effectIntensity = 1.5f,
            effectDuration = 2.0f,
            nameColor = Color.blue,
            textColor = Color.black
        };
        dialogues.Add(dialogue1);

        // 对话2：弹跳特效测试
        DialogueData dialogue2 = new DialogueData
        {
            speakerName = "角色B",
            text = "这是弹跳特效的测试！可以表现角色的欢快或激动情绪。",
            portrait = testPortrait2,
            avatar = testAvatar2,
            portraitPosition = PortraitPosition.Right,
            showPortrait = true,
            portraitEffect = PortraitEffect.Bounce,
            effectIntensity = 1.2f,
            effectDuration = 2.5f,
            nameColor = Color.red,
            textColor = Color.black
        };
        dialogues.Add(dialogue2);

        // 对话3：旋转特效测试
        DialogueData dialogue3 = new DialogueData
        {
            speakerName = "角色A",
            text = "这是旋转特效的测试！可以表现角色的困惑或晕眩情绪。",
            portrait = testPortrait1,
            avatar = testAvatar1,
            portraitPosition = PortraitPosition.Center,
            showPortrait = true,
            portraitEffect = PortraitEffect.Spin,
            effectIntensity = 1.0f,
            effectDuration = 2.0f,
            nameColor = Color.blue,
            textColor = Color.black,
            hideOtherPortraits = true
        };
        dialogues.Add(dialogue3);

        // 对话4：闪烁特效测试
        DialogueData dialogue4 = new DialogueData
        {
            speakerName = "角色B",
            text = "这是闪烁特效的测试！可以表现角色的魔法效果或特殊状态。",
            portrait = testPortrait2,
            avatar = testAvatar2,
            portraitPosition = PortraitPosition.Left,
            showPortrait = true,
            portraitEffect = PortraitEffect.Flash,
            effectIntensity = 1.5f,
            effectDuration = 3.0f,
            nameColor = Color.red,
            textColor = Color.black
        };
        dialogues.Add(dialogue4);

        // 对话5：测试隐藏其他立绘
        DialogueData dialogue5 = new DialogueData
        {
            speakerName = "角色A",
            text = "这个对话会隐藏其他所有立绘，只显示当前说话角色。这对于强调说话者很有用。",
            portrait = testPortrait1,
            avatar = testAvatar1,
            portraitPosition = PortraitPosition.Right,
            showPortrait = true,
            hideOtherPortraits = true,
            nameColor = Color.blue,
            textColor = Color.black
        };
        dialogues.Add(dialogue5);

        // 对话6：结束测试
        DialogueData dialogue6 = new DialogueData
        {
            speakerName = "系统",
            text = "特效测试结束！可以自由组合这些特效与对话实现更丰富的表现力。",
            nameColor = Color.green,
            textColor = Color.black
        };
        dialogues.Add(dialogue6);

        // 设置故事数据
        testStory.dialogues = dialogues;

        // 启动故事
        StoryManager.Instance.EnterStoryMode(testStory);
    }
}