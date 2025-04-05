using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;

    public bool isPaused;

    public string select, settings, beginSceneName;

    void Start(){
        // 确保游戏开始时是运行状态
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.Escape)) {
            TogglePause();
        }
    }

    public void TogglePause(){
        if (isPaused) {
            isPaused = false;
            Time.timeScale = 1f; // 恢复游戏时间
            pauseMenu.SetActive(false);
        }
        else {
            isPaused = true;
            Time.timeScale = 0f; // 暂停游戏时间
            pauseMenu.SetActive(true);
        }
    }

    public void PauseOrResume(){
        if (isPaused) {
            isPaused = false;
            Time.timeScale = 1f; // 恢复游戏时间
            pauseMenu.SetActive(false);
        }
        else {
            isPaused = true;
            Time.timeScale = 0f; // 暂停游戏时间
            pauseMenu.SetActive(true);
        }
    }

    public void SelectGame(){
        Time.timeScale = 1f; // 确保切换场景前恢复时间
        SceneManager.LoadScene(select);
    }

    public void SettingGame(){
        // 设置菜单逻辑
    }

    public void GoToTitle(){
        Time.timeScale = 1f; // 确保切换场景前恢复时间
        SceneManager.LoadScene(beginSceneName);
    }
}