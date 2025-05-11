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
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ContinueGame();
        }
    }

    public void ContinueGame(){
        print("我是暂停面板，我被点了");
        if (!(StoryManager.Instance.currentState == GameState.StoryMode)) {
            if (isPaused) {
                isPaused = false;
                Time.timeScale = 1;
                pauseMenu.SetActive(false);
                AudioManager.Instance.Play(SFXClip.Cilck3);
            }
            else {
                isPaused = true;
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
                AudioManager.Instance.Play(SFXClip.Cilck4);
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
        SceneManager.LoadScene(0);
    }

    public void RestartLevel(){
        levelManager.instance.RestartLevel();
    }
}
