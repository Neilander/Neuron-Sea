using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchmoveBox : MonoBehaviour
{
    [Header("移动设置")]
    public Transform target;
    public Vector3 pointA = Vector3.zero;
    public Vector3 pointB = new Vector3(0, 1, 0);
    public float moveDuration = 1f;
    public float cooldownDuration = 1f;
    public AnimationCurve moveCurve;

    private bool isMoving = false;
    private bool atA = true; // 当前是否在A点（决定下次去哪）

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("未指定 target 子物体！");
            return;
        }

        target.localPosition = pointA;
        atA = true;
    }

    public bool TriggerMove()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveOnce());
            return true;
        }
        else
            return false;
    }

    private IEnumerator MoveOnce()
    {
        isMoving = true;

        Vector3 from = atA ? pointA : pointB;
        Vector3 to = atA ? pointB : pointA;

        float time = 0f;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float curvedT = moveCurve.Evaluate(t);
            target.localPosition = Vector3.Lerp(from, to, curvedT);
            yield return null;
        }

        target.localPosition = to;
        atA = !atA;

        yield return new WaitForSeconds(cooldownDuration);

        isMoving = false;
    }
}
