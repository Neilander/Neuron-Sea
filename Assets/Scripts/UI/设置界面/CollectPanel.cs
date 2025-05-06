using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CollectPanel : MonoBehaviour
{
    public static CollectPanel Instance{get; private set;}

    public GameObject OutGameObject;
    [Header("Buttons")] public Button btn1;

    public Button btn2;

    public Button btn3;
    public Button btn4;
    
    public GameObject collectPanel;

    public Image displayImage;
    public Button closeBtn;
    // Start is called before the first frame update
    void Start()
    {
        // 绑定按钮点击事件
        btn1.onClick.AddListener(() => ShowPanel("1"));
        btn2.onClick.AddListener(() => ShowPanel("2"));
        btn3.onClick.AddListener(() => ShowPanel("3"));
        btn4.onClick.AddListener(() => ShowPanel("4"));
        closeBtn.onClick.AddListener(()=>ClosePanel());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShowPanel(String targetPanel){
        collectPanel.SetActive(true);
        // 显示目标面板
        switch (targetPanel) {
            case "1":
                displayImage.sprite=btn1.transform.GetComponent<Image>().sprite;
                // 可选：保持原始宽高比例
                displayImage.preserveAspect = true;
                PuzzleManager.Instance.ResetPuzzle();
                break;
            case "2":
                displayImage.sprite = btn2.transform.GetComponent<Image>().sprite;
                // 可选：保持原始宽高比例
                displayImage.preserveAspect = true;
                PuzzleManager.Instance.ResetPuzzle();
                break;
            case "3":
                displayImage.sprite = btn3.transform.GetComponent<Image>().sprite;
                // 可选：保持原始宽高比例
                displayImage.preserveAspect = true;
                PuzzleManager.Instance.ResetPuzzle();
                break;
            case "4":
                displayImage.sprite = btn4.transform.GetComponent<Image>().sprite;
                // 可选：保持原始宽高比例
                displayImage.preserveAspect = true;
                PuzzleManager.Instance.ResetPuzzle();
                break;
        }
    }

    private void ClosePanel(){
        

        // 清空当前拼图
        var initializer = FindObjectOfType<PuzzleInitializer>();
        if (initializer != null) {
            initializer.DeleteAllPieces();
        }
        ClearChildren(OutGameObject.transform);
        PuzzleManager.Instance.ResetPuzzle();
        collectPanel.SetActive(false);
    }

    void ClearChildren(Transform parent){
        foreach (Transform child in parent) {
            GameObject.Destroy(child.gameObject);
        }
    }
}
