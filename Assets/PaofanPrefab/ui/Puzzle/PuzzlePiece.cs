using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;

    private int originalSiblingIndex;

    public Vector2 correctPos; // 正确位置
    public bool isCorrect = false; // 是否正确放置
    public GameObject highlightBorder; // 可选：高亮边框对象
    public Image image; // UI Image 组件

    [HideInInspector] public RectTransform rectTransform;

    private Vector2 offset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        GetComponent<CanvasGroup>().blocksRaycasts = true;
        Debug.Log($"[PuzzlePiece] 初始化完成，目标位置为：{correctPos}");
    }

    public bool IsInCorrectPosition(){
        Vector3 worldCorrectPos = rectTransform.parent.TransformPoint(correctPos);
        return Vector3.Distance(rectTransform.position, worldCorrectPos) < 5f;
    }


    public void OnBeginDrag(PointerEventData eventData){
        if (isCorrect) return;

        // 记录原始父级和顺序
        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();

        // 提升到 Canvas 下（或某个专用容器）
        transform.SetParent(PuzzleManager.Instance.dragLayer); // 需要事先在 PuzzleManager 中设置 dragLayer

        // 偏移计算
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out offset);
        offset = rectTransform.anchoredPosition - offset;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (isCorrect) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint + offset;
            Debug.Log($"[PuzzlePiece] 拖动中，当前位置：{rectTransform.anchoredPosition}");
        }
    }

    public void OnEndDrag(PointerEventData eventData){
        Vector3 worldCorrectPos = rectTransform.parent.TransformPoint(correctPos);

        if (Vector3.Distance(rectTransform.position, worldCorrectPos) < 30f) // 30像素吸附范围
        {
            rectTransform.position = worldCorrectPos;
            isCorrect = true;
            PuzzleManager.Instance.CheckWinCondition();
        }
    }


}
