using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingMenu;
    public GameObject levelSelectPanel;

    public bool isPaused;
    public string select,settings,beginSceneName;

    private void Awake(){
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (!settingMenu.activeSelf && !levelSelectPanel.activeSelf)
            {
                ContinueGame();
            }
        }
    }

    public void ContinueGame(){
        print("我是暂停面板，我被点了");
        //if (!(StoryManager.Instance.currentState == GameState.StoryMode)) {
        if (!ActivityGateCenter.IsStateActiveAny(ActivityState.Story,ActivityState.StartEffectMove))
        {
            if (isPaused) {
                ForceResume();
                AudioManager.Instance.Play(SFXClip.Cilck3,gameObject.name);
            }
            else {
                ActivityGateCenter.EnterState(ActivityState.Pause);
                isPaused = true;
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
                AudioManager.Instance.Play(SFXClip.Cilck4,gameObject.name);
            }
        }
    }

    public void SelectGame(){
        levelSelectPanel.SetActive(true);
    }

    public void ExitSetPanel(){
        levelSelectPanel.SetActive(false);
    }

    public void SettingGame(){
        print("我是设置面板,我被电了");
        settingMenu.SetActive(true);
        print("注意看,开了");
        Canvas canvas = settingMenu.GetComponent<Canvas>();
        // if (canvas != null) {
            canvas.sortingOrder = 100; // 设置一个比其他 UI 更高的值
        // }
    }

    public void GoToTitle(){
        ForceResume();
        SceneManager.LoadScene(0);
    }

    public void RestartLevel(){
        ForceResume();
        levelManager.instance.RestartLevel();
    }

    public void ForceResume(){
        ActivityGateCenter.ExitState(ActivityState.Pause);
        isPaused = false;
        Time.timeScale = ActivityGateCenter.IsStateActive(ActivityState.BulletTime)?0.2f:1;
        levelSelectPanel.SetActive(false);
        settingMenu.SetActive(false);
        pauseMenu.SetActive(false);
    }
}
