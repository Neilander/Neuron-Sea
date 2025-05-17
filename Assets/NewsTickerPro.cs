using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class NewsTickerPro : MonoBehaviour
{
    // 基础配置
    public float scrollSpeed = 100f;
    public TextMeshProUGUI newsPrefab;
    public RectTransform containerRect;
    public List<string> newsList = new List<string>();
    public float spacing = 20f;

    // 运行时变量
    private List<TextMeshProUGUI> activeItems = new List<TextMeshProUGUI>();
    private Queue<string> newsQueue = new Queue<string>();
    private float containerWidth;
    private RectTransform tickerRect;
    private float totalGeneratedWidth;

    void Start()
    {
        // 初始化组件
        tickerRect = GetComponent<RectTransform>();
        containerWidth = containerRect.rect.width;

        // 设置锚点保证正确坐标系
        tickerRect.anchorMin = new Vector2(0, 0.5f);
        tickerRect.anchorMax = new Vector2(0, 0.5f);
        tickerRect.pivot = new Vector2(0, 0.5f);

        // 初始化内容
        ShuffleNews();
        GenerateInitialContent();
        ResetInitialPosition();
    }

    void ShuffleNews()
    {
        // Fisher-Yates洗牌算法
        for (int i = newsList.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (newsList[i], newsList[j]) = (newsList[j], newsList[i]);
        }
        newsList.ForEach(n => newsQueue.Enqueue(n));
    }

    void GenerateInitialContent()
    {
        float currentX = 0;
        float minWidth = containerWidth * 2;

        while (currentX < minWidth || activeItems.Count < 2)
        {
            var item = CreateNewsItem(newsQueue.Dequeue());
            activeItems.Add(item);

            // 显式设置位置（关键修改点）
            item.rectTransform.anchoredPosition = new Vector2(currentX, 0);

            // 累计宽度时包含间距
            currentX += item.preferredWidth + spacing;

            newsQueue.Enqueue(item.text);
        }
    }

    TextMeshProUGUI CreateNewsItem(string content)
    {
        var item = Instantiate(newsPrefab, transform);
        item.text = content;

        // 三阶段强制更新布局
        item.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(item.rectTransform);
        Canvas.ForceUpdateCanvases();

        return item;
    }

    void ResetInitialPosition()
    {
        // 初始定位到容器右边界外
        tickerRect.anchoredPosition = new Vector2(containerWidth, 0);
    }

    void Update()
    {
        // 向左移动
        tickerRect.anchoredPosition += Vector2.left * scrollSpeed * Time.unscaledDeltaTime;
        CheckRecycleConditions();
    }

    void CheckRecycleConditions()
    {
        // 可视范围计算（基于容器坐标系）
        float currentPosition = -tickerRect.anchoredPosition.x;
        float visibleStart = currentPosition;
        float visibleEnd = currentPosition + containerWidth;

        // 回收左侧超出元素
        while (activeItems.Count > 0)
        {
            var firstItem = activeItems[0];
            float itemEnd = GetItemPosition(firstItem) + GetItemWidth(firstItem);

            if (itemEnd < visibleStart)
            {
                RecycleItem(firstItem);
                activeItems.RemoveAt(0);
            }
            else
            {
                break;
            }
        }

        // 补充右侧元素
        if (activeItems.Count > 0)
        {
            var lastItem = activeItems[activeItems.Count - 1];
            float lastItemEnd = GetItemPosition(lastItem) + GetItemWidth(lastItem);

            while (lastItemEnd < visibleEnd + containerWidth)
            {
                var newItem = AddNewItem(lastItemEnd);
                lastItemEnd = GetItemPosition(newItem) + GetItemWidth(newItem);
            }
        }
    }

    TextMeshProUGUI AddNewItem(float startX)
    {
        var item = CreateNewsItem(newsQueue.Dequeue());

        // 新位置 = 前一个元素结束位置 + 间距
        float newX = startX + spacing;
        item.rectTransform.anchoredPosition = new Vector2(newX, 0);

        activeItems.Add(item);
        newsQueue.Enqueue(item.text);

        return item;
    }

    void RecycleItem(TextMeshProUGUI item)
    {
        Destroy(item.gameObject);
        // 若使用对象池：item.gameObject.SetActive(false);
    }

    // 辅助方法
    float GetItemWidth(TextMeshProUGUI item) => item.preferredWidth;
    float GetItemPosition(TextMeshProUGUI item) => item.rectTransform.anchoredPosition.x;
}