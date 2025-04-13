using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class levelManager : MonoBehaviour
{
    public int currentLevelIndex = 0;  // 当前关卡编号

    private GameObject currentLevelGO;
    private CameraControl cameraControl;

    private Rect recordRect;
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

    public Rect LoadLevel(int newLevelIndex)
    {
        string newLevelName = $"Level_{newLevelIndex}";
        GameObject newLevelGO = FindInactiveObjectByName($"Level_{newLevelIndex}");

        if (newLevelGO == null)
        {
            Debug.LogError($"未找到名为 {newLevelName} 的关卡对象！");
            return new Rect();
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

        PlayerController controller = FindAnyObjectByType<PlayerController>();
        controller.SetMovementBounds(data.levelBound);
        return data.levelBound;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SwitchToNextLevel();
    }

    public void SwitchToNextLevel()
    {
        recordRect = LoadLevel(currentLevelIndex + 1);

        //需要获取到当前关卡的初始为止，把StartEffectController设置到该位置；下面这个是临时的
        StartCoroutine(DelayEffect());
    }

    IEnumerator DelayEffect()
    {
        yield return null;
        StartEffectController controller = FindAnyObjectByType<StartEffectController>();
        PlayerController pController = FindAnyObjectByType<PlayerController>();
        controller.transform.position = new Vector3(recordRect.xMin+1,pController.transform.position.y+5, controller.transform.position.z);
       controller.TriggerStartEffect();
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
