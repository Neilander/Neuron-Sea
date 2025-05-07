using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class levelManager : MonoBehaviour
{

    public static levelManager instance { get; private set; }
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
    [Header("当前场景index")]
    public int sceneIndex = 1;


    public int hasCollectedNum = 0;


    [FormerlySerializedAs("ifStartStory")]
    [Header("是否开启剧情")]
    public bool isStartStory;

    [Header("背景调整")]
    public float yAdjust = 20;


    [Header("Level 13特殊出生")]
    public bool enableLevel13SpecialSpawn = true; // 新增开关，控制是否启用第13关特殊出生
    public float walkInDistance = 10f; // 从重生点左边多远开始走
    private bool isWalkingToSpawn = false;
    // 新增：用于区分是否是重启/重生等特殊流程
    private bool isRestarting = false;
    // 新增：用于锁定玩家位置
    private bool isPositionLocked = false;
    private Vector3 lockedPosition = Vector3.zero;
    private float positionLockDuration = 2f; // 锁定持续时间



    const int sceneLimit = 2;

    // 添加位置监测相关变量
    [Header("位置监测")]
    public bool enablePositionMonitoring = true;
    private float positionMonitorTimer = 0f;
    private float positionMonitorInterval = 1f; // 每秒检查一次
    private Vector3 lastRecordedPosition = Vector3.zero;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject); // ⬅️ 不在场景切换中销毁

            cameraControl = Camera.main.GetComponent<CameraControl>();
            if (cameraControl == null)
            {
                Debug.LogError("新场景中主相机缺少 CameraControl！");
                return;
            }
            // 重新加载当前关卡（基于 currentLevelIndex）
            var cameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            if (cameraData != null)
            {
                cameraData.SetRenderer(sceneIndex - 1);
            }
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
        Debug.Log($"保存解锁状态：{unlockedLevelsStr}");
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
        Debug.Log($"加载解锁状态：{string.Join(",", unlockedLevels)}");
    }

    // 解锁下一关
    public void UnlockNextLevel()
    {
        Debug.Log($"解锁下一关：当前关卡 {currentLevelIndex}，解锁关卡 {currentLevelIndex}");
        UnlockLevel(currentLevelIndex);  // 解锁下一关
    }

    // 解锁指定关卡
    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex >= minLevel && levelIndex <= maxLevel)
        {
            unlockedLevels.Add(levelIndex);
            // SaveUnlockedLevels();
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
        SaveUnlockedLevels();  // 确保立即保存解锁状态
        LoadUnlockedLevels();  // 重新加载确保状态一致

        // 刷新关卡选择界面
        if (LevelSelectManager.Instance != null)
        {
            LevelSelectManager.Instance.RefreshButtons();
        }
    }

    public Rect LoadLevel(int newLevelIndex, bool ifSetPlayer)
    {
        if (GridManager.Instance != null) GridManager.Instance.RefreshSelection();
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
        if (isStartStory && newLevelGO.name == "Level_1")
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
                // PlayerController controller = FindAnyObjectByType<PlayerController>();

                if (effectController != null)
                {
                    // 无论何种情况，始终将开始特效放在原始重生点位置
                    effectController.transform.position = respawnTarget.position;

                    // 检查是否是第13关，并且是首次加载（不是死亡重生或重新加载）
                    if (newLevelIndex == 13 && ifSetPlayer && !isRestarting && enableLevel13SpecialSpawn)
                    {
                        Debug.Log($"[LevelManager] 进入第13关特殊出生逻辑，当前关卡：{newLevelIndex}，特殊出生开关：{enableLevel13SpecialSpawn}");

                        // 禁用玩家输入
                        controller.DisableInput();
                        Debug.Log("[LevelManager] 玩家输入已禁用");

                        // 计算出生点的实际位置（带偏移）
                        Vector3 actualSpawnPosition = respawnTarget.position + Vector3.down * 0.49f;

                        // 设置玩家初始位置（在重生点左边）
                        Vector3 startPos = actualSpawnPosition + Vector3.left * walkInDistance;
                        Debug.Log($"[LevelManager] 玩家初始位置：{startPos}，目标重生点位置：{actualSpawnPosition}");

                        // 移动玩家到左侧位置
                        controller.MovePosition(startPos);

                        // 开始走路动画 - 走向原始出生点
                        Debug.Log("[LevelManager] 启动WalkToRespawnPoint协程");
                        StartCoroutine(WalkToRespawnPoint(controller, actualSpawnPosition));
                    }
                    else if (ifSetPlayer)
                    {
                        // 其他情况（包括死亡重生和重新加载）直接放在重生点
                        controller.MovePosition(respawnTarget.position + Vector3.down * 0.49f);
                    }
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
        /*
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
        }*/
        backGround.transform.position = newLevelGO.transform.position + Vector3.up * yAdjust;
        foreach (Transform child in backGround.transform)
        {
            child.localPosition = Vector3.zero;
        }
        Vector3 intPos = new Vector3(
            Mathf.Round(newLevelGO.transform.position.x),
            Mathf.Round(newLevelGO.transform.position.y),
            Mathf.Round(newLevelGO.transform.position.z)
        );
        if (GridManager.Instance != null) GridManager.Instance.transform.position = intPos;
        //Vector3 topCenter = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f));

        if (ifSetPlayer)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            Debug.Log($"[位置监测] 设置玩家位置后: 位置={player?.transform.position}, 关卡={newLevelIndex}, 是否重启={isRestarting}");
        }

        return data.levelBound;
    }

    private void Update()
    {
        // 每秒监测一次玩家位置 (限制日志频率)
        if (positionMonitorTimer <= 0)
        {
            MonitorPlayerPosition();
            positionMonitorTimer = positionMonitorInterval;
        }
        else
        {
            positionMonitorTimer -= Time.deltaTime;
        }

        // 如果开启了位置锁定，随时检查并修正位置
        if (isPositionLocked)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null && Vector3.Distance(player.transform.position, lockedPosition) > 0.1f)
            {
                player.transform.position = lockedPosition;
            }
        }
    }

    // 监测玩家位置
    private void MonitorPlayerPosition()
    {
        if (!enablePositionMonitoring) return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player == null) return;

        Vector3 currentPosition = player.transform.position;

        // 只有当位置变化超过阈值时才记录
        if (Vector3.Distance(currentPosition, lastRecordedPosition) > 0.5f)
        {
            Debug.Log($"[位置监测] 玩家当前位置: {currentPosition}, 关卡: {currentLevelIndex}, 是否走到重生点中: {isWalkingToSpawn}");
            lastRecordedPosition = currentPosition;
        }
    }

    // 添加走路动画协程
    private IEnumerator WalkToRespawnPoint(PlayerController controller, Vector3 targetPosition)
    {
        isWalkingToSpawn = true;
        Debug.Log($"[位置监测] 开始走向重生点: 起始位置={controller.transform.position}, 目标位置={targetPosition}");

        // 设置朝向（面向右边）
        Vector3 scale = controller.transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        controller.transform.localScale = scale;

        // 播放行走动画
        Animator animator = controller.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        // 行走速度
        float walkSpeed = 5f;

        // 走向重生点
        float startTime = Time.time;
        while (Vector3.Distance(controller.transform.position, targetPosition) > 0.1f)
        {
            controller.transform.position = Vector3.MoveTowards(
                controller.transform.position,
                targetPosition,
                walkSpeed * Time.deltaTime
            );

            // 每2秒记录一次位置
            if (Time.time - startTime > 2f)
            {
                Debug.Log($"[位置监测] 走向重生点中: 当前位置={controller.transform.position}, 距离目标={Vector3.Distance(controller.transform.position, targetPosition)}");
                startTime = Time.time;
            }

            yield return null;
        }

        // 到达重生点
        controller.transform.position = targetPosition;
        Debug.Log($"[位置监测] 到达重生点: 最终位置={controller.transform.position}");

        // 停止行走动画
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        // 等待一小段时间
        yield return new WaitForSeconds(0.5f);

        // 记录恢复控制前的位置
        Vector3 positionBeforeControl = controller.transform.position;
        Debug.Log($"[位置监测] 恢复控制前位置: {positionBeforeControl}");

        // 开启位置锁定
        isPositionLocked = true;
        lockedPosition = positionBeforeControl;
        Debug.Log($"[位置监测] 开启位置锁定: {lockedPosition}");

        // 启动位置锁定协程
        StartCoroutine(LockPlayerPosition(controller, positionLockDuration));

        // 恢复玩家控制
        controller.EnableInput();
        isWalkingToSpawn = false;
        Debug.Log("[位置监测] 玩家控制已恢复");

        // 监控恢复控制后的位置变化
        yield return null; // 等待一帧

        if (Vector3.Distance(controller.transform.position, positionBeforeControl) > 0.1f)
        {
            Debug.LogWarning($"[位置监测] 警告: 恢复控制后位置立即改变! 从 {positionBeforeControl} 变为 {controller.transform.position}");
            // 立即修正位置
            controller.transform.position = positionBeforeControl;
        }
    }

    // 添加位置锁定协程
    private IEnumerator LockPlayerPosition(PlayerController controller, float duration)
    {
        float startTime = Time.time;

        Debug.Log($"[位置监测] 开始位置锁定协程，持续{duration}秒");

        // 锁定期间每帧强制玩家位置
        while (Time.time - startTime < duration && isPositionLocked)
        {
            // 检查位置是否改变
            if (Vector3.Distance(controller.transform.position, lockedPosition) > 0.1f)
            {
                Debug.Log($"[位置监测] 检测到位置变化，从 {lockedPosition} 变为 {controller.transform.position}，正在修正");
                controller.transform.position = lockedPosition;
            }

            yield return null; // 等待下一帧
        }

        // 解除锁定
        isPositionLocked = false;
        Debug.Log("[位置监测] 位置锁定结束");
    }

    // 添加检查方法，用于其他脚本查询是否在走路中
    public bool IsWalkingToSpawn()
    {
        return isWalkingToSpawn;
    }
    public void SwitchToNextLevel()
    {
        GridManager.Instance.RenewSwitch();
        if (currentLevelIndex == maxLevel && sceneIndex < sceneLimit)
        {
            //Destroy(gameObject);
            SceneManager.LoadScene(sceneIndex + 1);
        }
        else
        {
            recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, minLevel, maxLevel), false);
            FindAnyObjectByType<StartEffectController>().transform.position = FindAnyObjectByType<PlayerController>().transform.position + Vector3.up * 1.6f + Vector3.right * 0.1f;
            FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
            //需要获取到当前关卡的初始为止，把StartEffectController设置到该位置；下面这个是临时的
            //StartCoroutine(DelayEffect());
        }
    }

    public void SwitchToNextLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        if (currentLevelIndex == maxLevel && sceneIndex < sceneLimit)
        {
            //Destroy(gameObject);
            SceneManager.LoadScene(sceneIndex + 1);
        }
        else
        {
            recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, minLevel, maxLevel), true);
        }

    }

    public void SwitchToBeforeLevel()
    {
        GridManager.Instance.RenewSwitch();
        if (currentLevelIndex == minLevel && sceneIndex > 1)
        {
            //Destroy(gameObject);
            SceneManager.LoadScene(sceneIndex - 1);
        }
        else
        {
            recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, minLevel, maxLevel), true);
            FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
        }

    }

    public void SwitchToBeforeLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        if (currentLevelIndex == minLevel && sceneIndex > 1)
        {
            //Destroy(gameObject);
            SceneManager.LoadScene(sceneIndex - 1);
        }
        else
        {
            recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, minLevel, maxLevel), true);
        }
    }

    public void RestartLevel()
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        Debug.Log($"[位置监测] 重启关卡前: 玩家位置={player?.transform.position}, 关卡={currentLevelIndex}");

        isRestarting = true;
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(currentLevelIndex, true);
        isRestarting = false;

        player = FindAnyObjectByType<PlayerController>();
        Debug.Log($"[位置监测] 重启关卡后: 玩家位置={player?.transform.position}, 关卡={currentLevelIndex}");
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
