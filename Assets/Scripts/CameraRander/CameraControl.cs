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

    // ✅ 新增：平滑移动控制
    private Vector3 smoothTargetPosition;
    private bool isTransitioning = false;
    public float smoothSpeed = 5f;

    void Start()
    {
        cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        smoothTargetPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // ✅ 每帧更新目标位置
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y + 1.5f, transform.position.z);

        // 应用限制（如果有）
        if (currentLimit != null)
        {
            if (currentLimit.left.HasValue && currentLimit.right.HasValue)
            {
                float leftBound = currentLimit.left.Value + halfWidth;
                float rightBound = currentLimit.right.Value - halfWidth;
                if (leftBound < rightBound)
                {
                    desiredPos.x = Mathf.Clamp(desiredPos.x, leftBound, rightBound);
                }
            }

            if (currentLimit.top.HasValue || currentLimit.bottom.HasValue)
            {
                float topBound = currentLimit.top.HasValue ? currentLimit.top.Value - halfHeight : float.MaxValue;
                float bottomBound = currentLimit.bottom.HasValue ? currentLimit.bottom.Value + halfHeight : float.MinValue;
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