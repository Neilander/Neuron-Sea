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

    public GameObject currentLevelGO { get; private set; }
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
    public bool enableLevel13SpecialSpawn = true; // 控制是否启用第13关特殊出生
    public float walkInDistance = 10f; // 从重生点左边多远开始走
    private bool isWalkingToSpawn = false;
    private bool isRestarting = false;
    public float specialStartTime = 1;
    
    private Vector3 lockedPosition = Vector3.zero;
    private float positionLockDuration = 2f; // 锁定持续时间



    const int sceneLimit = 3;

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
            bool ifDirect = true;
            switch (sceneIndex)
            {
                case 1:
                    Debug.Log("至少在这里");
                    if (PlayerPrefs.GetInt("hasLoadOnce") == 1)
                    {
                        Debug.Log("开始场景");
                        CameraControl.Instance.hasLoadOnce = !cameraControl.ifReverTutorialTrigger;
                    }
                    break;

                case 2:
                    if (PlayerPrefs.GetInt("hasScene2LoadOnce") == 1)
                    {
                        cameraControl.hasLoadOnce = !cameraControl.ifReverTutorialTrigger;
                        if (!cameraControl.hasLoadOnce)
                            ifDirect = false;
                    }
                    break;

                case 3:
                    if (PlayerPrefs.GetInt("hasScene3LoadOnce") == 1)
                    {
                        cameraControl.hasLoadOnce = !cameraControl.ifReverTutorialTrigger;
                        if (!cameraControl.hasLoadOnce)
                            ifDirect = false;
                    }
                    break;
            }
            LoadLevel(Mathf.Clamp(currentLevelIndex, minLevel, maxLevel),ifDirect);
            for (int i = 0; i < 4; i++)
            {
                if(i == sceneIndex)
                {
                    AudioManager.Instance.Play((BGMClip)i);
                }
                else
                {
                    AudioManager.Instance.Stop((BGMClip)i);
                    AudioManager.Instance.Stop((WhiteNoiseClip)i);
                }
            }
            if (sceneIndex == 1)
            {
                AudioManager.Instance.Play(WhiteNoiseClip.Scene1);
            }
            else
            {
                AudioManager.Instance.Stop(WhiteNoiseClip.Scene1);
            }
            StartCoroutine(RegisterNextFrame());



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

    private IEnumerator RegisterNextFrame()
    {
        yield return null; // 等待当前帧结束（也就是本次 sceneLoaded 已经发出）
        SceneManager.sceneLoaded += OnSceneLoaded;
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

    public void RefreshEdgeCheck()
    {
        FindAnyObjectByType<PlayerController>().CheckEdge = true;
    }

    public Rect LoadLevel(int newLevelIndex, bool ifSetPlayerToAndNoMovement)
    {
        if (GridManager.Instance != null) GridManager.Instance.RefreshSelection();
        PlayerController controller = FindAnyObjectByType<PlayerController>();
        controller.CheckEdge = false;
        Invoke("RefreshEdgeCheck",0.1f);
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
            //Debug.Log("已经加载level bound");
        }
        else
        {
            Debug.LogWarning($"关卡 {newLevelName} 上没有找到 LevelData 组件！");
        }

        //PlayerController controller = FindAnyObjectByType<PlayerController>();
        if (isStartStory && newLevelGO.name == "Level_1")
        {
            controller.DisableInput();
        }
        controller.SetMovementBounds(data.levelBound);

        Transform entities = newLevelGO.transform.Find("Entities");
        if (entities != null) {
            Transform respawnTarget = null;

            foreach (Transform child in entities) {
                if (cameraControl.hasLoadOnce) {
                    Debug.Log("多次触发");
                    if (child.name.StartsWith("Respawn")) {
                        respawnTarget = child;
                        this.respawnTarget = child;

                        // 找到重生点后，立即设置给DeathController
                        DeathController deathController = FindAnyObjectByType<DeathController>();
                        if (deathController != null) {
                            deathController.respawnTarget = respawnTarget;
                            //Debug.Log($"已将重生点 {respawnTarget.name} 设置给DeathController" + deathController.gameObject.name);
                        }
                        else {
                            Debug.LogError("未找到DeathController，无法设置重生点！");
                        }

                        break;
                    }
                }
                else 
                {
                    Debug.Log("第一次触发");
                    if (child.name.StartsWith("Start")) {
                        respawnTarget = child;
                        this.respawnTarget = child;

                        // 找到重生点后，立即设置给DeathController
                        DeathController deathController = FindAnyObjectByType<DeathController>();
                        if (deathController != null) {
                            deathController.respawnTarget = respawnTarget;
                            Debug.Log($"已将重生点 {respawnTarget.name} 设置给DeathController" + deathController.gameObject.name);
                        }
                        else {
                            Debug.LogError("未找到DeathController，无法设置重生点！");
                        }


                        switch (sceneIndex)
                        {
                            case 1:
                                PlayerPrefs.SetInt("hasLoadOnce",1);
                                break;

                            case 2:
                                PlayerPrefs.SetInt("hasScene2LoadOnce", 1);
                                break;

                            case 3:
                                PlayerPrefs.SetInt("hasScene3LoadOnce", 1);
                                break;
                        }
                        
                        break;
                    }
                }
            }

            if (respawnTarget != null) {
                    StartEffectController effectController = FindAnyObjectByType<StartEffectController>();
                    // PlayerController controller = FindAnyObjectByType<PlayerController>();

                    if (effectController != null) {
                        // 无论何种情况，始终将开始特效放在原始重生点位置
                        effectController.transform.position = respawnTarget.position;

                    // 检查是否是第13关，并且是首次加载（不是死亡重生或重新加载）
                    /*
                    if (!ifSetPlayerToAndNoMovement||(newLevelIndex == 13 && !isRestarting && enableLevel13SpecialSpawn && !cameraControl.hasLoadOnce)|| (newLevelIndex == 25 && !isRestarting&& !cameraControl.hasLoadOnce))
                    {
                        // // 禁用玩家输入
                        // controller.DisableInput();
                        //
                        // 计算出生点的实际位置（带偏移）
                        Vector3 actualSpawnPosition = respawnTarget.position + Vector3.down * 0.49f;
                        //Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                        // 设置玩家初始位置（在重生点左边）
                        Vector3 startPos = actualSpawnPosition + Vector3.left * walkInDistance;
                        Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                        // 移动玩家到左侧位置
                        controller.MovePosition(startPos);
                        Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                        // // 开始走路动画 - 走向原始出生点
                        // StartCoroutine(WalkToRespawnPoint(controller, actualSpawnPosition));
                        effectController.TriggerStartEffect(true, specialStartTime);
                        Debug.Log(FindObjectOfType<PlayerController>().transform.position);
                        
                    }
                    else
                    {
                        controller.MovePosition(respawnTarget.position + Vector3.down * 0.49f);
                    }*/
                    Debug.Log("错误检测0");
                    if (ifSetPlayerToAndNoMovement)
                    {
                        controller.MovePosition(respawnTarget.position + Vector3.down * 0.49f);
                    }
                    else
                    {
                        Debug.Log("错误检测1");
                        if ((newLevelIndex == 13 && !isRestarting && enableLevel13SpecialSpawn && !cameraControl.hasLoadOnce) || (newLevelIndex == 25 && !isRestarting && !cameraControl.hasLoadOnce))
                        {
                            Debug.Log("错误检测2");
                            // // 禁用玩家输入
                            // controller.DisableInput();
                            //
                            // 计算出生点的实际位置（带偏移）
                            Vector3 actualSpawnPosition = respawnTarget.position + Vector3.down * 0.49f;
                            //Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                            // 设置玩家初始位置（在重生点左边）
                            Vector3 startPos = actualSpawnPosition + Vector3.left * walkInDistance;
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                            // 移动玩家到左侧位置
                            controller.MovePosition(startPos);
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                            // // 开始走路动画 - 走向原始出生点
                            // StartCoroutine(WalkToRespawnPoint(controller, actualSpawnPosition));
                            effectController.TriggerStartEffect(true, specialStartTime);
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);
                        }
                    }

                    /*
                        if (newLevelIndex == 13 && !ifSetPlayer && !isRestarting && enableLevel13SpecialSpawn) {
                        Debug.Log("不像在这啊");
                        // // 禁用玩家输入
                        // controller.DisableInput();
                        //
                        // 计算出生点的实际位置（带偏移）
                        Vector3 actualSpawnPosition = respawnTarget.position + Vector3.down * 0.49f;
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                            // 设置玩家初始位置（在重生点左边）
                            Vector3 startPos = actualSpawnPosition + Vector3.left * walkInDistance;
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                            // 移动玩家到左侧位置
                            controller.MovePosition(startPos);
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                            // // 开始走路动画 - 走向原始出生点
                            // StartCoroutine(WalkToRespawnPoint(controller, actualSpawnPosition));
                            effectController.TriggerStartEffect(true, specialStartTime);
                            Debug.Log(FindObjectOfType<PlayerController>().transform.position);

                        }
                        else if (ifSetPlayer) {
                            // 其他情况（包括死亡重生和重新加载）直接放在重生点
                            Debug.Log("冲冲冲");
                            controller.MovePosition(respawnTarget.position + Vector3.down * 0.49f);
                        }*/
                }
                }
            else {
                Debug.Log("难道在这？");
                    // 如果没有找到重生点，创建一个默认的重生点
                    GameObject defaultRespawnObj = new GameObject("Respawn_Default");
                    defaultRespawnObj.transform.parent = entities;
                    defaultRespawnObj.transform.position = new Vector3(data.levelBound.center.x, data.levelBound.center.y, 0);

                    respawnTarget = defaultRespawnObj.transform;
                    this.respawnTarget = respawnTarget;

                    // 设置给DeathController
                    DeathController deathController = FindAnyObjectByType<DeathController>();
                    if (deathController != null) {
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
        if (!cameraControl.hasLoadOnce && (sceneIndex == 1))
        {
            cameraControl.specialStartForScene1 = true;
        }
        cameraControl.hasLoadOnce = true;

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

        if (ifSetPlayerToAndNoMovement)
        {
            PlayerController player = FindAnyObjectByType<PlayerController>();
            Debug.Log($"[位置监测] 设置玩家位置后: 位置={player?.transform.position}, 关卡={newLevelIndex}, 是否重启={isRestarting}");
        }

        return data.levelBound;
    }

    private void Update()
    {
        
    }

    // 修改WalkToRespawnPoint协程，使用Speed控制但不禁止移动
    private IEnumerator WalkToRespawnPoint(PlayerController controller, Vector3 targetPosition)
    {
        isWalkingToSpawn = true;
        Debug.Log($"开始特殊出生走路 - 起点:{controller.Position}, 终点:{targetPosition}");

        // 设置朝向（面向右边）
        Vector3 scale = controller.transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        controller.transform.localScale = scale;

        // 播放行走动画
        Animator animator = controller.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetFloat("Speed", 0.5f);
        }

        // 只禁用输入，但允许移动
        controller.DisableInput();  // 禁用输入，玩家不能控制
        controller.EnableMovement(); // 允许移动，这样Speed设置会生效

        // 行走速度设置更高一些
        float walkSpeed = 7f;
        Vector2 direction = ((Vector2)targetPosition - controller.Position).normalized;
        Debug.Log($"方向向量: {direction}, 速度大小: {walkSpeed}");

        // 走向重生点 - 强制设置移动参数
        float progress = 0;
        while (Mathf.Abs(controller.Position.x - targetPosition.x) > 0.1f)
        {
            progress += Time.deltaTime;
            Debug.Log($"走路进度: {progress}秒, 当前位置: {controller.Position}, 距离目标: {Vector2.Distance(controller.Position, targetPosition)}");

            // 使用多种方式确保移动生效
            controller.Speed = new Vector2(walkSpeed, 0); // 直接设置水平速度

            // // 如果完全没有移动，尝试直接设置位置
            // if (progress > 1 && Vector2.Distance(controller.Position, controller.Position + new Vector2(walkSpeed * Time.deltaTime, 0)) < 0.01f)
            // {
            //     Debug.Log("检测到没有移动，尝试直接调整位置");
            //     Vector2 newPos = Vector2.MoveTowards(controller.Position, targetPosition, walkSpeed * Time.deltaTime);
            //     controller.MovePosition(newPos);
            // }

            yield return null;
        }

        Debug.Log("到达目标位置");

        // 到达重生点 - 使用MovePosition方法直接设置最终位置
        controller.MovePosition(targetPosition);

        // 停止行走动画
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        // 设置速度为0
        controller.Speed = Vector2.zero;

        // 等待一小段时间
        yield return new WaitForSeconds(0.5f);

        // 恢复玩家输入控制
        controller.EnableInput();
        isWalkingToSpawn = false;
        Debug.Log("特殊出生过程完成，已恢复玩家控制");

        // 触发故事
        StartCoroutine(TriggerStoryAfterDelay(0.2f));
    }

    // 添加一个延迟触发故事的协程
    private IEnumerator TriggerStoryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 查找并触发故事
        GameObject storyObj = GameObject.Find("Story4.1");
        if (storyObj != null)
        {
            StoryTrigger storyTrigger = storyObj.GetComponent<StoryTrigger>();
            if (storyTrigger != null)
            {
                storyTrigger.ForceStartStory();
            }
            else
            {
                Debug.LogWarning("未找到StoryTrigger组件");
            }
        }
        else
        {
            Debug.LogWarning("未找到Story4.1对象");
        }
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
            //SceneManager.LoadScene(sceneIndex + 1);
        }
        else
        {
            Debug.Log("我要走入"+ Mathf.Clamp(currentLevelIndex + 1, minLevel, maxLevel));
            recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, minLevel, maxLevel), false);
            FindAnyObjectByType<StartEffectController>().transform.position = FindAnyObjectByType<PlayerController>().transform.position + Vector3.up * 1.6f + Vector3.right * 0.1f;
            FindAnyObjectByType<StartEffectController>().TriggerStartEffect(true);
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
            Debug.Log("返回时，现在的Level是"+currentLevelIndex);
            recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, minLevel, maxLevel), false);
            //FindAnyObjectByType<StartEffectController>().TriggerStartEffect(false);
            Transform entities = currentLevelGO.transform.Find("Entities");
            if (entities != null)
            {
                foreach (Transform child in entities)
                {
                    if (child.name.StartsWith("Piece"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
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
        if (cameraControl.endTeach&&!(StoryManager.Instance.currentState==GameState.StoryMode)) {
            isRestarting = true;
            GridManager.Instance.RenewSwitch();
            recordRect = LoadLevel(currentLevelIndex, true);
            isRestarting = false;
        }
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
