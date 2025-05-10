using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class StoryEnd : MonoBehaviour
{
    public List<StoryTrigger> storys= new List<StoryTrigger>();

    [CanBeNull] public GameObject endingUIPanel;
    public void EndEffectTriggerNext(){
        Debug.Log("[StoryEnd] EndEffectTriggerNext called, storys[0]=" + storys[0]);
    if (storys[0] == null)
    {
        Debug.Log("[StoryEnd] storys[0] is null!");
        return;
    }
        //结束特效
        //...
        //开始下一段
        // storys[0].ForceStartStory();
    }

    public void ChangeAni(){
        FindObjectOfType<CompanionController>().transform.GetComponent<Animator>().Play("robot2");
        // storys[1].ForceStartStory();
    }

    public void FinishThirdPart(){
        FindObjectOfType<PlayerController>().EnableMovement();
    }

    public void FinallyEnd(){
        CameraControl camControl = Camera.main.transform.GetComponent<CameraControl>();
        CompanionController companionController = FindObjectOfType<CompanionController>();
        //不跟随玩家
        companionController.canFollow=false;
        
        //摄像头跟随伙伴
        //camControl.target = companionController.transform;
        camControl.isTransitioning = true; // 开启平滑过渡
        camControl.smoothSpeed = 5f; // 设置平滑速度
        camControl.IgnoreHorizontalLimit();
        if(endingUIPanel != null){
            companionController.StartMoveRightForSeconds(2f,5f, endingUIPanel);
        }
    }
}
