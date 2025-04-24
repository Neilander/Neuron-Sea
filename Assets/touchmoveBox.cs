using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchmoveBox : MonoBehaviour
{
    [Header("移动设置")]
    public Transform target;
    public Vector3 pointA = Vector3.zero;
    public Vector3 pointB = new Vector3(0, 1, 0);
    public float moveStanbyDuration = .3f;
    public float moveDuration = 1f;
    public float cooldownDuration = 1f;
    public AnimationCurve moveCurve;

    private bool isMoving = false;
    private bool atA = true; // 当前是否在A点（决定下次去哪）

    public PlayerController playerController;
    public BoxCollider2D targetCollider;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("未指定 target 子物体！");
            return;
        }

        target.localPosition = pointA;
        atA = true;

        playerController = FindObjectOfType<PlayerController>();
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

        Vector2 prevPos;
        float time = -moveStanbyDuration;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float curvedT = moveCurve.Evaluate(t);
            prevPos = target.localPosition;
            target.localPosition = Vector3.Lerp(from, to, curvedT);
            if (playerController.CollideCheck(new Rect((Vector2)target.transform.position + targetCollider.offset - targetCollider.size * 0.5f + 0.02f * Vector2.one, targetCollider.size + 0.04f * Vector2.one)))
            {
                playerController.MovePosition(playerController.Position + (Vector2)target.localPosition - prevPos);
            }
            yield return null;
        }

        prevPos = target.localPosition;
        target.localPosition = to;
        playerController.MovePosition(playerController.Position + (Vector2)target.localPosition - prevPos);

        atA = !atA;

        yield return new WaitForSeconds(cooldownDuration);

        isMoving = false;
    }
}
