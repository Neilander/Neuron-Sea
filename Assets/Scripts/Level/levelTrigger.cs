using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class levelTrigger : MonoBehaviour
{
    private AsyncOperation preloadOp;
    private string nextSceneName;
    private bool isPreloading = false;
    private bool isReady = false;
    private bool hasTriedLoad = false;

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
    }

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
}