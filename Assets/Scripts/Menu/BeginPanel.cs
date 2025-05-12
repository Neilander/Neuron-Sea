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

    private Image img;

    public bool ifStartVid;
    public GameObject vidObj;
    // Start is called before the first frame update
    void Start()
    {
        //尝试修改分辨率
        int screenWidth = Display.main.systemWidth;
    int screenHeight = Display.main.systemHeight;

    if (screenWidth == 1920)
    {
        Screen.SetResolution(1920, 1080, FullScreenMode.ExclusiveFullScreen);
    }
    else
    {
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }
        img = volume.GetComponent<Image>();

        ifStartVid = PlayerPrefs.GetInt("BeginSceneVid") == 0;

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

    //private void OnApplicationQuit()
    //{
    //    PlayerPrefs.SetInt("BeginSceneVid", 0);
    //}

    public void StartGame()
    {
        // 首先禁用当前面板
        gameObject.SetActive(false);

        if (ifStartVid)
        {
            if (vidObj != null) //&& ifStartVid
            {
                vidObj.SetActive(true);
                gameObject.SetActive(false);
                PlayerPrefs.SetInt("BeginSceneVid", 1);
                PlayerPrefs.Save();
                AudioManager.Instance.Stop(BGMClip.SceneBegin);
                AudioManager.Instance.Play(SFXClip.StartVideo,gameObject.name);
            }
        }
        else
        {
            LoadGameScene();
        }

        // 检查是否需要播放视频
        /*
        if (videoPlayer != null && videoCanvas != null)
        {
            // 激活视频画布
            videoCanvas.SetActive(true);

            // 开始播放视频
            videoPlayer.Play();
            isVideoPlaying = true;

            Debug.Log("开始播放开场视频");
        }
        else
        {
            // 如果没有视频组件，直接加载场景
            Debug.LogWarning("未找到视频播放器组件或视频画布，将直接加载场景");
            LoadGameScene();
        }*/
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
    public void LoadGameScene()
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
