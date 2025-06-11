using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConceptArtUnlockManagerNew : MonoBehaviour
{
    public static ConceptArtUnlockManagerNew Instance { get; private set; }
    [Header("Button List")] public Button[] artButtons; // ��ť����

    [Header("Lock Settings")]
    [SerializeField] private GameObject bubblePrefab; // ����Ԥ����
    private GameObject[] bubbleInstances; // ���ڴ洢ÿ����ť�ϵ�����ʵ��
    [SerializeField] private Sprite[] lockedImgs;
    [SerializeField] private Sprite[] unlockImgs;


    [SerializeField] private ConceptArt conceptArt;

    void Awake()
    {
        // if (ReddotManager.ReddotPath != null) {
        //     foreach (var path in ReddotManager.ReddotPath) {
        //         // bool hasRead = PlayerPrefs.GetInt(path, 0) == 1;
        //         ReddotManager.AddPath(path);
        //     }
        // }
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ReddotManager.onReddotPathChange -= PersistPaths;
        ReddotManager.onReddotPathChange += PersistPaths;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        ReddotManager.onReddotPathChange -= PersistPaths;
    }

    void Start()
    {
        // ��ʼ������ʵ������
        bubbleInstances = new GameObject[artButtons.Length];
        // Ϊÿ����ť�������ݲ��󶨵���¼�
        for (int i = 0; i < artButtons.Length; i++)
        {
            int temp = i;
            artButtons[i].onClick.AddListener(() => conceptArt.ShowPic(temp));

            // Ϊÿ����ť�������ݵ�ʵ��
            GameObject bubbleInstance = Instantiate(bubblePrefab, artButtons[i].transform);
            bubbleInstance.transform.SetAsLastSibling(); // ȷ��������ʾ�����ϲ�
            bubbleInstance.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime; // ���ö�������ģʽΪ����ʱ������Ӱ��
            bubbleInstances[i] = bubbleInstance;
        }
        // ���������趨ͼ������״̬
        UpdateArtLockStatus();
    }

    private void OnEnable()
    {
        //触发进入相关
        print("进入了收集物面板");
        AudioManager.Instance.EnterGallary();
    }

    private void OnDisable()
    {
        print("离开了收集物面板");
        AudioManager.Instance.LeaveGallary();
    }

    // ���������趨ͼ������״̬
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

            // ���ð�ť�Ƿ�ɽ���
            artButtons[i].interactable = isUnlocked;
            // ������������
            TextMeshProUGUI lockNumberText = bubbleInstances[i].GetComponentInChildren<TextMeshProUGUI>();
            lockNumberText.text = isUnlocked ? $"{CollectableManager.Instance.totalCollected}/{(i + 1) * 9}" : $"<color=#E73CA6>{CollectableManager.Instance.totalCollected}</color>/{(i + 1) * 9}"; // ������������

            artButtons[i].GetComponent<Image>().sprite = isUnlocked ? unlockImgs[i] : lockedImgs[i]; // ����������ʾ״̬
            string pathAll = $"escBtn/Options/Collect/Collection{i + 1}";
            // 解锁，则添加对应图片路径
            if (artButtons[i].interactable) {
                if (PlayerPrefs.GetInt(pathAll + "/", 0) == 0) {
                    ReddotManager.AddPath(pathAll);
                }
            }
        }
    }

    private void PersistPaths(){
        var snapshot = ReddotManager.ReddotPath.ToArray();
        foreach (var p in snapshot) // p 是单个 path
        {
            PlayerPrefs.SetInt(p, 1); // 1 == 已触发
        }
        PlayerPrefs.Save(); // 立即落盘
    }
}
