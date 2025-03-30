using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SwitchableObj))]
public class SwitchableObjectEditor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();

        if (GUILayout.Button("Set to Expected Size"))
        {
            SwitchableObj switchableObject = (SwitchableObj)target;

            // 获取 GridManager 实例
            if (GridManager.Instance != null)
            {
                switchableObject.SizeToExpectedSize();
            }
            else
            {
                Debug.LogWarning("GridManager not found in scene!");
            }
        }

        // 添加一个按钮
        if (GUILayout.Button("Set to Closest Grid Point")) {
            // 获取当前选中的对象
            SwitchableObj switchableObject = (SwitchableObj)target;

            // 获取 GridManager 实例
            if (GridManager.Instance != null) {
                switchableObject.SetToClosestGridPoint();
            }
            else {
                Debug.LogWarning("GridManager not found in scene!");
            }
        }

        
    }
}