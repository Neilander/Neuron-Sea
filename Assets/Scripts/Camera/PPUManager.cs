using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;
// Unity URP中PixelPerfectCamera的其他可能命名空间
#if UNITY_2020_2_OR_NEWER
#endif

/// <summary>
/// PPU管理器 - 更高级的像素完美相机管理
/// 可以为不同场景设置不同的目标PPU值，并提供平滑过渡
/// </summary>
public class PPUManager : MonoBehaviour
{
    [System.Serializable]
    public class ScenePPUSetting
    {
        public string sceneName;  // 场景名称
        public int targetPPU = 32;  // 该场景的目标PPU值
    }

    [Header("基本设置")]
    [SerializeField] private int defaultStartPPU = 100;  // 默认起始PPU值
    [SerializeField] private int defaultTargetPPU = 32;  // 默认目标PPU值
    [SerializeField] private float transitionDuration = 2.0f;  // 过渡持续时间(秒)
    [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);  // 过渡动画曲线

    [Header("场景特定设置")]
    [SerializeField] private List<ScenePPUSetting> sceneSettings = new List<ScenePPUSetting>();  // 各场景的PPU设置

    [Header("组件引用")]
    [SerializeField] private PixelPerfectCamera pixelPerfectCamera;  // 像素完美摄像机组件

    private Dictionary<string, int> sceneTargetPPUMap = new Dictionary<string, int>();  // 场景名称到目标PPU的映射
    private Coroutine currentTransition;  // 当前正在执行的过渡协程
    private int lastTargetPPU;  // 上一个目标PPU值

    // 单例模式
    public static PPUManager Instance { get; private set; }

    private void Awake()
    {
        // 单例模式设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 如果没有手动指定PixelPerfectCamera组件，尝试获取
        if (pixelPerfectCamera == null)
        {
            pixelPerfectCamera = GetComponent<PixelPerfectCamera>();
            if (pixelPerfectCamera == null)
            {
                pixelPerfectCamera = Camera.main?.GetComponent<PixelPerfectCamera>();
                if (pixelPerfectCamera == null)
                {
                    Debug.LogError("未找到PixelPerfectCamera组件！请确保组件存在或正确指定。");
                    enabled = false;
                    return;
                }
            }
        }

        // 初始化场景到目标PPU的映射
        InitializeScenePPUMap();

        // 注册场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 初始设置
        pixelPerfectCamera.assetsPPU = defaultStartPPU;
    }

    private void OnDestroy()
    {
        // 取消注册场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 初始场景处理
        string currentSceneName = SceneManager.GetActiveScene().name;
        int targetPPU = GetTargetPPUForScene(currentSceneName);
        StartPPUTransition(defaultStartPPU, targetPPU);
    }

    /// <summary>
    /// 初始化场景PPU映射
    /// </summary>
    private void InitializeScenePPUMap()
    {
        sceneTargetPPUMap.Clear();
        foreach (var setting in sceneSettings)
        {
            if (!string.IsNullOrEmpty(setting.sceneName))
            {
                sceneTargetPPUMap[setting.sceneName] = setting.targetPPU;
            }
        }
    }

    /// <summary>
    /// 场景加载时的回调
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 获取当前场景的目标PPU值
        int targetPPU = GetTargetPPUForScene(scene.name);

        // 记住上一个值作为起点
        int startPPU = pixelPerfectCamera.assetsPPU;

        // 如果当前已经有过渡在进行，先停止
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // 开始新的过渡
        StartPPUTransition(startPPU, targetPPU);
    }

    /// <summary>
    /// 获取指定场景的目标PPU值
    /// </summary>
    private int GetTargetPPUForScene(string sceneName)
    {
        if (sceneTargetPPUMap.TryGetValue(sceneName, out int targetPPU))
        {
            return targetPPU;
        }
        return defaultTargetPPU;  // 返回默认值
    }

    /// <summary>
    /// 开始PPU过渡
    /// </summary>
    private void StartPPUTransition(int startPPU, int targetPPU)
    {
        currentTransition = StartCoroutine(TransitionPPU(startPPU, targetPPU));
        lastTargetPPU = targetPPU;
    }

    /// <summary>
    /// 平滑过渡PPU值的协程
    /// </summary>
    private IEnumerator TransitionPPU(int startPPU, int targetPPU)
    {
        // 如果起始值和目标值相同，无需过渡
        if (startPPU == targetPPU)
        {
            pixelPerfectCamera.assetsPPU = targetPPU;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / transitionDuration);

            // 使用动画曲线平滑过渡
            float curvedT = transitionCurve.Evaluate(t);

            // 计算当前PPU值
            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(startPPU, targetPPU, curvedT));

            // 应用到摄像机
            pixelPerfectCamera.assetsPPU = currentPPU;

            yield return null;
        }

        // 确保最终设置为目标值
        pixelPerfectCamera.assetsPPU = targetPPU;

        Debug.Log($"PPU过渡完成: {startPPU} -> {targetPPU}");
        currentTransition = null;
    }

    /// <summary>
    /// 公开方法：手动触发PPU过渡到指定值
    /// </summary>
    public void TransitionToTargetPPU(int targetPPU, float customDuration = -1)
    {
        if (pixelPerfectCamera == null) return;

        int startPPU = pixelPerfectCamera.assetsPPU;

        // 如果当前已经有过渡在进行，先停止
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }

        // 使用自定义持续时间或默认值
        float duration = customDuration > 0 ? customDuration : transitionDuration;

        // 开始新的过渡
        currentTransition = StartCoroutine(TransitionPPU(startPPU, targetPPU, duration));
        lastTargetPPU = targetPPU;
    }

    /// <summary>
    /// 重载的过渡方法，支持自定义持续时间
    /// </summary>
    private IEnumerator TransitionPPU(int startPPU, int targetPPU, float duration)
    {
        // 如果起始值和目标值相同，无需过渡
        if (startPPU == targetPPU)
        {
            pixelPerfectCamera.assetsPPU = targetPPU;
            yield break;
        }

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);

            // 使用动画曲线平滑过渡
            float curvedT = transitionCurve.Evaluate(t);

            // 计算当前PPU值
            int currentPPU = Mathf.RoundToInt(Mathf.Lerp(startPPU, targetPPU, curvedT));

            // 应用到摄像机
            pixelPerfectCamera.assetsPPU = currentPPU;

            yield return null;
        }

        // 确保最终设置为目标值
        pixelPerfectCamera.assetsPPU = targetPPU;

        Debug.Log($"PPU过渡完成: {startPPU} -> {targetPPU}，持续时间: {duration}秒");
        currentTransition = null;
    }

    /// <summary>
    /// 公开方法：立即设置PPU值，不使用过渡效果
    /// </summary>
    public void SetPPUImmediate(int ppu)
    {
        if (pixelPerfectCamera == null) return;

        // 如果当前有过渡在进行，先停止
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
            currentTransition = null;
        }

        pixelPerfectCamera.assetsPPU = ppu;
        lastTargetPPU = ppu;
    }

    /// <summary>
    /// 公开方法：添加或更新场景PPU设置
    /// </summary>
    public void SetScenePPU(string sceneName, int targetPPU)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        sceneTargetPPUMap[sceneName] = targetPPU;

        // 如果是当前场景，立即开始过渡
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            TransitionToTargetPPU(targetPPU);
        }
    }
}