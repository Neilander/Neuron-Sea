using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

/// <summary>
/// 故事导入工具，用于从CSV等表格数据导入对话内容
/// </summary>
public class StoryImporter : EditorWindow
{
    private StoryData targetStoryData;
    private string dialogueCsvPath = "";
    private string characterCsvPath = "";
    private string statusMessage = "";
    private bool showHelp = false;
    private Vector2 scrollPosition;
    private bool showDialogueTab = true; // 当前显示的标签页

    // 添加菜单项
    [MenuItem("Tools/故事/故事导入工具")]
    public static void ShowWindow()
    {
        GetWindow<StoryImporter>("故事导入工具");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("剧情数据表格导入工具", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox("该工具用于从CSV表格文件导入对话数据到StoryData资源中", MessageType.Info);
        EditorGUILayout.Space(10);

        // 标签页
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Toggle(showDialogueTab, "对话数据", EditorStyles.toolbarButton))
        {
            showDialogueTab = true;
        }
        if (GUILayout.Toggle(!showDialogueTab, "角色表情", EditorStyles.toolbarButton))
        {
            showDialogueTab = false;
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // 显示帮助信息按钮
        showHelp = EditorGUILayout.Foldout(showHelp, "显示帮助信息");
        if (showHelp)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
            
            if (showDialogueTab)
            {
                EditorGUILayout.HelpBox(
                    "对话CSV文件格式要求：\n" +
                    "1. 第一行为标题行，包含字段名称\n" +
                    "2. 从第二行开始为数据行\n" +
                    "3. 主要字段包括：\n" +
                    "   - SpeakerName: 角色名称\n" +
                    "   - DialogueText: 对话内容\n" +
                    "   - PortraitPosition: 立绘位置 (Left/Center/Right)\n" +
                    "   - Expression: 表情 (Normal/Happy/Sad/Angry/Surprised/Custom)\n" +
                    "   - Effect: 特效 (None/Shake/Bounce/Spin/Flash)\n" +
                    "   - EffectIntensity: 特效强度\n" +
                    "   - EffectDuration: 特效持续时间\n" +
                    "   - PortraitPath: Resources下的立绘路径\n" +
                    "   - AvatarPath: Resources下的头像路径\n\n" +
                    "注意：立绘和头像需要放在Resources文件夹下，CSV中填写相对路径",
                    MessageType.Info
                );
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "角色表情CSV文件格式要求：\n" +
                    "1. 第一行为标题行，包含字段名称\n" +
                    "2. 从第二行开始为数据行\n" +
                    "3. 主要字段包括：\n" +
                    "   - CharacterName: 角色名称\n" +
                    "   - Expression: 表情类型 (Normal/Happy/Sad/Angry/Surprised/Custom)\n" +
                    "   - SpritePath: Resources下的立绘路径\n" +
                    "   - AvatarPath: Resources下的头像路径(可选)\n\n" +
                    "注意：\n" +
                    "1. 同一角色的不同表情应该分行记录\n" +
                    "2. 立绘和头像需要放在Resources文件夹下，CSV中填写相对路径",
                    MessageType.Info
                );
            }
            
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.Space(10);

        // 选择目标StoryData资源
        EditorGUILayout.LabelField("步骤1: 选择目标剧情数据资源", EditorStyles.boldLabel);
        targetStoryData = EditorGUILayout.ObjectField("目标故事数据", targetStoryData, typeof(StoryData), false) as StoryData;

        if (targetStoryData == null)
        {
            EditorGUILayout.HelpBox("请先选择一个StoryData资源，如果没有，请通过右键 -> Create -> Story -> Story Data 创建", MessageType.Warning);

            if (GUILayout.Button("创建新的StoryData资源"))
            {
                string path = EditorUtility.SaveFilePanelInProject(
                    "创建StoryData",
                    "NewStoryData",
                    "asset",
                    "请选择保存路径和文件名");

                if (!string.IsNullOrEmpty(path))
                {
                    StoryData newStoryData = CreateInstance<StoryData>();
                    AssetDatabase.CreateAsset(newStoryData, path);
                    AssetDatabase.SaveAssets();
                    targetStoryData = newStoryData;
                }
            }
        }

        EditorGUILayout.Space(10);

        if (showDialogueTab)
        {
            // 对话数据导入
            ShowDialogueImportGUI();
        }
        else
        {
            // 角色表情导入
            ShowCharacterImportGUI();
        }

        EditorGUILayout.Space(10);

        // 显示状态消息
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);
        }
    }

    private void ShowDialogueImportGUI()
    {
        // 选择CSV文件
        EditorGUILayout.LabelField("步骤2: 选择对话CSV文件", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("CSV文件路径", dialogueCsvPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string newPath = EditorUtility.OpenFilePanel("选择对话CSV文件", "", "csv");
            if (!string.IsNullOrEmpty(newPath))
            {
                dialogueCsvPath = newPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (string.IsNullOrEmpty(dialogueCsvPath))
        {
            EditorGUILayout.HelpBox("请选择一个对话CSV文件", MessageType.Info);

            if (GUILayout.Button("创建对话CSV模板"))
            {
                string templatePath = EditorUtility.SaveFilePanel(
                    "保存对话CSV模板",
                    "",
                    "DialogueTemplate.csv",
                    "csv");

                if (!string.IsNullOrEmpty(templatePath))
                {
                    if (StoryData.CreateCSVTemplate(templatePath))
                    {
                        statusMessage = "成功创建对话CSV模板";
                        dialogueCsvPath = templatePath;
                    }
                    else
                    {
                        statusMessage = "创建对话CSV模板失败";
                    }
                }
            }
        }

        EditorGUILayout.Space(10);

        // 导入按钮
        EditorGUILayout.LabelField("步骤3: 执行导入", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(targetStoryData == null || string.IsNullOrEmpty(dialogueCsvPath));
        if (GUILayout.Button("导入对话数据"))
        {
            ImportDialogueData();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // 将CSV存储到Resources文件夹选项
        EditorGUILayout.LabelField("步骤4: 复制CSV到Resources(可选)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("此步骤将CSV文件复制到Resources文件夹，以便游戏运行时加载", MessageType.Info);
        
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(dialogueCsvPath));
        if (GUILayout.Button("复制对话CSV到Resources文件夹"))
        {
            CopyToResources(dialogueCsvPath, "Dialogue");
        }
        EditorGUI.EndDisabledGroup();
    }

    private void ShowCharacterImportGUI()
    {
        // 选择CSV文件
        EditorGUILayout.LabelField("步骤2: 选择角色表情CSV文件", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.TextField("CSV文件路径", characterCsvPath);
        if (GUILayout.Button("浏览...", GUILayout.Width(80)))
        {
            string newPath = EditorUtility.OpenFilePanel("选择角色表情CSV文件", "", "csv");
            if (!string.IsNullOrEmpty(newPath))
            {
                characterCsvPath = newPath;
            }
        }
        EditorGUILayout.EndHorizontal();

        if (string.IsNullOrEmpty(characterCsvPath))
        {
            EditorGUILayout.HelpBox("请选择一个角色表情CSV文件", MessageType.Info);

            if (GUILayout.Button("创建角色表情CSV模板"))
            {
                string templatePath = EditorUtility.SaveFilePanel(
                    "保存角色表情CSV模板",
                    "",
                    "CharacterTemplate.csv",
                    "csv");

                if (!string.IsNullOrEmpty(templatePath))
                {
                    if (StoryData.CreateCharacterCSVTemplate(templatePath))
                    {
                        statusMessage = "成功创建角色表情CSV模板";
                        characterCsvPath = templatePath;
                    }
                    else
                    {
                        statusMessage = "创建角色表情CSV模板失败";
                    }
                }
            }
        }

        EditorGUILayout.Space(10);

        // 导入按钮
        EditorGUILayout.LabelField("步骤3: 执行导入", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(targetStoryData == null || string.IsNullOrEmpty(characterCsvPath));
        if (GUILayout.Button("导入角色表情数据"))
        {
            ImportCharacterData();
        }
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(10);

        // 将CSV存储到Resources文件夹选项
        EditorGUILayout.LabelField("步骤4: 复制CSV到Resources(可选)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("此步骤将CSV文件复制到Resources文件夹，以便游戏运行时加载", MessageType.Info);
        
        EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(characterCsvPath));
        if (GUILayout.Button("复制角色表情CSV到Resources文件夹"))
        {
            CopyToResources(characterCsvPath, "Character");
        }
        EditorGUI.EndDisabledGroup();
    }

    /// <summary>
    /// 执行对话数据导入
    /// </summary>
    private void ImportDialogueData()
    {
        if (targetStoryData == null)
        {
            statusMessage = "错误：未指定目标StoryData资源";
            return;
        }

        if (string.IsNullOrEmpty(dialogueCsvPath) || !File.Exists(dialogueCsvPath))
        {
            statusMessage = "错误：对话CSV文件不存在";
            return;
        }

        // 记录原始对话数量
        int originalCount = targetStoryData.dialogues.Count;

        // 执行导入
        bool success = targetStoryData.ImportFromCSV(dialogueCsvPath);

        if (success)
        {
            // 标记资源已修改，以便保存
            EditorUtility.SetDirty(targetStoryData);
            AssetDatabase.SaveAssets();

            statusMessage = $"导入成功! 从{originalCount}条对话更新为{targetStoryData.dialogues.Count}条对话";
        }
        else
        {
            statusMessage = "导入失败，请检查控制台错误信息";
        }
    }

    /// <summary>
    /// 执行角色表情数据导入
    /// </summary>
    private void ImportCharacterData()
    {
        if (targetStoryData == null)
        {
            statusMessage = "错误：未指定目标StoryData资源";
            return;
        }

        if (string.IsNullOrEmpty(characterCsvPath) || !File.Exists(characterCsvPath))
        {
            statusMessage = "错误：角色表情CSV文件不存在";
            return;
        }

        // 记录原始角色数量
        int originalCount = targetStoryData.characters.Count;

        // 执行导入
        bool success = targetStoryData.ImportCharactersFromCSV(characterCsvPath);

        if (success)
        {
            // 标记资源已修改，以便保存
            EditorUtility.SetDirty(targetStoryData);
            AssetDatabase.SaveAssets();

            statusMessage = $"导入成功! 从{originalCount}个角色更新为{targetStoryData.characters.Count}个角色";
        }
        else
        {
            statusMessage = "导入失败，请检查控制台错误信息";
        }
    }

    /// <summary>
    /// 将CSV文件复制到Resources文件夹
    /// </summary>
    private void CopyToResources(string csvFilePath, string folderName)
    {
        if (string.IsNullOrEmpty(csvFilePath) || !File.Exists(csvFilePath))
        {
            statusMessage = "错误：CSV文件不存在";
            return;
        }

        // 确保Resources/StoryData/CSV目录存在
        string resourcesPath = Path.Combine(Application.dataPath, "Resources");
        string storyDataPath = Path.Combine(resourcesPath, "StoryData");
        string csvDirPath = Path.Combine(storyDataPath, "CSV");
        string targetDirPath = Path.Combine(csvDirPath, folderName);

        if (!Directory.Exists(resourcesPath))
        {
            Directory.CreateDirectory(resourcesPath);
        }

        if (!Directory.Exists(storyDataPath))
        {
            Directory.CreateDirectory(storyDataPath);
        }

        if (!Directory.Exists(csvDirPath))
        {
            Directory.CreateDirectory(csvDirPath);
        }

        if (!Directory.Exists(targetDirPath))
        {
            Directory.CreateDirectory(targetDirPath);
        }

        // 获取文件名
        string fileName = Path.GetFileName(csvFilePath);
        string destPath = Path.Combine(targetDirPath, fileName);

        try
        {
            // 复制文件
            File.Copy(csvFilePath, destPath, true);
            statusMessage = $"已将CSV文件复制到: {destPath.Replace(Application.dataPath, "Assets")}";

            // 刷新资源数据库
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            statusMessage = $"复制CSV文件失败: {e.Message}";
        }
    }
}

/// <summary>
/// 提供在StoryData资源上的右键菜单功能
/// </summary>
public static class StoryDataContextMenu
{
    [MenuItem("Assets/Story/从CSV导入对话", true)]
    private static bool ValidateImportFromCSV()
    {
        return Selection.activeObject is StoryData;
    }

    [MenuItem("Assets/Story/从CSV导入对话")]
    private static void ImportFromCSV()
    {
        StoryData storyData = Selection.activeObject as StoryData;
        if (storyData == null) return;

        string csvPath = EditorUtility.OpenFilePanel("选择CSV文件", "", "csv");
        if (string.IsNullOrEmpty(csvPath)) return;

        bool success = storyData.ImportFromCSV(csvPath);
        if (success)
        {
            EditorUtility.SetDirty(storyData);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("导入成功", $"成功导入 {storyData.dialogues.Count} 条对话数据", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("导入失败", "导入CSV数据失败，请检查控制台错误信息", "确定");
        }
    }

    [MenuItem("Assets/Story/从CSV导入角色表情", true)]
    private static bool ValidateImportCharactersFromCSV()
    {
        return Selection.activeObject is StoryData;
    }

    [MenuItem("Assets/Story/从CSV导入角色表情")]
    private static void ImportCharactersFromCSV()
    {
        StoryData storyData = Selection.activeObject as StoryData;
        if (storyData == null) return;

        string csvPath = EditorUtility.OpenFilePanel("选择角色表情CSV文件", "", "csv");
        if (string.IsNullOrEmpty(csvPath)) return;

        bool success = storyData.ImportCharactersFromCSV(csvPath);
        if (success)
        {
            EditorUtility.SetDirty(storyData);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("导入成功", $"成功导入 {storyData.characters.Count} 个角色表情配置", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("导入失败", "导入角色表情数据失败，请检查控制台错误信息", "确定");
        }
    }

    [MenuItem("Assets/Story/创建CSV模板")]
    private static void CreateCSVTemplate()
    {
        string templatePath = EditorUtility.SaveFilePanel(
            "保存CSV模板",
            "",
            "StoryTemplate.csv",
            "csv");

        if (string.IsNullOrEmpty(templatePath)) return;

        if (StoryData.CreateCSVTemplate(templatePath))
        {
            EditorUtility.DisplayDialog("成功", "CSV模板已创建", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("失败", "创建CSV模板失败", "确定");
        }
    }

    [MenuItem("Assets/Story/创建角色表情CSV模板")]
    private static void CreateCharacterCSVTemplate()
    {
        string templatePath = EditorUtility.SaveFilePanel(
            "保存角色表情CSV模板",
            "",
            "CharacterTemplate.csv",
            "csv");

        if (string.IsNullOrEmpty(templatePath)) return;

        if (StoryData.CreateCharacterCSVTemplate(templatePath))
        {
            EditorUtility.DisplayDialog("成功", "角色表情CSV模板已创建", "确定");
        }
        else
        {
            EditorUtility.DisplayDialog("失败", "创建角色表情CSV模板失败", "确定");
        }
    }
}
#endif