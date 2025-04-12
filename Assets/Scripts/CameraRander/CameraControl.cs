using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target;

    private Camera cam;
    private float halfWidth;
    private float halfHeight;

    private bool setted = false;
    private bool queued = false;
    private CameraLimitRegion currentLimit = null;
    private CameraLimitRegion queuedLimit = null;

    
    private CameraLimitRegion defaultLimit;


    // ✅ 新增：平滑移动控制
    private Vector3 smoothTargetPosition;
    private bool isTransitioning = false;
    public float smoothSpeed = 5f;

    [Header("默认区域配置")]
    public Vector2 defaultOrigin; // 左下角坐标
    public float defaultWidth = 10f;
    public float defaultHeight = 5f;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // 只有在编辑器下且设置了默认区域参数才绘制
        float left = defaultOrigin.x;
        float right = defaultOrigin.x + defaultWidth;
        float bottom = defaultOrigin.y;
        float top = defaultOrigin.y + defaultHeight;

        Vector3 topLeft = new Vector3(left, top, 0);
        Vector3 topRight = new Vector3(right, top, 0);
        Vector3 bottomLeft = new Vector3(left, bottom, 0);
        Vector3 bottomRight = new Vector3(right, bottom, 0);

        // 画矩形边框
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // 可选：在 Scene 里显示文字（需要 Handles）
#if UNITY_EDITOR
    UnityEditor.Handles.Label(new Vector3(left, top + 0.5f, 0), "Default Camera Limit", new GUIStyle()
    {
        fontStyle = FontStyle.Bold,
        normal = new GUIStyleState { textColor = Color.cyan }
    });
#endif
    }

    void Start()
    {
        cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        smoothTargetPosition = transform.position;

        // ✅ 构建默认限制区域
        float left = defaultOrigin.x;
        float right = defaultOrigin.x + defaultWidth;
        float bottom = defaultOrigin.y;
        float top = defaultOrigin.y + defaultHeight;

        defaultLimit = new CameraLimitRegion(left, right, top, bottom, null);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ✅ 每帧更新目标位置
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y + 1.5f, transform.position.z);

        // ✅ 选择使用 currentLimit 或 defaultLimit
        CameraLimitRegion limitToUse = setted ? currentLimit : defaultLimit;

        if (limitToUse != null)
        {
            if (limitToUse.left.HasValue && limitToUse.right.HasValue)
            {
                float leftBound = limitToUse.left.Value + halfWidth;
                float rightBound = limitToUse.right.Value - halfWidth;
                if (leftBound < rightBound)
                {
                    desiredPos.x = Mathf.Clamp(desiredPos.x, leftBound, rightBound);
                }
            }

            if (limitToUse.top.HasValue || limitToUse.bottom.HasValue)
            {
                float topBound = limitToUse.top.HasValue ? limitToUse.top.Value - halfHeight : float.MaxValue;
                float bottomBound = limitToUse.bottom.HasValue ? limitToUse.bottom.Value + halfHeight : float.MinValue;
                if (bottomBound < topBound)
                {
                    desiredPos.y = Mathf.Clamp(desiredPos.y, bottomBound, topBound);
                }
            }
        }

        // ✅ 是否平滑移动中
        if (isTransitioning)
        {
            // 每帧更新目标位置
            smoothTargetPosition = desiredPos;

            // 平滑插值移动
            transform.position = Vector3.Lerp(transform.position, smoothTargetPosition, Time.deltaTime * smoothSpeed);

            // 到达目标，停止平滑
            if (Vector3.Distance(transform.position, smoothTargetPosition) < 0.01f)
            {
                transform.position = smoothTargetPosition;
                isTransitioning = false;
            }
        }
        else
        {
            // 正常锁定逻辑
            transform.position = desiredPos;
        }
    }

    public void SetLimitRegion(CameraLimitRegion newRegion)
    {
        if (setted)
        {
            Debug.Log("设置到 queue");
            queuedLimit = newRegion;
            queued = true;
        }
        else
        {
            currentLimit = newRegion;
            setted = true;
            isTransitioning = true; // ✅ 开启平滑过渡
        }
    }

    public void ClearLimitRegion(CameraRegionTrigger sender)
    {
        if (setted && currentLimit != null && currentLimit.setter == sender)
        {
            if (queued)
            {
                currentLimit = queuedLimit;
                queuedLimit = null;
                queued = false;
                Debug.Log("替换了当前");
                isTransitioning = true; // ✅ 新区域 → 平滑进入
            }
            else
            {
                currentLimit = null;
                Debug.Log("移除了当前");
                setted = false;
                isTransitioning = true; // ✅ 清空限制，也平滑
            }
        }
    }
}

[System.Serializable]
public class CameraLimitRegion
{
    public float? left;
    public float? right;
    public float? top;
    public float? bottom;

    public CameraRegionTrigger setter; // 谁设置的

    public CameraLimitRegion(float? left, float? right, float? top, float? bottom, CameraRegionTrigger setter)
    {
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
        this.setter = setter;
    }
}