using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class automoveBox : MonoBehaviour, INeilLDTkImportCompanion
{
    [Header("移动设置")]
    public Transform target;               // 被移动的子物体
    public Vector3 pointA = Vector3.zero;  // 相对起点
    public Vector3 pointB = new Vector3(0, 1, 0);  // 相对终点
    public float moveStanbyDuration = .3f;
    public float moveDuration = 1f;        // 移动时间 x 秒
    public float waitDuration = 0.5f;      // 停顿时间 y 秒
    public AnimationCurve moveCurve;       // 运动曲线
    public bool reverse;//如果为false，起点左下角，终点右上角；如果为true，起点右下角，终点左上角

    public PlayerController playerController;
    public BoxCollider2D targetCollider;

    //自动导入关卡设定数据
    public void OnAfterImport(SwitchableObj father, LDtkFields fields)
    {
        reverse = fields.GetBool("Reverse");
        float xLength = transform.localScale.x;
        float yLength = transform.localScale.y;
        transform.localScale = Vector3.one;
        if (xLength == 1)
        {
            pointA = new Vector3(0, -0.5f * (yLength*3-3), 0);
            pointB = new Vector3(0, 0.5f * (yLength*3-3), 0);
            father.ChangeExpectedSize(3, Mathf.RoundToInt(yLength * 3));
            father.SpecialEdgeChecker.transform.localScale = new Vector3(3, Mathf.RoundToInt(yLength * 3), 1);
        }
        else
        {
            pointA = new Vector3(-0.5f*(xLength*3-3), 0, 0);
            pointB = new Vector3(0.5f*(xLength*3-3), 0, 0);
            father.ChangeExpectedSize(Mathf.RoundToInt(xLength * 3), 3);
            father.SpecialEdgeChecker.transform.localScale = new Vector3(Mathf.RoundToInt(xLength * 3), 3, 1);
        }

        target.localPosition = !reverse ? pointA : pointB;
        //father.ChangeExpectedSize(Mathf.RoundToInt(xLength*3),Mathf.RoundToInt(yLength*3));
        father.GetRenderer().enabled = false;
        father.IfSpecialEdgeChecker = true;
        

    }

    private void Start()
    {
        
        if (target == null)
        {
            Debug.LogError("请指定要移动的子物体！");
            return;
        }
        playerController = FindObjectOfType<PlayerController>();
        target.localPosition = !reverse ? pointA : pointB;

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
        float time = -moveStanbyDuration;
        while (time < moveDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / moveDuration);
            float curvedT = moveCurve.Evaluate(t);
            MoveStep(Vector3.Lerp(start, end, curvedT) - target.localPosition);
            
            yield return null;
        }

        // 确保最终精确到达
        MoveStep(end - target.localPosition);
    }

    public void MoveStep(Vector2 step)
    {
        float CheckOffset = 0.03f;
        float leftCheckOffset = step.x < 0 ? -CheckOffset : 0;
        float rightCheckOffset = step.x > 0 ? CheckOffset : 0;
        float upCheckOffset = CheckOffset;
        float downCheckOffset = step.y < 0 ? -CheckOffset : 0;
        if (playerController.CollideCheck(new Rect((Vector2)target.transform.position + targetCollider.offset - targetCollider.size * 0.5f, targetCollider.size + new Vector2(0, upCheckOffset))) && downCheckOffset != 0)
        {
            playerController.MovePosition(playerController.Position + step);
        }
        else if (playerController.CollideCheck(new Rect((Vector2)target.transform.position + targetCollider.offset - targetCollider.size * 0.5f + new Vector2(leftCheckOffset, downCheckOffset), targetCollider.size + new Vector2(rightCheckOffset - leftCheckOffset, upCheckOffset - downCheckOffset))))
        {
            playerController.AdjustPosition(step);
        }
        target.localPosition += (Vector3)step;
        if (playerController.CollideCheck(new Rect((Vector2)target.transform.position + targetCollider.offset - targetCollider.size * 0.5f, targetCollider.size)))
        {
            PlayerDeathEvent.Trigger(gameObject, DeathType.Squish);
        }
    }
}


public interface INeilLDTkImportCompanion
{
    void OnAfterImport(SwitchableObj father, LDtkFields fields);
}
