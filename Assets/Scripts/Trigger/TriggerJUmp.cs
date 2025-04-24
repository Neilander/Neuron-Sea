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

    [SerializeField] private string JumpText="跳跃";

    private void OnTriggerEnter2D(Collider2D other){
        if (other.transform.GetComponent<PlayerController>()!= null&&!istriggered) {
            print("Player entered");
            if (JumpImage != null) {
                if (!JumpImage.transform.parent.gameObject.activeInHierarchy) {
                    JumpImage.transform.parent.gameObject.SetActive(true);
                }
                JumpImage.text = JumpText;
                istriggered = true;
                StartCoroutine(Delay());
            }
        }
    }

    

    private IEnumerator Delay(){
        yield return new WaitForSecondsRealtime(5f);
        JumpImage.transform.parent.gameObject.SetActive(false);
    }
}
