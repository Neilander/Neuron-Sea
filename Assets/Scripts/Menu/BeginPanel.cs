using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class BeginPanel : MonoBehaviour
{
    public string LevelOneName;
    public string aboutUs;

    public GameObject volume;

    [Header("视频设置")]
    public VideoPlayer videoPlayer;  // 视频播放器组件
    public GameObject videoCanvas;   // 视频画布对象
    public bool skipVideoOnClick = true; // 是否允许点击跳过视频

    private bool isVideoPlaying = false;

    private GameObject blueObject;
    private Image img;
    // Start is called before the first frame update
    void Start()
    {
        blueObject = GameObject.Find("blue");
        blueObject.SetActive(false);
        img = volume.GetComponent<Image>();

        // 初始化视频播放器设置
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;

            // 确保视频画布初始时是禁用的
            if (videoCanvas != null)
            {
                videoCanvas.SetActive(true);
            }
        }
        Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
    }

    // Update is called once per frame
    void Update()
    {
        // 如果视频正在播放且允许点击跳过
        if (isVideoPlaying && skipVideoOnClick && Input.GetMouseButtonDown(0))
        {
            StopVideo();
            LoadGameScene();
        }
    }

    public void StartGame()
    {
        // 首先禁用当前面板

        // 查找名为 "blue" 的对象
        if (blueObject != null)
        {
            // 激活对象
            blueObject.SetActive(true);
            print("激活了");
            // 获取 Animator 组件
            Animator animatorComponent = blueObject.GetComponent<Animator>();
            if (animatorComponent != null)
            {
                // 启用组件并播放动画（需指定动画状态名称或哈希）
                animatorComponent.enabled = true; // 替换为实际动画状态名
                StartCoroutine(WaitForAnimationAndLoadScene(animatorComponent));
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("未在 blue 对象上找到 Animator 组件！");
            }
        }
        else
        {
            Debug.LogError("未找到名为 blue 的对象！");
        }

        // 检查视频播放器（需提前声明变量）
        if (videoPlayer != null && videoCanvas != null)
        {
            // 播放视频的逻辑
        }
        // 检查是否需要播放视频
        if (videoPlayer != null && videoCanvas != null)
        {
            // 激活视频画布
            videoCanvas.SetActive(true);

            // 开始播放视频
            videoPlayer.Play();
            isVideoPlaying = true;

            Debug.Log("开始播放开场视频");
        }
        // else
        // {
        //     // 如果没有视频组件，直接加载场景
        //     Debug.LogWarning("未找到视频播放器组件或视频画布，将直接加载场景");
        //     LoadGameScene();
        // }
    }
    // 协程：等待动画播放完毕

    private IEnumerator WaitForAnimationAndLoadScene(Animator animator)
    {
        // 等待动画开始播放（避免未初始化状态）
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0);

        // 标记是否已经检测到至少一个完整循环
        bool animationCompletedOnce = false;
        float previousTime = 0f;

        // 持续检测动画是否播放完毕
        while (!animationCompletedOnce)
        {
            float currentTime = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            // 如果时间从大变小，说明动画循环了一次
            if (currentTime < previousTime)
            {
                animationCompletedOnce = true;
            }
            // 或者如果时间超过1，也说明完成了一次
            else if (currentTime >= 1.0f)
            {
                animationCompletedOnce = true;
            }

            previousTime = currentTime;
            yield return null; // 每帧等待
        }

        // 动画播放完成后加载场景
        LoadGameScene();
    }

    // 视频播放完成的回调
    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("视频播放完成");
        StopVideo();
        LoadGameScene();
    }

    // 停止视频并隐藏视频画布
    private void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            isVideoPlaying = false;
        }

        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
        }
    }

    // 加载游戏场景
    private void LoadGameScene()
    {
        SceneManager.LoadScene(LevelOneName);
    }

    public void AboutUs()
    {

    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Volume()
    {
        SetPanel.Instance.OpenCanvas();
        FindObjectOfType<ButtonMgr>().SetDefaultState();
        /*
        if(isRed){
            img.color = new Color(255, 255, 255);
        }
        else {
            img.color = new Color(255, 192, 203);
        }*/
    }
}
