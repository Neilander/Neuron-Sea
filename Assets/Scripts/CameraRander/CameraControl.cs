using System;
using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static CameraControl Instance{get; private set;}
    public Transform target;
    //已经播过一次，true：播过，false：没播过
    //public bool hasLoadOnce;
    private bool _hasLoadOnce;

    public bool endTeach;
    public bool hasLoadOnce
    {
        get => _hasLoadOnce;
        set
        {
            if (_hasLoadOnce != value)
            {
                _hasLoadOnce = value;
                Debug.Log("hasLoadOnce 被改动了，现在是: " + value);
            }
        }
    }
    public bool ifReverTutorialTrigger = false;
    
    public Transform startTarget;
    private Camera cam;
    private float halfWidth;
    private float halfHeight;

    private bool _setted = false;

    public bool Setted
    {
        get => _setted;
        set
        {
            if (_setted != value)
            {
                _setted = value;
                Debug.Log("setted 被改动了，现在是: " + value);
            }
        }
    }
    private bool queued = false;
    private CameraLimitRegion currentLimit = null;
    private CameraLimitRegion queuedLimit = null;

    // 无视摄像机边界
    private bool ignoreLimit = false;
    private CameraLimitRegion defaultLimit;
    private bool ignoreHorizontalLimit = false;
    [CanBeNull] public CompanionController companionController;

    
    // ✅ 新增：平滑移动控制
    private Vector3 smoothTargetPosition;
    public bool isTransitioning = false;
    public float smoothSpeed = 5f;

    [Header("默认区域配置")]
    public Vector2 defaultOrigin; // 左下角坐标
    public float defaultWidth = 10f;
    public float defaultHeight = 5f;

    [Header("y上offset")]
    public float yOffset = 0.5f;

    private Animator ani;
    private float realSmoothSpeed;
    public bool specialStartForScene1 = false;
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        // 只有在编辑器下且设置了默认区域参数才绘制
        float left = defaultOrigin.x;
        float right = defaultOrigin.x + defaultWidth;
        float bottom = defaultOrigin.y;
        float top = defaultOrigin.y + defaultHeight;

        Vector3 topLeft = new Vector3(left, top, 0);
        Vector3 topRight = new Vector3(right, top, 0);
        Vector3 bottomLeft = new Vector3(left, bottom, 0);
        Vector3 bottomRight = new Vector3(right, bottom, 0);

        // 画矩形边框
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);

        // 可选：在 Scene 里显示文字（需要 Handles）
#if UNITY_EDITOR
    UnityEditor.Handles.Label(new Vector3(left, top + 0.5f, 0), "Default Camera Limit", new GUIStyle()
    {
        fontStyle = FontStyle.Bold,
        normal = new GUIStyleState { textColor = Color.cyan }
    });
#endif
    }

    private void Awake(){
        Instance = this;
        
        
        realSmoothSpeed = smoothSpeed;

        /* 原本剧情相关
        if (hasLoadOnce) { //这里泡饭改的是从注册表获取
            target = FindObjectOfType<PlayerController>().transform;
        }*/

        print("我在这里");
        StoryGlobalLoadManager.instance.RegisterOnStartWithoutStory(_ => { target = FindObjectOfType<PlayerController>().transform; });
        StoryGlobalLoadManager.instance.RegisterOnStartWithStory(PrepareForLevelStory);
    }

    private void OnDestroy()
    {
        StoryGlobalLoadManager.instance.UnregisterOnStartWithoutStory(_ => { target = FindObjectOfType<PlayerController>().transform; });
        StoryGlobalLoadManager.instance.UnregisterOnStartWithStory(PrepareForLevelStory);
    }

    public void PrepareForLevelStory(int n)
    {
        if (n != 1)
            return;
        GridManager.Instance.LockStates(true);
        IgnoreCameraLimit();
        FindObjectOfType<PlayerController>().DisableInput();
        if (companionController != null)
        {
            companionController.SetTarget(null);
        }
        ActivityGateCenter.EnterState(ActivityState.Story);
        StartCoroutine(BeginningDelay(1f));
        
    }

    void Start(){
        
        //HelperToolkit.PrintBoolStates(()=> levelManager.instance.isStartStory,()=>specialStartForScene1,()=>hasLoadOnce);
        /* 原本剧情相关
        if (levelManager.instance.currentLevelIndex == 1 && 
            levelManager.instance.isStartStory&&
            specialStartForScene1&& !(hasLoadOnce)) { //这里泡饭改的是从注册表获取
            // setted = true;
            GridManager.Instance.LockStates(true);
            //IgnoreHorizontalLimit();
            IgnoreCameraLimit();
            FindObjectOfType<PlayerController>().DisableInput();
            Debug.Log("正常触发");
        }
        if (!specialStartForScene1) {
            target=FindObjectOfType<PlayerController>().transform;
            
        }*/

        ani = companionController.GetComponent<Animator>();
        cam = Camera.main;
        halfHeight = cam.orthographicSize;
        halfWidth = halfHeight * cam.aspect;
        smoothTargetPosition = transform.position;
        
        /* 原本剧情相关
        if (levelManager.instance.isStartStory&& 
            levelManager.instance.currentLevelIndex == 1&&
            specialStartForScene1&& !(hasLoadOnce)) { //这里泡饭改的是从注册表获取
            if (companionController != null) {
                companionController.SetTarget(null);
            }
            StartCoroutine(BeginningDelay(1f));
            Debug.Log("正常触发");
            
        }*/
        // ✅ 构建默认限制区域
        float left = defaultOrigin.x;
        float right = defaultOrigin.x + defaultWidth;
        float bottom = defaultOrigin.y;
        float top = defaultOrigin.y + defaultHeight;

        defaultLimit = new CameraLimitRegion(left, right, top, bottom, null,0);
    }

    private IEnumerator BeginningDelay(float time){
        ani.Play("robot_move");
        companionController.CannotMove();
        // 等待动画状态真正进入 robot_move 状态
        yield return new WaitUntil(() => ani.GetCurrentAnimatorStateInfo(0).IsName("robot_move"));

        // 等待动画播放完（normalizedTime >= 1）
        yield return new WaitUntil(() =>
            ani.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f

        );
        companionController.CanMove();
        // companionController.GetComponent<Animator>().enabled = true;
        companionController.SetTarget(FindAnyObjectByType<PlayerController>().transform);
    }
    void LateUpdate()
    {
        if (target == null) return;

        // ✅ 每帧更新目标位置
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y + yOffset, transform.position.z);
        
        // 新增：如果ignoreLimit为true，直接跟随目标，不做边界限制
        if (ignoreLimit)
        {
            transform.position = desiredPos;
            return;
        }
        //Debug.Log("相机被限制，使用的是"+(setted?"currentLimit":"defaultLimit"));
        // ✅ 选择使用 currentLimit 或 defaultLimit
        CameraLimitRegion limitToUse = Setted ? currentLimit : defaultLimit;

        if (limitToUse != null)
        {
            if (!ignoreHorizontalLimit &&limitToUse.left.HasValue && limitToUse.right.HasValue)
            {
                float leftBound = limitToUse.left.Value + halfWidth;
                float rightBound = limitToUse.right.Value - halfWidth;
                if (leftBound < rightBound)
                {
                    desiredPos.x = Mathf.Clamp(desiredPos.x, leftBound, rightBound);
                }
            }

            if (limitToUse.top.HasValue || limitToUse.bottom.HasValue)
            {
                float topBound = limitToUse.top.HasValue ? limitToUse.top.Value - halfHeight : float.MaxValue;
                float bottomBound = limitToUse.bottom.HasValue ? limitToUse.bottom.Value + halfHeight : float.MinValue;
                if (bottomBound < topBound)
                {
                    desiredPos.y = Mathf.Clamp(desiredPos.y+limitToUse.extraYOffset, bottomBound, topBound);
                }
            }
        }

        // ✅ 是否平滑移动中
        if (isTransitioning)
        {
            
            // 每帧更新目标位置
            smoothTargetPosition = desiredPos;

            // 平滑插值移动
            transform.position = Vector3.Lerp(transform.position, smoothTargetPosition, Time.deltaTime * realSmoothSpeed);

            // 到达目标，停止平滑
            if (Vector3.Distance(transform.position, smoothTargetPosition) < 0.01f)
            {
                transform.position = smoothTargetPosition;
                isTransitioning = false;
                realSmoothSpeed = smoothSpeed;
            }
        }
        else
        {
            // 正常锁定逻辑
            transform.position = desiredPos;
        }
    }

    Vector3 calculateDesiredPos()
    {
        Vector3 desiredPos = new Vector3(target.position.x, target.position.y + yOffset, transform.position.z);
        CameraLimitRegion limitToUse = Setted ? currentLimit : defaultLimit;

        if (limitToUse != null)
        {
            if (!ignoreHorizontalLimit && limitToUse.left.HasValue && limitToUse.right.HasValue)
            {
                float leftBound = limitToUse.left.Value + halfWidth;
                float rightBound = limitToUse.right.Value - halfWidth;
                if (leftBound < rightBound)
                {
                    desiredPos.x = Mathf.Clamp(desiredPos.x, leftBound, rightBound);
                }
            }

            if (limitToUse.top.HasValue || limitToUse.bottom.HasValue)
            {
                float topBound = limitToUse.top.HasValue ? limitToUse.top.Value - halfHeight : float.MaxValue;
                float bottomBound = limitToUse.bottom.HasValue ? limitToUse.bottom.Value + halfHeight : float.MinValue;
                if (bottomBound < topBound)
                {
                    desiredPos.y = Mathf.Clamp(desiredPos.y, bottomBound, topBound);
                }
            }
        }
        return desiredPos;
    }

    /// <summary>
    /// 忽略左右边界限制（X轴）
    /// </summary>
    public void IgnoreHorizontalLimit()
    {
        ignoreHorizontalLimit = true;
    }

    /// <summary>
    /// 恢复左右边界限制
    /// </summary>
    public void RestoreHorizontalLimit()
    {
        ignoreHorizontalLimit = false;
    }
    public void SetLimitRegion(CameraLimitRegion newRegion)
    {
        if (Setted)
        {
            if (currentLimit.setter.priority >= newRegion.setter.priority)
            {
                Debug.Log("设置到 queue");
                queuedLimit = newRegion;
            }
            else
            {
                Debug.Log("抢占先机");
                queuedLimit = currentLimit;

                currentLimit = newRegion;
                isTransitioning = true;
            }
            queued = true;

        }
        else
        {
            Debug.Log("没有已经设置的，直接来");
            currentLimit = newRegion;
            Setted = true;
            isTransitioning = true; // ✅ 开启平滑过渡
        }
    }
/// <summary>
    /// 调用此方法后摄像机会无视边界限制
    /// </summary>
    public void IgnoreCameraLimit()
    {
        ignoreLimit = true;
    }

    /// <summary>
    /// 恢复摄像机边界限制
    /// </summary>
    public void RestoreCameraLimit(bool ifChangeSS = false ,float tempSmoothSpeed = 0)
    {
        if(ifChangeSS)realSmoothSpeed = tempSmoothSpeed;
        ignoreLimit = false;
    }
    public void ClearLimitRegion(CameraRegionTrigger sender)
    {
        if (Setted && currentLimit != null && currentLimit.setter == sender)
        {
            if (queued)
            {
                currentLimit = queuedLimit;
                queuedLimit = null;
                queued = false;
                Debug.Log("替换了当前");
                isTransitioning = true; // ✅ 新区域 → 平滑进入
            }
            else
            {
                currentLimit = null;
                Debug.Log("移除了当前");
                Setted = false;
                isTransitioning = true; // ✅ 清空限制，也平滑
            }
        }
        else if (queued && queuedLimit != null && queuedLimit.setter == sender )
        {
            queuedLimit = null;
            queued = false;
            Debug.Log("替换了当前");
            isTransitioning = false;
        }
    }

    public void DirectSetPos()
    {
        if (isTransitioning)
        {
            transform.position = calculateDesiredPos();
            isTransitioning = false;
            realSmoothSpeed = smoothSpeed;
        }
    }

    public void SetDefaultRegionFromRect(Rect rect)
    {
        defaultOrigin = rect.position;         // 左下角
        defaultWidth = rect.width;
        defaultHeight = rect.height;

        float left = defaultOrigin.x;
        float right = defaultOrigin.x + defaultWidth;
        float bottom = defaultOrigin.y;
        float top = defaultOrigin.y + defaultHeight;

        defaultLimit = new CameraLimitRegion(left, right, top, bottom, null,0);

        Debug.Log($"[Camera] 设置默认区域：{rect}");

        if (target == null)
        {
            Debug.LogWarning("未找到跟随目标，无法立即设置相机位置");
            return;
        }

        Vector3 desiredPos = new Vector3(target.position.x, target.position.y + yOffset, transform.position.z);

        if (defaultLimit.left.HasValue && defaultLimit.right.HasValue)
        {
            float leftBound = defaultLimit.left.Value + halfWidth;
            float rightBound = defaultLimit.right.Value - halfWidth;
            if (leftBound < rightBound)
            {
                desiredPos.x = Mathf.Clamp(desiredPos.x, leftBound, rightBound);
            }
        }

        if (defaultLimit.top.HasValue || defaultLimit.bottom.HasValue)
        {
            float topBound = defaultLimit.top.HasValue ? defaultLimit.top.Value - halfHeight : float.MaxValue;
            float bottomBound = defaultLimit.bottom.HasValue ? defaultLimit.bottom.Value + halfHeight : float.MinValue;
            if (bottomBound < topBound)
            {
                desiredPos.y = Mathf.Clamp(desiredPos.y, bottomBound, topBound);
            }
        }

        transform.position = desiredPos;
        smoothTargetPosition = desiredPos;
        isTransitioning = false;
    }
}

[System.Serializable]
public class CameraLimitRegion
{
    public float? left;
    public float? right;
    public float? top;
    public float? bottom;
    public float extraYOffset;

    public CameraRegionTrigger setter; // 谁设置的

    public CameraLimitRegion(float? left, float? right, float? top, float? bottom, CameraRegionTrigger setter, float extraYOffset)
    {
        this.left = left;
        this.right = right;
        this.top = top;
        this.bottom = bottom;
        this.setter = setter;
        this.extraYOffset = extraYOffset;
    }
}