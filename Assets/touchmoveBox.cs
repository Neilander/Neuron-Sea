using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchmoveBox : MonoBehaviour, INeilLDTkImportCompanion
{
    [Header("移动设置")]
    public Transform target;
    public Vector3 pointA = Vector3.zero;
    public Vector3 pointB = new Vector3(0, 1, 0);
    public float moveStanbyDuration = .3f;
    public float moveDuration = 1f;
    public float cooldownDuration = 1f;
    public AnimationCurve moveCurve;
    public bool reverse;//如果为false，起点左下角，终点右上角；如果为true，起点右下角，终点左上角

    private bool isMoving = false;
    private bool atA; // 当前是否在A点（决定下次去哪）

    public PlayerController playerController;
    public BoxCollider2D targetCollider;

    //自动导入关卡设定数据
    public void OnAfterImport(SwitchableObj father, LDtkFields fields)
    {
        reverse = fields.GetBool("Reverse");
        Debug.Log(gameObject.name +"获取的reverse是"+reverse);
        float xLength = transform.localScale.x;
        float yLength = transform.localScale.y;
        transform.localScale = Vector3.one;
        if (xLength == 1)
        {
            pointA = new Vector3(0, -0.5f * (yLength * 3 - 3), 0);
            pointB = new Vector3(0, 0.5f * (yLength * 3 - 3), 0);
            father.ChangeExpectedSize(3, Mathf.RoundToInt(yLength * 3));
            father.SpecialEdgeChecker.transform.localScale = new Vector3(3, Mathf.RoundToInt(yLength * 3), 1);
        }
        else
        {
            pointA = new Vector3(-0.5f * (xLength * 3 - 3), 0, 0);
            pointB = new Vector3(0.5f * (xLength * 3 - 3), 0, 0);
            father.ChangeExpectedSize(Mathf.RoundToInt(xLength * 3), 3);
            father.SpecialEdgeChecker.transform.localScale = new Vector3(Mathf.RoundToInt(xLength * 3), 3, 1);
        }

        target.localPosition = reverse ? pointA : pointB;
        atA = reverse;
        //father.ChangeExpectedSize(Mathf.RoundToInt(xLength*3),Mathf.RoundToInt(yLength*3));
        father.GetRenderer().enabled = false;
        father.IfSpecialEdgeChecker = true;
    }

    
    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("未指定 target 子物体！");
            return;
        }

        atA = reverse;
        Debug.Log(gameObject.name+"的atA是"+atA);
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
        Debug.Log(gameObject.name+"被触发移动，初始的atA状态是"+atA);
        Vector3 start = atA ? pointA : pointB;
        Vector3 end = atA ? pointB : pointA;
        
        Vector2 prevPos;
        float time = -moveStanbyDuration;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float curvedT = moveCurve.Evaluate(t);
            prevPos = target.localPosition;
            float CheckOffset = 0.03f;
            float leftCheckOffset = (end - start).x < 0 ? -CheckOffset : 0;
            float rightCheckOffset = (end - start).x > 0 ? CheckOffset : 0;
            float upCheckOffset = CheckOffset;
            float downCheckOffset = (end - start).y < 0 ? -CheckOffset : 0;
            if (playerController.CollideCheck(new Rect((Vector2)target.transform.position + targetCollider.offset - targetCollider.size * 0.5f + new Vector2(leftCheckOffset, downCheckOffset), targetCollider.size + new Vector2(rightCheckOffset - leftCheckOffset, upCheckOffset - downCheckOffset))))
            {
                playerController.MovePosition(playerController.Position + (Vector2)Vector3.Lerp(start, end, curvedT) - prevPos);
            }
            target.localPosition = Vector3.Lerp(start, end, curvedT);
            yield return null;
        }

        prevPos = target.localPosition;
        target.localPosition = end;
        playerController.MovePosition(playerController.Position + (Vector2)target.localPosition - prevPos);

        atA = !atA;

        yield return new WaitForSeconds(cooldownDuration);

        isMoving = false;
    }
}
