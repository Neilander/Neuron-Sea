using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 拼图初始化器：用于切割图片、生成拼图块、设置正确位置和交互逻辑
/// </summary>
public class PuzzleInitializer : MonoBehaviour
{
    // 关联的 Puzzle 数据对象（包含原始图片等信息）
    public Puzzle puzzle;

    // 拼图块的预制体（UI对象）
    [SerializeField] GameObject puzzlePiecePrefab;

    // 拼图网格的大小，例如3代表3x3
    [SerializeField] int gridSize = 3;

    // 每个拼图块的尺寸（UI单位）
    [SerializeField] Vector2 pieceSize = new Vector2(200, 200);

    // 拼图块之间的间距
    [SerializeField] Vector2 spacing = new Vector2(10, 10);

    // 在脚本启用时自动开始生成拼图
    void OnEnable(){
        DeleteAllPieces();
        StartCoroutine(GeneratePuzzle());
    }

    // 在脚本禁用时清除所有拼图块
    void OnDisable() => DeleteAllPieces();

    /// <summary>
    /// 主生成流程：等待图片加载完成后，切割图片并创建拼图块
    /// </summary>
    IEnumerator GeneratePuzzle()
    {
        // 等待直到 puzzle 和图片都已准备好
        yield return new WaitUntil(() => puzzle != null && puzzle.puzzlePhoto != null);

        Sprite targetImage = puzzle.puzzlePhoto;

        // 切割原始图片成若干小块
        List<Sprite> pieces = SliceImage(targetImage);
    
        // 创建一个UI容器用于放置所有拼图块
        RectTransform container = CreateLayoutContainer();

        // 遍历每一块图片，生成拼图块对象并配置其功能
        for (int i = 0; i < pieces.Count; i++)
        {
            GameObject piece = CreateUIPiece(pieces[i], container, i);
            SetupPieceComponents(piece, i);
            yield return null; // 等待一帧，避免卡顿
        }

        // 随机打乱拼图块的位置
        PuzzleManager.Instance.ShufflePieces();
    }

    /// <summary>
    /// 创建拼图容器（挂在当前脚本对象下）
    /// </summary>
    RectTransform CreateLayoutContainer()
    {
        GameObject container = new GameObject("PuzzleContainer");
        RectTransform rt = container.AddComponent<RectTransform>();
        rt.SetParent(transform);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
        return rt;
    }
    
    /// <summary>
    /// 创建一个拼图块的UI对象，设置图片和初始位置
    /// </summary>
    GameObject CreateUIPiece(Sprite sprite, RectTransform parent, int index)
    {
        GameObject piece = Instantiate(puzzlePiecePrefab, parent);
        Image img = piece.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;

        RectTransform rt = piece.GetComponent<RectTransform>();
        rt.sizeDelta = pieceSize;
        rt.anchoredPosition = GetOriginalPosition(index); // 设置正确位置
        return piece;
    }

    /// <summary>
    /// 设置拼图块功能：位置记录与点击交互
    /// </summary>
    void SetupPieceComponents(GameObject piece, int index)
    {
        // 添加自定义脚本，记录正确位置
        PuzzlePiece puzzlePiece = piece.AddComponent<PuzzlePiece>();
        puzzlePiece.correctPos = GetOriginalPosition(index);
        PuzzleManager.Instance.allPieces.Add(puzzlePiece);
        // 添加点击事件监听器
        EventTrigger trigger = piece.AddComponent<EventTrigger>();
        AddTriggerEvent(trigger, EventTriggerType.PointerClick, () => 
            PuzzleManager.Instance.SelectPiece(puzzlePiece));
    }

    /// <summary>
    /// 封装的添加UI事件的方法
    /// </summary>
    void AddTriggerEvent(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = type
        };
        entry.callback.AddListener((data) => action());
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// 根据拼图索引计算其在UI中的正确位置（居中布局）
    /// </summary>
    Vector2 GetOriginalPosition(int index)
    {
        int row = index / gridSize;
        int col = index % gridSize;

        // 左上角为起始点，整体向中心偏移
        float startX = -(gridSize * (pieceSize.x + spacing.x)) / 2 + pieceSize.x / 2;
        float startY = (gridSize * (pieceSize.y + spacing.y)) / 2 - pieceSize.y / 2;

        return new Vector2(
            startX + col * (pieceSize.x + spacing.x),
            startY - row * (pieceSize.y + spacing.y)
        );
    }

    /// <summary>
    /// 将一张Sprite图片按网格切割为若干小块
    /// </summary>
    List<Sprite> SliceImage(Sprite source)
    {
        List<Sprite> pieces = new List<Sprite>();
        Texture2D sourceTex = source.texture;

        int cellWidth = sourceTex.width / gridSize;
        int cellHeight = sourceTex.height / gridSize;

        for (int row = 0; row < gridSize; row++)
        {
            for (int col = 0; col < gridSize; col++)
            {
                // 注意Y轴坐标从顶部开始
                Rect rect = new Rect(
                    col * cellWidth,
                    sourceTex.height - (row + 1) * cellHeight,
                    cellWidth,
                    cellHeight
                );

                // 创建切割后的子Sprite
                Sprite pieceSprite = Sprite.Create(
                    sourceTex,
                    rect,
                    new Vector2(0.5f, 0.5f), // 中心点
                    100,  // Pixels Per Unit
                    0,
                    SpriteMeshType.FullRect
                );

                pieces.Add(pieceSprite);
            }
        }
        return pieces;
    }

    /// <summary>
    /// 删除生成的拼图块（清空PuzzleContainer）
    /// </summary>
    public void DeleteAllPieces()
    {
        foreach (Transform child in transform)
        {
            if (child.name == "PuzzleContainer")
            {
                Destroy(child.gameObject);
                break;
            }
        }
        // 清空PuzzleManager里的缓存列表
        if (PuzzleManager.Instance != null) {
            PuzzleManager.Instance.allPieces.Clear();
        }
    }
}