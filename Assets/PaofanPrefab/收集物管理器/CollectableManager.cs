using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance;

    //总收集次数，用于显示拼图
    public int totalCollected = 0;

    public HashSet<int> collectedLevels = new HashSet<int>();
    public HashSet<int> collectedViewedLevels = new HashSet<int>();

    private const string CollectedKey = "CollectedLevels";
    private const string CollectedViewedKey = "CollectViewedLevels";


    private void Awake(){
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保持跨场景不销毁
        }
        else {
            Destroy(gameObject);
        }
        LoadCollectedLevels();
        LoadCollectViewedLevels();
    }
    public void RemoveCollection(int levelIndex)
    {
        if (HasCollectedLevel(levelIndex))
        {
            totalCollected = Mathf.Max(0, totalCollected - 1);
            collectedLevels.Remove(levelIndex);
            SaveCollectedLevels(); // 这里可以调用私有方法，因为是在类内部
            Debug.Log($"已移除关卡 {levelIndex} 的收集物，当前总数：{totalCollected}");
        }
        else
        {
            Debug.LogWarning($"关卡 {levelIndex} 未被收集，无法移除");
        }
    }
    public void TryAddCollection(int levelName){
        if (!collectedLevels.Contains(levelName)) {
            totalCollected++;
            collectedLevels.Add(levelName);
            Debug.Log($"Collected in {levelName}. Total: {totalCollected}");
            SaveCollectedLevels();
            LoadCollectedLevels();
            // CollectableEvents.onCollectableAdded?.Invoke(levelName, totalCollected);

            if (LevelSelectManager.Instance != null && ConceptArtUnlockManagerNew.Instance != null)
            {
                ConceptArtUnlockManagerNew.Instance.UpdateArtLockStatus();
                LevelSelectManager.Instance.RefreshButtons();
            }
            else
                Debug.LogWarning("收集刷新失败");
        }
    }

    public void TryAddCollectedViewed(int levelName)
    {
        if (!collectedViewedLevels.Contains(levelName))
        {
            if (levelManager.instance != null && (levelManager.instance.currentLevelIndex == levelName))
                levelManager.instance.SetupForViewed();
            collectedViewedLevels.Add(levelName);
            Debug.Log($"Collected Viewed in {levelName}. Total: {totalCollected}");
            SaveCollectedViewedLevels();
            LoadCollectViewedLevels();
        }
    }

    public int GetTotalCollected(){
        return totalCollected;
    }
    //仅用于剧情计数,仅清空内存数据
    public void ResetLevelData(){
        collectedLevels.Clear();
    }

    public bool HasCollectedLevel(int levelIndex)
    {
        return collectedLevels.Contains(levelIndex);
    }

    public bool HasCollectedViewedLevel(int levelIndex)
    {
        return collectedViewedLevels.Contains(levelIndex);
    }

    private void SaveCollectedLevels()
    {
        string collectedStr = string.Join(",", collectedLevels);
        PlayerPrefs.SetString(CollectedKey, collectedStr);
        PlayerPrefs.Save();
        Debug.Log($"已保存收集状态：{collectedStr}");
    }

    private void SaveCollectedViewedLevels()
    {
        string collectedStr = string.Join(",", collectedViewedLevels);
        PlayerPrefs.SetString(CollectedViewedKey, collectedStr);
        PlayerPrefs.Save();
        Debug.Log($"已保存收集看到状态：{collectedStr}");
    }

    private void LoadCollectedLevels()
    {
        string collectedStr = PlayerPrefs.GetString(CollectedKey, "");
        collectedLevels.Clear();
        totalCollected = 0;

        foreach (var str in collectedStr.Split(','))
        {
            if (int.TryParse(str, out int level))
            {
                collectedLevels.Add(level);
                totalCollected++;
            }
        }

        ConceptArtUnlockManager.Instance?.UpdateArtLockStatus();
        Debug.Log($"已加载收集状态：{string.Join(",", collectedLevels)}");
    }

    private void LoadCollectViewedLevels()
    {
        string collectedStr = PlayerPrefs.GetString(CollectedViewedKey, "");
        collectedViewedLevels.Clear();

        foreach (var str in collectedStr.Split(','))
        {
            if (int.TryParse(str, out int level))
            {
                collectedViewedLevels.Add(level);
            }
        }

        //ConceptArtUnlockManager.Instance?.UpdateArtLockStatus();
        Debug.Log($"已加载收集看到状态：{string.Join(",", collectedViewedLevels)}");
    }

    private void OnApplicationQuit()
    {
        SaveCollectedLevels(); // 防止意外丢失
        SaveCollectedViewedLevels();
#if UNITY_EDITOR
        ClearCollectedData();
        ClearCollectedViewedData();
        
#endif
    }

    public void ClearCollectedData()
    {
        collectedLevels.Clear();
        totalCollected = 0;
        PlayerPrefs.DeleteKey(CollectedKey);
        PlayerPrefs.Save();
        Debug.Log("所有收集数据已清除");
    }

    public void ClearCollectedViewedData()
    {
        collectedViewedLevels.Clear();
        PlayerPrefs.DeleteKey(CollectedViewedKey);
        PlayerPrefs.Save();
        Debug.Log("所有收集看到数据已清除");
    }
}
