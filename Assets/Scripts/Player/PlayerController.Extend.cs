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
/// ��Ҳ���������
/// </summary>
public partial class PlayerController : MonoBehaviour
{
    public int GroundMask;

    int moveX;

    private float jumpResponseTimer;
    private float jumpGraceTimer;                //����ʱ�������
    private float jumpCooldownTimer;                //��Ծ��ȴʱ�������

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
        //���ݽ���ķ�ʽ,������ʼ״̬
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

    //������Ծ,��Ծʱ�򣬻����Ծǰ��һ��������ٶ�
    public void Jump()
    {
        GameInput.Jump.OnTrigger();

        jumpCooldownTimer = Constants.JumpCooldown;
        jumpGraceTimer = 0f;              //��������ʱ��
        jumpResponseTimer = Constants.JumpResponseTime;

        this.Speed = new Vector2(Speed.x + Constants.JumpXBoost * moveX, Constants.JumpSpeed);

        //���õĲ���
        animator.SetTrigger("Jump");
        AudioManager.Instance.Play(SFXClip.Jump);
    }

    public void MovePosition(Vector2 targetPosition)
    {
        //TODO : ������Ҫ������ײ���
        Position = targetPosition;
    }
}

public static class Constants
{
    public static float Gravity = 60f; //����

    public static float HalfGravThreshold = 4f; //�Ϳ�ʱ����ֵ����ֱ�ٶȾ���ֵС�ڴ���ʱ��������
    public static float MaxFall = -16f; //��ͨ��������ٶ�

    public static float MaxRun = 7f;//����ƶ��ٶ�
    //���������������
    public static float AirMult = 0.65f;
    //�ƶ����ٶ�
    public static float RunAccel = 100f;
    //��ͼ�ֿ�����ʱ�ļ��ٶ�
    public static float RunReduce = 40f;

    //��Ծ��ز���
    public static float JumpSpeed = 11.6f;  //�����Ծ�ٶ�
    public static float JumpResponseTime = 0.2f; //��Ծ��Ӧʱ��(����ʱ,�������Ӧ��Ծ����[JumpResponseTime]��,Ӱ����Ծ����߸߶�);
    public static float JumpXBoost = 2f; //�����������
    public static float JumpGraceTime = 0.12f;//����ʱ��
    public static float JumpCooldown = .15f;//��Ծ��ȴʱ��
    public static float JumpPreInputTime = .08f;//��ԾԤ����ʱ��
    public static float JumpMinEffectiveTime = .13f;//��Ծ��̳���ʱ��

    #region Corner Correct
    public static int UpwardCornerCorrection = 3; //�����ƶ���X���ϱ�ԵУ����������
    #endregion
}