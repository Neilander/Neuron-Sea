using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class LevelSelectManager : MonoBehaviour
{
    public static LevelSelectManager Instance { get; private set; }

    [Header("特殊按钮设置")]
    [SerializeField] private Button[] specialButtons;      // 特殊按钮数组
    [SerializeField] private Sprite normalSprite;          // 普通激活状态图片
    [SerializeField] private Sprite graySprite;           // 灰色状态图片
    [SerializeField] private Sprite finishSprite;         // 完成状态图片
    [SerializeField] private int[] checkpoints = { 1, 2, 3 };  // 检查点关卡

    [Header("Button List")] public Button[] levelButtons; // 按钮数组
    [Header("收集的Transform")]
    public Transform scene1;
    public Transform scene2;
    public Transform scene3;

    [Header("Lock Settings")]
    [SerializeField] private GameObject lockImagePrefab; // 锁的图片预制体

    private GameObject[] lockInstances; // 用于存储每个按钮上的锁实例

    #region state
    [SerializeField] private Sprite[] normalSprites; // 每个按钮的 normal 状态图

    [SerializeField] private Sprite[] graySprites; // 每个按钮的 gray 状态图

    [SerializeField] private Sprite[] finishSprites; // 每个按钮的 finish 状态图
     #endregion

     #region open

     [SerializeField] private GameObject openLockPrefab; // 开锁的图片预制体

     private GameObject[] openLockInstances; // 存储每个按钮的开锁图

     

     #endregion

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
        CollectActiveButtonsFrom(scene1, scene2, scene3);
        // 初始化锁实例数组
        lockInstances = new GameObject[levelButtons.Length];
        openLockInstances = new GameObject[levelButtons.Length];
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

            GameObject openLockInstance = Instantiate(openLockPrefab, levelButtons[i].transform);
            openLockInstance.transform.SetAsLastSibling(); // 确保显示在最上层
            openLockInstance.SetActive(false); // 默认隐藏
            openLockInstances[i] = openLockInstance;
        }
        InitializeSpecialButtons();
        // 更新所有关卡的锁定状态
        UpdateLevelLockStatus();
        UpdateSpecialButtons();
    }

    public void CollectActiveButtonsFrom(Transform t1, Transform t2, Transform t3)
    {
        List<Button> allButtons = new List<Button>();

        AddActiveButtons(t1, allButtons);
        AddActiveButtons(t2, allButtons);
        AddActiveButtons(t3, allButtons);
        if (t1.GetComponent<LevelNameSetter>() != null)
            t1.GetComponent<LevelNameSetter>().ParseAndSetTexts();

        if (t2.GetComponent<LevelNameSetter>() != null)
            t2.GetComponent<LevelNameSetter>().ParseAndSetTexts();

        if (t3.GetComponent<LevelNameSetter>() != null)
            t3.GetComponent<LevelNameSetter>().ParseAndSetTexts();

        levelButtons = allButtons.ToArray();
        Debug.Log($"共收集到 {levelButtons.Length} 个按钮");
    }

    private void AddActiveButtons(Transform root, List<Button> list)
    {
        if (root == null) return;

        Button[] buttons = root.GetComponentsInChildren<Button>(includeInactive: true);
        foreach (Button btn in buttons)
        {
                list.Add(btn);
        }
    }
    private void InitializeSpecialButtons()
    {
        if (specialButtons == null || specialButtons.Length < 3) 
        {
            Debug.LogError("需要设置3个特殊按钮！");
            return;
        }

        // 设置初始状态
        for (int i = 0; i < specialButtons.Length; i++)
        {
            if (specialButtons[i] == null){
                Debug.LogError($"specialButtons[{i}] 没有赋值！");
            }

            Image buttonImage = specialButtons[i].GetComponent<Image>();
            if (buttonImage != null)
            {
                if (i == 0) // 第一个按钮初始激活
                {
                    buttonImage.sprite = normalSprites[i];
                    specialButtons[i].interactable = true;
                }
                else // 其他按钮初始灰色
                {
                    buttonImage.sprite = graySprites[i];
                    specialButtons[i].interactable = false;
                }
            }
        }
    }

    private void UpdateSpecialButtons()
    {
        if (specialButtons == null || specialButtons.Length < 3) return;

        bool isLevel12Unlocked = levelManager.instance.IsLevelUnlocked(checkpoints[0]);
        bool isLevel24Unlocked = levelManager.instance.IsLevelUnlocked(checkpoints[1]);
        bool isLevel36Unlocked = levelManager.instance.IsLevelUnlocked(checkpoints[2]);

        // 更新第一个按钮
        UpdateSpecialButton(0, true, isLevel12Unlocked);

        // 更新第二个按钮
        if (isLevel12Unlocked)
        {
            UpdateSpecialButton(1, true, isLevel24Unlocked);
        }
        else
        {
            UpdateSpecialButton(1, false, false);
        }

        // 更新第三个按钮
        if (isLevel24Unlocked)
        {
            UpdateSpecialButton(2, true, isLevel36Unlocked);
        }
        else
        {
            UpdateSpecialButton(2, false, false);
        }
    }

    // 辅助方法：更新单个特殊按钮的状态
    /// <summary>
    /// 更新单个特殊按钮的状态
    /// </summary>
    /// <param name="index">按钮索引</param>
    /// <param name="isActive">激活</param>
    /// <param name="isFinished">完成</param>
    private void UpdateSpecialButton(int index, bool isActive, bool isFinished)
    {
        if (specialButtons[index] == null) return;

        Image buttonImage = specialButtons[index].GetComponent<Image>();
        if (buttonImage != null)
        {
            if (isActive) {
                buttonImage.sprite = isFinished ? finishSprites[index] : normalSprites[index];
                specialButtons[index].interactable = true;
            }
            else {
                buttonImage.sprite = graySprites[index];
                specialButtons[index].interactable = false;
            }
        }
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


            if (openLockInstances != null && i < openLockInstances.Length && openLockInstances[i] != null) {
                openLockInstances[i].SetActive(isUnlocked); // 解锁时显示开锁图标
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

    // 特殊按钮点击事件处理
    public void OnSpecialButtonClick(int buttonIndex)
    {
        Debug.Log($"Special button {buttonIndex} clicked!");
        // 在这里添加特殊按钮的具体逻辑
    }
}