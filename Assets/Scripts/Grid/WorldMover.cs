using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldMover : MonoBehaviour
{
    [Header("moveModule")]
    public float moveDuration = 1.0f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public List<UnityEvent> onMoveReachs;
    public int triggerIndex = 0;

    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float elapsedTime = 0;

    void Update()
    {
        if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    /// <summary>
    /// 开始平滑移动到指定位置
    /// </summary>
    /// <param name="destination">3D世界相对位置</param>
    public void MoveTo(Vector3 destination, int i = 0)
    {
        startPosition = transform.position;
        targetPosition = destination;
        elapsedTime = 0;
        isMoving = true;
        triggerIndex = i;
    }

    /// <summary>
    /// 开始平滑移动到指定位置 (可自定义曲线和时长)
    /// </summary>
    public void MoveTo(Vector3 destination, AnimationCurve newCurve, float newT, int i = 0)
    {
        startPosition = transform.position;
        targetPosition = destination;
        elapsedTime = 0;
        isMoving = true;
        moveCurve = newCurve;
        moveDuration = newT;
        triggerIndex = i;
    }

    /// <summary>
    /// 实际移动逻辑
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (!isMoving) return;

        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / moveDuration);
        float curveValue = moveCurve.Evaluate(t);

        transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

        if (t >= 1.0f)
        {
            transform.position = targetPosition;
            isMoving = false;
            if (onMoveReachs != null && triggerIndex >= 0 && triggerIndex < onMoveReachs.Count)
                onMoveReachs[triggerIndex]?.Invoke();
        }
    }
}
