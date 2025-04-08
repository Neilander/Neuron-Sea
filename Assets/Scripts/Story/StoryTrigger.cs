using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other){
        if (other.gameObject.tag == "Player") {
            print("碰到了！！");
            // 获取剧情数据资源
            StoryData storyData = Resources.Load<StoryData>("StoryData/IntroStory");

            // 进入剧情模式
            StoryManager.Instance.EnterStoryMode(storyData);
                
        }
    }
}
