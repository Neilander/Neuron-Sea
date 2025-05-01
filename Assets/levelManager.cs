using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelManager : MonoBehaviour
{

    public static levelManager instance;
    #region 已通关关卡记录
    // 添加已通关关卡记录
    private HashSet<int> unlockedLevels = new HashSet<int>();
    #endregion
    public Transform respawnTarget;
    public int currentLevelIndex = 0;  // 当前关卡编号

    private GameObject currentLevelGO;
    private CameraControl cameraControl;
    public GameObject backGround;

    private Rect recordRect;
    [Header("关卡区间")]
    public int minLevel = 1;
    public int maxLevel = 12;

    public int hasCollectedNum = 0;


    [Header("是否开启剧情")]
    public bool ifStartStory;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ⬅️ 不在场景切换中销毁

            cameraControl = Camera.main.GetComponent<CameraControl>();
            if (cameraControl == null)
            {
                Debug.LogError("新场景中主相机缺少 CameraControl！");
                return;
            }

            // 重新加载当前关卡（基于 currentLevelIndex）
            LoadLevel(Mathf.Clamp(currentLevelIndex, minLevel, maxLevel), true);
            AudioManager.Instance.Play(BGMClip.Level1);
            SceneManager.sceneLoaded += OnSceneLoaded; // ⬅️ 注册场景加载回调



            #region 初始化已通关关卡记录
            // 初始化第一关解锁
            UnlockLevel(1);

            // 从PlayerPrefs加载已解锁关卡
            LoadUnlockedLevels();
            #endregion
        }
        else
        {
            Destroy(gameObject);
            return;
        }


    }

    // 保存解锁状态
    private void SaveUnlockedLevels()
    {
        string unlockedLevelsStr = string.Join(",", unlockedLevels);
        PlayerPrefs.SetString("UnlockedLevels", unlockedLevelsStr);
        PlayerPrefs.Save();
    }

    // 加载解锁状态
    public void LoadUnlockedLevels()
    {
        string unlockedLevelsStr = PlayerPrefs.GetString("UnlockedLevels", "1");
        unlockedLevels.Clear();
        foreach (string levelStr in unlockedLevelsStr.Split(','))
        {
            if (int.TryParse(levelStr, out int level))
            {
                unlockedLevels.Add(level);
            }
        }
    }

    // 解锁下一关
    public void UnlockNextLevel()
    {
        UnlockLevel(currentLevelIndex);
    }

    // 解锁指定关卡
    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex >= minLevel && levelIndex <= maxLevel)
        {
            unlockedLevels.Add(levelIndex);
            SaveUnlockedLevels();
        }
    }

    // 检查关卡是否解锁
    public bool IsLevelUnlocked(int levelIndex)
    {
        return unlockedLevels.Contains(levelIndex);
    }

    // TODO:在通关时调用（需要在适当的地方调用这个方法）
    public void CompleteCurrentLevel()
    {
        UnlockNextLevel();
        LoadUnlockedLevels();        // 刷新关卡选择界面
        if (LevelSelectManager.Instance != null)
        {
            LevelSelectManager.Instance.RefreshButtons();
        }
    }

    public Rect LoadLevel(int newLevelIndex, bool ifSetPlayer)
    {
        GridManager.Instance.RefreshSelection();
        string newLevelName = $"Level_{newLevelIndex}";
        GameObject newLevelGO = FindInactiveObjectByName($"Level_{newLevelIndex}");
        Debug.Log("加载" + newLevelName);
        if (newLevelGO == null)
        {
            Debug.LogError($"未找到名为 {newLevelName} 的关卡对象！");
            return new Rect();
        }

        // 关闭当前关卡
        if (currentLevelGO != null)
        {
            DestroyImmediate(currentLevelGO);
        }

        // 启用新关卡
        //newLevelGO.SetActive(true);
        GameObject duplicatedLevel = Instantiate(newLevelGO);
        newLevelGO = duplicatedLevel;
        newLevelGO.SetActive(true);
        if (newLevelGO.GetComponent<levelRefresher>() != null)
            newLevelGO.GetComponent<levelRefresher>().Refresh();
        foreach (Transform child in newLevelGO.transform)
        {
            if (child.name.StartsWith("BackGrounds"))
            {
                child.gameObject.SetActive(false);
            }
        }
        currentLevelGO = newLevelGO;
        currentLevelIndex = newLevelIndex;

        // 获取 LevelData 并设置相机默认区域
        leveldata data = newLevelGO.GetComponent<leveldata>();
        if (data != null)
        {
            cameraControl.SetDefaultRegionFromRect(data.levelBound);
            Debug.Log("已经加载level bound");
        }
        else
        {
            Debug.LogWarning($"关卡 {newLevelName} 上没有找到 LevelData 组件！");
        }

        PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (ifStartStory && newLevelGO.name == "Level_1")
        {
            controller.DisableInput();
        }
        controller.SetMovementBounds(data.levelBound);

        Transform entities = newLevelGO.transform.Find("Entities");
        if (entities != null)
        {
            Transform respawnTarget = null;

            foreach (Transform child in entities)
            {
                if (child.name.StartsWith("Respawn"))
                {
                    respawnTarget = child;
                    this.respawnTarget = child;

                    // 找到重生点后，立即设置给DeathController
                    DeathController deathController = FindAnyObjectByType<DeathController>();
                    if (deathController != null)
                    {
                        deathController.respawnTarget = respawnTarget;
                        Debug.Log($"已将重生点 {respawnTarget.name} 设置给DeathController" + deathController.gameObject.name);
                    }
                    else
                    {
                        Debug.LogError("未找到DeathController，无法设置重生点！");
                    }

                    break;
                }
            }

            if (respawnTarget != null)
            {
                StartEffectController effectController = FindAnyObjectByType<StartEffectController>();
                if (effectController != null)
                {
                    effectController.transform.position = respawnTarget.position;
                    Debug.Log($"将 StartEffectController 移动到 {respawnTarget.name} 的位置");
                    if (ifSetPlayer) controller.MovePosition(respawnTarget.position + Vector3.down * 0.49f);
                    //
                }
                else
                {
                    Debug.LogWarning("未找到 StartEffectController");
                }
            }
            else
            {
                // 如果没有找到重生点，创建一个默认的重生点
                GameObject defaultRespawnObj = new GameObject("Respawn_Default");
                defaultRespawnObj.transform.parent = entities;
                defaultRespawnObj.transform.position = new Vector3(data.levelBound.center.x, data.levelBound.center.y, 0);

                respawnTarget = defaultRespawnObj.transform;
                this.respawnTarget = respawnTarget;

                // 设置给DeathController
                DeathController deathController = FindAnyObjectByType<DeathController>();
                if (deathController != null)
                {
                    deathController.respawnTarget = respawnTarget;
                    Debug.Log($"已将默认重生点设置给DeathController，位置：{respawnTarget.position}");
                }

                Debug.LogWarning($"在关卡 {newLevelName} 中没有找到以 Respawn 开头的物体，已创建默认重生点");
            }
        }
        else
        {
            Debug.LogWarning("未找到 Entities 物体");
        }

        if (backGround == null)
        {
            GameObject backgroundObject = GameObject.FindGameObjectWithTag("BackGround");

            backGround = backgroundObject;
        }

        // 1. 查找 Layer-0
        Transform layer0 = backGround.transform.Find("Layer-0");

        if (layer0 != null)
        {
            // 2. 记录移动前的世界 Y 坐标
            float worldY = layer0.position.y;

            // 3. 移动 background
            backGround.transform.position = newLevelGO.transform.position;

            // 4. 保持 Layer-0 的世界 Y 坐标不变
            Vector3 layer0Pos = layer0.position;
            layer0Pos.y = worldY;
            layer0.position = layer0Pos;
        }
        else
        {
            backGround.transform.position = newLevelGO.transform.position;
        }
        Vector3 intPos = new Vector3(
            Mathf.Round(newLevelGO.transform.position.x),
            Mathf.Round(newLevelGO.transform.position.y),
            Mathf.Round(newLevelGO.transform.position.z)
        );
        if (GridManager.Instance != null) GridManager.Instance.transform.position = intPos;
        //Vector3 topCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));

        return data.levelBound;
    }

    private void Update()
    {

    }

    public void SwitchToNextLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, minLevel, maxLevel), false);
        FindAnyObjectByType<StartEffectController>().transform.position = FindAnyObjectByType<PlayerController>().transform.position + Vector3.up * 1.6f + Vector3.right * 0.1f;
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
        // LevelSelectManager.Instance.RefreshButtons();
        //需要获取到当前关卡的初始为止，把StartEffectController设置到该位置；下面这个是临时的
        //StartCoroutine(DelayEffect());
    }

    public void SwitchToNextLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, minLevel, maxLevel), true);
    }

    public void SwitchToBeforeLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, minLevel, maxLevel), true);
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
    }

    public void SwitchToBeforeLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, minLevel, maxLevel), true);
    }

    IEnumerator DelayEffect()
    {
        yield return null;
        //StartEffectController controller = FindAnyObjectByType<StartEffectController>();
        //PlayerController pController = FindAnyObjectByType<PlayerController>();
        //controller.transform.position = new Vector3(recordRect.xMin+1,pController.transform.position.y+5, controller.transform.position.z);
        //controller.TriggerStartEffect();
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

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        cameraControl = Camera.main.GetComponent<CameraControl>();
        if (cameraControl == null)
        {
            Debug.LogWarning("新场景中主相机缺少 CameraControl！");
            return;
        }

        // 重新加载当前关卡（基于 currentLevelIndex）
        LoadLevel(currentLevelIndex, true);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

    }

    public void ReloadLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(currentLevelIndex, false);
    }
}
