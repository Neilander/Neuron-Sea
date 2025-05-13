using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class Scroll : MonoBehaviour
{
    [Header("滚动设置")]
    public ScrollRect scrollRect;            // ScrollRect引用
    public bool horizontalScroll = false;    // 是否水平滚动
    public float scrollSpeed = 2.0f;         // 滚动速度
    public float pauseBetweenScrolls = 0.5f; // 滚动间隔时间
    
    [Header("滚动序列")]
    [Range(0,1)]
    public float[] scrollPositions = { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f }; // 预设的滚动位置
    
    [Header("事件")]
    public UnityEvent onSequenceStarted;     // 序列开始时触发
    public UnityEvent onSequenceCompleted;   // 序列完成时触发
    public UnityEvent[] onPositionReached;   // 到达每个位置时触发
    
    private Coroutine scrollSequenceCoroutine;
    
    // 在Start中自动开始，或者删除此方法手动调用
    void Start() 
    {
        // 自动开始序列
        StartScrollSequence();
    }
    
    // 开始滚动序列
    public void StartScrollSequence() 
    {
        // 如果已有序列在运行，先停止
        if (scrollSequenceCoroutine != null) 
        {
            StopCoroutine(scrollSequenceCoroutine);
        }
        
        // 开始新的序列
        scrollSequenceCoroutine = StartCoroutine(PerformScrollSequence());
    }
    
    // 停止当前序列
    public void StopScrollSequence() 
    {
        if (scrollSequenceCoroutine != null) 
        {
            StopCoroutine(scrollSequenceCoroutine);
            scrollSequenceCoroutine = null;
        }
    }
    
    // 滚动到单个位置
    public void ScrollToPosition(float position) 
    {
        StartCoroutine(SmoothScrollToPosition(position));
    }
    
    // 主序列协程
    private IEnumerator PerformScrollSequence() 
    {
        // 触发开始事件
        onSequenceStarted?.Invoke();
        
        // 遍历所有位置
        for (int i = 0; i < scrollPositions.Length; i++) 
        {
            // 滚动到指定位置
            yield return StartCoroutine(SmoothScrollToPosition(scrollPositions[i]));
            
            // 触发位置到达事件
            if (onPositionReached != null && i < onPositionReached.Length) 
            {
                onPositionReached[i]?.Invoke();
            }
            
            // 等待指定时间
            yield return new WaitForSeconds(pauseBetweenScrolls);
        }
        
        // 触发完成事件
        onSequenceCompleted?.Invoke();
        scrollSequenceCoroutine = null;
    }
    
    // 平滑滚动到指定位置
    private IEnumerator SmoothScrollToPosition(float targetPosition) 
    {
        // 获取当前位置
        float startPosition = horizontalScroll 
            ? scrollRect.horizontalNormalizedPosition 
            : scrollRect.verticalNormalizedPosition;
        
        // 对于垂直滚动，调整方向（ScrollRect中0=底部，1=顶部）
        if (!horizontalScroll) 
        {
            targetPosition = 1 - targetPosition;
        }
        
        // 计算滚动时间
        float distance = Mathf.Abs(targetPosition - startPosition);
        float duration = distance / scrollSpeed;
        
        // 避免除以零错误
        if (duration <= 0) 
        {
            if (horizontalScroll) 
            {
                scrollRect.horizontalNormalizedPosition = targetPosition;
            } 
            else 
            {
                scrollRect.verticalNormalizedPosition = targetPosition;
            }
            yield break;
        }
        
        float elapsedTime = 0;
        
        while (elapsedTime < duration) 
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            
            // 使用缓动函数使移动更平滑
            float smoothT = Mathf.SmoothStep(0, 1, t);
            float newPosition = Mathf.Lerp(startPosition, targetPosition, smoothT);
            
            // 设置滚动位置
            if (horizontalScroll) 
            {
                scrollRect.horizontalNormalizedPosition = newPosition;
            } 
            else 
            {
                scrollRect.verticalNormalizedPosition = newPosition;
            }
            
            yield return null;
        }
        
        // 确保精确到达目标
        if (horizontalScroll) 
        {
            scrollRect.horizontalNormalizedPosition = targetPosition;
        } 
        else 
        {
            scrollRect.verticalNormalizedPosition = targetPosition;
        }
    }
    
    // 滚动到特定内容项
    public void ScrollToItem(RectTransform targetItem) 
    {
        if (scrollRect == null || targetItem == null) return;
        
        StartCoroutine(ScrollToItemCoroutine(targetItem));
    }
    
    private IEnumerator ScrollToItemCoroutine(RectTransform targetItem) 
    {
        // 确保布局已更新
        Canvas.ForceUpdateCanvases();
        yield return null; // 等待一帧确保更新完成
        
        // 获取视口大小和内容大小
        RectTransform contentPanel = scrollRect.content;
        RectTransform viewport = scrollRect.viewport as RectTransform;
        
        if (contentPanel == null || viewport == null) yield break;
        
        Vector2 viewportSize = viewport.rect.size;
        Vector2 contentSize = contentPanel.rect.size;
        
        // 计算目标位置
        float normalizedPosition;
        
        if (horizontalScroll) 
        {
            // 计算项目在内容中的位置
            float elementOffsetX = targetItem.anchoredPosition.x;
            
            // 考虑视口居中
            float elementCenter = elementOffsetX + (targetItem.rect.width / 2);
            float viewportCenter = viewportSize.x / 2;
            float targetOffsetX = elementCenter - viewportCenter;
            
            // 计算归一化位置
            normalizedPosition = Mathf.Clamp01(targetOffsetX / (contentSize.x - viewportSize.x));
            
            // 滚动到位置
            yield return StartCoroutine(SmoothScrollToPosition(normalizedPosition));
        } 
        else 
        {
            // 对于垂直滚动，注意Y轴锚点位置是反的
            float elementOffsetY = -targetItem.anchoredPosition.y;
            
            // 考虑视口居中
            float elementCenter = elementOffsetY + (targetItem.rect.height / 2);
            float viewportCenter = viewportSize.y / 2;
            float targetOffsetY = elementCenter - viewportCenter;
            
            // 计算归一化位置 (1-x因为垂直滚动是反的)
            normalizedPosition = 1 - Mathf.Clamp01(targetOffsetY / (contentSize.y - viewportSize.y));
            
            // 滚动到位置
            yield return StartCoroutine(SmoothScrollToPosition(1 - normalizedPosition));
        }
    }
    
    // 按索引滚动到项目（假设内容是均匀分布的）
    public void ScrollToItemAtIndex(int index, int totalItems) 
    {
        if (totalItems <= 1) return;
        
        float position = (float)index / (totalItems - 1);
        ScrollToPosition(position);
    }
}
