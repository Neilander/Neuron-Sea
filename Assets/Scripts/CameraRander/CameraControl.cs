using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Transform target;

    [Header("必填 - 水平限制")]
    public Transform LeftLimit;
    public Transform RightLimit;

    [Header("可选 - 垂直限制")]
    public Transform TopLimit;
    public Transform BottomLimit;

    private Camera cam;
    private float halfWidth;
    private float halfHeight;

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

        bool hasLeft = LeftLimit != null;
        bool hasRight = RightLimit != null;

        if (hasLeft && hasRight)
        {
            float leftBound = LeftLimit.position.x + halfWidth;
            float rightBound = RightLimit.position.x - halfWidth;

            if (leftBound < rightBound)
            {
                targetX = Mathf.Clamp(targetX, leftBound, rightBound);
            }
        }

        // 垂直逻辑保留（你之前写的）
        bool hasTop = TopLimit != null;
        bool hasBottom = BottomLimit != null;

        if (hasTop || hasBottom)
        {
            float topBound = hasTop ? TopLimit.position.y - halfHeight : float.MaxValue;
            float bottomBound = hasBottom ? BottomLimit.position.y + halfHeight : float.MinValue;

            if (bottomBound < topBound)
            {
                targetY = Mathf.Clamp(targetY, bottomBound, topBound);
            }
        }

        transform.position = new Vector3(targetX, targetY, transform.position.z);
    }
}