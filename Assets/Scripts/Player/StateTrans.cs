using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTrans : StateMachineBehaviour
{
    public string musicName;

    public string animationName;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName(animationName))
        {
            Debug.Log($"进入 {animationName} 状态");
        }

        if (stateInfo.IsName("Idle")) // 角色在地面
        {
            animator.SetBool("isGrounded", true);
            Debug.Log("进入 Idle 状态，设置 isGrounded = true");
        }
        else if (stateInfo.IsName("Jump") || stateInfo.IsName("Fall")) // 进入跳跃或下落状态
        {
            animator.SetBool("isGrounded", false);
            Debug.Log($"进入 {stateInfo.shortNameHash} 状态，设置 isGrounded = false");
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Jump") || stateInfo.IsName("Fall"))
        {
            Debug.Log($"退出 {stateInfo.shortNameHash} 状态");
        }
    }

    public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateIK(animator, stateInfo, layerIndex);
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Debug.Log("正在移动");
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (stateInfo.IsName("Jump") || stateInfo.IsName("Fall"))
        {
            animator.SetBool("isGrounded", false);
        }
    }

    //子状态进入时
    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        Debug.Log("进入状态机");
    }

    //子状态退出时
    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        Debug.Log("退出状态机");
    }
}