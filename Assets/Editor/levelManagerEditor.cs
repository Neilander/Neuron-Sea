using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(levelManager))]
public class levelManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // 保留原有 Inspector 内容

        if (EditorApplication.isPlaying)
        {
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Lock All"))
            {
                ((levelManager)target).LockAllLevel();
            }

            if (GUILayout.Button("Unlock All"))
            {
                ((levelManager)target).UnlockAllLevel();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
