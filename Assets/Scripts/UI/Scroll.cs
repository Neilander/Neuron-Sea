using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Scroll : MonoBehaviour
{
    [Header("滚动设置")]
    public ScrollRect scrollRect;            // ScrollRect引用
    public bool horizontalScroll = true;     // 默认设为水平滚动
    public float scrollSpeed = 4.0f;         // 滚动速度

    [Header("调试选项")]
    public bool enableDebugLogs = false;     // 是否启用调试日志

    private Coroutine currentScrollCoroutine; // 当前执行的滚动协程
    private float originalPosition = 0f;      // 保存原始位置
    private GameObject lastClickedButton; // 记录最后点击的按钮
    private bool isExpanded = false;      // 标记当前是否处于展开状态

    private void OnEnable()
    {
        // 检查ScrollRect是否设置正确
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
            print("得到了");
            if (scrollRect == null)
            {
                Debug.LogError("Scroll组件没有找到ScrollRect! 请在Inspector中指定或添加到同一GameObject上。");
                enabled = false;
                return;
            }
        }

        // // 记录初始位置
        // SaveOriginalPosition();

        // 添加按钮点击监听
        SetupButtonListeners();
    }

    // 保存原始位置
    public void SaveOriginalPosition()
    {
        if (scrollRect != null)
        {
            originalPosition = horizontalScroll
                ? scrollRect.horizontalNormalizedPosition
                : scrollRect.verticalNormalizedPosition;

            if (enableDebugLogs)
            {
                Debug.Log($"保存了原始位置: {originalPosition}");
            }
        }
    }

    // 为所有子按钮添加点击监听
    private void SetupButtonListeners()
    {
        ClearButtonListeners();
        // 临时存储找到的按钮
        List<Button> buttons = new List<Button>();

        // 获取content的直接子物体
        Transform contentTransform = scrollRect.content;
        for (int i = 0; i < contentTransform.childCount; i++) {
            Transform child = contentTransform.GetChild(i);

            // 检查子物体是否有Button组件
            Button button = child.GetComponent<Button>();
            if (button != null) {
                buttons.Add(button);

                if (enableDebugLogs) {
                    Debug.Log($"找到直接子物体按钮: {child.name}");
                }
            }
        }
        if (buttons.Count == 0 && enableDebugLogs)
        {
            Debug.LogWarning("ScrollRect的content中没有找到任何Button组件!");
        }

        // 为每个按钮添加点击事件
        foreach (Button button in buttons)
        {
            // 使用lambda捕获按钮引用
            button.onClick.AddListener(() => OnButtonClicked(button.gameObject));

            if (enableDebugLogs)
            {
                Debug.Log($"为按钮 '{button.gameObject.name}' 添加了点击监听");
            }
        }
    }

    private void ClearButtonListeners(){
        if (scrollRect == null || scrollRect.content == null) return;

        // 只清除直接子物体的按钮监听
        for (int i = 0; i < scrollRect.content.childCount; i++) {
            Transform child = scrollRect.content.GetChild(i);
            Button button = child.GetComponent<Button>();

            if (button != null) {
                // 移除我们可能添加的监听
                button.onClick.RemoveAllListeners();

                if (enableDebugLogs) {
                    Debug.Log($"清除了按钮 '{button.gameObject.name}' 的所有监听");
                }
            }
        }
    }
    // 当任何子按钮被点击时调用
    public void OnButtonClicked(GameObject buttonObj)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"按钮 '{buttonObj.name}' 被点击");
        }

        // // 判断是否是同一个按钮被再次点击
        // if (buttonObj == lastClickedButton && isExpanded)
        // {
        //     // 如果是同一个按钮再次点击，收起并滚动回原位置
        //     if (enableDebugLogs)
        //     {
        //         Debug.Log($"同一按钮再次点击，正在收起并滚动回原位置");
        //     }
        //
        //     isExpanded = false;
        //     // ScrollToOriginalPosition();
        //     return;
        // }

        // 记录当前点击的按钮，并标记为展开状态
        // lastClickedButton = buttonObj;
        // isExpanded = true;

        // 获取按钮的RectTransform(大小)
        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        if (buttonRect == null) return;

        // 将按钮移到最左/上边
        ScrollButtonToEdge(buttonRect);
    }

    // 将按钮滚动到最左边/最上边
    public void ScrollButtonToEdge(RectTransform buttonRect)
    {
        if (currentScrollCoroutine != null) {
            StopCoroutine(currentScrollCoroutine);
            currentScrollCoroutine = null; // 确保变量被重置
        }

        currentScrollCoroutine = StartCoroutine(ScrollToEdgeCoroutine(buttonRect));
    }

    // 滚动到边缘位置的协程
    private IEnumerator ScrollToEdgeCoroutine(RectTransform targetItem)
    {
        // 确保布局已更新
        Canvas.ForceUpdateCanvases();
        yield return null;

        // 获取必要的引用
        RectTransform contentPanel = scrollRect.content;
        
        RectTransform viewport = scrollRect.viewport as RectTransform;

        if (contentPanel == null || viewport == null)
        {
            Debug.LogError("无法获取ContentPanel或Viewport!");
            yield break;
        }

        // 计算目标位置
        float targetPosition;

        if (horizontalScroll)
        {
            // 水平滚动 - 计算要显示在最左边的位置
            float leftEdgeOffset = 0; // 最左边位置的偏移

            // 获取按钮的左边缘位置（而不是中心位置）
            float elementLeftEdge = targetItem.anchoredPosition.x - (targetItem.rect.width * targetItem.pivot.x);
            float elementPosition = elementLeftEdge;

            // 归一化位置计算
            float contentWidth = contentPanel.rect.width;
            print(contentWidth);
            float viewportWidth = viewport.rect.width;
            print(viewportWidth);

            // 确保内容宽度大于视口宽度
            if (contentWidth <= viewportWidth)
            {
                targetPosition = 0; // 内容不需要滚动，保持在最左边
            }
            else
            {
                // 计算归一化位置，确保按钮的左边缘在视口的左边缘
                targetPosition = elementPosition / (contentWidth - viewportWidth);
                print(targetPosition);
                targetPosition = Mathf.Clamp01(targetPosition);
            }
        }
        else
        {
            // 垂直滚动 - 计算要显示在最上边的位置
            float topEdgeOffset = 0; // 最上边位置的偏移

            // 获取按钮的上边缘位置（而不是中心位置）
            float elementTopEdge = -targetItem.anchoredPosition.y - (targetItem.rect.height * targetItem.pivot.y);
            float elementPosition = elementTopEdge; // Y是反的

            // 归一化位置计算
            float contentHeight = contentPanel.rect.height;
            float viewportHeight = viewport.rect.height;

            // 确保内容高度大于视口高度
            if (contentHeight <= viewportHeight)
            {
                targetPosition = 1; // 内容不需要滚动，保持在最上边(垂直滚动1为顶部)
            }
            else
            {
                // 计算归一化位置，确保按钮的上边缘在视口的上边缘
                // 垂直滚动时 1-x 因为ScrollRect的垂直位置是反的(1=顶部)
                targetPosition = 1 - (elementPosition / (contentHeight - viewportHeight));
                targetPosition = Mathf.Clamp01(targetPosition);
            }
        }
        // 滚动到目标位置
        yield return StartCoroutine(SmoothScrollToPosition(targetPosition));

        if (enableDebugLogs)
        {
            string edge = horizontalScroll ? "最左边" : "最上边";
            Debug.Log($"滚动完成，'{targetItem.gameObject.name}' 现在位于{edge}");
        }

        currentScrollCoroutine = null;
    }

    // 平滑滚动到指定位置
    private IEnumerator SmoothScrollToPosition(float targetPosition){
        // 获取当前位置
        float startPosition = horizontalScroll
            ? scrollRect.horizontalNormalizedPosition
            : scrollRect.verticalNormalizedPosition;

        // 调试输出初始状态
        Debug.Log($"开始滚动: 从 {startPosition} 到 {targetPosition}");

        // 如果位置接近，直接设置并退出
        if (Mathf.Abs(startPosition - targetPosition) < 0.01f) {
            SetScrollPosition(targetPosition);
            Debug.Log("距离太小，直接设置位置");
            yield break;
        }

        // 计算滚动时间
        float distance = Mathf.Abs(targetPosition - startPosition);
        float duration = distance / scrollSpeed;

        // 增加最大持续时间保护（防止无限循环）
        float maxDuration = 5.0f; // 最多执行5秒
        duration = Mathf.Min(duration, maxDuration);

        Debug.Log($"滚动距离: {distance}, 计算的持续时间: {duration}秒");

        // 开始计时（使用不受timeScale影响的时间）
        float startTime = Time.realtimeSinceStartup;
        float endTime = startTime + duration;
        int frameCount = 0;
        float lastLoggedPosition = startPosition;

        while (Time.realtimeSinceStartup < endTime) {
            frameCount++;
            // 使用不受timeScale影响的计时
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // 使用缓动函数使移动更平滑
            float smoothT = Mathf.SmoothStep(0, 1, t);
            float newPosition = Mathf.Lerp(startPosition, targetPosition, smoothT);

            // 设置滚动位置
            SetScrollPosition(newPosition);

            // 如果已经非常接近目标，直接结束
            if (Mathf.Abs(newPosition - targetPosition) < 0.001f) {
                Debug.Log($"已足够接近目标，提前结束: {newPosition:F4} vs {targetPosition:F4}");
                break;
            }

            yield return null;
        }

        // 确保最终精确到达目标
        SetScrollPosition(targetPosition);
        Debug.Log($"滚动结束，最终位置: {targetPosition:F4}");

        currentScrollCoroutine = null;
    }

    // 设置滚动位置辅助方法
    private void SetScrollPosition(float position)
    {
        if (horizontalScroll)
        {
            scrollRect.horizontalNormalizedPosition = position;
        }
        else
        {
            scrollRect.verticalNormalizedPosition = position;
        }
    }

    // 公共方法：滚动到指定游戏对象
    public void ScrollToGameObject(GameObject targetObject)
    {
        if (targetObject == null) return;

        RectTransform targetRect = targetObject.GetComponent<RectTransform>();
        if (targetRect != null)
        {
            ScrollButtonToEdge(targetRect);
        }
    }

    // 重新设置所有按钮监听 (如果动态添加了新按钮可调用此方法)
    public void RefreshButtonListeners()
    {
        SetupButtonListeners();
    }

    // 公共方法：切换水平/垂直模式
    public void SetHorizontalMode(bool isHorizontal)
    {
        horizontalScroll = isHorizontal;
    }

    // 公共方法：平滑滚动回到原始位置
    public void ScrollToOriginalPosition()
    {
        if (enableDebugLogs)
        {
            Debug.Log($"正在滚动回原始位置: {originalPosition}");
        }

        // 如果已有滚动协程在运行，先停止
        if (currentScrollCoroutine != null)
        {
            StopCoroutine(currentScrollCoroutine);
        }

        currentScrollCoroutine = StartCoroutine(SmoothScrollToPosition(originalPosition));
    }
}