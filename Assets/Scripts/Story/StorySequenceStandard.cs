using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity;
using UnityEngine.Experimental.Rendering.Universal;

/// <summary>
/// 剧情序列标准版本，使用普通Camera组件而非Cinemachine
/// </summary>
public class StorySequenceStandard : MonoBehaviour
{
    [Header("摄像机设置")]
    [SerializeField] private Camera mainCamera; // 主摄像机
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera; // 像素完美摄像机组件
    [SerializeField] private Transform[] cameraPositions; // 摄像机位置列表
    [SerializeField] private float cameraMoveSpeed = 2f; // 摄像机移动速度
    [SerializeField] private float rotationSpeed = 2f; // 摄像机旋转速度

    [Header("剧情设置")]
    [SerializeField] private StoryTrigger initialStoryTrigger; // 初始剧情触发器
    [SerializeField] private StoryTrigger nextStoryTrigger; // 下一个剧情触发器
    [SerializeField] private StoryData storyData; // 剧情数据，可以直接指定
    [SerializeField] private float delayBeforeStory = 0.5f; // 摄像机移动到位后，播放剧情前的延迟
    [SerializeField] private float delayAfterStory = 0.5f; // 剧情结束后，移回摄像机前的延迟

    [Header("摄像机跟随设置")]
    [SerializeField] private Transform defaultFollowTarget; // 默认跟随目标
    [SerializeField] private float followDistance = 5f; // 跟随距离
    [SerializeField] private Vector3 followOffset = new Vector3(0, 2, 0); // 跟随偏移

    // 当前活动的协程
    private Coroutine activeCameraMovement;
    // 摄像机初始设置
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Transform cameraFollowTarget;
    private bool isFollowingTarget = false;
    // 像素完美摄像机设置
    private bool wasPixelPerfect = false;

    private void Awake()
    {
        // 如果没有指定摄像机，尝试获取主摄像机
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("未找到主摄像机！请手动指定摄像机引用。");
            }
        }

        // 获取PixelPerfectCamera组件（如果存在）
        if (pixelPerfectCamera == null && mainCamera != null)
        {
            pixelPerfectCamera = mainCamera.GetComponent<PixelPerfectCamera>();
        }

        // 保存初始设置
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            // originalCameraRotation = mainCamera.transform.rotation;
        }

        // 设置默认跟随目标
        cameraFollowTarget = defaultFollowTarget;
        isFollowingTarget = (cameraFollowTarget != null);
    }

    private void Start()
    {
        // 如果设置了初始剧情触发器，注册事件
        if (initialStoryTrigger != null)
        {
            // 注册剧情完成事件
            initialStoryTrigger.onExitSpecificStory.AddListener(OnFirstStoryComplete);
        }
    }

    private void LateUpdate()
    {
        // 如果没有活动的摄像机移动，并且需要跟随目标
        if (activeCameraMovement == null && isFollowingTarget && cameraFollowTarget != null)
        {
            // 计算跟随位置
            Vector3 targetPosition = cameraFollowTarget.position + followOffset;
            Vector3 followPosition = targetPosition - mainCamera.transform.forward * followDistance;

            // 平滑移动摄像机
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                followPosition,
                cameraMoveSpeed * Time.deltaTime
            );

            // 注意：这里不使用LookAt，而是保持摄像机的当前朝向
        }
    }

    /// <summary>
    /// 第一段剧情完成时的处理
    /// </summary>
    private void OnFirstStoryComplete()
    {
        Debug.Log("第一段剧情完成，开始移动摄像机...");

        // 停止跟随目标
        isFollowingTarget = false;

        // 如果有活动的摄像机移动协程，停止它
        if (activeCameraMovement != null)
        {
            StopCoroutine(activeCameraMovement);
        }

        // 开始移动摄像机
        if (mainCamera != null && cameraPositions.Length > 0)
        {
            activeCameraMovement = StartCoroutine(ExecuteStorySequence(0));
        }
    }

    /// <summary>
    /// 执行完整的剧情序列：移动摄像机->播放剧情->移回摄像机
    /// </summary>
    private IEnumerator ExecuteStorySequence(int cameraPositionIndex)
    {
        if (mainCamera == null || cameraPositionIndex >= cameraPositions.Length) yield break;

        // 1. 移动摄像机到指定位置
        Debug.Log("1. 开始移动摄像机到指定位置...");

        // // 如果有PixelPerfectCamera组件，暂时禁用它以获得平滑移动
        // if (pixelPerfectCamera != null)
        // {
        //     wasPixelPerfect = pixelPerfectCamera.enabled;
        //     pixelPerfectCamera.enabled = false;
        // }

        yield return StartCoroutine(MoveCameraToPosition(cameraPositions[cameraPositionIndex], false));

        // 等待指定时间
        yield return new WaitForSeconds(delayBeforeStory);

        // 2. 播放剧情
        Debug.Log("2. 开始播放剧情...");

        bool storyPlayed = false;

        // 如果有下一个剧情触发器，则使用它
        if (nextStoryTrigger != null)
        {
            nextStoryTrigger.onExitSpecificStory.AddListener(() => storyPlayed = true);
            nextStoryTrigger.ForceStartStory();

            // 等待剧情完成
            while (!storyPlayed)
            {
                yield return null;
            }

            // 移除监听器
            nextStoryTrigger.onExitSpecificStory.RemoveListener(() => storyPlayed = true);
        }
        // 如果直接指定了剧情数据，则使用它
        else if (storyData != null)
        {
            StoryManager.Instance.onExitStoryMode.AddListener(() => storyPlayed = true);
            StoryManager.Instance.EnterStoryMode(storyData);

            // 等待剧情完成
            while (!storyPlayed)
            {
                yield return null;
            }

            // 移除监听器
            StoryManager.Instance.onExitStoryMode.RemoveListener(() => storyPlayed = true);
        }
        else
        {
            Debug.LogWarning("未设置剧情数据或触发器，跳过剧情播放环节");
            storyPlayed = true;
        }

        // 等待指定时间
        yield return new WaitForSeconds(delayAfterStory);

        // 3. 移回摄像机到原始位置
        Debug.Log("3. 移回摄像机到原始位置...");

        // 创建一个临时的Transform来表示原始位置
        GameObject tempObj = new GameObject("TempOriginalCameraPosition");
        tempObj.transform.position = originalCameraPosition;
        tempObj.transform.rotation = originalCameraRotation;

        yield return StartCoroutine(MoveCameraToPosition(tempObj.transform, true));

        Destroy(tempObj);

        // 恢复PixelPerfectCamera组件
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.enabled = wasPixelPerfect;
        }

        // 恢复跟随状态
        if (defaultFollowTarget != null)
        {
            cameraFollowTarget = defaultFollowTarget;
            isFollowingTarget = true;
        }

        Debug.Log("完整剧情序列执行完毕");
        activeCameraMovement = null;
    }

    /// <summary>
    /// 移动摄像机到指定位置
    /// </summary>
    private IEnumerator MoveCameraToPosition(Transform targetPosition, bool isReturning = false)
    {
        if (mainCamera == null) yield break;

        // 获取初始位置和旋转
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 计算移动时间
        float distance = Vector3.Distance(startPosition, targetPosition.position);
        float moveDuration = distance / cameraMoveSpeed;
        float elapsedTime = 0f;

        // 平滑移动摄像机
        while (elapsedTime < moveDuration)
        {
            // 计算当前进度
            float t = elapsedTime / moveDuration;

            // 使用平滑曲线
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 平滑插值位置和旋转
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition.position, smoothT);
            // mainCamera.transform.rotation = Quaternion.Slerp(startRotation, targetPosition.rotation, smoothT * rotationSpeed);

            // 更新时间
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保精确位置和旋转
        mainCamera.transform.position = targetPosition.position;
        // mainCamera.transform.rotation = targetPosition.rotation;

        // 不触发摄像机移动完成事件，因为我们在ExecuteStorySequence中处理了完整流程
        if (!isReturning)
        {
            Debug.Log("摄像机移动到指定位置完成");
        }
        else
        {
            Debug.Log("摄像机移回原始位置完成");
        }
    }

    /// <summary>
    /// 重置摄像机到初始状态
    /// </summary>
    public void ResetCamera()
    {
        // 停止任何活动的摄像机移动
        if (activeCameraMovement != null)
        {
            StopCoroutine(activeCameraMovement);
            activeCameraMovement = null;
        }

        // 恢复原始位置和旋转
        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCameraPosition;
            // mainCamera.transform.rotation = originalCameraRotation;
        }

        // 恢复PixelPerfectCamera状态
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.enabled = wasPixelPerfect;
        }

        // 恢复跟随状态
        if (defaultFollowTarget != null)
        {
            cameraFollowTarget = defaultFollowTarget;
            isFollowingTarget = true;
        }
        else
        {
            isFollowingTarget = false;
        }
    }

    /// <summary>
    /// 设置摄像机跟随目标
    /// </summary>
    public void SetCameraFollowTarget(Transform target, bool startFollowing = true)
    {
        cameraFollowTarget = target;
        isFollowingTarget = startFollowing && (target != null);

        // 如果有活动的摄像机移动，停止它
        if (activeCameraMovement != null && startFollowing)
        {
            StopCoroutine(activeCameraMovement);
            activeCameraMovement = null;
        }
    }

    /// <summary>
    /// 启动完整的剧情序列：摄像机移动->播放剧情->摄像机移回
    /// </summary>
    public void StartFullStorySequence(int cameraPositionIndex = 0)
    {
        if (cameraPositionIndex < 0 || cameraPositionIndex >= cameraPositions.Length)
        {
            Debug.LogError($"摄像机位置索引 {cameraPositionIndex} 超出范围 (0-{cameraPositions.Length - 1})");
            return;
        }

        // 停止跟随
        isFollowingTarget = false;

        // 停止任何活动的摄像机移动
        if (activeCameraMovement != null)
        {
            StopCoroutine(activeCameraMovement);
        }

        // 开始新的剧情序列
        activeCameraMovement = StartCoroutine(ExecuteStorySequence(cameraPositionIndex));
    }

    /// <summary>
    /// 设置剧情数据
    /// </summary>
    public void SetStoryData(StoryData newStoryData)
    {
        storyData = newStoryData;
    }
}