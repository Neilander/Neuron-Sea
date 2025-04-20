using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine; // 假设使用Cinemachine摄像机系统

/// <summary>
/// 剧情序列示例，展示如何在剧情间移动摄像机和其他操作
/// </summary>
public class StorySequenceExample : MonoBehaviour
{
    [Header("摄像机设置")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // 虚拟摄像机
    [SerializeField] private Transform[] cameraPositions; // 摄像机位置列表
    [SerializeField] private float cameraMoveSpeed = 2f; // 摄像机移动速度
    
    [Header("剧情触发器")]
    [SerializeField] private StoryTrigger initialStoryTrigger; // 初始剧情触发器
    
    // 当前活动的协程
    private Coroutine activeCameraMovement;
    
    private void Start()
    {
        // 如果设置了初始剧情触发器，注册事件
        if (initialStoryTrigger != null)
        {
            // 注册第一段剧情完成事件
            initialStoryTrigger.onExitSpecificStory.AddListener(OnFirstStoryComplete);
        }
    }
    
    /// <summary>
    /// 第一段剧情完成时的处理
    /// </summary>
    private void OnFirstStoryComplete()
    {
        Debug.Log("第一段剧情完成，开始移动摄像机...");
        
        // 如果有活动的摄像机移动协程，停止它
        if (activeCameraMovement != null)
        {
            StopCoroutine(activeCameraMovement);
        }
        
        // 开始移动摄像机
        if (virtualCamera != null && cameraPositions.Length > 0)
        {
            activeCameraMovement = StartCoroutine(MoveCameraToPosition(cameraPositions[0]));
        }
    }
    
    /// <summary>
    /// 移动摄像机到指定位置
    /// </summary>
    private IEnumerator MoveCameraToPosition(Transform targetPosition)
    {
        // 获取摄像机的跟随目标
        Transform cameraFollow = virtualCamera.Follow;
        
        // 临时取消摄像机的跟随目标，使其可以独立移动
        virtualCamera.Follow = null;
        
        // 获取初始位置
        Vector3 startPosition = virtualCamera.transform.position;
        
        // 计算移动时间
        float distance = Vector3.Distance(startPosition, targetPosition.position);
        float moveDuration = distance / cameraMoveSpeed;
        float elapsedTime = 0f;
        
        // 平滑移动摄像机
        while (elapsedTime < moveDuration)
        {
            // 计算当前进度
            float t = elapsedTime / moveDuration;
            
            // 平滑插值
            virtualCamera.transform.position = Vector3.Lerp(startPosition, targetPosition.position, t);
            virtualCamera.transform.rotation = Quaternion.Slerp(virtualCamera.transform.rotation, targetPosition.rotation, t);
            
            // 更新时间
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // 确保精确位置
        virtualCamera.transform.position = targetPosition.position;
        virtualCamera.transform.rotation = targetPosition.rotation;
        
        // 短暂延迟
        yield return new WaitForSeconds(1f);
        
        // 通知可以开始下一段剧情
        OnCameraMovementComplete();
        
        activeCameraMovement = null;
    }
    
    /// <summary>
    /// 摄像机移动完成后的处理
    /// </summary>
    private void OnCameraMovementComplete()
    {
        Debug.Log("摄像机移动完成，开始下一段剧情...");
        
        // 你可以在这里手动触发下一段剧情，或者让StoryTrigger自动处理
        // 例如：nextStoryTrigger.ForceStartStory();
    }
}