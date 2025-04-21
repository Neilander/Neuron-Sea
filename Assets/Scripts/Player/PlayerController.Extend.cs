using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Events;
using Unity.VisualScripting;
using UnityEngine.EventSystems;

public enum LevelEnterWay
{
    Checkpoint,
    FromLeft,
    FromRight,
    FromUp,
    SmashFromUp,
    FromDownToLeft,
    FromDownToRight,
}

/// <summary>
/// 玩家操作控制器
/// </summary>
public partial class PlayerController : MonoBehaviour
{
    public int GroundMask;

    int moveX;

    private float jumpResponseTimer;
    private float jumpGraceTimer;                //土狼时间计数器
    private float jumpCooldownTimer;                //跳跃冷却时间计数器

    private bool onGround;

    public float level_enter_timer;
    private LevelEnterWay level_enter_way;

    private Vector2 speed;
    public Vector2 Speed
    {
        get { return speed; }
        set { speed = value; }
    }


    public bool OnGround => this.onGround;
    public Vector2 Position;


    public int MoveX => moveX;

    public void Init(LevelEnterWay level_enter_way = LevelEnterWay.FromLeft)
    {
        this.GroundMask = LayerMask.GetMask("Ground");

        //this.collider = normalHitbox;
        //this.hurtCollider = normalHurtbox;
        collider = new Rect(boxCollider.offset, boxCollider.size);
        hurtCollider = collider;

        this.level_enter_way = level_enter_way;
        //根据进入的方式,决定初始状态
        if (level_enter_way == LevelEnterWay.FromRight || level_enter_way == LevelEnterWay.FromDownToLeft)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        if (level_enter_way == LevelEnterWay.FromRight || level_enter_way == LevelEnterWay.FromLeft || level_enter_way == LevelEnterWay.FromUp)
        {
            level_enter_timer = 0.4f;
        }
        else if (level_enter_way == LevelEnterWay.FromUp)
        {
            level_enter_timer = 0.4f;
        }
    }

    public bool AllowJump()
    {
        return jumpGraceTimer > 0 && jumpCooldownTimer <= 0;
    }

    //处理跳跃,跳跃时候，会给跳跃前方一个额外的速度
    public void Jump()
    {
        GameInput.Jump.OnTrigger();

        jumpCooldownTimer = Constants.JumpCooldown;
        jumpGraceTimer = 0f;              //重置土狼时间
        jumpResponseTimer = Constants.JumpResponseTime;

        this.Speed = new Vector2(Speed.x + Constants.JumpXBoost * moveX, Constants.JumpSpeed);

        //沿用的部分
        animator.SetTrigger("Jump");
        AudioManager.Instance.Play(SFXClip.Jump);
    }

    public void MovePosition(Vector2 targetPosition)
    {
        //TODO : 这里需要考虑碰撞检测
        Position = targetPosition;
    }
}

public static class Constants
{
    public static float Gravity = 60f; //重力

    public static float HalfGravThreshold = 4f; //滞空时间阈值，竖直速度绝对值小于此数时重力减半
    public static float MaxFall = -16f; //普通最大下落速度

    public static float MaxRun = 7f;//最大移动速度
    //横向空气阻力倍率
    public static float AirMult = 0.65f;
    //移动加速度
    public static float RunAccel = 100f;
    //试图抵抗减速时的加速度
    public static float RunReduce = 40f;

    //跳跃相关参数
    public static float JumpSpeed = 11.6f;  //最大跳跃速度
    public static float JumpResponseTime = 0.2f; //跳跃响应时间(跳起时,会持续响应跳跃按键[JumpResponseTime]秒,影响跳跃的最高高度);
    public static float JumpXBoost = 2f; //起跳横向加速
    public static float JumpGraceTime = 0.12f;//土狼时间
    public static float JumpCooldown = .15f;//跳跃冷却时间
    public static float JumpPreInputTime = .08f;//跳跃预输入时间
    public static float JumpMinEffectiveTime = .13f;//跳跃最短持续时间

    #region Corner Correct
    public static int UpwardCornerCorrection = 3; //向上移动，X轴上边缘校正的最大距离
    #endregion
}