using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
     public Vector2 correctPos;

    public bool isCorrect;

    private Vector3 offset;

    private bool isDragging;

    void OnMouseDown()
    {
        if (!isCorrect)
        {
            offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 newPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;
            newPos.z = 0;
            transform.position = newPos;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        PuzzleManager.Instance.CheckPiece(this);
    }
}
