using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class EndLevel : MonoBehaviour
{
    public string sceneName="场景2剧情";

    public Image fadeImage;
    
    public float fadeDuration = 1f;

    private bool isFading = false;
    private void OnTriggerEnter2D(Collider2D other){
        if (!other.CompareTag("Player")) return;
        other.GetComponent<PlayerController>().CheckEdge = true;
        isFading = true;
        StartCoroutine(FadeAndLoadScene());
        
    }

    private IEnumerator FadeAndLoadScene(){
        float timer = 0f;
        Color color = fadeImage.color;

        // 从透明变为黑色
        while (timer < fadeDuration) {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        // 确保完全黑了之后再切换场景
        CollectableManager.Instance.ResetLevelData();
        SceneManager.LoadScene(sceneName);
    }
}
