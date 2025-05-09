using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;

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
        if (!(StoryManager.Instance.currentState == GameState.StoryMode)) {
            if (isPaused) {
                isPaused = false;
                Time.timeScale = 1;

                pauseMenu.SetActive(false);
            }
            else {
                isPaused = true;
                Time.timeScale = 0;
                pauseMenu.SetActive(true);
            }
        }
    }

    public void SelectGame(){
        transform.Find("LevelSelect2").gameObject.SetActive(true);
    }

    public void ExitSetPanel(){
        transform.Find("LevelSelect2").gameObject.SetActive(false);
    }

    public void SettingGame(){
        if (SetPanel.Instance != null) SetPanel.Instance.OpenCanvas();
    }

    public void GoToTitle(){
        SceneManager.LoadScene(beginSceneName);
    }
}
