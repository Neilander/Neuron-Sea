using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CollectableManager))]
public class CollectableManagerEditor : Editor
{
    private int levelIndex = 0; // 用于输入关卡索引

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (EditorApplication.isPlaying)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("运行时调试工具", EditorStyles.boldLabel);

            // 添加关卡索引输入框
            levelIndex = EditorGUILayout.IntField("关卡索引", levelIndex);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("模拟收集"))
            {
                CollectableManager mgr = (CollectableManager)target;
                mgr.TryAddCollection(levelIndex);
                // 立即更新UI
                ConceptArtUnlockManagerNew.Instance?.UpdateArtLockStatus();
                Debug.Log($"已模拟收集关卡 {levelIndex} 的收集物，当前总数：{mgr.totalCollected}");
            }

            if (GUILayout.Button("直接+1(仅内存)"))
            {
                CollectableManager mgr = (CollectableManager)target;
                mgr.totalCollected++;
                // 立即更新UI
                ConceptArtUnlockManagerNew.Instance?.UpdateArtLockStatus();
                Debug.Log($"直接+1，当前总数：{mgr.totalCollected}");
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("减少收集物(带存档)"))
            {
                CollectableManager mgr = (CollectableManager)target;
                mgr.RemoveCollection(levelIndex);
                // 立即更新UI
                ConceptArtUnlockManagerNew.Instance?.UpdateArtLockStatus();
            }

            if (GUILayout.Button("直接-1(仅内存)"))
            {
                CollectableManager mgr = (CollectableManager)target;
                mgr.totalCollected = Mathf.Max(0, mgr.totalCollected - 1);
                // 立即更新UI
                ConceptArtUnlockManagerNew.Instance?.UpdateArtLockStatus();
                Debug.Log($"直接-1，当前总数：{mgr.totalCollected}");
            }
            EditorGUILayout.EndHorizontal();

            // 添加查看当前收集状态的按钮
            if (GUILayout.Button("查看收集状态"))
            {
                CollectableManager mgr = (CollectableManager)target;
                string collectedLevels = string.Join(", ", mgr.collectedLevels);
                Debug.Log($"当前收集状态：\n总数：{mgr.totalCollected}\n已收集关卡：{collectedLevels}");
            }
        }
    }
}