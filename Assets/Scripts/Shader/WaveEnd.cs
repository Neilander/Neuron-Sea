using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveEnd : MonoBehaviour
{
    public WaveMunController wave;

    private void OnTriggerEnter(Collider other){
        if (other.CompareTag("Player")) {
            wave.StartDisappearAnimation();
        }
    }
}
