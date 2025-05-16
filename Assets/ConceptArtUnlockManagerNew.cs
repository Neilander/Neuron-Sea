using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConceptArtUnlockManagerNew : MonoBehaviour
{
    public static ConceptArtUnlockManagerNew Instance { get; private set; }
    [Header("Button List")] public Button[] artButtons; // 按钮数组

    [Header("Lock Settings")]
    [SerializeField] private GameObject bubblePrefab; // 泡泡预制体
    private GameObject[] bubbleInstances; // 用于存储每个按钮上的泡泡实例
    [SerializeField] private Sprite[] lockedImgs;
    [SerializeField] private Sprite[] unlockImgs;


    [SerializeField] private ConceptArt conceptArt;

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
        // 初始化泡泡实例数组
        bubbleInstances = new GameObject[artButtons.Length];
        // 为每个按钮创建泡泡并绑定点击事件
        for (int i = 0; i < artButtons.Length; i++)
        {
            int temp = i;
            artButtons[i].onClick.AddListener(() => conceptArt.ShowPic(temp));

            // 为每个按钮创建泡泡的实例
            GameObject bubbleInstance = Instantiate(bubblePrefab, artButtons[i].transform);
            bubbleInstance.transform.SetAsLastSibling(); // 确保泡泡显示在最上层
            bubbleInstance.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime; // 设置动画更新模式为不受时间缩放影响
            bubbleInstances[i] = bubbleInstance;
        }
        // 更新所有设定图的锁定状态
        UpdateArtLockStatus();
    }

    // 更新所有设定图的锁定状态
    public void UpdateArtLockStatus()
    {
        if (artButtons == null || artButtons.Length == 0)
        {
            Debug.LogWarning("No level buttons found!");
            return;
        }

        for (int i = 0; i < artButtons.Length; i++)
        {
            if (artButtons[i] == null)
            {
                Debug.LogWarning($"Button at index {i} is null!");
                continue;
            }

            bool isUnlocked = CollectableManager.Instance.totalCollected >= (i + 1) * 0;

            // 设置按钮是否可交互
            artButtons[i].interactable = isUnlocked;

            // 设置锁的数字
            TextMeshProUGUI lockNumberText = bubbleInstances[i].GetComponentInChildren<TextMeshProUGUI>();
            lockNumberText.text = isUnlocked ? $"{CollectableManager.Instance.totalCollected}/{(i + 1) * 0}" : $"<color=#E73CA6>{CollectableManager.Instance.totalCollected}</color>/{(i + 1) * 0}"; // 设置锁的数字

            artButtons[i].GetComponent<Image>().sprite = isUnlocked ? unlockImgs[i] : lockedImgs[i]; // 设置锁的显示状态
        }
    }
}
