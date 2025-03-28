using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwitchState
{
    None,
    Switch,
    Move,
}

[ExecuteInEditMode] // 在编辑器中执行
public class GridManager : MonoBehaviour
{

    public static GridManager Instance;

    [Header("格子数据调整")]
    [SerializeField]
    private int gridWidth = 1;

    [SerializeField]
    [Range(0, 1)]
    private float offsetX = 0;
    [SerializeField]
    [Range(0, 1)]
    private float offsetY = 0;

    [SerializeField]
    private Vector2 displayAmount = new Vector2(10,10);

    [SerializeField]
    private bool displayInGizmos = true;
    [SerializeField]
    private GameObject displayCenter;

    private Counter counter = new Counter();
    //这部分是在编辑器中绘制网格
    private void OnDrawGizmos() {
        if(!displayInGizmos) return;
        if(displayCenter == null) return;

        // 设置网格线的颜色
        Gizmos.color = Color.red;
        
        // 获取displayCenter的位置
        Vector3 centerPos = displayCenter.transform.position;
        
        // 计算网格线的间距
        float gridSpacing = gridWidth;
        
        // 计算起始位置（向左和向下偏移5个格子）
        float startX = centerPos.x - (gridSpacing * displayAmount.x/2);
        float startY = centerPos.y - (gridSpacing * displayAmount.y/2);

        Vector3 displayCenterOffset = new Vector3(centerPos.x%gridWidth,centerPos.y%gridWidth,0);
        Vector3 inputOffset = new Vector3(offsetX,offsetY,0);
        
        // 绘制10x10的网格
        for(int i = 0; i <= displayAmount.x; i++) {
            // 绘制垂直线
            Vector3 startPos = new Vector3(startX + (gridSpacing * i), startY, 0)-displayCenterOffset+inputOffset;
            Vector3 endPos = new Vector3(startX + (gridSpacing * i), startY + (gridSpacing * displayAmount.y), 0)-displayCenterOffset+inputOffset;
            Gizmos.DrawLine(startPos, endPos);
        }

        for(int i = 0; i <= displayAmount.y; i++) 
        {
            // 绘制水平线
            Vector3 startPos = new Vector3(startX, startY + (gridSpacing * i), 0)-displayCenterOffset+inputOffset;
            Vector3 endPos = new Vector3(startX + (gridSpacing * displayAmount.x), startY + (gridSpacing * i), 0)-displayCenterOffset+inputOffset;
            Gizmos.DrawLine(startPos, endPos);

        }

    }

    private void OnEnable()
    {
        #if UNITY_EDITOR
        // 编辑器状态下（不是播放或将要播放）
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (Instance == null)
            {
                Instance = this;
            }
            return;
        }
        #endif


            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                DestroyImmediate(gameObject);
            }
    }
    

    [Header("测试用")]
    [SerializeField]
    private Vector3 testPosition;

    [SerializeField]
    private bool doTestGetPos = false;

    private void Update() {
        //编辑器中也会触发
        if(doTestGetPos) {
            doTestGetPos = false;
            Debug.Log(GetClosestGridPoint(testPosition));
        }
        //只在运行时触发
        if (!Application.isPlaying) return;
        switch(curState) 
        {
            case SwitchState.None:
                if (Input.GetMouseButtonDown(0) &&CanEnterSwitchState()) // 你需要自定义这个判断方法
                {
                    // 从鼠标位置转为世界坐标
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                    // 发射一条向上的 2D 射线（方向随意，这里是0向量，相当于一个点检测）
                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);
                    Debug.Log("有这么多："+hits.Length);
                    foreach (var hit in hits)
                    {
                        SwitchableObj switchable = hit.collider.GetComponent<SwitchableObj>();
                        if (switchable != null && !switchable.inSwitchState)
                        {
                            Debug.Log("进入switch state");
                            switchable.IntoSwitchState();
                            ReportSwitchableObj(switchable, true);
                            StartState(SwitchState.Switch);
                            break; // 只处理第一个
                        }
                    }
                }
                break;  
            case SwitchState.Switch:
                if(Input.GetMouseButtonUp(0)) {
                    // 从鼠标位置转为世界坐标
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                    // 发射一条向上的 2D 射线（方向随意，这里是0向量，相当于一个点检测）
                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);

                    foreach (var hit in hits)
                    {
                        SwitchableObj switchable = hit.collider.GetComponent<SwitchableObj>();
                        if (switchable != null && !switchable.inSwitchState)
                        {
                            //switchable.IntoSwitchState();
                            ReportSwitchableObj(switchable, false);
                            StartState(SwitchState.Switch);
                            break; // 只处理第一个
                        }
                    }

                    if(getBothTarget) {
                        if(switchableObjFrom.CheckIfCanMoveTo(switchableObjTo.SelfGridPos, switchableObjTo.gameObject)
                        && switchableObjTo.CheckIfCanMoveTo(switchableObjFrom.SelfGridPos, switchableObjFrom.gameObject)) {
                            DoSwitch();
                            Debug.Log("交换");
                            counter.SetCount(2);
                            StartState(SwitchState.Move);
                        }else
                        {
                            Debug.Log("不能交换");
                            switchableObjFrom.OutSwitchState();
                            switchableObjTo.FlashRed();
                            switchableObjFrom.FlashRed();
                            counter.SetCount(2);
                            ClearSwitchableObj();
                            StartState(SwitchState.Move);
                        }
                    }
                    else
                    {
                        Debug.Log("没有两个目标");
                        switchableObjFrom.OutSwitchState();
                        
                        ClearSwitchableObj();
                        StartState(SwitchState.None);
                    }
                }
                break;
            case SwitchState.Move:
                if(counter.IsZero()) {
                    StartNoneState();
                }   
                break;
        }
        

    }

    private void StartNoneState() {
        StartState(SwitchState.None);
    }

    public void StartState(SwitchState state) {
        switch(state)
        {
            case SwitchState.None:
            Debug.Log("进入none state");
                break;
            case SwitchState.Switch:
                break;
            case SwitchState.Move:
            
                break;
        }
        curState = state;
    }
    
    
    public Vector3 GetClosestGridPoint(Vector3 position) {
        Vector3 closestPoint = new Vector3(
            Mathf.Round(position.x-offsetX / gridWidth) * gridWidth+offsetX,
            Mathf.Round(position.y-offsetY / gridWidth) * gridWidth+offsetY,
            0
        );
        return closestPoint;
    }

    #region 交换物体
    private SwitchableObj switchableObjFrom;
    private SwitchableObj switchableObjTo;
    private SwitchState curState = SwitchState.None;
    private bool getBothTarget = false;

    private bool CanEnterSwitchState() {
        return true;
    }
    void ReportSwitchableObj(SwitchableObj obj, bool isFrom) {
        if(isFrom) {
            switchableObjFrom = obj;
        } else {
            switchableObjTo = obj;
            getBothTarget = true;
        }
    }

    private void DoSwitch()
    {
        Vector3 tempPos = switchableObjFrom.SelfGridPos;
        switchableObjFrom.OutSwitchState(switchableObjTo.SelfGridPos);
        switchableObjTo.SetAlpha(0.5f);
        switchableObjTo.MoveToGridPos(tempPos);
        ClearSwitchableObj();
    }

    private void ClearSwitchableObj() {
        switchableObjFrom = null;
        switchableObjTo = null;
        getBothTarget = false;
    }

    public void CountDown() {
        counter.CountDown();
    }
    
    #endregion
}
class Counter
{
    private int count = 0;

    public void SetCount(int count) {
        this.count = count;
    }

    public void CountDown() {
        count--;
    }

    public bool IsZero() {
        return count <= 0;
    }
}