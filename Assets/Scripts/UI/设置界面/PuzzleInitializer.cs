using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleInitializer : MonoBehaviour
{
    public Puzzle puzzle;
    [SerializeField] GameObject puzzlePiecePrefab;

    [SerializeField] int gridSize = 3; // 3x3分割

    void OnEnable(){
        //开始携程
        StartCoroutine(GeneratePuzzle());
    }

    void OnDisable(){
        StopAllCoroutines();
        DeleteAllPieces();
    }
    IEnumerator GeneratePuzzle(){
        //图片
        // 先等待 puzzle 本身不为 null
        yield return new WaitUntil(() => puzzle != null);

        // 再等待 puzzle.puzzlePhoto 不为 null
        yield return new WaitUntil(() => puzzle.puzzlePhoto != null);
        Sprite targetImage = puzzle.puzzlePhoto;//得到图片
        
        if (puzzle.puzzlePhoto != null) {


            // 切割图片
            List<Sprite> pieces = SliceImage(targetImage);

            // 生成拼图块
            for (int i = 0; i < pieces.Count; i++) {
                //在挂载这个脚本的物体内生成拼图块
                GameObject piece = Instantiate(puzzlePiecePrefab, transform);
                //得到一个拼图
                piece.GetComponent<Image>().sprite = pieces[i];
                //计算原始位置,添加脚本
                piece.AddComponent<PuzzlePiece>().correctPos = GetOriginalPosition(i);

                // 每帧生成一个防止卡顿
                yield return null;
            }

            // 打乱位置
            PuzzleManager.Instance.ShufflePieces();
        }
    }
    //清空列表
    public void DeleteAllPieces(){
        // 遍历所有子物体
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }
    }
    // 拼图切割器,把拼图切割到list里
    List<Sprite> SliceImage(Sprite source){
        List<Sprite> pieces = new List<Sprite>();

        // 参数配置
        int gridSize = 3; // 3x3网格
        Texture2D sourceTex = source.texture;

        // 计算单格尺寸
        int cellWidth = sourceTex.width / gridSize;
        int cellHeight = sourceTex.height / gridSize;

        // 切割处理
        for (int row = 0; row < gridSize; row++) {
            for (int col = 0; col < gridSize; col++) {
                // 创建临时纹理
                Texture2D cellTex = new Texture2D(cellWidth, cellHeight);

                // 计算像素区域
                int startX = col * cellWidth;
                int startY = row * cellHeight;

                // 提取像素数据（考虑左下角坐标系）
                Color[] pixels = sourceTex.GetPixels(
                    startX,
                    sourceTex.height - (startY + cellHeight), // Y轴反转
                    cellWidth,
                    cellHeight
                );

                cellTex.SetPixels(pixels);
                cellTex.Apply();

                // 创建Sprite
                Sprite pieceSprite = Sprite.Create(
                    cellTex,
                    new Rect(0, 0, cellTex.width, cellTex.height),
                    new Vector2(0.5f, 0.5f), // 中心锚点
                    100 // 像素单位
                );

                pieces.Add(pieceSprite);
            }
        }

        return pieces;
    }

// 位置计算器
    Vector2 GetOriginalPosition(int index){
        // 参数配置
        int gridSize = 3;
        float spacing = 1.2f; // 块间距

        // 计算行列
        int row = index / gridSize;
        int col = index % gridSize;

        // 计算偏移量（中心对齐）
        float totalWidth = gridSize * spacing;
        float startX = -(totalWidth / 2) + spacing / 2;
        float startY = totalWidth / 2 - spacing / 2;

        return new Vector2(
            startX + col * spacing,
            startY - row * spacing
        );
    }

}