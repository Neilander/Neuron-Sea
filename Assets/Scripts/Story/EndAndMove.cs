using System;
using System.Collections;
using UnityEngine;

public class EndAndMove : MonoBehaviour
{

    public Camera mainCamera;
    private CameraControl camControl;
    public Transform newTarget;
    public float smoothSpeed = 5f; // 平滑移动速度
    public float delayBeforeReturn = 2.0f; // 停留时间
    public bool useDirectMovement = false; // 是否使用直接移动方式
    public PlayerController playerController;
    // 调试标记
    public bool debugMode = true;

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

        if (useDirectMovement)
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



        Transform tansTex = transform.Find("Square");
        tansTex.gameObject.SetActive(true);
        //TODO:时间停止，玩家交换一次物体，结束时停

        //// 摄像机移回玩家
        camControl.target = playerController.transform;
        camControl.isTransitioning = true; // 开启平滑过渡
        camControl.smoothSpeed = smoothSpeed; // 设置平滑速度
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
        camControl.smoothSpeed = smoothSpeed; // 设置平滑速度

        Log("已设置摄像机目标并开启平滑过渡");
        // StoryTrigger tri = transform.GetComponent<StoryTrigger>();
        // if (tri != null && tri.nextStoryTrigger != null) {
        //     StoryTrigger triNext = tri.nextStoryTrigger;
        //     
        //     // 注册剧情完成事件，在剧情完成后恢复相机
        //     StoryManager.Instance.onDialogueComplete += () => {
        //         // 确保只执行一次
        //         StoryManager.Instance.onDialogueComplete -= () => {};
        //         // 触发下一段剧情
        //         triNext.ForceStartStory();
        //         Log("已触发下一段剧情: " + triNext.name);
        //         // 恢复相机到原始目标
        //         if (camControl != null && originalTarget != null) {
        // //             camControl.target = originalTarget;
        // //             camControl.isTransitioning = true;
        //             Log("剧情完成，恢复摄像机原始目标");
        //         }
        //     };


        // }
        // // 等待相机移动完成后返回原位
        // StartCoroutine(ResetCameraTarget(camControl, originalTarget, tempTarget, delayBeforeReturn));
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
            cam.target = original;


            cam.isTransitioning = true; // 确保返回时也平滑过渡
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
}
