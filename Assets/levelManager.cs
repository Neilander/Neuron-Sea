using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelManager : MonoBehaviour
{

    public static levelManager instance;

    public Transform respawnTarget;
    public int currentLevelIndex = 0;  // 当前关卡编号

    private GameObject currentLevelGO;
    private CameraControl cameraControl;
    public GameObject backGround;

    private Rect recordRect;

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
            LoadLevel(currentLevelIndex, true);
            SceneManager.sceneLoaded += OnSceneLoaded; // ⬅️ 注册场景加载回调
        }
        else
        {
            Destroy(gameObject);
            return;
        }


    }

    private void Start()
    {
        AudioManager.Instance.Play(WhiteNoiseClip.Scene1);
    }

    public Rect LoadLevel(int newLevelIndex, bool ifSetPlayer)
    {
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
            currentLevelGO.SetActive(false);
        }

        // 启用新关卡
        newLevelGO.SetActive(true);
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
                        Debug.Log($"已将重生点 {respawnTarget.name} 设置给DeathController"+deathController.gameObject.name);
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
                    if (ifSetPlayer) controller.transform.position = respawnTarget.position + Vector3.down * 0.5f;
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
        backGround.transform.position = newLevelGO.transform.position;
        return data.levelBound;
    }

    private void Update()
    {

    }

    public void SwitchToNextLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, 1, 12), false);
        FindAnyObjectByType<StartEffectController>().transform.position = FindAnyObjectByType<PlayerController>().transform.position + Vector3.up * 1.6f + Vector3.right * 0.1f;
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
        //需要获取到当前关卡的初始为止，把StartEffectController设置到该位置；下面这个是临时的
        //StartCoroutine(DelayEffect());
    }

    public void SwitchToNextLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, 1, 12), true);
    }

    public void SwitchToBeforeLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, 1, 12), false);
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
    }

    public void SwitchToBeforeLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, 1, 12), true);
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
            Debug.LogError("新场景中主相机缺少 CameraControl！");
            return;
        }

        // 重新加载当前关卡（基于 currentLevelIndex）
        LoadLevel(currentLevelIndex, true);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
