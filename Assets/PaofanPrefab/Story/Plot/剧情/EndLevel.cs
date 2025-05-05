using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
    private void OnTriggerExit(Collider other){
        if (!other.CompareTag("Player")) return;
        SceneManager.LoadScene("场景2剧情");
    }
}
