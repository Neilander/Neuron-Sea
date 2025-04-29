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

    [Header("生成轨道")]
    public GameObject trackPrefab;
    public SpriteMask mask;


    const float waitTime = 0.3f;
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
            GenerateTrack(yLength * 3, false);
        }
        else
        {
            pointA = new Vector3(-0.5f * (xLength * 3 - 3), 0, 0);
            pointB = new Vector3(0.5f * (xLength * 3 - 3), 0, 0);
            father.ChangeExpectedSize(Mathf.RoundToInt(xLength * 3), 3);
            father.SpecialEdgeChecker.transform.localScale = new Vector3(Mathf.RoundToInt(xLength * 3), 3, 1);
            GenerateTrack(xLength * 3, true);
        }

        target.localPosition = !reverse ? pointA : pointB;
        atA = !reverse;
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

        target.localPosition = !reverse ? pointA : pointB;
        atA = !reverse;
        Debug.Log(gameObject.name+"的atA是"+atA);
        playerController = FindObjectOfType<PlayerController>();
        PlayerDeathEvent.OnDeathTriggered += StopMove;
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
        yield return new WaitForSeconds(waitTime);
        
        Debug.Log(gameObject.name+"被触发移动，初始的atA状态是"+atA);
        Vector3 start = atA ? pointA : pointB;
        Vector3 end = atA ? pointB : pointA;
        
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

        atA = !atA;

        yield return new WaitForSeconds(cooldownDuration);

        isMoving = false;
    }
    public void MoveStep(Vector2 step)
    {
        float CheckOffset = 0.03f;
        float leftCheckOffset = step.x < 0 ? -CheckOffset + step.x * 4f : 0;
        float rightCheckOffset = step.x > 0 ? CheckOffset + step.x * 4f : 0;
        float upCheckOffset = step.y > 0 ? CheckOffset + step.y * 4f : CheckOffset;
        float downCheckOffset = step.y < 0 ? -CheckOffset + step.y * 4f : 0;
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

    private void OnDestroy()
    {
        PlayerDeathEvent.OnDeathTriggered -= StopMove;
    }

    public void StopMove(GameObject trigger)
    {
        StopAllCoroutines();
    }

    void GenerateTrack(float length, bool ifLeftRight)
    {
        int toGenerate = Mathf.CeilToInt(length / 3f);
        int LeftDownSide = toGenerate / 2;
        float startNum = (LeftDownSide - 1) * -3f - 1.5f - (toGenerate % 2) * 1.5f;
        for (int i = 0; i < toGenerate; i++)
        {
            GameObject gmo = Instantiate(trackPrefab, transform);


            gmo.transform.rotation = ifLeftRight ? Quaternion.Euler(0, 0, 90) : Quaternion.identity;
            gmo.transform.localPosition = new Vector3(ifLeftRight ? startNum + i * 3 : 0, ifLeftRight ? 0 : startNum + i * 3, 0);


        }
        mask.transform.localScale = new Vector3(ifLeftRight ? length : 1, ifLeftRight ? 1 : length, 1);

    }

}
