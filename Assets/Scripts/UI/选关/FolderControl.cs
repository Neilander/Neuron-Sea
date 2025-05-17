using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolderControl : MonoBehaviour
{
    // 这个是当前按钮对应的ScrollView (关卡列表)
    public GameObject associatedScrollView;
    
    // 所有其他的FolderControl组件引用(不是GameObject)
    public FolderControl[] otherFolders;
    
    // 当前文件夹是否展开
    [SerializeField] private bool isExpanded = false;
    
    void Start()
    {
        // 确保初始状态下关卡列表是隐藏的
        if (associatedScrollView != null)
        {
            associatedScrollView.SetActive(false);
        }
    }
    
    public void ToggleLevels()
    {
        // 切换展开状态
        isExpanded = !isExpanded;
        
        // 更新UI显示
        if (associatedScrollView != null)
        {
            associatedScrollView.SetActive(isExpanded);
        }
        
        // 如果展开当前文件夹，则关闭所有其他文件夹
        if (isExpanded)
        {
            CloseOtherFolders();
        }
    }
    
    // 关闭所有其他文件夹
    private void CloseOtherFolders()
    {
        foreach (FolderControl otherFolder in otherFolders)
        {
            if (otherFolder != null && otherFolder != this)
            {
                otherFolder.CloseFolder();
            }
        }
    }
    
    // 直接关闭当前文件夹
    public void CloseFolder()
    {
        isExpanded = false;
        
        if (associatedScrollView != null)
        {
            associatedScrollView.SetActive(false);
        }
    }
    
    // 获取当前展开状态
    public bool IsExpanded()
    {
        return isExpanded;
    }
}
