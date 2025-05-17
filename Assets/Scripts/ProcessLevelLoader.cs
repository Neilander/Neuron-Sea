using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProcessLevelLoader : MonoBehaviour
{
    public static ProcessLevelLoader instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    [Header("填入进度条 Image（类型需为 Filled）")]
    public Image loadingFillImage;

    public GameObject GroupToOpen;

    // 调用这个函数来加载场景
    public void LoadSceneWithTransition(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        GroupToOpen.SetActive(true);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;

        float fakeProgress = 0f;

        while (!op.isDone)
        {
            // 匀速推进 fakeProgress 到 1f（速度可调）
            fakeProgress = Mathf.MoveTowards(fakeProgress, 1f, Time.deltaTime * 0.5f);

            // 实际加载进度：Unity 最大只到 0.9，这里映射到 1.0
            float realProgress = Mathf.Clamp01(op.progress / 1f);

            // 最终展示进度：取两者中的最大值（总能看到前进）
            float displayProgress = Mathf.Min(fakeProgress, realProgress);
            Debug.Log(displayProgress);
            if (loadingFillImage != null)
                loadingFillImage.fillAmount = displayProgress;

            yield return null;
        }
    }
}
