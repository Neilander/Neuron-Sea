using UnityEngine;

public class ScreenResolutionManager : MonoBehaviour
{
    public static ScreenResolutionManager Instance { get; private set; }

    [Header("分辨率设置")]
    public int fullscreenWidth = 1920;
    public int fullscreenHeight = 1080;
    public int windowedWidth = 1920;
    public int windowedHeight = 1080;


    private const string FULLSCREEN_KEY = "FullScreenButton";
    private const string WINDOWWIDTH_KEY = "WindowWidth";
    private const string WINDOWHEIGHT_KEY = "WindowHeight";

    public bool isFullscreen { get; private set; }
    private bool lastFullScreenState;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize(){
        InitializeResolution();
    }
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        lastFullScreenState = Screen.fullScreen;
        AutoSetResolution();
    }
    void Update()
    {
        // 每帧检查状态是否变化
        bool currentState = Screen.fullScreen;

        if (currentState != lastFullScreenState)
        {
            // 状态发生变化
            lastFullScreenState = currentState;
            Debug.Log("全屏状态变化: " + currentState);

            // 调用自定义事件
            OnFullScreenChanged(currentState);
        }
    }

    void OnFullScreenChanged(bool isFullScreen)
    {
        // 在这里添加全屏状态变化后的处理逻辑
        Debug.Log("处理全屏变化: " + isFullScreen);

        if ((PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) != 0) != isFullScreen)
        {
            SetPanel.Instance?.SwitchFullscreen();
        }
    }

    public void AutoSetResolution()
    {
        //    if (PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) != 0)
        //    {
        //        if (Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.MaximizedWindow) return;
        //        int screenWidth = Display.main.systemWidth;
        //        int screenHeight = Display.main.systemHeight;
        //        Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.FullScreenWindow);
        //        isFullscreen = true;Debug.Log(Screen.fullScreenMode == FullScreenMode.FullScreenWindow || Screen.fullScreenMode == FullScreenMode.MaximizedWindow);
        //    }
        //    else
        //    {
        //        if (Screen.fullScreenMode != FullScreenMode.FullScreenWindow && Screen.fullScreenMode != FullScreenMode.MaximizedWindow) return;
        //        Screen.SetResolution(PlayerPrefs.GetInt(WINDOWWIDTH_KEY, 1920), PlayerPrefs.GetInt(WINDOWHEIGHT_KEY, 1080), FullScreenMode.Windowed);
        //        isFullscreen = false;
        //    }
        if (PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) != 0)
        {
            if (Screen.fullScreen) return;
            PlayerPrefs.SetInt(WINDOWWIDTH_KEY, Screen.width);
            PlayerPrefs.SetInt(WINDOWHEIGHT_KEY, Screen.height);
            int screenWidth = Display.main.systemWidth;
            int screenHeight = Display.main.systemHeight;
            Screen.SetResolution(screenWidth, screenHeight, FullScreenMode.FullScreenWindow);
        }
        else
        {
            if (!Screen.fullScreen) return; 
            Screen.SetResolution(PlayerPrefs.GetInt(WINDOWWIDTH_KEY, 1920), PlayerPrefs.GetInt(WINDOWHEIGHT_KEY, 1080), FullScreenMode.Windowed);
        }
    }


    public void SetFullscreenMode()
    {
        Screen.SetResolution(fullscreenWidth, fullscreenHeight, FullScreenMode.ExclusiveFullScreen);
        isFullscreen = true;
    }

    
    public void SetWindowedMode()
    {
        Screen.SetResolution(windowedWidth, windowedHeight, FullScreenMode.Windowed);
        isFullscreen = false;
    }
    
    public void ToggleFullscreen()
    {
        if (isFullscreen)
        {
            SetWindowedMode();
        }
        else
        {
            SetFullscreenMode();
        }
    }
    
    public static void InitializeResolution()
    {
        // 如果实例不存在，创建一个
        if (Instance == null)
        {
            GameObject go = new GameObject("ScreenResolutionManager");
            go.AddComponent<ScreenResolutionManager>();
        }
        else
        {
            Instance.AutoSetResolution();
        }
    }
}