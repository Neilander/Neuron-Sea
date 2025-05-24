using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SwitchState
{
    None,

    Switch,

    Move,
}

public enum SwitchLogic
{
    OrderBasedAndNoCancel,
    CancelWhenClickAgain
}

public enum SelectionLogic
{
    onlyBulletTime,
    allTime
}

public enum ExtraButtonLogic
{
    One,
    Two
}

[ExecuteInEditMode] // 在编辑器中执行
public class GridManager : MonoBehaviour
{
    [Header("状态控制")]
    [SerializeField] private bool isStateLocked = false;

    public static GridManager Instance;

    public SwitchLogic curSwitchLogic;
    public SelectionLogic curSelectionLogic;
    public ExtraButtonLogic curExtraButtonLogic;
    private const string SELECTOUTBULLET_KEY = "SelectableOutBulletTime";
    private const string DESELECT_KEY = "Deselectable";
    private const string EXTRASWITCHBUTTON_KEY = "ExtraSwitchButton";

    [Header("格子数据调整")] public int gridWidth = 1;

    [SerializeField][Range(0, 1)] private float offsetX = 0;

    [SerializeField][Range(0, 1)] private float offsetY = 0;

    [SerializeField] private Vector2 displayAmount = new Vector2(10, 10);

    [SerializeField] private bool displayInGizmos = true;

    [SerializeField] private GameObject displayCenter;

    [Header("交换控制")]
    [SerializeField] private KeyCode switchCode = KeyCode.E;
    [SerializeField] private KeyCode modeCode = KeyCode.Mouse1;
    [SerializeField] private bool autoSelectUnderMouse = true; // 是否自动选择鼠标下的物体
    // [SerializeField] private KeyCode toggleAutoSelectKey = KeyCode.F; // 用于切换自动选择功能的按键

    [Header("卡肉时间")]
    [SerializeField] public float waitTime = 0.2f;

    [Header("提示grid")]
    [SerializeField] private GameObject gridObj;

    private Counter counter = new Counter();

    private SwitchableObj switchableObjFrom;

    //private SwitchableObj switchableObjTo;

    private SwitchState curState = SwitchState.None;

    private bool getBothTarget = false;
    private SwitchableObj tempSwitchableObj;
    private bool ifLegalMove = false;
    

    public int SwitchTime { get; private set; }

    private TwoObjectContainer<SwitchableObj> switchInfoRecorder = new TwoObjectContainer<SwitchableObj>();

    const float switchCoolDown = 0.41f;
    private bool switchCoolDownFinished = true;

    //这部分是在编辑器中绘制网格
    private void OnDrawGizmos()
    {
        if (!displayInGizmos) return;
        if (displayCenter == null) return;

        // 设置网格线的颜色
        Gizmos.color = Color.red;

        // 获取displayCenter的位置
        Vector3 centerPos = displayCenter.transform.position;

        // 计算网格线的间距
        float gridSpacing = gridWidth;

        // 计算起始位置（向左和向下偏移5个格子）
        float startX = centerPos.x - (gridSpacing * displayAmount.x / 2);
        float startY = centerPos.y - (gridSpacing * displayAmount.y / 2);

        Vector3 displayCenterOffset = new Vector3(centerPos.x % gridWidth, centerPos.y % gridWidth, 0);
        Vector3 inputOffset = new Vector3(offsetX, offsetY, 0);

        // 绘制10x10的网格
        for (int i = 0; i <= displayAmount.x; i++)
        {
            // 绘制垂直线
            Vector3 startPos = new Vector3(startX + (gridSpacing * i), startY, 0) - displayCenterOffset + inputOffset;
            Vector3 endPos = new Vector3(startX + (gridSpacing * i), startY + (gridSpacing * displayAmount.y), 0) - displayCenterOffset + inputOffset;
            Gizmos.DrawLine(startPos, endPos);
        }

        for (int i = 0; i <= displayAmount.y; i++)
        {
            // 绘制水平线
            Vector3 startPos = new Vector3(startX, startY + (gridSpacing * i), 0) - displayCenterOffset + inputOffset;
            Vector3 endPos = new Vector3(startX + (gridSpacing * displayAmount.x), startY + (gridSpacing * i), 0) - displayCenterOffset + inputOffset;
            Gizmos.DrawLine(startPos, endPos);
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        // 编辑器状态下（不是播放或将要播放）
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode) {
            if (Instance == null) {
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

        LoadSettings();
    }

    public void LoadSettings()
    {
        curSwitchLogic = PlayerPrefs.GetInt(DESELECT_KEY, 0) != 0 ? SwitchLogic.CancelWhenClickAgain : SwitchLogic.OrderBasedAndNoCancel;
        curSelectionLogic = PlayerPrefs.GetInt(SELECTOUTBULLET_KEY, 0) != 0 ? SelectionLogic.allTime : SelectionLogic.onlyBulletTime;
        curExtraButtonLogic = PlayerPrefs.GetInt(EXTRASWITCHBUTTON_KEY, 0) != 0 ? ExtraButtonLogic.Two : ExtraButtonLogic.One;
    }

    public void LockStates(bool lockState)
    {
        isStateLocked = lockState;
        if (lockState)
        {
            // 如果当前在Switch或Move状态，强制退出到None状态
            if (curState == SwitchState.Switch || curState == SwitchState.Move)
            {
                StartState(SwitchState.None);
            }
        }
    }

    







    [Header("测试用")][SerializeField] private Vector3 testPosition;

    public void LogTimeAction() { Debug.Log("logSwitchTime" + SwitchTime); }

    private SwitchableObj GetTheOnlyInSwitchInfoRecorder()
    {
        if (switchInfoRecorder.IfHaveBoth())
        {
            Debug.LogError("当info recorder还记录了两个的时候，就尝试获取唯一一个");
        }
        SwitchableObj toReturn = null;
        if (switchInfoRecorder.hasFirst)
            toReturn = switchInfoRecorder.obj1;

        if (switchInfoRecorder.hasSecond)
            toReturn = switchInfoRecorder.obj2;
        //if(toReturn == null)
            //Debug.LogError("当info recorder没有记录的时候，就尝试获取唯一一个");
        return toReturn;
    }

    [SerializeField] private bool doTestGetPos = false;
    private void Update()
    {
        //编辑器中也会触发
        if (doTestGetPos)
        {
            doTestGetPos = false;
            Debug.Log(GetClosestGridPoint(testPosition));
        }
        //只在运行时触发
        if (!Application.isPlaying) return;

        // // 检测是否按下了切换自动选择功能的按键
        // if (Input.GetKeyDown(toggleAutoSelectKey))
        // {
        //     ToggleAutoSelect();
        // }

        switch (curState)
        {
            case SwitchState.None:

                if (GameInput.TimeStopsHere.Checked())
                {
                    StartState(SwitchState.Switch);

                    // 如果启用了自动选择，尝试自动选择鼠标下的物体
                    if (autoSelectUnderMouse)
                    {
                        SwitchableObj obj;
                        if (TryGetSwitchableUnderMouse(out obj))
                        {
                            SwitchableObj temp1, temp2;
                            switchInfoRecorder.Record(obj, out temp1, out temp2);
                            Debug.LogError("这里没有更新，现在需要刷新ifBothVisible");
                            obj.SetLockedToSwitch(true, true, false, Vector3.zero);
                            Log("已自动选择鼠标下的物体作为第一个交换对象");
                        }
                    }
                }
                else
                {
                    switch (curSelectionLogic)
                    {
                        case SelectionLogic.allTime:
                            Selection();
                            break;
                    }
                }

                checkIfSwitchableInView(switchInfoRecorder);
                RuntimeCheckIfLegal();


                /*
                if (Input.GetMouseButtonDown(0) && CanEnterSwitchState()) // 你需要自定义这个判断方法
                {
                    // 从鼠标位置转为世界坐标
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                    // 发射一条向上的 2D 射线（方向随意，这里是0向量，相当于一个点检测）
                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);
                    Debug.Log("有这么多：" + hits.Length);
                    foreach (var hit in hits) {
                        SwitchableObj switchable = hit.collider.GetComponent<SwitchableObj>();
                        if (switchable != null && !switchable.inSwitchState&& switchable.IfCanSwitch()) {
                            Debug.Log("进入switch state");
                            switchable.IntoSwitchState();
                            ReportSwitchableObj(switchable, true);

                            InAndOutSwitchEvent.InSwitch();
                            StartState(SwitchState.Switch);
                            break; // 只处理第一个
                        }
                    }
                }*/
                break;
            case SwitchState.Switch:
                /*
                if (Input.GetMouseButton(0))
                {
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);

                    bool getSwitchable = false;
                    SwitchableObj switchable = null;
                    foreach (var hit in hits) {
                        switchable = hit.collider.GetComponent<SwitchableObj>();
                        if (switchable != null && !switchable.inSwitchState&& switchable.IfCanSwitch())
                        {
                            getSwitchable = true;
                            break;
                        }
                    }
                    if (getSwitchable)
                    {
                        if (switchable != tempSwitchableObj)
                        {
                            tempSwitchableObj = switchable;
                            //检查是否合法
                            ifLegalMove = switchableObjFrom.CheckIfCanMoveTo(switchable.SelfGridPos, switchable.gameObject)
                            && switchable.CheckIfCanMoveTo(switchableObjFrom.SelfGridPos, switchableObjFrom.gameObject);
                            if (ifLegalMove)
                            {
                                tempSwitchableObj.IntoTempMoveState(switchableObjFrom.SelfGridPos);
                            }
                            else
                            {
                                
                                tempSwitchableObj.ControlFlash(true);
                                switchableObjFrom.ControlFlash(true);
                            }
                        }
                    } else
                    {
                        if (tempSwitchableObj != null)
                        {
                            if (ifLegalMove)
                            {
                                tempSwitchableObj.OutTempMoveState();
                            }
                            else
                            {
                                tempSwitchableObj.ControlFlash(false);
                                switchableObjFrom.ControlFlash(false);
                            }
                            
                            tempSwitchableObj = null;
                        }
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    if (tempSwitchableObj != null)
                    {
                        if (ifLegalMove)
                        {
                            DoSwitch();
                        }
                        else
                        {
                            tempSwitchableObj.ControlFlash(false);
                            switchableObjFrom.ControlFlash(false);
                            switchableObjFrom.OutSwitchState();
                        }
                    }
                    else
                    {
                        switchableObjFrom.OutSwitchState();
                    }
                    InAndOutSwitchEvent.OutSwitch();
                    StartState(SwitchState.None);
                }

                */

                if (!GameInput.TimeStopsHere.Checked())
                {
                    StartState(SwitchState.None);
                }
                checkIfSwitchableInView(switchInfoRecorder);

                RuntimeCheckIfLegal();

                Selection();

                break;
            case SwitchState.Move:

                break;
        }
    }

    private void Selection()
    {
        if (GameInput.SwitchableSelection.Pressed(false))//GameInput.SwitchableSelection.Pressed()
        {
            SwitchableObj tryGet;
            Debug.Log("尝试获取物体");
            if (TryGetSwitchableUnderMouse(out tryGet))
            {
                //如果选中已经被选中的物体
                if (switchInfoRecorder.Take(tryGet))
                {
                    SwitchableObj temp1;
                    SwitchableObj temp2;
                    switch (curSwitchLogic)
                    {
                        case SwitchLogic.OrderBasedAndNoCancel:
                            if (switchInfoRecorder.hasFirst || switchInfoRecorder.hasSecond)
                            {
                                //原来选择了两个，现在要刷新顺序
                                SwitchableObj onlyObj = GetTheOnlyInSwitchInfoRecorder();
                                switchInfoRecorder.Take(onlyObj);
                                switchInfoRecorder.Record(onlyObj, out temp1, out temp2);
                                switchInfoRecorder.Record(tryGet, out temp1, out temp2);
                                bool ifLegal = IsLegalMoveBetween(switchInfoRecorder.obj1, switchInfoRecorder.obj2);
                                switchInfoRecorder.obj1.SetLockedToSwitch(true, ifLegal, true, switchInfoRecorder.obj2.SelfGridPos);
                                switchInfoRecorder.obj2.SetLockedToSwitch(true, ifLegal, true, switchInfoRecorder.obj1.SelfGridPos);
                                ifLegalMove = ifLegal;
                                canViewBothSelection = true;

                            }
                            else
                            {
                                //原来只有一个，现在要保持不变
                                switchInfoRecorder.Record(tryGet, out temp1, out temp2);
                                tryGet.SetLockedToSwitch(true, true, false, Vector3.zero);
                            }

                            break;

                        case SwitchLogic.CancelWhenClickAgain:
                            SwitchableObj obj = GetTheOnlyInSwitchInfoRecorder();
                            if (obj != null)
                                obj.SetLockedToSwitch(true, true, false, Vector3.zero);

                            tryGet.SetLockedToSwitch(false, true, false, Vector3.zero);
                            break;
                    }

                    //switchInfoRecorder.Refresh();


                    //现在take已经取出了一个
                    //如果还存储了另外一个，可以得知原本的情况是选中了两个，所以两个都要取消，这里先修改表示
                    //选中在最后取消
                    //bool ifChanged = false;

                    /*
                     * 这一部分是取消两个的
                    if (switchInfoRecorder.hasFirst)
                    {
                        switchInfoRecorder.obj1.SetLockedToSwitch(false, true, false, Vector3.zero);
                        //ifChanged = true;
                    }
                    if (switchInfoRecorder.hasSecond)
                    {
                        switchInfoRecorder.obj2.SetLockedToSwitch(false, true, false, Vector3.zero);
                        //ifChanged = true;
                    }
                    tryGet.SetLockedToSwitch(false, true, false, Vector3.zero);
                    switchInfoRecorder.Refresh();
                    */
                    /*
                    if (ifChanged)
                    {
                        //修改已经取出的tryGet的表现
                        tryGet.SetLockedToSwitch(false, true, false, Vector3.zero);
                    }
                    //统一取消选中
                    switchInfoRecorder.Refresh();
                    */
                    //如果之前没有改变，那么说明之前只选择了一个，那么就要保持tryGet的选择
                    /*
                    SwitchableObj temp1;
                    SwitchableObj temp2;
                    if (!ifChanged)
                    {
                        switchInfoRecorder.Record(tryGet, out temp1, out temp2);
                        tryGet.SetLockedToSwitch(true, true, false, Vector3.zero);
                    }*/

                }
                else//选中一个新的物体
                {

                    //首先记录新选择的物体
                    SwitchableObj temp1;
                    SwitchableObj temp2;
                    if (switchInfoRecorder.Record(tryGet, out temp1, out temp2))
                    {
                        switch (curSwitchLogic)
                        {
                            case SwitchLogic.OrderBasedAndNoCancel:
                                //如果此前已经记录了两个，就改变另外2个的显示，取消选择
                                temp1.SetLockedToSwitch(false, true, false, Vector3.zero);
                                temp2.SetLockedToSwitch(false, true, false, Vector3.zero);
                                //新选择的物体在后面处理显示
                                canViewBothSelection = true;

                                if (temp1.IsSpriteVisibleOnScreen() ^ temp2.IsSpriteVisibleOnScreen())
                                {
                                    switchInfoRecorder.Take(tryGet);
                                    if (temp1.IsSpriteVisibleOnScreen()) switchInfoRecorder.Record(temp1, out temp1, out temp2);
                                    else if (temp2.IsSpriteVisibleOnScreen()) switchInfoRecorder.Record(temp2, out temp1, out temp2);

                                    switchInfoRecorder.Record(tryGet, out temp1, out temp2);
                                }
                                else if (temp1.IsSpriteVisibleOnScreen() && temp2.IsSpriteVisibleOnScreen())
                                {
                                    switchInfoRecorder.Take(tryGet);
                                    switchInfoRecorder.Record(temp2, out temp1, out temp2);
                                    switchInfoRecorder.Record(tryGet, out temp1, out temp2);
                                }

                                break;

                            case SwitchLogic.CancelWhenClickAgain:
                                //如果此前已经记录了两个，就改变另外2个的显示，取消选择
                                temp1.SetLockedToSwitch(false, true, false, Vector3.zero);
                                temp2.SetLockedToSwitch(false, true, false, Vector3.zero);
                                //新选择的物体在后面处理显示
                                canViewBothSelection = true;

                                if (temp1.IsSpriteVisibleOnScreen() ^ temp2.IsSpriteVisibleOnScreen())
                                {
                                    if (temp1.IsSpriteVisibleOnScreen()) switchInfoRecorder.Record(temp1, out temp1, out temp2);
                                    else if (temp2.IsSpriteVisibleOnScreen()) switchInfoRecorder.Record(temp2, out temp1, out temp2);
                                }
                                break;
                        }

                    }

                    //选择后，如果有两个并且是非法的
                    if (switchInfoRecorder.IfHaveBoth() && !IsLegalMoveBetween(switchInfoRecorder.obj1, switchInfoRecorder.obj2))
                    {
                        switchInfoRecorder.obj1.SetLockedToSwitch(true, false, true, switchInfoRecorder.obj2.SelfGridPos);
                        switchInfoRecorder.obj2.SetLockedToSwitch(true, false, true, switchInfoRecorder.obj1.SelfGridPos);
                        ifLegalMove = false;
                        canViewBothSelection = true;
                    }
                    else
                    {
                        //剩下的状态里只有选择两个并且合法+只选择了一个
                        if (switchInfoRecorder.IfHaveBoth())
                        {
                            //选择两个必定合法
                            ifLegalMove = true;
                            switchInfoRecorder.obj1.SetLockedToSwitch(true, true, true, switchInfoRecorder.obj2.SelfGridPos);
                            switchInfoRecorder.obj2.SetLockedToSwitch(true, true, true, switchInfoRecorder.obj1.SelfGridPos);

                            canViewBothSelection = true;
                        }
                        else
                        {
                            //只选择了一个
                            switchInfoRecorder.obj1.SetLockedToSwitch(true, true, false, Vector3.zero);
                        }

                    }

                }
                Debug.Log("播放选中音效");
                AudioManager.Instance.Play(SFXClip.ObjSelection, Time.unscaledTime.ToString());
            }
        }
    }

    private void StartNoneState()
    {
        StartState(SwitchState.None);
    }

    private void RuntimeCheckIfLegal()
    {
        if (switchInfoRecorder.IfHaveBoth())
        {
            if (!ifLegalMove && IsLegalMoveBetween(switchInfoRecorder.obj1, switchInfoRecorder.obj2))
            {
                //从非法转变为合法
                switchInfoRecorder.obj1.SetLockedToSwitch(true, true, true, switchInfoRecorder.obj2.SelfGridPos);
                switchInfoRecorder.obj2.SetLockedToSwitch(true, true, true, switchInfoRecorder.obj1.SelfGridPos);
                ifLegalMove = true;
            }
            else if (ifLegalMove && !IsLegalMoveBetween(switchInfoRecorder.obj1, switchInfoRecorder.obj2))
            {
                //从合法转变为非法
                switchInfoRecorder.obj1.SetLockedToSwitch(true, false, true, switchInfoRecorder.obj2.SelfGridPos);
                switchInfoRecorder.obj2.SetLockedToSwitch(true, false, true, switchInfoRecorder.obj1.SelfGridPos);
                ifLegalMove = false;
            }

            if (ifLegalMove&& canViewBothSelection&& switchCoolDownFinished)
            {
                if (GameInput.SwitchObjects.Pressed() || (curExtraButtonLogic == ExtraButtonLogic.Two && GameInput.SwitchObjects2.Pressed()))
                    ShiftSwitch();
            }
        }
    }

    public void StartState(SwitchState state)
    {
        // 如果状态被锁定，只允许进入None状态
        if (isStateLocked && state != SwitchState.None) {
            return;
        }
        EndState(curState, state);

        switch (state)
        {
            case SwitchState.None:

                Debug.Log("进入none state");
                break;
            case SwitchState.Switch:
                //AudioManager.Instance.PauseBGM();
                if (curState == SwitchState.None)
                {
                    AudioManager.Instance.Play(SFXClip.BulletTimeIn, "GridManager");
                }
                AudioManager.Instance.Play(SFXClip.BulletContinune, "GridManager");
                InAndOutSwitchEvent.InSwitch();
                PauseEvent.Pause();
                gridObj.SetActive(true);
                // // 显示当前自动选择状态
                // Log("当前自动选择功能" + (autoSelectUnderMouse ? "已开启" : "已关闭") + "，按" + toggleAutoSelectKey + "键切换");
                ActivityGateCenter.EnterState(ActivityState.BulletTime);
                break;
            case SwitchState.Move:
                Time.timeScale = 0;

                StartCoroutine(NoneStateCor());
                break;
        }
        curState = state;
    }

    private void EndState(SwitchState lastState, SwitchState newState)
    {
        switch (lastState)
        {
            case SwitchState.Switch:
                if (newState == SwitchState.None)
                {
                    AudioManager.Instance.Play(SFXClip.BulletTimeOut,"GridManager");
                }
                AudioManager.Instance.Stop(SFXClip.BulletContinune, "GridManager" ,true);
                //AudioManager.Instance.ResumeBGM();
                InAndOutSwitchEvent.OutSwitch();
                PauseEvent.Resume();
                gridObj.SetActive(false);
                ActivityGateCenter.ExitState(ActivityState.BulletTime);
                break;

            case SwitchState.Move:
                Time.timeScale = 1;
                break;
        }
    }


    public Vector3 GetClosestGridPoint(Vector3 position)
    {
        Vector3 closestPoint = new Vector3(
            Mathf.Round(position.x - offsetX / gridWidth) * gridWidth + offsetX,
            Mathf.Round(position.y - offsetY / gridWidth) * gridWidth + offsetY,
            0
        );
        return closestPoint;
    }

    #region 交换物体



    private bool CanEnterSwitchState()
    {
        return true;
    }

    void ReportSwitchableObj(SwitchableObj obj, bool isFrom)
    {
        if (isFrom)
        {
            switchableObjFrom = obj;
        }
        else
        {

            getBothTarget = true;
        }
    }

    /*
    private void DoSwitch(){
        switchTime += 1;
        Vector3 tempPos = switchableObjFrom.SelfGridPos;


        switchableObjFrom.OutSwitchState(tempSwitchableObj.SelfGridPos);
        tempSwitchableObj.ChangeFromTempMoveToNormal();
        ClearSwitchableObj();
    }*/

    private void ShiftSwitch()
    {
        SwitchTime += 1;
        if (SwitchLimitInUi.instance != null)
            SwitchLimitInUi.instance.SetSwitchTime(SwitchTime);
        Vector3 tempPos = switchInfoRecorder.obj1.SelfGridPos;
        switchInfoRecorder.obj1.SetToGridPos(switchInfoRecorder.obj2.SelfGridPos);
        switchInfoRecorder.obj2.SetToGridPos(tempPos);
        AudioManager.Instance.Play(SFXClip.Switch, Time.unscaledTime.ToString());
        switchCoolDownFinished = false;
        Invoke("ResetCoolDownFinish", switchCoolDown);
        StartState(SwitchState.Move);
    }

    private void ResetCoolDownFinish() => switchCoolDownFinished = true;

    public void RenewSwitch()
    {
        SwitchTime = 0;
    }

    private void ClearSwitchableObj()
    {
        switchableObjFrom = null;

        tempSwitchableObj = null;
        getBothTarget = false;
    }

    public void CountDown()
    {
        counter.CountDown();
    }

    public static bool TryGetSwitchableUnderMouse(out SwitchableObj target)
    {
        target = null;

        // 1. 获取鼠标在世界中的位置
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        // 2. 发射一条零向量射线（点检测）
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);

        // 3. 遍历命中的所有物体
        foreach (var hit in hits)
        {
            SwitchableObj switchable = hit.collider.GetComponentInParent<SwitchableObj>();
            if (switchable != null && switchable.IfCanSwitch())// && !switchable.inSwitchState 这个留着之后可能用
            {
                target = switchable;
                return true; // 找到第一个合适的就返回
            }
        }

        return false;
    }

    private bool IsLegalMoveBetween(SwitchableObj from, SwitchableObj to)
    {
        if (from == null || to == null) return false;

        bool fromCanMove = from.CheckIfCanMoveTo(to.SelfGridPos, to.gameObject);
        bool toCanMove = to.CheckIfCanMoveTo(from.SelfGridPos, from.gameObject);

        return fromCanMove && toCanMove;
    }

    //销毁所有的switchable都要通过这个
    public void DestroySwitchable(SwitchableObj obj)
    {
        if (obj.GetComponent<IDeathActionOverrider>() != null)
        {
            if (!obj.GetComponent<IDeathActionOverrider>().DeathAction())
            {
                return;
            }
        }
        ReleaseSelection(obj);
        Destroy(obj.gameObject);

    }

    public void ReleaseSelection(SwitchableObj obj)
    {
        if (switchInfoRecorder.Take(obj))
        {
            //如果有另外一个，就更改显示
            if (switchInfoRecorder.hasFirst)
                switchInfoRecorder.obj1.SetLockedToSwitch(true, true, false, Vector3.zero);
            if (switchInfoRecorder.hasSecond)
            {
                switchInfoRecorder.obj2.SetLockedToSwitch(true, true, false, Vector3.zero);
                SwitchableObj secondObj = switchInfoRecorder.obj2;
                switchInfoRecorder.Take(secondObj);
                SwitchableObj p1;
                SwitchableObj p2;
                switchInfoRecorder.Record(secondObj, out p1, out p2);
            }
                
        }
    }

    #endregion

    // 在GridManager类中添加获取当前状态的方法
    public SwitchState GetCurrentState()
    {
        return curState;
    }

    // 添加强制选择两个可交换物体的方法
    public void ForceSelectObjectsForSwitch(SwitchableObj obj1, SwitchableObj obj2)
    {
        // 清空当前选择
        switchInfoRecorder.Refresh();

        // 记录新的选择
        if (obj1 != null && obj2 != null)
        {
            // 记录第一个对象
            SwitchableObj poopOut, poopOut2;
            switchInfoRecorder.Record(obj1, out poopOut, out poopOut2);

            // 记录第二个对象
            switchInfoRecorder.Record(obj2, out poopOut, out poopOut2);

            // 检查是否为合法移动
            ifLegalMove = IsLegalMoveBetween(obj1, obj2);

            // 设置对象状态
            if (ifLegalMove)
            {
                obj1.SetLockedToSwitch(true, true, true, obj2.SelfGridPos);
                obj2.SetLockedToSwitch(true, true, true, obj1.SelfGridPos);
                Log("已强制选择两个可交换物体，它们可以合法交换");

                // 如果当前处于Switch状态，允许按E键交换
                if (curState == SwitchState.Switch)
                {
                    // 在这里可以添加提示UI，显示"按E交换"
                }
            }
            else
            {
                obj1.SetLockedToSwitch(true, false, true, obj2.SelfGridPos);
                obj2.SetLockedToSwitch(true, false, true, obj1.SelfGridPos);
                Log("已强制选择两个可交换物体，但它们不能合法交换");
            }
        }
    }

    private void Log(string message)
    {
        Debug.Log("[GridManager] " + message);
    }

    // 切换自动选择功能
    public void ToggleAutoSelect()
    {
        autoSelectUnderMouse = !autoSelectUnderMouse;
        Log("自动选择功能已" + (autoSelectUnderMouse ? "开启" : "关闭"));
    }

    // 获取自动选择状态
    public bool GetAutoSelectState()
    {
        return autoSelectUnderMouse;
    }


    IEnumerator NoneStateCor()
    {
        yield return new WaitForSecondsRealtime(waitTime);

        // ✅ 这里写你想触发的事件
        if (GameInput.TimeStopsHere.Checked())
        {
            StartState(SwitchState.Switch);
        }
        else
        {
            StartState(SwitchState.None);
        }
    }

    private bool canViewBothSelection = true;
    void checkIfSwitchableInView(TwoObjectContainer<SwitchableObj> obj)
    {
        SwitchableObj record1 = obj.obj1;
        SwitchableObj record2 = obj.obj2;
        if (canViewBothSelection)
        {
            

            bool ifChangeOne = !obj.hasFirst;
            bool ifChangeTwo = !obj.hasSecond;
            if (obj.hasFirst && !obj.obj1.IsSpriteVisibleOnScreen())
            {
                //取消1的选中显示但是保持选中
                //obj.Take(record1);
                record1.SetLockedToSwitch(true, true, false, Vector3.zero);
                ifChangeOne = true;
                if(obj.IfHaveBoth())
                    canViewBothSelection = false;
                Debug.Log("1看不见了！");

            }

            if (obj.hasSecond && !obj.obj2.IsSpriteVisibleOnScreen())
            {
                //取消2的选中显示但是保持选中
                //obj.Take(record2);
                record2.SetLockedToSwitch(true, true, false, Vector3.zero);
                ifChangeTwo = true;
                if (obj.IfHaveBoth())
                    canViewBothSelection = false;
                Debug.Log("2看不见了！");

            }

            if (ifChangeOne ^ ifChangeTwo)
            {
                //没有改变one，就说明原来一定有one并且没有更改
                if (!ifChangeOne) obj.obj1.SetLockedToSwitch(true, true, false, Vector3.zero);
                else if (!ifChangeTwo) obj.obj2.SetLockedToSwitch(true, true, false, Vector3.zero);
                
            }
        }
        else
        {
            if (obj.hasFirst && obj.hasSecond && obj.obj1.IsSpriteVisibleOnScreen() && obj.obj2.IsSpriteVisibleOnScreen())
            {
                canViewBothSelection = true;
                obj.obj1.SetLockedToSwitch(true, true, true, obj.obj2.SelfGridPos);
                obj.obj2.SetLockedToSwitch(true, true, true, obj.obj1.SelfGridPos);
            }
        }
        
    }

    public void RefreshSelection()
    {
        if (switchInfoRecorder == null)
            return;
        if (switchInfoRecorder.hasFirst)
            switchInfoRecorder.obj1.SetLockedToSwitch(false, true, false, Vector3.zero);
        if (switchInfoRecorder.hasSecond)
            switchInfoRecorder.obj2.SetLockedToSwitch(false, true, false, Vector3.zero);
        switchInfoRecorder.Refresh();
    }
}

class Counter
{
    private int count = 0;

    public void SetCount(int count)
    {
        this.count = count;
    }

    public void CountDown()
    {
        count--;
    }

    public bool IsZero()
    {
        return count <= 0;
    }
}

class TwoObjectContainer<Type>
{
    public Type obj1 { get; private set; }
    public Type obj2 { get; private set; }


    public bool hasFirst { get; private set; }
    public bool hasSecond { get; private set; }

    public TwoObjectContainer()
    {
        hasFirst = false;
        hasSecond = false;
    }

    public TwoObjectContainer(Type obj1, Type obj2)
    {
        this.obj1 = obj1;
        this.obj2 = obj2;
        hasFirst = true;
        hasSecond = true;
    }

    public TwoObjectContainer(Type obj1)
    {
        this.obj1 = obj1;
        hasFirst = true;
        hasSecond = false;
    }

    public bool Record(Type n, out Type poopOut, out Type poopOut2)
    {
        poopOut = n;
        poopOut2 = n;
        if (hasFirst)
        {
            if (hasSecond)
            {
                poopOut = obj1;
                poopOut2 = obj2;
                obj1 = n;
                hasSecond = false;
                return true;
            }
            else
            {
                obj2 = n;
                hasSecond = true;
            }
        }
        else
        {
            obj1 = n;
            hasFirst = true;
        }

        return false;
    }

    public bool Take(Type obj)
    {
        if (hasFirst && obj1.Equals(obj))
        {
            hasFirst = false;
            return true;
        }
        else if (hasSecond && obj2.Equals(obj))
        {
            hasSecond = false;
            return true;
        }

        return false;
    }

    public bool IfHaveBoth()
    {
        return hasFirst && hasSecond;
    }

    public void Refresh()
    {
        hasFirst = false;
        hasSecond = false;
    }









}

//用来管理一些零散的开启/关闭Switch函数
public static class InAndOutSwitchEvent
{
    public static event UnityAction OnInSwitchTriggered;
    public static event UnityAction OnOutSwitchTriggered;

    public static void InSwitch()
    {
        OnInSwitchTriggered?.Invoke();
    }

    public static void OutSwitch()
    {
        OnOutSwitchTriggered?.Invoke();
    }
}

//之前让两个物体移动的代码
/*
                if (Input.GetMouseButtonUp(0)) {
                    // 从鼠标位置转为世界坐标
                    Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 rayOrigin = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

                    // 发射一条向上的 2D 射线（方向随意，这里是0向量，相当于一个点检测）
                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.zero);

                    foreach (var hit in hits) {
                        SwitchableObj switchable = hit.collider.GetComponent<SwitchableObj>();
                        if (switchable != null && !switchable.inSwitchState) {
                            //switchable.IntoSwitchState();
                            ReportSwitchableObj(switchable, false);
                            StartState(SwitchState.Switch);
                            break; // 只处理第一个
                        }
                    }

                    if (getBothTarget) {
                        if (switchableObjFrom.CheckIfCanMoveTo(switchableObjTo.SelfGridPos, switchableObjTo.gameObject)
                            && switchableObjTo.CheckIfCanMoveTo(switchableObjFrom.SelfGridPos, switchableObjFrom.gameObject)) {
                            DoSwitch();
                            Debug.Log("交换");
                            counter.SetCount(2);
                            StartState(SwitchState.Move);
                        }
                        else {
                            Debug.Log("不能交换");
                            switchableObjFrom.OutSwitchState();
                            switchableObjTo.FlashRed();
                            switchableObjFrom.FlashRed();
                            counter.SetCount(2);
                            ClearSwitchableObj();
                            StartState(SwitchState.Move);
                        }
                    }
                    else {
                        Debug.Log("没有两个目标");
                        switchableObjFrom.OutSwitchState();

                        ClearSwitchableObj();
                        StartState(SwitchState.None);
                    }
                }*/