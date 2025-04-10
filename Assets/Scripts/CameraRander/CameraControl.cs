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

    void Start()
    {
        cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        float targetX = target.position.x;
        float targetY = target.position.y + 1.5f;

        if (currentLimit != null)
        {
            if (currentLimit.left.HasValue && currentLimit.right.HasValue)
            {
                float leftBound = currentLimit.left.Value + halfWidth;
                float rightBound = currentLimit.right.Value - halfWidth;
                if (leftBound < rightBound)
                {
                    //Debug.Log("限制x");
                    targetX = Mathf.Clamp(targetX, leftBound, rightBound);
                }
            }

            if (currentLimit.top.HasValue || currentLimit.bottom.HasValue)
            {
                float topBound = currentLimit.top.HasValue ? currentLimit.top.Value - halfHeight : float.MaxValue;
                float bottomBound = currentLimit.bottom.HasValue ? currentLimit.bottom.Value + halfHeight : float.MinValue;
                if (bottomBound < topBound)
                {
                    //Debug.Log("限制y");
                    targetY = Mathf.Clamp(targetY, bottomBound, topBound);
                }
            }
        }

        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }

    public void SetLimitRegion(CameraLimitRegion newRegion)
    {
        
        
        if (setted)
        {
            Debug.Log("设置到queue");
            queuedLimit = newRegion;
            queued = true;
        }
        else
        {
            currentLimit = newRegion;
            setted = true;
        }
    }

    public void ClearLimitRegion(CameraRegionTrigger sender)
    {
        if (setted && currentLimit.setter == sender)
        {
            if (queued)
            {
                currentLimit = queuedLimit;
                queuedLimit = null;
                Debug.Log("替换了当前");
                queued = false;
            }
            else
            {
                currentLimit = null;
                Debug.Log("移除了当前");
                setted = false;
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