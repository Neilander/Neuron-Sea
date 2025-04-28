using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;

    public List<PuzzlePiece> allPieces = new List<PuzzlePiece>();

    void Awake()
    {
        Instance = this;
    }

    public void ShufflePieces()
    {
        foreach (var piece in allPieces)
        {
            piece.isCorrect = false;
            Vector2 randomPos = new Vector2(
                Random.Range(-5f, 5f),
                Random.Range(-3f, 3f));
            piece.transform.position = randomPos;
        }
    }

    public void CheckPiece(PuzzlePiece piece)
    {
        float distance = Vector2.Distance(piece.transform.position, piece.correctPos);
        if (distance < 0.5f)
        {
            piece.transform.position = piece.correctPos;
            piece.isCorrect = true;
            CheckWinCondition();
        }
    }

    void CheckWinCondition()
    {
        foreach (var piece in allPieces)
        {
            if (!piece.isCorrect) return;
        }
        // 胜利逻辑
        Debug.Log("拼图完成！");
        // ShowWinUI();
    }

    public void ResetPuzzle()
    {
        foreach (var piece in allPieces)
        {
            piece.isCorrect = false;
        }
        ShufflePieces();
    }
}
