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

    private void Awake(){
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保持跨场景不销毁
        }
        else {
            Destroy(gameObject);
        }
    }

    public void TryAddCollection(int levelName){
        if (!collectedLevels.Contains(levelName)) {
            totalCollected++;
            collectedLevels.Add(levelName);
            Debug.Log($"Collected in {levelName}. Total: {totalCollected}");
        }
    }

    public int GetTotalCollected(){
        return totalCollected;
    }

    public void ResetLevelData(){
        collectedLevels.Clear();
    }
}
