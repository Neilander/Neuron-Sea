using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelManager : MonoBehaviour
{

    public static levelManager instance;
    public int currentLevelIndex = 0;  // 当前关卡编号

    private GameObject currentLevelGO;
    private CameraControl cameraControl;

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
            LoadLevel(currentLevelIndex,true);
            SceneManager.sceneLoaded += OnSceneLoaded; // ⬅️ 注册场景加载回调
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        
    }
    
    public Rect LoadLevel(int newLevelIndex, bool ifSetPlayer)
    {
        string newLevelName = $"Level_{newLevelIndex}";
        GameObject newLevelGO = FindInactiveObjectByName($"Level_{newLevelIndex}");
        Debug.Log("加载"+newLevelName);
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

        Transform entities = newLevelGO.transform.Find("Entities");
        if (entities != null)
        {
            Transform respawnTarget = null;

            foreach (Transform child in entities)
            {
                if (child.name.StartsWith("Respawn"))
                {
                    respawnTarget = child;
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
                    if(ifSetPlayer)controller.transform.position = respawnTarget.position + Vector3.down*0.5f;
                    //
                }
                else
                {
                    Debug.LogWarning("未找到 StartEffectController");
                }
            }
            else
            {
                Debug.LogWarning("Entities 中没有找到以 Respawn 开头的物体");
            }
        }
        else
        {
            Debug.LogWarning("未找到 Entities 物体");
        }
        
        return data.levelBound;
    }

    private void Update()
    {
        
    }

    public void SwitchToNextLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, 1, 12), false);
        FindAnyObjectByType<StartEffectController>().transform.position = FindAnyObjectByType<PlayerController>().transform.position+ Vector3.up*0.5f+Vector3.right*0.1f;
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
        //需要获取到当前关卡的初始为止，把StartEffectController设置到该位置；下面这个是临时的
        //StartCoroutine(DelayEffect());
    }

    public void SwitchToNextLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex + 1, 1, 12),true);
    }

    public void SwitchToBeforeLevel()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1,1,12),false);
        FindAnyObjectByType<StartEffectController>().TriggerStartEffect();
    }

    public void SwitchToBeforeLevel_Direct()
    {
        GridManager.Instance.RenewSwitch();
        recordRect = LoadLevel(Mathf.Clamp(currentLevelIndex - 1, 1, 12),true);
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
        LoadLevel(currentLevelIndex,true);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
