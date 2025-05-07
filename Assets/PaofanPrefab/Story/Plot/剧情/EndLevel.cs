using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndLevel : MonoBehaviour
{
    public string sceneName="场景2剧情";
    private void OnTriggerEnter2D(Collider other){
        if (!other.CompareTag("Player")) return;
        CollectableManager.Instance.ResetLevelData();
        SceneManager.LoadScene(sceneName);
    }
}
