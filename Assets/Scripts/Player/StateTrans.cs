using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTrans : StateMachineBehaviour
{
    public string musicName;

    public string animationName;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        //如果当前状态是animationName则执行什么样的逻辑
        if (stateInfo.IsName(animationName))
            Debug.Log("进入Blend Tree状态了");
        // animator.gameObject.transform.localScale = Vector3.one;//改变动作对象的所有信息
        Debug.Log("进入动画状态: " + stateInfo.fullPathHash);

        if (stateInfo.IsTag("Idle")) // 适配 Blend Tree 里的 Idle
        {
            animator.SetBool("isGrounded", true);
            Debug.Log("进入 Idle 状态，设置 isGrounded = true");
        }
        else if (stateInfo.IsTag("Jump") || stateInfo.IsTag("Fall")) // 检测 Blend Tree 内跳跃/下落动画
        {
            animator.SetBool("isGrounded", false);
            Debug.Log("进入 Jump/Fall 状态，设置 isGrounded = false");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        if (stateInfo.IsTag("Jump") || stateInfo.IsTag("Fall")) {
            Debug.Log("退出 Jump/Fall 状态");
        }
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        base.OnStateIK(animator, stateInfo, layerIndex);
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        Debug.Log("正在移动");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
        if (stateInfo.IsName("Blend Tree"))
            Debug.Log("处于Blend Tree状态了");

        if (stateInfo.IsTag("Jump") || stateInfo.IsTag("Fall")) {
            animator.SetBool("isGrounded", false);
            Debug.Log("正在跳跃/下落");
        }
    }

//子状态进入时
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash){
        base.OnStateMachineEnter(animator, stateMachinePathHash);
        
    }

    //子状态退出时
    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash){
        base.OnStateMachineEnter(animator, stateMachinePathHash);
    }
}