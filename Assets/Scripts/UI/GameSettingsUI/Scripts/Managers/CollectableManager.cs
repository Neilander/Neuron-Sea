using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 收集物管理器 - 管理游戏中的所有收集物
/// </summary>
public class CollectableManager : MonoBehaviour
{
    #region 单例实现
    public static CollectableManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [System.Serializable]
    public class CollectableData
    {
        public string id;                    // 唯一ID
        public string displayName;           // 显示名称
        public string description;           // 描述
        public Sprite icon;                  // 图标
        public int rarity;                   // 稀有度（1-5）
        public bool isCollected;             // 是否已收集
        public DateTime collectedTime;       // 收集时间
    }

    [Header("收集物数据")]
    [SerializeField] private List<CollectableData> collectableDatabase = new List<CollectableData>();

    // 收集物变化的事件
    public event Action<string> OnCollectableCollected;
    public event Action OnCollectablesUpdated;

    // 运行时收集状态的字典
    private Dictionary<string, bool> collectedItems = new Dictionary<string, bool>();
    private Dictionary<string, DateTime> collectionTimes = new Dictionary<string, DateTime>();

    // 关卡中的收集物总数
    public int TotalCollectablesInLevel { get; private set; }

    // 已收集的收集物数量
    public int CollectedCount => GetCollectedCount();

    private void Start()
    {
        LoadCollectableData();
    }

    /// <summary>
    /// 初始化收集物管理器
    /// </summary>
    public void Initialize()
    {
        // 从数据库加载收集物数据
        LoadCollectableData();

        // 更新运行时状态
        UpdateRuntimeStatus();
    }

    /// <summary>
    /// 更新运行时状态
    /// </summary>
    private void UpdateRuntimeStatus()
    {
        collectedItems.Clear();
        collectionTimes.Clear();

        foreach (var data in collectableDatabase)
        {
            collectedItems[data.id] = data.isCollected;
            if (data.isCollected && data.collectedTime != default)
            {
                collectionTimes[data.id] = data.collectedTime;
            }
        }
    }

    /// <summary>
    /// 注册收集物
    /// </summary>
    /// <param name="id">收集物ID</param>
    public void RegisterCollectable(string id)
    {
        if (!collectedItems.ContainsKey(id))
        {
            collectedItems.Add(id, false);
            TotalCollectablesInLevel++;
        }
    }

    /// <summary>
    /// 标记收集物为已收集
    /// </summary>
    /// <param name="id">收集物ID</param>
    public void MarkAsCollected(string id)
    {
        if (collectedItems.ContainsKey(id) && !collectedItems[id])
        {
            collectedItems[id] = true;
            collectionTimes[id] = DateTime.Now;

            // 更新数据库中的收集状态
            for (int i = 0; i < collectableDatabase.Count; i++)
            {
                if (collectableDatabase[i].id == id)
                {
                    var data = collectableDatabase[i];
                    data.isCollected = true;
                    data.collectedTime = DateTime.Now;
                    collectableDatabase[i] = data;
                    break;
                }
            }

            // 触发事件
            OnCollectableCollected?.Invoke(id);
            OnCollectablesUpdated?.Invoke();

            // 保存数据
            SaveCollectableData();
        }
    }

    /// <summary>
    /// 获取已收集的收集物数量
    /// </summary>
    private int GetCollectedCount()
    {
        int count = 0;
        foreach (var collected in collectedItems.Values)
        {
            if (collected) count++;
        }
        return count;
    }

    /// <summary>
    /// 获取收集物数据
    /// </summary>
    /// <param name="id">收集物ID</param>
    public CollectableData GetCollectableData(string id)
    {
        foreach (var data in collectableDatabase)
        {
            if (data.id == id)
                return data;
        }
        return null;
    }

    /// <summary>
    /// 获取所有收集物数据
    /// </summary>
    public List<CollectableData> GetAllCollectables()
    {
        return new List<CollectableData>(collectableDatabase);
    }

    /// <summary>
    /// 获取已收集的收集物
    /// </summary>
    public List<CollectableData> GetCollectedCollectables()
    {
        List<CollectableData> result = new List<CollectableData>();
        foreach (var data in collectableDatabase)
        {
            if (data.isCollected)
                result.Add(data);
        }
        return result;
    }

    /// <summary>
    /// 获取未收集的收集物
    /// </summary>
    public List<CollectableData> GetUncollectedCollectables()
    {
        List<CollectableData> result = new List<CollectableData>();
        foreach (var data in collectableDatabase)
        {
            if (!data.isCollected)
                result.Add(data);
        }
        return result;
    }

    /// <summary>
    /// 按稀有度获取收集物
    /// </summary>
    /// <param name="rarity">稀有度</param>
    public List<CollectableData> GetCollectablesByRarity(int rarity)
    {
        List<CollectableData> result = new List<CollectableData>();
        foreach (var data in collectableDatabase)
        {
            if (data.rarity == rarity)
                result.Add(data);
        }
        return result;
    }

    /// <summary>
    /// 保存收集物数据
    /// </summary>
    public void SaveCollectableData()
    {
        for (int i = 0; i < collectableDatabase.Count; i++)
        {
            string id = collectableDatabase[i].id;
            if (collectedItems.ContainsKey(id))
            {
                var data = collectableDatabase[i];
                data.isCollected = collectedItems[id];

                if (collectionTimes.ContainsKey(id))
                {
                    data.collectedTime = collectionTimes[id];
                }

                collectableDatabase[i] = data;

                // 保存到PlayerPrefs（简单存储）
                PlayerPrefs.SetInt("Collectable_" + id, data.isCollected ? 1 : 0);

                if (data.isCollected)
                {
                    PlayerPrefs.SetString("CollectableTime_" + id, data.collectedTime.ToString("O"));
                }
            }
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 加载收集物数据
    /// </summary>
    public void LoadCollectableData()
    {
        for (int i = 0; i < collectableDatabase.Count; i++)
        {
            string id = collectableDatabase[i].id;

            // 从PlayerPrefs加载（简单存储）
            if (PlayerPrefs.HasKey("Collectable_" + id))
            {
                var data = collectableDatabase[i];
                data.isCollected = PlayerPrefs.GetInt("Collectable_" + id) == 1;

                if (data.isCollected && PlayerPrefs.HasKey("CollectableTime_" + id))
                {
                    string timeStr = PlayerPrefs.GetString("CollectableTime_" + id);
                    if (DateTime.TryParse(timeStr, out DateTime time))
                    {
                        data.collectedTime = time;
                    }
                }

                collectableDatabase[i] = data;
            }
        }

        UpdateRuntimeStatus();
    }

    /// <summary>
    /// 重置所有收集物状态
    /// </summary>
    public void ResetAllCollectables()
    {
        collectedItems.Clear();
        collectionTimes.Clear();

        for (int i = 0; i < collectableDatabase.Count; i++)
        {
            var data = collectableDatabase[i];
            data.isCollected = false;
            data.collectedTime = default;
            collectableDatabase[i] = data;

            collectedItems[data.id] = false;

            // 从PlayerPrefs中删除
            PlayerPrefs.DeleteKey("Collectable_" + data.id);
            PlayerPrefs.DeleteKey("CollectableTime_" + data.id);
        }

        PlayerPrefs.Save();
        OnCollectablesUpdated?.Invoke();
    }

    /// <summary>
    /// 重置关卡计数
    /// </summary>
    public void ResetLevelCount()
    {
        TotalCollectablesInLevel = 0;
    }

    /// <summary>
    /// 检查是否收集了所有的收集物
    /// </summary>
    public bool HasCollectedAll()
    {
        foreach (var data in collectableDatabase)
        {
            if (!data.isCollected)
                return false;
        }
        return true;
    }

    /// <summary>
    /// 检查是否收集了指定类型的所有收集物
    /// </summary>
    /// <param name="rarity">稀有度</param>
    public bool HasCollectedAllOfRarity(int rarity)
    {
        foreach (var data in collectableDatabase)
        {
            if (data.rarity == rarity && !data.isCollected)
                return false;
        }
        return true;
    }
}