using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class nextTrigger : MonoBehaviour
{
    private void Awake()
    {
        transform.parent.SetParent(null);
        DontDestroyOnLoad(transform.parent.gameObject);
    }
    public BeginPanel bp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PreloadScene()
    {
        PreloadScene(bp.LevelOneName);
    }

    public void DestroySelf()
    {
        Destroy(transform.parent.gameObject);
    }

    public void StartLevel()
    {
        //bp.LoadGameScene();
        ActivateScene();
    }

    private AsyncOperation preloadOperation;

    // 预加载（但不激活）
    public void PreloadScene(string sceneName)
    {
        StartCoroutine(PreloadRoutine(sceneName));
    }

    private IEnumerator PreloadRoutine(string sceneName)
    {
        preloadOperation = SceneManager.LoadSceneAsync(sceneName);
        preloadOperation.allowSceneActivation = false;

        while (preloadOperation.progress < 0.9f)
        {
            Debug.Log($"预加载中... 进度: {preloadOperation.progress}");
            yield return null;
        }

        Debug.Log("场景预加载完成，等待激活...");
    }

    // 激活场景
    void ActivateScene()
    {
        if (preloadOperation != null && preloadOperation.progress >= 0.9f)
        {
            Debug.Log("激活场景！");
            preloadOperation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogWarning("场景尚未完成预加载，无法激活！");
        }
    }
}
