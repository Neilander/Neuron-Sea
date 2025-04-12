using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class preloader : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 尝试从父物体获取预加载脚本
        var loader = GetComponentInParent<levelTrigger>();
        if (loader != null)
        {
            loader.SendMessage("PreloadNextScene", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogWarning("父物体未挂载 LevelTriggerLoader，无法预加载");
        }
    }
}
