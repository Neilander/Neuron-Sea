using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelManager : MonoBehaviour
{
    public int currentLevelIndex = 0;  // 当前关卡编号

    private GameObject currentLevelGO;
    private CameraControl cameraControl;

    
    void Start()
    {
        cameraControl = Camera.main.GetComponent<CameraControl>();
        if (cameraControl == null)
        {
            Debug.LogError("未找到主相机上的 CameraControl 脚本！");
            return;
        }

        LoadLevel(currentLevelIndex);
    }

    public void LoadLevel(int newLevelIndex)
    {
        string newLevelName = $"Level_{newLevelIndex}";
        GameObject newLevelGO = FindInactiveObjectByName($"Level_{newLevelIndex}");

        if (newLevelGO == null)
        {
            Debug.LogError($"未找到名为 {newLevelName} 的关卡对象！");
            return;
        }

        // 关闭当前关卡
        if (currentLevelGO != null)
        {
            currentLevelGO.SetActive(false);
        }

        // 启用新关卡
        newLevelGO.SetActive(true);
        currentLevelGO = newLevelGO;
        currentLevelIndex = newLevelIndex;

        // 获取 LevelData 并设置相机默认区域
        leveldata data = newLevelGO.GetComponent<leveldata>();
        if (data != null)
        {
            cameraControl.SetDefaultRegionFromRect(data.levelBound);
        }
        else
        {
            Debug.LogWarning($"关卡 {newLevelName} 上没有找到 LevelData 组件！");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SwitchToNextLevel();
    }

    public void SwitchToNextLevel()
    {
        LoadLevel(currentLevelIndex + 1);

        //需要获取到当前关卡的初始为止，把StartEffectController设置到该位置；下面这个是临时的
        FindAnyObjectByType<StartEffectController>().transform.position = new Vector3(-159, 122);
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
    }

    GameObject FindInactiveObjectByName(string name)
    {
        GameObject[] allGameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allGameObjects)
        {
            if (go.name == name && go.hideFlags == HideFlags.None && go.scene.IsValid())
            {
                return go;
            }
        }
        return null;
    }
}
