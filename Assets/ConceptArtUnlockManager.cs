using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConceptArtUnlockManager : MonoBehaviour
{
    public static ConceptArtUnlockManager Instance { get; private set; }
    [Header("Button List")] public Button[] artButtons; // ��ť����

    [Header("Lock Settings")]
    [SerializeField] private GameObject lockImagePrefab; // ����ͼƬԤ����
    private GameObject[] lockInstances; // ���ڴ洢ÿ����ť�ϵ���ʵ��
    [SerializeField] private GameObject openLockPrefab; // ������ͼƬԤ����
    private GameObject[] openLockInstances; // �洢ÿ����ť�Ŀ���ͼ
    [SerializeField] private GameObject lockNumberPrefab; // ��������Ԥ����
    private GameObject[] lockNumberInstances; // �洢ÿ����ť�Ŀ���ͼ


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
        // ��ʼ����ʵ������
        lockInstances = new GameObject[artButtons.Length];
        openLockInstances = new GameObject[artButtons.Length];
        lockNumberInstances = new GameObject[artButtons.Length];
        // Ϊÿ����ť���������󶨵���¼�
        for (int i = 0; i < artButtons.Length; i++)
        {
            int temp = i;
            artButtons[i].onClick.AddListener(() => conceptArt.ShowPic(temp));

            // Ϊÿ����ť��������ʵ��
            GameObject lockInstance = Instantiate(lockImagePrefab, artButtons[i].transform);
            lockInstance.transform.SetAsLastSibling(); // ȷ������ʾ�����ϲ�
            lockInstances[i] = lockInstance;
            lockInstance.GetComponent<RectTransform>().localScale = new Vector2(3, 3);
            lockInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 100);

            GameObject openLockInstance = Instantiate(openLockPrefab, artButtons[i].transform);
            openLockInstance.transform.SetAsLastSibling(); // ȷ����ʾ�����ϲ�
            openLockInstance.SetActive(false); // Ĭ������
            openLockInstances[i] = openLockInstance;
            openLockInstance.GetComponent<RectTransform>().localScale = new Vector2(3, 3);

            GameObject lockNumberInstance = Instantiate(lockNumberPrefab, artButtons[i].transform);
            lockNumberInstance.transform.SetAsLastSibling(); // ȷ����ʾ�����ϲ�
            lockNumberInstances[i] = lockNumberInstance;
            lockNumberInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(175, -150);
        }
        // ���������趨ͼ������״̬
        UpdateArtLockStatus();
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
            if (lockNumberInstances != null && i < lockNumberInstances.Length && lockNumberInstances[i] != null)
            {
                lockNumberInstances[i].SetActive(!isUnlocked); 
                TextMeshProUGUI lockNumberText = lockNumberInstances[i].GetComponent<TextMeshProUGUI>();
                if (lockNumberText != null && !isUnlocked)
                {
                    lockNumberText.text = $"<color=#E73CA6>{CollectableManager.Instance.totalCollected}</color>/{(i + 1) * 9}"; // ������������
                }
            }

            // ����������ʾ״̬
            if (lockInstances != null && i < lockInstances.Length && lockInstances[i] != null)
            {
                lockInstances[i].SetActive(!isUnlocked);
            }

            if (openLockInstances != null && i < openLockInstances.Length && openLockInstances[i] != null)
            {
                openLockInstances[i].SetActive(isUnlocked); // ����ʱ��ʾ����ͼ��
            }
        }
    }
}
