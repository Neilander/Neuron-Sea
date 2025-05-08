using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UiMover : MonoBehaviour
{
    /*
     * 使用这个Mover，如果是固定移动，可以挂载后直接在inspector里设置移动曲线等
     * 如果是多种不同的移动，可以在挂载的ui上加一个管理器，然后用管理器输入时间和move curve来进行不同的移动
     */

    #region moveModule
    [Header("moveModule")]
    public float moveDuration = 1.0f; // 移动所需时间
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 自定义移动曲线

    public List<UnityEvent> onMoveReachs; // 可在 Inspector 中设置的 UnityEvent
    public int triggerIndex = 0;//触发第几个reach event
    private bool isMoving = false; // 是否正在移动
    private Vector2 startPosition; // 起始位置
    private Vector2 targetPosition; // 目标位置
    private float elapsedTime = 0; // 累计时间

    private RectTransform rectTransform; // UI 的 RectTransform

    //private IActionWhileMove selfExtraAction;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>(); // 获取 RectTransform
    }

    void Update()
    {
        if (isMoving)
        {
            //CallSound();
            MoveTowardsTarget();
        }
    }

    /// <summary>
    /// 开始平滑移动到指定位置
    /// </summary>
    /// <param name="destination">目标位置 (Vector2)</param>
    public void MoveTo(Vector2 destination, int i = 0)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform 未找到，确保此脚本附加在 UI 对象上！");
            return;
        }

        startPosition = rectTransform.anchoredPosition; // 记录起始位置
        targetPosition = destination; // 设置目标位置
        elapsedTime = 0; // 重置计时
        isMoving = true; // 开始移动
        triggerIndex = i;
    }

    /// <summary>
    /// 开始平滑移动到指定位置
    /// </summary>
    /// <param name="destination">目标位置 (Vector2)</param>
    /// <param name="newCurve">用于控制移动速度的动画曲线 (AnimationCurve)</param>
    /// <param name="newT">移动的持续时间 (float)</param>
    /// <param name="i">触发索引，默认值为 0 (int)</param>
    public void MoveTo(Vector2 destination, AnimationCurve newCurve, float newT , int i = 0)
    {
        if (rectTransform == null)
        {
            Debug.LogError("RectTransform 未找到，确保此脚本附加在 UI 对象上！");
            return;
        }

        startPosition = rectTransform.anchoredPosition; // 记录起始位置
        targetPosition = destination; // 设置目标位置
        elapsedTime = 0; // 重置计时
        isMoving = true; // 开始移动
        moveCurve = newCurve;
        moveDuration = newT;
        triggerIndex = i;
    }

    /// <summary>
    /// 平滑移动到目标点
    /// </summary>
    private void MoveTowardsTarget()
    {
        if (!isMoving) return;

        // 增加时间
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / moveDuration); // 归一化时间 [0, 1]

        // 根据曲线计算插值
        float curveValue = moveCurve.Evaluate(t);
        rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, curveValue);

        // 检测是否完成移动
        if (t >= 1.0f)
        {
            rectTransform.anchoredPosition = targetPosition; // 确保完全对齐
            isMoving = false; // 停止移动
            if (onMoveReachs != null && (triggerIndex >= 0) && (triggerIndex < onMoveReachs.Count))
                onMoveReachs[triggerIndex]?.Invoke();
        }
    }

    #endregion
}

/*
public interface IActionWhileMove
{
    void DoWhenMove(float process);
}*/
