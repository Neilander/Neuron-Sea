using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTrigger : MonoBehaviour
{
    public WaveMunController waveMunController;
    private void OnCollitionEnter2D(Collider2D other){
        if (other.CompareTag("Player")) {
            print("触发");
            waveMunController.StartDisappearAnimation();
        }
    }
}
