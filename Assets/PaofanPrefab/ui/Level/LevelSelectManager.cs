using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }

    [Header("Button List")] public Button[] levelButtons; // 按钮数组

    [Header("Lock Settings")]
    [SerializeField] private GameObject lockImagePrefab; // 锁的图片预制体

    private GameObject[] lockInstances; // 用于存储每个按钮上的锁实例

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        // 初始化锁实例数组
        lockInstances = new GameObject[levelButtons.Length];

        // 为每个按钮创建锁并绑定点击事件
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelIndex = i + 1; // 关卡索引，从1开始
            int capturedIndex = levelIndex; // 捕获正确的索引值
            levelButtons[i].onClick.AddListener(() => LoadLevel(capturedIndex));

            // 为每个按钮创建锁的实例
            GameObject lockInstance = Instantiate(lockImagePrefab, levelButtons[i].transform);
            lockInstance.transform.SetAsLastSibling(); // 确保锁显示在最上层
            lockInstances[i] = lockInstance;
        }

        // 更新所有关卡的锁定状态
        UpdateLevelLockStatus();
    }

    // 公共方法用于刷新按钮状态
    public void RefreshButtons()
    {
        levelManager.instance.LoadUnlockedLevels();
        Debug.Log("Refreshing level select buttons");
        UpdateLevelLockStatus();
    }

    // 更新所有关卡的锁定状态
    public void UpdateLevelLockStatus()
    {
        if (levelButtons == null || levelButtons.Length == 0)
        {
            Debug.LogWarning("No level buttons found!");
            return;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (levelButtons[i] == null)
            {
                Debug.LogWarning($"Button at index {i} is null!");
                continue;
            }

            int levelIndex = i + 1;
            bool isUnlocked = levelManager.instance.IsLevelUnlocked(levelIndex);

            // 设置按钮是否可交互
            levelButtons[i].interactable = isUnlocked;

            // 设置锁的显示状态
            if (lockInstances != null && i < lockInstances.Length && lockInstances[i] != null)
            {
                lockInstances[i].SetActive(!isUnlocked);
            }
        }
    }

    // 加载对应关卡
    void LoadLevel(int levelIndex)
    {
        // 检查关卡是否解锁
        if (!levelManager.instance.IsLevelUnlocked(levelIndex))
        {
            Debug.Log($"Level {levelIndex} is locked!");
            return;
        }

        string levelName = "Level_" + levelIndex; // 拼接关卡名称
        levelManager.instance.LoadLevel(levelIndex, true); // 加载场景
    }
}