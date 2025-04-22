using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerJUmp : MonoBehaviour
{
    public TMP_Text JumpImage;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.tag == "Player") {
            if (JumpImage != null) {
                JumpImage.text = "跳跃";
            }
        }
    }
}
