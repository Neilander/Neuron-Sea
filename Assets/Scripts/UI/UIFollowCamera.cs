using UnityEngine;
using UnityEngine.UI;

public class UIFollowCamera: MonoBehaviour
{
    public Transform target; // 需要跟随的场景物体

    public Vector3 screenOffset; // UI相对于屏幕位置的偏移量

    public Camera renderCamera; // 绑定Canvas的渲染摄像机

    private RectTransform uiRect;

    void Start(){
        uiRect = GetComponent<RectTransform>();
        if (renderCamera == null) renderCamera = Camera.main;
    }

    void LateUpdate(){
        if (target == null || renderCamera == null) return;

        // 将物体的世界坐标转换为屏幕坐标
        Vector3 screenPos = renderCamera.WorldToScreenPoint(target.position);

        // 应用偏移量
        screenPos += screenOffset;

        // 更新UI位置
        uiRect.position = screenPos;

        // 可选：处理物体超出屏幕边缘的情况
        // ClampToScreen(screenPos);
    }

    // 限制UI在屏幕范围内（可选）
    void ClampToScreen(Vector3 screenPos){
        Vector3 clampedPos = screenPos;
        clampedPos.x = Mathf.Clamp(clampedPos.x, 0, Screen.width);
        clampedPos.y = Mathf.Clamp(clampedPos.y, 0, Screen.height);
        uiRect.position = clampedPos;
    }
}