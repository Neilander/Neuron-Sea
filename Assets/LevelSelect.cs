using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


public class LevelSelect : MonoBehaviour
{
    [SerializeField] private Transform levelButtonContainer;

    [SerializeField] private GameObject levelButtonPrefab;

    [SerializeField] private Button forwardButton;

    [SerializeField] private Button nextButton;
    private int currentPage = 0;

    private int levelsPerPage = 6; // 每页显示的关卡数量

    private int totalLevels = 36; // 总关卡数量（假设有20个关卡）
    
    // Start is called before the first frame update
    void Start()
    {
        forwardButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);
        UpdateLevelButtons();
    }

    private void UpdateLevelButtons(){
        // 清空当前关卡按钮
        foreach (Transform child in levelButtonContainer) {
            Destroy(child.gameObject);
        }

        // 计算当前页的关卡索引范围
        int startLevelIndex = currentPage * levelsPerPage;
        int endLevelIndex = Mathf.Min(startLevelIndex + levelsPerPage, totalLevels);

        // 生成当前页的关卡按钮
        for (int i = startLevelIndex; i < endLevelIndex; i++) {
            GameObject levelButton = Instantiate(levelButtonPrefab, levelButtonContainer);
            levelButton.GetComponentInChildren<TMP_Text>().text = "Level " + (i + 1);
            // 绑定关卡点击事件
            int levelIndex = i; // 需要局部变量来捕获当前索引
            levelButton.GetComponent<Button>().onClick.AddListener(() => LoadLevel(levelIndex));
        }

        // 更新翻页按钮状态
        forwardButton.interactable = currentPage > 0;
        nextButton.interactable = endLevelIndex < totalLevels;
    }

    private void PreviousPage(){
        if (currentPage > 0) {
            currentPage--;
            UpdateLevelButtons();
        }
    }

    private void NextPage(){
        if ((currentPage + 1) * levelsPerPage < totalLevels) {
            currentPage++;
            UpdateLevelButtons();
        }
    }

    private void LoadLevel(int levelIndex){
        Debug.Log("Loading Level: " + (levelIndex + 1));
        levelManager.instance.LoadLevel(levelIndex+1,true);
        FindAnyObjectByType<PauseMenu>().transform.Find("PauseMenu").gameObject.SetActive(false);
        Time.timeScale = 1;
        gameObject.SetActive(false);
    }
    
}
