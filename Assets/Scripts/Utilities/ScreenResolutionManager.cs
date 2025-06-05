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

    public bool isFullscreen { get; private set; }

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

        
        AutoSetResolution();
    }

    
    public void AutoSetResolution()
    {
        //int screenWidth = Display.main.systemWidth;

        //if (screenWidth == fullscreenWidth)
        //{
        //    SetFullscreenMode();
        //}
        //else
        //{
        //    SetWindowedMode();
        //}
        Screen.fullScreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) != 0;
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