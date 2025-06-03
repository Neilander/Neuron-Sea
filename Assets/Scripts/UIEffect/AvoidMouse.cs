using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AvoidMouse : MonoBehaviour
{
    [Header("躲避设置")]
    [SerializeField] private float avoidRadius = 100f;
    [SerializeField] private float moveSpeed = 500f;
    [SerializeField] private float edgePadding = 20f;
    
    [Header("复位设置")]
    [SerializeField] private float maxMoveDistance = 12f; // 新增：最大移动距离
    [SerializeField] private float returnSpeed = 8f;      // 新增：复位速度

    private RectTransform rectTransform;
    private Canvas parentCanvas;
    private Vector2 targetPosition;
    private Vector2 originalPosition; // 新增：记录初始位置

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
        originalPosition = rectTransform.anchoredPosition; // 记录原始位置
        targetPosition = originalPosition;
    }

    void Update()
    {
        // 获取鼠标位置（逻辑不变）
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            Input.mousePosition,
            parentCanvas.worldCamera,
            out mousePos);

        Vector2 uiPosition = rectTransform.anchoredPosition;
        float distanceToMouse = Vector2.Distance(mousePos, uiPosition);

        // 躲避逻辑
        if (distanceToMouse < avoidRadius)
        {
            Vector2 dirToMouse = (mousePos - uiPosition).normalized;
            Vector2 avoidDirection = -dirToMouse;
            
            // 计算新目标位置
            Vector2 newTarget = uiPosition + avoidDirection * moveSpeed * Time.unscaledDeltaTime;
            
            // 新增：限制最大移动距离
            if (Vector2.Distance(newTarget, originalPosition) <= maxMoveDistance)
            {
                targetPosition = newTarget;
            }
        }
        else
        {
            // 修改：始终以原始位置为复位目标
            targetPosition = originalPosition;
        }

        // 边界限制
        targetPosition = ClampToCanvas(targetPosition);
        
        // 修改：使用更平滑的复位方式
        rectTransform.anchoredPosition = Vector2.Lerp(
            rectTransform.anchoredPosition,
            targetPosition,
            Time.unscaledDeltaTime* (distanceToMouse < avoidRadius ? 10f : returnSpeed)); // 根据状态切换速度
    }

    // 修改后的边界限制方法
    private Vector2 ClampToCanvas(Vector2 targetPos)
    {
        Rect canvasRect = parentCanvas.GetComponent<RectTransform>().rect;
        Vector2 canvasSize = new Vector2(canvasRect.width, canvasRect.height) / 2;
        canvasSize -= new Vector2(edgePadding, edgePadding);

        // 新增：限制与原始位置的最大距离
        Vector2 clampedPos = new Vector2(
            Mathf.Clamp(targetPos.x, originalPosition.x - maxMoveDistance, originalPosition.x + maxMoveDistance),
            Mathf.Clamp(targetPos.y, originalPosition.y - maxMoveDistance, originalPosition.y + maxMoveDistance));

        // 保留原有屏幕边缘限制
        clampedPos.x = Mathf.Clamp(clampedPos.x, -canvasSize.x, canvasSize.x);
        clampedPos.y = Mathf.Clamp(clampedPos.y, -canvasSize.y, canvasSize.y);

        return clampedPos;
    }
}