using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolderControl : MonoBehaviour
{


    public GameObject levels; // Reference to the Levels GameObject

    public GameObject[] otherLevels;
    private bool isExpanded = false;

    public void ToggleLevels(){
        isExpanded = !isExpanded;
        levels.SetActive(isExpanded);
        
        // 点击后显示当前文件夹的关卡，隐藏其他文件夹的关卡
        if (isExpanded) {
            // 显示当前文件夹的关卡
            levels.SetActive(true);
            
            // 隐藏其他文件夹的关卡
            foreach (GameObject otherLevel in otherLevels) {
                otherLevel.SetActive(false);
            }
        }
    }
}
