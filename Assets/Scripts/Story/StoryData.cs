using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO; // 添加文件操作命名空间

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
    Custom,      // 自定义表情(使用自定义sprite)

    Thoughtful,
    Annoyed,
    New,
    Proud,
    Relieved,
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
    [Tooltip("不同表情对应的立绘")]
    public List<ExpressionSprite> expressions = new List<ExpressionSprite>(); // 表情对应的立绘列表
    public Sprite defaultAvatar; // 默认头像

    // 获取指定表情的立绘
    public Sprite GetExpressionSprite(PortraitExpression expression)
    {
        foreach (var item in expressions)
        {
            if (item.expression == expression)
            {
                return item.sprite;
            }
        }
        // 如果找不到指定表情，返回Normal表情或第一个可用的表情
        foreach (var item in expressions)
        {
            if (item.expression == PortraitExpression.Normal)
            {
                return item.sprite;
            }
        }
        // 如果连Normal都没有，返回第一个表情
        if (expressions.Count > 0)
        {
            return expressions[0].sprite;
        }
        return null;
    }
}

/// <summary>
/// 表情与立绘的对应关系
/// </summary>
[System.Serializable]
public class ExpressionSprite
{
    public PortraitExpression expression; // 表情类型
    public Sprite sprite; // 对应的立绘
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

    #region 表格导入功能

    /// <summary>
    /// 从CSV文件导入对话数据
    /// </summary>
    /// <param name="csvFilePath">CSV文件路径</param>
    /// <returns>是否成功导入</returns>
    public bool ImportFromCSV(string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError($"CSV文件不存在: {csvFilePath}");
            return false;
        }

        try
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            if (lines.Length <= 1) // 至少需要标题行和一行数据
            {
                Debug.LogError("CSV文件格式不正确，至少需要标题行和一行数据");
                return false;
            }

            // 清空现有对话数据
            dialogues.Clear();

            // 跳过标题行，从第二行开始解析
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // 解析CSV行
                string[] fields = ParseCSVLine(line);
                if (fields.Length < 2) // 至少需要角色名和对话内容
                {
                    Debug.LogWarning($"跳过无效行: {line}");
                    continue;
                }

                // 创建对话数据
                DialogueData dialogueData = new DialogueData();

                // 解析基本字段
                int fieldIndex = 0;
                dialogueData.speakerName = fields[fieldIndex++];
                dialogueData.text = fields[fieldIndex++];

                // 如果有更多字段，解析其他设置
                if (fields.Length > fieldIndex)
                {
                    // 解析立绘位置
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (System.Enum.TryParse(fields[fieldIndex], out PortraitPosition position))
                        {
                            dialogueData.portraitPosition = position;
                        }
                    }
                    fieldIndex++;

                    // 解析表情
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (System.Enum.TryParse(fields[fieldIndex], out PortraitExpression expression))
                        {
                            dialogueData.expression = expression;
                        }
                    }
                    fieldIndex++;

                    // 解析特效
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (System.Enum.TryParse(fields[fieldIndex], out PortraitEffect effect))
                        {
                            dialogueData.portraitEffect = effect;
                        }
                    }
                    fieldIndex++;

                    // 解析特效强度
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (float.TryParse(fields[fieldIndex], out float intensity))
                        {
                            dialogueData.effectIntensity = intensity;
                        }
                    }
                    fieldIndex++;

                    // 解析特效持续时间
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (float.TryParse(fields[fieldIndex], out float duration))
                        {
                            dialogueData.effectDuration = duration;
                        }
                    }
                    fieldIndex++;

                    // 解析显示立绘
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (bool.TryParse(fields[fieldIndex], out bool showPortrait))
                        {
                            dialogueData.showPortrait = showPortrait;
                        }
                    }
                    fieldIndex++;

                    // 解析隐藏其他立绘
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        if (bool.TryParse(fields[fieldIndex], out bool hideOthers))
                        {
                            dialogueData.hideOtherPortraits = hideOthers;
                        }
                    }
                    fieldIndex++;

                    // 解析动画触发器
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        dialogueData.animationTrigger = fields[fieldIndex];
                    }
                    fieldIndex++;

                    // 立绘和头像需要通过资源路径加载
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        string portraitPath = fields[fieldIndex];
                        Sprite portraitSprite = Resources.Load<Sprite>(portraitPath);
                        if (portraitSprite != null)
                        {
                            dialogueData.portrait = portraitSprite;
                        }
                        else
                        {
                            Debug.LogWarning($"无法加载立绘: {portraitPath}");
                        }
                    }
                    fieldIndex++;

                    // 解析头像路径
                    if (fieldIndex < fields.Length && !string.IsNullOrEmpty(fields[fieldIndex]))
                    {
                        string avatarPath = fields[fieldIndex];
                        Sprite avatarSprite = Resources.Load<Sprite>(avatarPath);
                        if (avatarSprite != null)
                        {
                            dialogueData.avatar = avatarSprite;
                        }
                        else
                        {
                            Debug.LogWarning($"无法加载头像: {avatarPath}");
                        }
                    }
                }

                // 添加到对话列表
                dialogues.Add(dialogueData);
            }

            Debug.Log($"成功从CSV导入 {dialogues.Count} 条对话数据");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入CSV数据时出错: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 解析CSV行，处理引号包围的字段
    /// </summary>
    private string[] ParseCSVLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        string currentField = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }

        // 添加最后一个字段
        fields.Add(currentField);

        return fields.ToArray();
    }

    /// <summary>
    /// 创建CSV表格模板
    /// </summary>
    /// <param name="filePath">保存路径</param>
    /// <returns>是否成功创建</returns>
    public static bool CreateCSVTemplate(string filePath)
    {
        try
        {
            // 定义CSV表头
            string header = "SpeakerName,DialogueText,PortraitPosition,Expression,Effect,EffectIntensity,EffectDuration,ShowPortrait,HideOtherPortraits,AnimationTrigger,PortraitPath,AvatarPath";

            // 添加示例数据
            string exampleRow1 = "角色A,\"这是第一句对话，CSV格式支持换行和特殊字符\",Left,Normal,None,1.0,1.0,true,false,,Characters/CharacterA,Avatars/AvatarA";
            string exampleRow2 = "角色B,这是第二句对话,Right,Happy,Shake,1.5,2.0,true,false,,Characters/CharacterB,Avatars/AvatarB";

            // 写入文件
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(header);
                writer.WriteLine(exampleRow1);
                writer.WriteLine(exampleRow2);
            }

            Debug.Log($"CSV模板已创建: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建CSV模板时出错: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从CSV文件导入角色表情配置
    /// </summary>
    /// <param name="csvFilePath">CSV文件路径</param>
    /// <returns>是否成功导入</returns>
    public bool ImportCharactersFromCSV(string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
        {
            Debug.LogError($"角色配置CSV文件不存在: {csvFilePath}");
            return false;
        }

        try
        {
            string[] lines = File.ReadAllLines(csvFilePath);
            if (lines.Length <= 1) // 至少需要标题行和一行数据
            {
                Debug.LogError("角色配置CSV文件格式不正确，至少需要标题行和一行数据");
                return false;
            }

            // 清空现有角色数据
            characters.Clear();

            // 临时存储角色数据
            Dictionary<string, CharacterConfig> characterDict = new Dictionary<string, CharacterConfig>();

            // 跳过标题行，从第二行开始解析
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                // 解析CSV行
                string[] fields = ParseCSVLine(line);
                if (fields.Length < 3) // 至少需要角色名、表情类型和立绘路径
                {
                    Debug.LogWarning($"跳过无效行: {line}");
                    continue;
                }

                // 解析角色名
                string characterName = fields[0];

                // 如果角色不存在于字典中，创建新角色
                if (!characterDict.ContainsKey(characterName))
                {
                    CharacterConfig newCharacter = new CharacterConfig();
                    newCharacter.characterName = characterName;
                    characterDict.Add(characterName, newCharacter);
                }

                // 获取角色配置
                CharacterConfig character = characterDict[characterName];

                // 解析表情类型
                if (System.Enum.TryParse(fields[1], out PortraitExpression expression))
                {
                    // 解析立绘路径
                    string spritePath = fields[2];
                    Sprite expressionSprite = Resources.Load<Sprite>(spritePath);

                    if (expressionSprite != null)
                    {
                        // 添加表情立绘
                        ExpressionSprite expSprite = new ExpressionSprite();
                        expSprite.expression = expression;
                        expSprite.sprite = expressionSprite;
                        character.expressions.Add(expSprite);
                    }
                    else
                    {
                        Debug.LogWarning($"无法加载立绘: {spritePath}");
                    }
                }

                // 解析头像（如果有）
                if (fields.Length > 3 && !string.IsNullOrEmpty(fields[3]))
                {
                    string avatarPath = fields[3];
                    Sprite avatarSprite = Resources.Load<Sprite>(avatarPath);
                    if (avatarSprite != null)
                    {
                        character.defaultAvatar = avatarSprite;
                    }
                }
            }

            // 将字典中的角色添加到列表
            foreach (var character in characterDict.Values)
            {
                characters.Add(character);
            }

            Debug.Log($"成功从CSV导入 {characters.Count} 个角色配置");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"导入角色配置时出错: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 创建角色配置CSV模板
    /// </summary>
    /// <param name="filePath">保存路径</param>
    /// <returns>是否成功创建</returns>
    public static bool CreateCharacterCSVTemplate(string filePath)
    {
        try
        {
            // 定义CSV表头
            string header = "CharacterName,Expression,SpritePath,AvatarPath";

            // 添加示例数据
            string exampleRow1 = "角色A,Normal,Characters/CharacterA/Normal,Avatars/AvatarA";
            string exampleRow2 = "角色A,Happy,Characters/CharacterA/Happy,";
            string exampleRow3 = "角色B,Normal,Characters/CharacterB/Normal,Avatars/AvatarB";
            string exampleRow4 = "角色B,Sad,Characters/CharacterB/Sad,";

            // 写入文件
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine(header);
                writer.WriteLine(exampleRow1);
                writer.WriteLine(exampleRow2);
                writer.WriteLine(exampleRow3);
                writer.WriteLine(exampleRow4);
            }

            Debug.Log($"角色配置CSV模板已创建: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建角色配置CSV模板时出错: {e.Message}");
            return false;
        }
    }

    #endregion
}