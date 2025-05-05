using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public static CollectableManager Instance;

    private int totalCollected = 0;

    private HashSet<string> collectedLevels = new HashSet<string>();

    private void Awake(){
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 保持跨场景不销毁
        }
        else {
            Destroy(gameObject);
        }
    }

    public void TryAddCollection(string levelName){
        if (!collectedLevels.Contains(levelName)) {
            totalCollected++;
            collectedLevels.Add(levelName);
            Debug.Log($"Collected in {levelName}. Total: {totalCollected}");
        }
    }

    public int GetTotalCollected(){
        return totalCollected;
    }
}
