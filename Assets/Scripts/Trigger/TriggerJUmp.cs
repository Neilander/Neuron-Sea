using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerJUmp : MonoBehaviour
{
    public TMP_Text JumpImage;

    private bool istriggered;
    private void OnTriggerEnter2D(Collider2D other){
        if (other.transform.GetComponent<PlayerController>()!= null&&!istriggered) {
            print("Player entered");
            if (JumpImage != null) {
                JumpImage.transform.parent.gameObject.SetActive(true);
                JumpImage.text = "跳跃";
                istriggered = true;
            }
        }
    }
}
