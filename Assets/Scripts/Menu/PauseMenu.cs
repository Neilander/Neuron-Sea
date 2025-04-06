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
        if (isPaused) {
            isPaused = false;
            pauseMenu.SetActive(false);
        }
        else {
            isPaused = true;
            pauseMenu.SetActive(true);
        }
    }

    public void SelectGame(){
        SceneManager.LoadScene(select);
    }

    public void SettingGame(){
        
    }

    public void GoToTitle(){
        SceneManager.LoadScene(beginSceneName);
    }
}
