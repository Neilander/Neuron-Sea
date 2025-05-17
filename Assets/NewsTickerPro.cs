using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class NewsTickerPro : MonoBehaviour
{
    // ��������
    public float scrollSpeed = 100f;
    public TextMeshProUGUI newsPrefab;
    public RectTransform containerRect;
    public List<string> newsList = new List<string>();
    public float spacing = 20f;

    // ����ʱ����
    private List<TextMeshProUGUI> activeItems = new List<TextMeshProUGUI>();
    private Queue<string> newsQueue = new Queue<string>();
    private float containerWidth;
    private RectTransform tickerRect;
    private float totalGeneratedWidth;

    void Start()
    {
        // ��ʼ�����
        tickerRect = GetComponent<RectTransform>();
        containerWidth = containerRect.rect.width;

        // ����ê�㱣֤��ȷ����ϵ
        tickerRect.anchorMin = new Vector2(0, 0.5f);
        tickerRect.anchorMax = new Vector2(0, 0.5f);
        tickerRect.pivot = new Vector2(0, 0.5f);

        // ��ʼ������
        ShuffleNews();
        GenerateInitialContent();
        ResetInitialPosition();
    }

    void ShuffleNews()
    {
        // Fisher-Yatesϴ���㷨
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

            // ��ʽ����λ�ã��ؼ��޸ĵ㣩
            item.rectTransform.anchoredPosition = new Vector2(currentX, 0);

            // �ۼƿ��ʱ�������
            currentX += item.preferredWidth + spacing;

            newsQueue.Enqueue(item.text);
        }
    }

    TextMeshProUGUI CreateNewsItem(string content)
    {
        var item = Instantiate(newsPrefab, transform);
        item.text = content;

        // ���׶�ǿ�Ƹ��²���
        item.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(item.rectTransform);
        Canvas.ForceUpdateCanvases();

        return item;
    }

    void ResetInitialPosition()
    {
        // ��ʼ��λ�������ұ߽���
        tickerRect.anchoredPosition = new Vector2(containerWidth, 0);
    }

    void Update()
    {
        // �����ƶ�
        tickerRect.anchoredPosition += Vector2.left * scrollSpeed * Time.unscaledDeltaTime;
        CheckRecycleConditions();
    }

    void CheckRecycleConditions()
    {
        // ���ӷ�Χ���㣨������������ϵ��
        float currentPosition = -tickerRect.anchoredPosition.x;
        float visibleStart = currentPosition;
        float visibleEnd = currentPosition + containerWidth;

        // ������೬��Ԫ��
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

        // �����Ҳ�Ԫ��
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

        // ��λ�� = ǰһ��Ԫ�ؽ���λ�� + ���
        float newX = startX + spacing;
        item.rectTransform.anchoredPosition = new Vector2(newX, 0);

        activeItems.Add(item);
        newsQueue.Enqueue(item.text);

        return item;
    }

    void RecycleItem(TextMeshProUGUI item)
    {
        Destroy(item.gameObject);
        // ��ʹ�ö���أ�item.gameObject.SetActive(false);
    }

    // ��������
    float GetItemWidth(TextMeshProUGUI item) => item.preferredWidth;
    float GetItemPosition(TextMeshProUGUI item) => item.rectTransform.anchoredPosition.x;
}