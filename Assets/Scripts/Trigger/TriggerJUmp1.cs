using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TriggerJUmp1 : MonoBehaviour
{
    public TMP_Text JumpImage;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.transform.GetComponent<PlayerController>()!= null) {
            print("Player entered");
            if (JumpImage != null) {
                StartCoroutine(StopJumpPanel());

            }
        }
    }

    private IEnumerator StopJumpPanel(){
        yield return new WaitForSecondsRealtime(1f);
        JumpImage.transform.parent.gameObject.SetActive(false);
    }
}
