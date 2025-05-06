using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;

    [SerializeField] private TMPro.TextMeshProUGUI dialogueText;

    [SerializeField] private Vector3 worldOffset = new Vector3(0, 1.5f, 0); // 偏移量：在头部上方 1.5 单位

    [SerializeField] private Canvas canvas; // 需要你的 UI Canvas，用于计算位置（推荐使用 Screen Space - Camera 模式）

    [SerializeField] private Camera mainCamera; // 主摄像机

    private void Awake(){
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update(){
        if (dialoguePanel != null && dialoguePanel.activeSelf) {
            Vector3 worldPosition = transform.position + worldOffset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            dialoguePanel.transform.position = screenPosition;
        }
    }

    public void ShowDialogue(string message){
        if (dialoguePanel != null && dialogueText != null) {
            dialoguePanel.SetActive(true);
            dialogueText.text = message;
            StartCoroutine(DelayShow());
        }
    }

    IEnumerator DelayShow(){
        yield return new WaitForSeconds(6f);
        HideDialogue();
    }
    public void HideDialogue(){
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}