using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    [Header("Button List")] public Button[] levelButtons; // 按钮数组

    void Start(){
        // 为每个按钮绑定点击事件
        for (int i = 0; i < levelButtons.Length; i++) {
            int levelIndex = i + 1; // 关卡索引，从1开始
            levelButtons[i].onClick.AddListener(() => LoadLevel(levelIndex));
        }
    }

    // 加载对应关卡
    void LoadLevel(int levelIndex){
        string levelName = "Level_" + levelIndex; // 拼接关卡名称
        levelManager.instance.LoadLevel(levelIndex,true); // 加载场景
    }
}