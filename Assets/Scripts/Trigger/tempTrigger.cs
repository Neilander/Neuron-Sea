using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        StoryData storyData = Resources.Load<StoryData>("StoryData/IntroStory");

        // 进入剧情模式
        StoryManager.Instance.EnterStoryMode(storyData);
    }
}
