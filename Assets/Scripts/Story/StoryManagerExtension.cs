using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StoryManager扩展 - 用于增强CSV立绘加载功能
/// </summary>
public static class StoryManagerExtension
{
    /// <summary>
    /// 在原有的StoryManager中添加这个方法来支持CSV立绘自动加载
    /// </summary>
    public static void LoadCharacterConfigurations(StoryData storyData)
    {
        if (storyData == null) return;

        // 如果剧情中已有角色配置，直接返回
        if (storyData.characters != null && storyData.characters.Count > 0)
        {
            return;
        }

        // 尝试查找所有角色
        HashSet<string> characterNames = new HashSet<string>();
        foreach (var dialogue in storyData.dialogues)
        {
            if (!string.IsNullOrEmpty(dialogue.speakerName))
            {
                characterNames.Add(dialogue.speakerName);
            }
        }

        Debug.Log($"CSV剧情中找到 {characterNames.Count} 个角色，开始自动加载立绘");

        // 对每个角色尝试加载配置
        foreach (var characterName in characterNames)
        {
            // 创建角色配置
            CharacterConfig character = new CharacterConfig
            {
                characterName = characterName,
                expressions = new List<ExpressionSprite>()
            };

            // 尝试从Resources加载不同表情的立绘
            foreach (PortraitExpression expression in System.Enum.GetValues(typeof(PortraitExpression)))
            {
                string expressionName = expression.ToString();
                
                // 尝试几种可能的路径格式
                string[] pathFormats = new string[]
                {
                    $"Characters/{characterName}/{expressionName}",
                    $"Characters/{characterName}_{expressionName}",
                    $"Portraits/{characterName}/{expressionName}",
                    $"Portraits/{characterName}_{expressionName}"
                };

                foreach (var path in pathFormats)
                {
                    Sprite sprite = Resources.Load<Sprite>(path);
                    if (sprite != null)
                    {
                        // 添加找到的表情
                        ExpressionSprite expressionSprite = new ExpressionSprite
                        {
                            expression = expression,
                            sprite = sprite
                        };
                        character.expressions.Add(expressionSprite);
                        Debug.Log($"加载了角色 {characterName} 的 {expressionName} 表情: {path}");
                        break; // 找到就不继续尝试其他路径
                    }
                }
            }

            // 尝试加载默认头像
            string[] avatarPathFormats = new string[]
            {
                $"Avatars/{characterName}",
                $"Characters/{characterName}/Avatar",
                $"Portraits/{characterName}/Avatar"
            };

            foreach (var path in avatarPathFormats)
            {
                Sprite avatar = Resources.Load<Sprite>(path);
                if (avatar != null)
                {
                    character.defaultAvatar = avatar;
                    Debug.Log($"加载了角色 {characterName} 的头像: {path}");
                    break;
                }
            }

            // 如果找到了至少一个表情或头像，添加角色配置
            if (character.expressions.Count > 0 || character.defaultAvatar != null)
            {
                storyData.characters.Add(character);
                Debug.Log($"自动加载角色配置: {characterName}, 找到 {character.expressions.Count} 个表情");
            }
            else
            {
                Debug.LogWarning($"无法为角色 {characterName} 找到任何立绘或头像");
            }
        }
    }
}