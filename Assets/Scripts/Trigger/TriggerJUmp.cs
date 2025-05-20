using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerJUmp : MonoBehaviour
{
    public Image JumpImage;

    private bool istriggered;
    private bool enable;

    [SerializeField] private Sprite sprite2;

    void Start()
    {
        enable = PlayerPrefs.GetInt("SGLM_Scene1Loaded", 0) == 0;
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.transform.GetComponent<PlayerController>()!= null&&!istriggered) {
            print("Player entered");
            if (JumpImage != null&& enable) {
                if (!JumpImage.transform.gameObject.activeInHierarchy) {
                    JumpImage.transform.gameObject.SetActive(true);
                }
                JumpImage.sprite = sprite2;
                istriggered = true;
                StartCoroutine(WaitForSpaceAndDelay());
            }
        }
    }

    private IEnumerator WaitForSpaceAndDelay(){
        // 等待玩家按下空格键
        while(!GameInput.Jump.Pressed()){
            yield return null;
        }
        // 等待1秒
        yield return new WaitForSecondsRealtime(1f);
        if (JumpImage.sprite == sprite2) {
            JumpImage.transform.gameObject.SetActive(false);
        }
    }
}
