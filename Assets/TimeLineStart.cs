using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimeLineStart : MonoBehaviour
{
    public PlayableDirector director;
    private string triggerId;

    private void Start()
    {
        // 生成唯一ID用于记录此时间线是否已播放
        triggerId = $"Timeline_{gameObject.name}_{transform.position}";
    }

    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")) {
            // 检查是否已播放过此时间线
            if (StoryGlobalLoadManager.instance.IsTriggerDisabled(triggerId)) {
                return; // 如果已播放过，直接返回
            }
            other.GetComponent<PlayerController>().DisableInput();
            director.Play();
            GetComponent<Collider2D>().enabled = false;
            
            // 标记此时间线已播放
            StoryGlobalLoadManager.instance.DisableTrigger(triggerId);
        }
    }
}
