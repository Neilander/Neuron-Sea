using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class automoveBox : MonoBehaviour
{
    [Header("移动设置")]
    public Transform target;               // 被移动的子物体
    public Vector3 pointA = Vector3.zero;  // 相对起点
    public Vector3 pointB = new Vector3(0, 1, 0);  // 相对终点
    public float moveDuration = 1f;        // 移动时间 x 秒
    public float waitDuration = 0.5f;      // 停顿时间 y 秒
    public AnimationCurve moveCurve;       // 运动曲线

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("请指定要移动的子物体！");
            return;
        }

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
        float time = 0f;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float curvedT = moveCurve.Evaluate(t);
            target.localPosition = Vector3.Lerp(start, end, curvedT);
            yield return null;
        }

        // 确保最终精确到达
        target.localPosition = end;
    }
}
