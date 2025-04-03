using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollower : MonoBehaviour
{
    public Transform target; // 需要跟随的场景物体

    public Vector3 offset; // UI相对于物体的偏移量

    public Camera mainCamera; // 主摄像机

    private RectTransform uiRect;

    void Start(){
        uiRect = GetComponent<RectTransform>();
        if (mainCamera == null) mainCamera = Camera.main;
    }

    void Update(){
        // if (target == null || mainCamera == null) return;

        // 将物体的世界坐标转换为屏幕坐标
        Vector3 screenPos = target.position + offset;

        // 更新UI位置
        uiRect.position = screenPos;

        // 处理物体超出屏幕边缘的情况
        // ClampToScreen(screenPos);
    }

    // 限制UI在屏幕范围内
    // void ClampToScreen(Vector3 screenPos){
    //     Vector3 clampedPos = screenPos;
    //     clampedPos.x = Mathf.Clamp(clampedPos.x, 0, Screen.width);
    //     clampedPos.y = Mathf.Clamp(clampedPos.y, 0, Screen.height);
    //     uiRect.position = clampedPos;
    // }
}
