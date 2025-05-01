using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelTrigger : MonoBehaviour
{
    private bool hasTriedLoad = false;
    public Vector3 SetPos;

    [Header("提取设置")]
    public string targetObjectName = "Level_13"; // 想从TotalControl中提取的物体名
    private string nextSceneName = "Level 2";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.LogError("我是在leveltrigger切换的");
        // 完成当前关卡
        levelManager levelMgr = FindAnyObjectByType<levelManager>();
        levelMgr.CompleteCurrentLevel();


        // 切换到下一关
        levelMgr.SwitchToNextLevel();

        // 确保在切换关卡后刷新UI
        if (LevelSelectManager.Instance != null)
        {
            LevelSelectManager.Instance.RefreshButtons();
        }
    }

    IEnumerator PreloadAndExtract(string nextName)
    {
        //string currentScene = SceneManager.GetActiveScene().name;

        /*
        Match match = Regex.Match(currentScene, @"Level (\d+)");
        if (!match.Success)
        {
            Debug.LogWarning("当前场景名不是 'Level n' 格式，无法加载");
            yield break;
        }*/

        //int currentLevel = int.Parse(match.Groups[1].Value);
        //int nextLevel = currentLevel + 1;
        //nextSceneName = $"Level {nextLevel}";

        if (!Application.CanStreamedLevelBeLoaded(nextName))
        {
            Debug.LogWarning($"场景 {nextName} 不存在！");
            yield break;
        }

        Debug.Log($"开始加载场景：{nextName}");
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOp.isDone);

        // 获取加载出来的场景
        Scene loadedScene = SceneManager.GetSceneByName(nextName);
        GameObject rootObjects = loadedScene.GetRootGameObjects()[0];

        GameObject target = null;
        var root = rootObjects;


        if (root.name == "TotalControl")
        {
            Transform child = root.transform.Find(targetObjectName);
            if (child != null)
            {
                target = Instantiate(child.gameObject);
                target.transform.position = SetPos;

                // 关闭 BackGrounds
                Transform backgrounds = target.transform.Find("BackGrounds");
                if (backgrounds != null)
                {
                    backgrounds.gameObject.SetActive(false);
                    Debug.Log("已禁用子物体 BackGrounds");
                }

                // 关闭 gridAndSwitchManager
                Transform grid = target.transform.Find("gridAndSwitchManager");
                if (grid != null)
                {
                    grid.gameObject.SetActive(false);
                    Debug.Log("已禁用子物体 gridAndSwitchManager");
                }
            }
        }


        if (target != null)
        {
            target.SetActive(true);
            Debug.Log($"成功提取目标对象：{target.name}");
        }
        else
        {
            Debug.LogWarning($"在 TotalControl 中找不到子物体：{targetObjectName}");
        }

        // 卸载加载用的场景
        SceneManager.UnloadSceneAsync(nextName);
    }

    /*
    private AsyncOperation preloadOp;
    private string nextSceneName;
    private bool isPreloading = false;
    private bool isReady = false;
    private bool hasTriedLoad = false;
    public Vector3 SetPos;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 如果已经加载完成，直接激活
        if (isReady)
        {
            Debug.Log($"激活场景：{nextSceneName}");
            preloadOp.allowSceneActivation = true;
            return;
        }

        // 如果之前已开始加载，说明还没准备好
        if (isPreloading)
        {
            Debug.Log("场景正在加载中...");
            return;
        }

        // ✅ 第一次进入，尝试加载
        if (!hasTriedLoad)
        {
            hasTriedLoad = true;
            PreloadNextScene();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine(LoadSingleObjectFromScene("Level 2", "Level_13", SetPos));
        }
    }*/

    /*
    void PreloadNextScene()
    {
        if (hasTriedLoad)
        {
            Debug.Log("已经尝试过预加载，跳过");
            return;
        }
        hasTriedLoad = true; // ✅ 提前设置为已尝试，避免重复触发

        string currentScene = SceneManager.GetActiveScene().name;

        Match match = Regex.Match(currentScene, @"Level (\d+)");
        if (!match.Success)
        {
            Debug.LogWarning("当前场景名不是 'Level n' 格式，无法预加载");
            return;
        }

        int currentLevel = int.Parse(match.Groups[1].Value);
        int nextLevel = currentLevel + 1;
        nextSceneName = $"Level {nextLevel}";

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogWarning($"场景 {nextSceneName} 不存在！");
            return;
        }

        Debug.Log($"开始预加载场景：{nextSceneName}");
        preloadOp = SceneManager.LoadSceneAsync(nextSceneName, LoadSceneMode.Additive);
        preloadOp.allowSceneActivation = false;
        isPreloading = true;

        StartCoroutine(CheckPreloadReady());
    }*/

    /*
    IEnumerator CheckPreloadReady()
    {
        while (preloadOp != null && preloadOp.progress < 0.9f)
        {
            yield return null;
        }

        if (preloadOp != null)
        {
            isReady = true;
            isPreloading = false;
            Debug.Log($"场景 {nextSceneName} 已预加载完成，可激活");
        }
    }

    IEnumerator LoadSingleObjectFromScene(string sceneName, string objectName, Vector3 targetPosition)
    {
        // 加载场景（Additive 不会切换当前场景）
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => loadOp.isDone);

        // 获取加载出来的场景
        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        GameObject[] rootObjects = loadedScene.GetRootGameObjects();

        GameObject target = null;

        foreach (var root in rootObjects)
        {
            if (root.name == "TotalControl")
            {
                // 在 TotalControl 下查找 objectName（比如 "Level"）
                Transform child = root.transform.Find(objectName);
                if (child != null)
                {
                    target = Instantiate(child.gameObject);
                    target.transform.position = targetPosition;

                    // 查找并禁用 BackGrounds
                    Transform backgrounds = target.transform.Find("BackGrounds");
                    if (backgrounds != null)
                    {
                        backgrounds.gameObject.SetActive(false);
                        Debug.Log("已禁用子物体 BackGrounds");
                    }

                    // 查找并禁用 gridAndSwitchManager
                    Transform grid = target.transform.Find("gridAndSwitchManager");
                    if (grid != null)
                    {
                        grid.gameObject.SetActive(false);
                        Debug.Log("已禁用子物体 gridAndSwitchManager");
                    }

                    break;
                }
            }
        }
        target.SetActive(true);

        // 卸载加载用的场景
        SceneManager.UnloadSceneAsync(sceneName);

        if (target != null)
        {
            Debug.Log($"成功提取目标对象：{target.name}");
        }
        else
        {
            Debug.LogWarning($"在 TotalControl 中找不到子物体：{objectName}");
        }
    }*/


}