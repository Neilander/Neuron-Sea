using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConceptArtUnlockManager : MonoBehaviour
{
    public static ConceptArtUnlockManager Instance { get; private set; }
    [Header("Button List")] public Button[] artButtons; // 按钮数组

    [Header("Lock Settings")]
    [SerializeField] private GameObject lockImagePrefab; // 锁的图片预制体
    private GameObject[] lockInstances; // 用于存储每个按钮上的锁实例
    [SerializeField] private GameObject openLockPrefab; // 开锁的图片预制体
    private GameObject[] openLockInstances; // 存储每个按钮的开锁图
    [SerializeField] private GameObject lockNumberPrefab; // 锁的数字预制体
    private GameObject[] lockNumberInstances; // 存储每个按钮的开锁图


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
        // 初始化锁实例数组
        lockInstances = new GameObject[artButtons.Length];
        openLockInstances = new GameObject[artButtons.Length];
        lockNumberInstances = new GameObject[artButtons.Length];
        // 为每个按钮创建锁并绑定点击事件
        for (int i = 0; i < artButtons.Length; i++)
        {
            int temp = i;
            artButtons[i].onClick.AddListener(() => conceptArt.ShowPic(temp));

            // 为每个按钮创建锁的实例
            GameObject lockInstance = Instantiate(lockImagePrefab, artButtons[i].transform);
            lockInstance.transform.SetAsLastSibling(); // 确保锁显示在最上层
            lockInstances[i] = lockInstance;
            lockInstance.GetComponent<RectTransform>().localScale = new Vector2(3, 3);
            lockInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            GameObject openLockInstance = Instantiate(openLockPrefab, artButtons[i].transform);
            openLockInstance.transform.SetAsLastSibling(); // 确保显示在最上层
            openLockInstance.SetActive(false); // 默认隐藏
            openLockInstances[i] = openLockInstance;
            openLockInstance.GetComponent<RectTransform>().localScale = new Vector2(3, 3);

            GameObject lockNumberInstance = Instantiate(lockNumberPrefab, artButtons[i].transform);
            lockNumberInstance.transform.SetAsLastSibling(); // 确保显示在最上层
            lockNumberInstances[i] = lockNumberInstance;
            lockNumberInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(175, -150);
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

            bool isUnlocked = CollectableManager.Instance.totalCollected >= (i + 1) * 9;

            // 设置按钮是否可交互
            artButtons[i].interactable = isUnlocked;

            // 设置锁的数字
            if (lockNumberInstances != null && i < lockNumberInstances.Length && lockNumberInstances[i] != null)
            {
                lockNumberInstances[i].SetActive(!isUnlocked); 
                TextMeshProUGUI lockNumberText = lockNumberInstances[i].GetComponent<TextMeshProUGUI>();
                if (lockNumberText != null && !isUnlocked)
                {
                    lockNumberText.text = $"<color=#E73CA6>{CollectableManager.Instance.totalCollected}</color>/{(i + 1) * 9}"; // 设置锁的数字
                }
            }

            // 设置锁的显示状态
            if (lockInstances != null && i < lockInstances.Length && lockInstances[i] != null)
            {
                lockInstances[i].SetActive(!isUnlocked);
            }

            if (openLockInstances != null && i < openLockInstances.Length && openLockInstances[i] != null)
            {
                openLockInstances[i].SetActive(isUnlocked); // 解锁时显示开锁图标
            }
        }
    }
}
