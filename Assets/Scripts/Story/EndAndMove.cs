using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndAndMove : MonoBehaviour
{
    public Image UIphoto;

    public Sprite sprite1;
    public Sprite sprite3;
    private bool isSwitchActive;
    public StoryTrigger[] storyTriggers;
    private bool isSwitchCompleted;

    private Transform text;
    [SerializeField] private float switchTimeout;
    public Camera mainCamera;
    private CameraControl camControl;
    public Transform newTarget;
    public float smoothSpeed = 5f; // 平滑移动速度
    public float delayBeforeReturn = 2.0f; // 停留时间
    public bool useDirectMovement = false; // 是否使用直接移动方式
    public PlayerController playerController;
    // 调试标记
    public bool debugMode = true;

    [SerializeField] private string ExchangeText="交换";

    [SerializeField] private GameObject myCameraLimit;

    void Start()
    {
        // 获取主摄像机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            LogError("未找到主摄像机！");
            return;
        }

        // 获取相机控制组件
        camControl = mainCamera.GetComponent<CameraControl>();
        if (camControl == null)
        {
            LogError("主摄像机上没有CameraControl组件！将尝试添加一个");
            camControl = mainCamera.gameObject.AddComponent<CameraControl>();
        }

        Log("EndAndMove初始化成功");
    }
    // private void Start(){
    //     // 向StoryManager注册剧情完成事件
    //     if (StoryManager.Instance != null) {
    //         StoryManager.Instance.onExitStoryMode+=MoveEnd;
    //     }
    // }
    //
    // private void OnDestroy(){
    //     // 取消注册事件
    //     if (StoryManager.Instance != null) {
    //         StoryManager.Instance.onExitStoryMode-=MoveEnd;
    //     }
    // }
    // 方法1: 使用CameraControl的目标跟随功能移动
    public void StopPlayerMove(){
        playerController.DisableMovement();
        print("endAndMove禁用玩家移动");
    }
    public void StartFirstStory(){
        StoryTrigger trigger1=storyTriggers[0];
        trigger1.ForceStartStory();
    }

    public void EnablePicture(){
        // FindAnyObjectByType<CompanionController>().transform.GetComponent<Animator>().Play("robot_idle");
        UIphoto.gameObject.SetActive(true);
        StartCoroutine(WaitForInputAndDisablePicture());
        StoryManager.Instance._currentState = GameState.ActionMode;
        print("我还不能点，为什么");
    }

    private IEnumerator WaitForInputAndDisablePicture(){
        while(GameInput.Aim.Value == Vector2.zero){
            yield return null;
        }
        StartCoroutine(DisablePictureAfterDelay(1f, sprite1));
    }
    
    private IEnumerator DisablePictureAfterDelay(float disableTime,Sprite sprite){
        yield return new WaitForSeconds(disableTime);
        if(UIphoto.sprite==sprite)
        UIphoto.gameObject.SetActive(false);
    }

    public void ReturnCameraToPlayer(){
        camControl.isTransitioning = true; // 开启平滑过渡
        camControl.smoothSpeed = 3f; // 设置平滑速度
        Camera.main.transform.GetComponent<CameraControl>().target = playerController.transform;
        // Camera.main.transform.GetComponent<CameraControl>().RestoreCameraLimit();
        FindAnyObjectByType<CompanionController>().canFollow = true;
        camControl.hasLoadOnce=true;
        // PlayerPrefs.SetInt("hasLoadOnce",1);
        //结束跳跃面板时设置PlayerPrefs.SetInt("hasLoadOnce",1)
        FindAnyObjectByType<CompanionController>().transform.localScale = new Vector3(1f, 1f, 1f);
        // playerController.EnableMovement();
        StoryManager.Instance._currentState = GameState.ActionMode;
        print("我能点了吗");
        print("我不能");
    }
    public void MoveEnd()
    {
        print("Move End");
        if (camControl == null)
        {
            LogError("CameraControl组件为空，无法移动摄像机！");
            return;
        }

        if (newTarget == null)
        {
            LogError("未设置目标位置！");
            return;
        }

        Log("开始移动摄像机到: " + newTarget.position);

        if (useDirectMovement)//x
        {
            // 方法2: 直接使用协程控制摄像机位置
            StartCoroutine(MoveDirectly());
        }
        else
        {
            // 方法1: 使用目标跟随
            UseTargetFollow();
        }
    }

    public void MoveToStart()
    {



        // Transform tansTex = transform.Find("Square");
        // tansTex.gameObject.SetActive(true);
        //TODO:时间停止，玩家交换一次物体，结束时停
        // StartCoroutine(StartSwitchMode());
        
    }

    private IEnumerator StartSwitchMode(){
        isSwitchActive = true;
        Log("开始交换物体模式");

        // // 暂停游戏时间
        // Time.timeScale = 0;

        // 设置交换状态
        // if (GridManager.Instance != null) {
        //     // // 准备两个物体进行交换
        //     // if (switchableObj1 != null && switchableObj2 != null) {
        //     //     // 将两个物体添加到交换记录中
        //     //     GridManager.Instance.ForceSelectObjectsForSwitch(switchableObj1, switchableObj2);
        //     //     Log("已设置要交换的两个物体");
        //     // }
        //
        //     // 切换到交换状态
        //     GridManager.Instance.StartState(SwitchState.Switch);
        //     text=UIphoto.gameObject.transform.Find("Text (TMP)");
        //     text.GetComponent<TMP_Text>().text = "交换";
        //     Log("已进入交换状态");
        // }

        // 等待玩家完成交换或超时
        float timer = 0;
        while (!isSwitchCompleted) { //&& timer < switchTimeout 不计时了，换一次才消失
            // 检查是否交换了物体
            if (GridManager.Instance != null && GridManager.Instance.SwitchTime > 0) {
                // 交换完成
                isSwitchCompleted = true;
                StartCoroutine(DisablePictureAfterDelay(1f, sprite3));
                Log("交换物体完成!");
                camControl.endTeach = true;
                // 等待玩家确认（按键）
                yield return new WaitForSecondsRealtime(0.5f);
                break;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }
        StartCoroutine(DisablePictureAfterDelay(1f, sprite3));
        // 结束交换模式
        // EndSwitchMode();
    }

    private void EndSwitchMode(){
        // // 恢复游戏时间
        // Time.timeScale = 1;

        // 如果仍在交换状态，退出交换状态
        if (GridManager.Instance != null && GridManager.Instance.GetCurrentState() == SwitchState.Switch) {
            GridManager.Instance.StartState(SwitchState.None);
            Log("已退出交换状态");
        }

        isSwitchActive = false;
        
    }

    public void RuturnCamera(){
        //// 摄像机移回玩家
        camControl.target = playerController.transform;
        camControl.isTransitioning = true; // 开启平滑过渡
        camControl.smoothSpeed = smoothSpeed; // 设置平滑速度
        UIphoto.gameObject.SetActive(false);
    }
    // 使用目标跟随的方法
    private void UseTargetFollow()
    {
        // 保存原始目标
        Transform originalTarget = camControl.target;

        // 创建临时目标并移动
        GameObject tempTarget = new GameObject("TempTarget");
        tempTarget.transform.position = newTarget.position;

        // 设置摄像机控制参数
        camControl.target = tempTarget.transform;
        camControl.isTransitioning = true; // 开启平滑过渡
        camControl.smoothSpeed = 5f; // 设置平滑速度
        
        // text = UIphoto.gameObject.transform.Find("Text (TMP)");
        // text.GetComponent<TMP_Text>().text = "交换";
        Log("已设置摄像机目标并开启平滑过渡");
        // StoryTrigger tri = transform.GetComponent<StoryTrigger>();
        // if (tri != null && tri.nextStoryTrigger != null) {
        //     // StoryTrigger triNext = tri.nextStoryTrigger;
        //     
        //     // 注册剧情完成事件，在剧情完成后恢复相机
        //     StoryManager.Instance.onDialogueComplete += () => {
        //         // 确保只执行一次
        //         StoryManager.Instance.onDialogueComplete -= () => {};
        //         // 触发下一段剧情
        //         // triNext.ForceStartStory();
        //         // Log("已触发下一段剧情: " + triNext.name);
        //         // 恢复相机到原始目标
        //         if (camControl != null && originalTarget != null) {
        //             camControl.target = originalTarget;
        //             camControl.isTransitioning = true;
        //             Log("剧情完成，恢复摄像机原始目标");
        //         }
        //     };
        //
        //
        // }
        // 等待相机移动完成后返回原位
        // StartCoroutine(ResetCameraTarget(camControl, originalTarget, tempTarget, delayBeforeReturn));
        // playerController.EnableMovement();
        // // myCameraLimit.transform.GetComponent<CameraRegionTrigger>().RestoreCameraLimit();
        //
        // UIphoto.gameObject.SetActive(true);
        // text = UIphoto.gameObject.transform.Find("Text (TMP)");
        // text.GetComponent<TMP_Text>().text = ExchangeText;
        // // StartCoroutine(StartSwitchMode());
        //
    }

    public void BackCamera(){
        StartCoroutine(ResetCameraTarget(camControl, FindObjectOfType<PlayerController>().transform, newTarget.gameObject, delayBeforeReturn));
        playerController.EnableMovement();
        // myCameraLimit.transform.GetComponent<CameraRegionTrigger>().RestoreCameraLimit();
        GridManager.Instance.LockStates(false);
        UIphoto.gameObject.SetActive(true);
        UIphoto.sprite = sprite3;
        StartCoroutine(StartSwitchMode());
        // StartCoroutine(DisablePictureAfterDelay(5f,sprite3));
    }
    // 直接移动摄像机的方法
    private IEnumerator MoveDirectly()
    {
        Vector3 startPosition = mainCamera.transform.position;
        Vector3 targetPosition = new Vector3(newTarget.position.x, newTarget.position.y, mainCamera.transform.position.z);

        float elapsedTime = 0;

        while (elapsedTime < 1.0f)
        {
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * (smoothSpeed / 5);
            yield return null;
        }

        // 确保到达目标位置
        mainCamera.transform.position = targetPosition;
        Log("摄像机已直接移动到目标位置");
        
        // 停留一段时间
        yield return new WaitForSeconds(delayBeforeReturn);
        // UIphoto.SetActive(false);
        // 可以在这里实现返回原位的逻辑
    }

    // 恢复原始目标的协程
    private IEnumerator ResetCameraTarget(CameraControl cam, Transform original, GameObject temp, float delay)
    {
        // 等待延迟
        yield return new WaitForSeconds(delay);

        if (original != null)
        {
            Log("恢复摄像机原始目标: " + original.name);
            cam.target = playerController.transform;


            cam.isTransitioning = true; // 确保返回时也平滑过渡
            // myCameraLimit.transform.GetComponent<CameraRegionTrigger>().RestoreCameraLimit();
            LockCamera();
            // UIphoto.SetActive(false);
        }
        else
        {
            LogWarning("原始目标为空，无法返回");
        }

        // 销毁临时目标
        if (temp != null)
        {
            Destroy(temp);
        }
        
        // // 确保摄像机完全回到原位后恢复相机限制
        yield return new WaitUntil(() => !cam.isTransitioning);
        // playerController.EnableMovement();
        // myCameraLimit.transform.GetComponent<CameraRegionTrigger>().RestoreCameraLimit();

    }

    public void FreeCamera(){
        //camControl.Setted = false;
    }

    public void LockCamera(){
        //camControl.Setted = true;
    }
    
    
    
    // 调试日志方法
    private void Log(string message)
    {
        if (debugMode) Debug.Log("[<color=cyan>EndAndMove</color>] " + message);
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning("[EndAndMove] " + message);
    }

    private void LogError(string message)
    {
        Debug.LogError("[EndAndMove] " + message);
    }

    public void ForceEnterCollectStory(){
        //禁用移动,剧情结束后自动启用移动
        playerController.DisableInput();
        //在第一关
        if (levelManager.instance.currentLevelIndex == 1) {
            //收集成功,没播过
            if (GridManager.Instance.SwitchTime <=3&& !StoryGlobalLoadManager.instance.IsTriggerDisabled("场景1剧情_收集剧情1")) {
                storyTriggers[3].ForceStartStory();
            }
            //没成功
            else if(!StoryGlobalLoadManager.instance.IsTriggerDisabled("场景1剧情_收集剧情2")){
                storyTriggers[4].ForceStartStory();
            }
        }
    }
    public void ForceEnterConsciousnessStory(){
        storyTriggers[5].ForceStartStory();
    }
}
