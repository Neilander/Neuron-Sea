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

    private const string CollectedKey = "CollectedLevels";


    private void Awake(){
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保持跨场景不销毁
        }
        else {
            Destroy(gameObject);
        }
        LoadCollectedLevels();
    }

    public void TryAddCollection(int levelName){
        if (!collectedLevels.Contains(levelName)) {
            totalCollected++;
            collectedLevels.Add(levelName);
            Debug.Log($"Collected in {levelName}. Total: {totalCollected}");
            SaveCollectedLevels();
            LoadCollectedLevels();
            // CollectableEvents.onCollectableAdded?.Invoke(levelName, totalCollected);
            
            if (LevelSelectManager.Instance != null)
                LevelSelectManager.Instance.RefreshButtons();
        }
    }

    public int GetTotalCollected(){
        return totalCollected;
    }

    public void ResetLevelData(){
        collectedLevels.Clear();
    }

    public bool HasCollectedLevel(int levelIndex)
    {
        return collectedLevels.Contains(levelIndex);
    }

    private void SaveCollectedLevels()
    {
        string collectedStr = string.Join(",", collectedLevels);
        PlayerPrefs.SetString(CollectedKey, collectedStr);
        PlayerPrefs.Save();
        Debug.Log($"📝 已保存收集状态：{collectedStr}");
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

    private void OnApplicationQuit()
    {
        SaveCollectedLevels(); // 防止意外丢失
#if UNITY_EDITOR
        ClearCollectedData();
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
}
