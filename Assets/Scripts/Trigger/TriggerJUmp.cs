using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerJUmp : MonoBehaviour
{
    public Image JumpImage;
    public Sprite JumpSprite;
    private bool istriggered;

    [SerializeField] private string JumpText="跳跃";

    

    private void OnTriggerEnter2D(Collider2D other){
        if (other.transform.GetComponent<PlayerController>()!= null&&!(PlayerPrefs.GetInt("isTriggered")==1)) {
            print("Player entered");
            if (JumpImage != null) {
                if (!JumpImage.transform.gameObject.activeInHierarchy) {
                    JumpImage.transform.gameObject.SetActive(true);
                }
                JumpImage.sprite = JumpSprite;
                PlayerPrefs.SetInt("isTriggered", 1);
                istriggered = true;
                StartCoroutine(Delay());
            }
        }
    }

    

    private IEnumerator Delay(){
        yield return new WaitForSecondsRealtime(5f);
        JumpImage.transform.gameObject.SetActive(false);
    }
}
