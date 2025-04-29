using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理整个拼图游戏的逻辑，包括选择、高亮、位置提示、洗牌、检测胜利等
/// </summary>
public class PuzzleManager : MonoBehaviour
{
    // 单例模式，方便全局访问
    public static PuzzleManager Instance;

    public Transform dragLayer; // 在 Inspector 里手动指定一个空 GameObject，放在 Canvas 下
    // 所有拼图块的列表（需在Inspector中赋值或动态添加）
    public List<PuzzlePiece> allPieces = new List<PuzzlePiece>();

    // 当前选中的拼图块
    private PuzzlePiece selectedPiece;

    // 位置提示用的预制体（通常为一个UI图形，如半透明框）
    public GameObject positionIndicatorPrefab;

    // 拼图块选中时的高亮颜色
    public Color highlightColor = new Color(1, 0.8f, 0, 0.3f);

    // 当前正在显示的提示物体
    private GameObject currentIndicator;

    // UI Canvas，用于挂载提示UI元素
    private Canvas canvas;

    /// <summary>
    /// 初始化单例和Canvas引用
    /// </summary>
    void Awake()
    {
        Instance = this;
        canvas = GetComponentInParent<Canvas>(); // 确保挂载在Canvas下
    }

    /// <summary>
    /// 被PuzzlePiece调用，处理拼图块点击后的选择逻辑
    /// </summary>
    public void SelectPiece(PuzzlePiece piece){
        Debug.Log($"选择拼图块：{piece.name}");

        // 若已有拼图块被选中，先取消其选中状态
        if (selectedPiece != null) {
            DeselectPiece(selectedPiece);
        }

        // 设置当前为新选中的拼图块
        selectedPiece = piece;

        // 显示高亮效果
        ShowSelectionHighlight(piece, true);

        // 显示该块应放置的位置提示
        ShowCorrectPosition(piece);
    }

    /// <summary>
    /// 取消拼图块的选中状态和高亮效果
    /// </summary>
    void DeselectPiece(PuzzlePiece piece){
        ShowSelectionHighlight(piece, false);

        // 若取消的是当前选中的块，清空引用
        if (piece == selectedPiece) {
            selectedPiece = null;
        }

        // 隐藏位置提示
        HideCorrectPosition();
    }

    /// <summary>
    /// 显示或隐藏拼图块的高亮效果
    /// </summary>
    void ShowSelectionHighlight(PuzzlePiece piece, bool show){
        // 方法一：通过修改颜色高亮（简单粗暴）
        piece.transform.GetComponent<Image>().color = show ? highlightColor : Color.white;

        // 方法二：启用拼图块子物体中的高亮边框（推荐更灵活）
        // piece.highlightBorder.SetActive(show);
    }

    /// <summary>
    /// 在拼图块的正确位置显示提示指示器
    /// </summary>
    void ShowCorrectPosition(PuzzlePiece piece){
        // 若已有指示器，先销毁
        if (currentIndicator != null) {
            Destroy(currentIndicator);
        }

        // 实例化一个新的提示器，挂在Canvas下
        currentIndicator = Instantiate(positionIndicatorPrefab, canvas.transform);

        // 设置其位置为拼图块的目标位置
        RectTransform indicatorRT = currentIndicator.GetComponent<RectTransform>();
        indicatorRT.anchoredPosition = piece.correctPos;

        // 自动隐藏提示器的协程
        StartCoroutine(AutoHideIndicator());
    }

    /// <summary>
    /// 两秒后自动隐藏提示器
    /// </summary>
    IEnumerator AutoHideIndicator(){
        yield return new WaitForSeconds(2f);
        HideCorrectPosition();
    }

    /// <summary>
    /// 隐藏当前提示器（如果存在）
    /// </summary>
    void HideCorrectPosition(){
        if (currentIndicator != null) {
            Destroy(currentIndicator);
            currentIndicator = null;
        }
    }

    /// <summary>
    /// 随机打乱所有拼图块的位置（使用 UI 坐标）
    /// </summary>
    public void ShufflePieces(){
        Debug.Log("开始洗牌拼图块位置...");

        if (allPieces == null || allPieces.Count == 0) {
            Debug.LogWarning("没有拼图块可供洗牌");
            return;
        }

        // 获取拼图容器
        RectTransform container = allPieces[0].transform.parent as RectTransform;
        if (container == null) {
            Debug.LogError("拼图块没有有效的父容器 RectTransform");
            return;
        }

        Vector2 containerSize = container.rect.size;
        float xMin = -containerSize.x / 2 + 50;
        float xMax = containerSize.x / 2 - 50;
        float yMin = -containerSize.y / 2 + 50;
        float yMax = containerSize.y / 2 - 50;

        foreach (var piece in allPieces) {
            piece.isCorrect = false;

            // 使用 anchoredPosition 在 UI 区域内打乱位置
            RectTransform rt = piece.GetComponent<RectTransform>();
            Vector2 randomPos = new Vector2(
                Random.Range(xMin, xMax),
                Random.Range(yMin, yMax)
            );

            rt.anchoredPosition = randomPos;
            Debug.Log($"拼图块 {piece.name} 洗牌后 UI 坐标：{randomPos}");
        }
    }

    /// <summary>
    /// 检查某个拼图块是否已正确放置，并尝试吸附到目标位置
    /// </summary>
    public void CheckPiece(PuzzlePiece piece)
    {
        float distance = Vector2.Distance(piece.transform.position, piece.correctPos);
        Debug.Log($"拼图块 {piece.name} 与目标距离: {distance}");
        // 若与目标位置足够接近，则吸附到位并标记为正确
        if (distance < 0.5f)
        {
            Debug.Log($"拼图块 {piece.name} 自动吸附到正确位置！");
            piece.transform.position = piece.correctPos;
            piece.isCorrect = true;

            // 检查是否已完成所有拼图
            CheckWinCondition();
        }
        else {
            Debug.Log($"拼图块 {piece.name} 未达到吸附距离，保持当前位置。");
        }
    }

    /// <summary>
    /// 检查所有拼图块是否都处于正确位置，若是则宣布胜利
    /// </summary>
    public void CheckWinCondition()
    {
        foreach (var piece in allPieces)
        {
            if (!piece.isCorrect) {
                Debug.Log($"尚未完成：{piece.name} 未放对。");
                return;
            }
        }

        // 所有拼图块都正确后执行胜利逻辑
        Debug.Log("所有拼图块都放置正确！游戏胜利！");
        // 可在此调用 ShowWinUI() 显示胜利界面
    }

    /// <summary>
    /// 重置拼图状态并重新打乱位置
    /// </summary>
    public void ResetPuzzle()
    {
        Debug.Log("重置拼图...");
        foreach (var piece in allPieces)
        {
            piece.isCorrect = false;
        }
        ShufflePieces();
    }

    

}
