using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 管理聚光灯展示效果
/// 自动在第一个场景加载时添加聚光灯效果
/// </summary>
public class SpotlightManager : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("聚光灯跟随的目标（通常是玩家）")]
    public Transform playerTransform;

    [Header("效果设置")]
    [Tooltip("是否在场景加载时自动添加效果")]
    public bool autoAddOnFirstScene = true;

    [Tooltip("第一个场景的名称（用于自动检测）")]
    public string firstSceneName = "";

    [Tooltip("聚光灯效果预制体（如果为空，将动态创建）")]
    public GameObject spotlightPrefab;

    private SpotlightReveal currentSpotlight;

    private void Awake()
    {
        // 确保场景切换时不销毁此对象
        DontDestroyOnLoad(gameObject);

        // 注册场景加载事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // 注销场景加载事件
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 检查是否是第一个场景
        if (autoAddOnFirstScene && (scene.name == firstSceneName || string.IsNullOrEmpty(firstSceneName)))
        {
            // 如果没有指定第一个场景名称，默认使用第一次加载的场景
            if (string.IsNullOrEmpty(firstSceneName))
            {
                firstSceneName = scene.name;
            }

            // 添加聚光灯效果
            AddSpotlightEffect();
        }
    }

    /// <summary>
    /// 手动添加聚光灯效果
    /// </summary>
    public void AddSpotlightEffect()
    {
        // 如果已存在聚光灯效果，先销毁
        if (currentSpotlight != null)
        {
            Destroy(currentSpotlight.gameObject);
        }

        // 创建聚光灯效果对象
        GameObject spotlightObj;
        if (spotlightPrefab != null)
        {
            spotlightObj = Instantiate(spotlightPrefab);
        }
        else
        {
            spotlightObj = new GameObject("SpotlightEffect");
            currentSpotlight = spotlightObj.AddComponent<SpotlightReveal>();
        }

        // 确保不随场景切换销毁
        DontDestroyOnLoad(spotlightObj);

        // 获取聚光灯组件
        currentSpotlight = spotlightObj.GetComponent<SpotlightReveal>();
        if (currentSpotlight == null)
        {
            Debug.LogError("无法找到SpotlightReveal组件！");
            return;
        }

        // 设置玩家对象
        if (playerTransform != null)
        {
            currentSpotlight.targetToFollow = playerTransform;
        }
        else
        {
            // 尝试查找玩家对象
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                currentSpotlight.targetToFollow = player.transform;
            }
            else
            {
                Debug.LogWarning("无法找到玩家对象，聚光灯效果将居中显示");
            }
        }
    }
}