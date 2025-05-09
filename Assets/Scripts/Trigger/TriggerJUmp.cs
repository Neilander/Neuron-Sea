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

    [SerializeField] private Sprite sprite2;

    private void OnTriggerEnter2D(Collider2D other){
        if (other.transform.GetComponent<PlayerController>()!= null&&!istriggered) {
            print("Player entered");
            if (JumpImage != null) {
                if (!JumpImage.transform.gameObject.activeInHierarchy) {
                    JumpImage.transform.gameObject.SetActive(true);
                }
                JumpImage.sprite = sprite2;
                istriggered = true;
                StartCoroutine(Delay());
            }
        }
    }

    

    private IEnumerator Delay(){
        yield return new WaitForSecondsRealtime(5f);
        if (JumpImage.sprite == sprite2) {
            JumpImage.transform.gameObject.SetActive(false);
        }
    }
}
