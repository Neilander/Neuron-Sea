using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SwitchableObj))]
public class SwitchableObjectEditor : Editor
{
    public override void OnInspectorGUI(){
        base.OnInspectorGUI();
        SwitchableObj switchableObject = (SwitchableObj)target;
        if (GUILayout.Button("设置到预期大小")) {
            // 获取 GridManager 实例
            if (GridManager.Instance != null) {
                switchableObject.SizeToExpectedSize();
            }
            else {
                Debug.LogWarning("GridManager not found in scene!");
            }
        }

        if (GUILayout.Button("设置锚点到预期位置")) {
            // 获取 GridManager 实例
            if (GridManager.Instance != null) {
                switchableObject.SetAnchorToAnchorPos();
            }
            else {
                Debug.LogWarning("GridManager not found in scene!");
            }
        }

        // 添加一个按钮
        if (GUILayout.Button("设置到最近坐标点")) {
            // 获取当前选中的对象

            // 获取 GridManager 实例
            if (GridManager.Instance != null) {
                switchableObject.SetToClosestGridPoint();
            }
            else {
                Debug.LogWarning("GridManager not found in scene!");
            }
        }


        if (GUILayout.Button(switchableObject.IfCanSwitch() ? "关闭交换" : "开启交换")) {
            switchableObject.SwitchEnableSwitchState();
        }
    }
}