using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetPanel : MonoBehaviour
{
    public static SetPanel Instance{get; private set;}
    [Header("Panels")] public GameObject panel1;

    public GameObject panel2;

    public GameObject panel3;

    [Header("Buttons")] public Button btn1;

    public Button btn2;

    public Button btn3;
    public Button closeBtn;

    public Animator backgroundAnimator;
    public Animator backgroundAnimator2;

    public GameObject conceptArt;

    

    private void Awake(){
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // 防止重复
            return;
        }

        Instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        // 绑定按钮点击事件
        btn1.onClick.AddListener(() => ShowPanel(panel1));
        btn2.onClick.AddListener(() => ShowPanel(panel2));
        btn3.onClick.AddListener(() => ShowPanel(panel3));
        // 默认显示第一个面板
        ShowPanel(panel1);
        if(backgroundAnimator) backgroundAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        if(backgroundAnimator2) backgroundAnimator2.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (!conceptArt.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            GetComponentInChildren<ClickAndExit>().Exit();
        }
    }

    public void OpenCanvas(){
        gameObject.SetActive(true);
        // 隐藏所有面板
        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);

        // 显示目标面板
        panel1.SetActive(true);
    }

    private void ShowPanel(GameObject targetPanel){
        // 隐藏所有面板
        panel1.SetActive(false);
        panel2.SetActive(false);
        panel3.SetActive(false);

        // 显示目标面板
        targetPanel.SetActive(true);
    }

    public void ResetStory()
    {
        Debug.Log("清除剧情加载记录");
        PlayerPrefs.SetInt("SGLM_Scene1Loaded", 0);
        PlayerPrefs.SetInt("SGLM_Scene2Loaded", 0);
        PlayerPrefs.SetInt("SGLM_Scene3Loaded", 0);
        PlayerPrefs.SetInt("BeginSceneVid", 0);
        PlayerPrefs.Save();
        StoryGlobalLoadManager.instance.ResetStory();
        BeginPanel beginPanel = FindAnyObjectByType<BeginPanel>();
        if (beginPanel != null)
        {
            beginPanel.ifStartVid = true;
            Debug.Log("可以播放开场剧情了！");
        }
    }
}
