using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTrigger : MonoBehaviour
{
    public WaveMunController waveMunController;
    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player") && waveMunController != null) {
            print("触发");
            waveMunController.StartDisappearAnimation();
        }
    }
}
