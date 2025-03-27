using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateTrans : StateMachineBehaviour
{
   public string musicName;
   public string animationName;
   public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
      //如果当前状态是animationName则执行什么样的逻辑
      if(stateInfo.IsName(animationName))
         Debug.Log("进入Blend Tree状态了");
      animator.gameObject.transform.localScale = Vector3.one;//改变动作对象的所有信息
   }

   public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex){
      if (stateInfo.IsName("Blend Tree"))
         Debug.Log("退出Blend Tree状态了");
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
