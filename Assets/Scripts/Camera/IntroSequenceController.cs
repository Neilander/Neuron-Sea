using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 游戏开场序列控制器
/// 管理游戏开场的动画和相机变换效果，展示如何使用CameraSequencePlayer
/// </summary>
public class IntroSequenceController : MonoBehaviour
{
    [Header("组件引用")]
    [SerializeField] private CameraSequencePlayer cameraSequencer;
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private GameObject introUIPanel;
    [SerializeField] private AudioSource introMusic;
    
    [Header("序列设置")]
    [SerializeField] private float initialDelay = 1.0f;  // 游戏开始后延迟多久开始播放序列
    [SerializeField] private bool skipIfAlreadyPlayed = true;  // 如果已经播放过则跳过

    // 用于记录是否已经播放过
    private static bool hasIntroBeenPlayed = false;
    
    private void Awake()
    {
        // 检查组件
        if (cameraSequencer == null)
        {
            cameraSequencer = GetComponent<CameraSequencePlayer>();
            if (cameraSequencer == null)
            {
                Debug.LogError("未找到CameraSequencePlayer组件！");
                enabled = false;
                return;
            }
        }

        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();
        }

        // 设置初始状态
        if (playerObject != null)
        {
            playerObject.SetActive(false);  // 在序列播放期间隐藏玩家
        }

        if (introUIPanel != null)
        {
            introUIPanel.SetActive(false);  // 初始隐藏UI
        }
    }

    private void Start()
    {
        // 如果设置了跳过且已经播放过，直接跳到最终状态
        if (skipIfAlreadyPlayed && hasIntroBeenPlayed)
        {
            SkipToFinalState();
            return;
        }

        // 延迟一段时间后开始播放序列
        if (initialDelay > 0)
        {
            StartCoroutine(DelayedStart());
        }
        else
        {
            StartIntroSequence();
        }
    }

    /// <summary>
    /// 延迟开始播放序列
    /// </summary>
    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(initialDelay);
        StartIntroSequence();
    }

    /// <summary>
    /// 开始播放开场序列
    /// </summary>
    public void StartIntroSequence()
    {
        // 播放开场音乐
        if (introMusic != null)
        {
            introMusic.Play();
        }

        // 显示开场UI
        if (introUIPanel != null)
        {
            introUIPanel.SetActive(true);
        }

        // 启动序列播放器
        if (cameraSequencer != null)
        {
            cameraSequencer.PlaySequence();
        }

        // 标记为已播放
        hasIntroBeenPlayed = true;
    }

    /// <summary>
    /// 序列开始前的准备工作
    /// 这个方法可以绑定到CameraSequencePlayer的onSequenceStart事件
    /// </summary>
    public void OnSequenceStart()
    {
        Debug.Log("开场序列开始播放");
        
        // 确保玩家不可见/不可控制
        if (playerObject != null)
        {
            playerObject.SetActive(false);
        }
    }

    /// <summary>
    /// 在相机过渡前的准备工作
    /// 这个方法可以绑定到CameraSequencePlayer的beforeTransition事件
    /// </summary>
    public void BeforeCameraTransition()
    {
        Debug.Log("准备开始相机过渡");
        
        // 可以在这里做一些准备工作
        // 例如播放过渡音效、闪屏效果等
    }

    /// <summary>
    /// 在相机过渡后的工作
    /// 这个方法可以绑定到CameraSequencePlayer的afterTransition事件
    /// </summary>
    public void AfterCameraTransition()
    {
        Debug.Log("相机过渡完成");
        
        // 可以在这里做一些后续工作
        // 例如显示游戏元素、启动游戏逻辑等
    }

    /// <summary>
    /// 序列完成后的工作
    /// 这个方法可以绑定到CameraSequencePlayer的onSequenceComplete事件
    /// </summary>
    public void OnSequenceComplete()
    {
        Debug.Log("开场序列播放完成");
        
        // 隐藏开场UI
        if (introUIPanel != null)
        {
            introUIPanel.SetActive(false);
        }

        // 显示玩家并启用控制
        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }

        // 可以在这里启动游戏主逻辑
        // GameManager.Instance.StartGame();
    }

    /// <summary>
    /// 跳过开场序列，直接设置到最终状态
    /// </summary>
    public void SkipToFinalState()
    {
        Debug.Log("跳过开场序列");
        
        // 设置相机到最终状态
        if (pixelPerfectCamera != null)
        {
            pixelPerfectCamera.assetsPPU = 32;  // 直接设置为目标值
        }
        
        // 隐藏开场UI
        if (introUIPanel != null)
        {
            introUIPanel.SetActive(false);
        }
        
        // 显示玩家
        if (playerObject != null)
        {
            playerObject.SetActive(true);
        }
        
        // 标记为已播放
        hasIntroBeenPlayed = true;
    }

    /// <summary>
    /// 重置开场序列状态（用于测试）
    /// </summary>
    public void ResetIntroState()
    {
        hasIntroBeenPlayed = false;
    }
}