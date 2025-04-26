using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class automoveBox : MonoBehaviour, ILDtkImportedFields
{
    [Header("移动设置")]
    public Transform target;               // 被移动的子物体
    public Vector3 pointA = Vector3.zero;  // 相对起点
    public Vector3 pointB = new Vector3(0, 1, 0);  // 相对终点
    public float moveStanbyDuration = .3f;
    public float moveDuration = 1f;        // 移动时间 x 秒
    public float waitDuration = 0.5f;      // 停顿时间 y 秒
    public AnimationCurve moveCurve;       // 运动曲线

    public PlayerController playerController;
    public BoxCollider2D targetCollider;

    private void Start()
    {
        
        if (target == null)
        {
            Debug.LogError("请指定要移动的子物体！");
            return;
        }
        playerController = FindObjectOfType<PlayerController>();

        StartCoroutine(MoveLoop());
    }

    private IEnumerator MoveLoop()
    {
        while (true)
        {
            yield return StartCoroutine(MoveFromTo(pointA, pointB));
            yield return new WaitForSeconds(waitDuration);
            yield return StartCoroutine(MoveFromTo(pointB, pointA));
            yield return new WaitForSeconds(waitDuration);
        }
    }

    private IEnumerator MoveFromTo(Vector3 start, Vector3 end)
    {
        Vector2 prevPos;
        float time = -moveStanbyDuration;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float curvedT = moveCurve.Evaluate(t);
            prevPos = target.localPosition;
            target.localPosition = Vector3.Lerp(start, end, curvedT);
            if (playerController.CollideCheck(new Rect((Vector2)target.transform.position + targetCollider.offset - targetCollider.size * 0.5f - 0.02f * Vector2.one, targetCollider.size + 0.04f * Vector2.one)))
            {
                playerController.MovePosition(playerController.Position + (Vector2)target.localPosition - prevPos);
            }
            yield return null;
        }

        // 确保最终精确到达
        prevPos = target.localPosition;
        target.localPosition = end;
        playerController.MovePosition(playerController.Position + (Vector2)target.localPosition - prevPos);
    }

    public void OnLDtkImportFields(LDtkFields fields)
    {
        
        float xLength = transform.localScale.x;
        float yLength = transform.localScale.y;
        transform.localScale = Vector3.one;
        if (xLength == 1)
        {
            pointA = new Vector3(0, -0.5f * (yLength*3-3), 0);
            pointB = new Vector3(0, 0.5f * (yLength*3-3), 0);
        }
        else
        {
            pointA = new Vector3(-0.5f*(xLength*3-3), 0, 0);
            pointB = new Vector3(0.5f*(xLength*3-3), 0, 0);
        }

        target.localPosition = fields.GetBool("Reverse") ? pointA : pointB;
    }
}
