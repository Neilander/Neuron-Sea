using System;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public partial class PlayerController
{
    const float STEP = 0.1f;  //碰撞检测步长，对POINT检测用
    const float DEVIATION = 0.02f;  //碰撞检测误差

    private readonly Rect normalHitbox = new Rect(0, -0.1f, 0.6f, 1.4f);
    private readonly Rect normalHurtbox = new Rect(0f, 0f, 0.55f, 1.2f);

    private new Rect collider;
    private Rect hurtCollider;

    public void AdjustPosition(Vector2 adjust)
    {
        UpdateCollideX(adjust.x);
        UpdateCollideY(adjust.y);
        transform.position = Position;
    }

    //碰撞检测
    public RaycastHit2D CollideCheck(Vector2 position, Vector2 dir, float dist = 0)
    {
        Vector2 origin = position + collider.position;
        return Physics2D.BoxCast(origin, collider.size, 0, dir, dist + DEVIATION, GroundMask);
    }

    //根据碰撞调整X轴上的最终移动距离
    protected void UpdateCollideX(float distX)
    {
        if (distX == 0)
        {
            return;
        }
        Vector2 targetPosition = this.Position;
        //使用校正
        float distance = distX;
        int correctTimes = 1;
        while (true)
        {
            float moved = MoveXStepWithCollide(distance);
            //无碰撞退出循环
            this.Position += Vector2.right * moved;
            if (moved == distance || correctTimes == 0) //无碰撞，且校正次数为0
            {
                break;
            }
            float tempDist = distance - moved;
            correctTimes--;
            if (!CorrectX(tempDist))
            {
                this.Speed = new Vector2(0, Speed.y);//未完成校正，则速度清零
                break;
            }
            distance = tempDist;
        }
    }

    protected void UpdateCollideY(float distY)
    {
        if (distY == 0)
        {
            return;
        }
        Vector2 targetPosition = this.Position;
        //使用校正
        float distance = distY;
        int correctTimes = 1; //默认可以迭代位置10次
        bool collided = true;
        float speedY = Mathf.Abs(this.Speed.y);
        while (true)
        {
            float moved = MoveYStepWithCollide(distance);
            //无碰撞退出循环
            this.Position += Vector2.up * moved;
            if (moved == distance || correctTimes == 0) //无碰撞，且校正次数为0
            {
                collided = false;
                break;
            }
            float tempDist = distance - moved;
            correctTimes--;
            if (!CorrectY(tempDist))
            {
                this.Speed = new Vector2(Speed.x, 0);//未完成校正，则速度清零
                break;
            }
            distance = tempDist;
        }

        //落地效果
        if (collided && distY < 0 && speedY > 2f)
        {
            //this.PlayLandEffect(this.SpritePosition, speedY);
        }
    }

    private bool CheckGround()
    {
        return CheckGround(Vector2.zero);
    }

    //针对横向,进行碰撞检测.如果发生碰撞,
    private bool CheckGround(Vector2 offset)
    {
        Vector2 origin = this.Position + collider.position + offset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, collider.size, 0, Vector2.down, DEVIATION, GroundMask);
        if (hit && (hit.normal - Vector2.up).magnitude <= 0.05f)
        {
            return true;
        }
        return false;
    }

    //单步移动，参数和返回值都带方向，表示Y轴
    private float MoveYStepWithCollide(float distY)
    {
        Vector2 moved = Vector2.zero;
        Vector2 direct = Math.Sign(distY) > 0 ? Vector2.up : Vector2.down;
        Vector2 origin = this.Position + collider.position;
        RaycastHit2D hit = Physics2D.BoxCast(origin, collider.size, 0, direct, Mathf.Abs(distY) + DEVIATION, GroundMask);
        if (hit)
        //if (hit && (hit.normal + direct).magnitude < 0.05f)
        {
            //如果发生碰撞,则移动距离
            moved += direct * Mathf.Max((hit.distance - DEVIATION), 0);
        }
        else
        {
            moved += Vector2.up * distY;
        }
        return moved.y;
    }

    private float MoveXStepWithCollide(float distX)
    {
        Vector2 moved = Vector2.zero;
        Vector2 direct = Math.Sign(distX) > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = this.Position + collider.position;
        RaycastHit2D hit = Physics2D.BoxCast(origin, collider.size, 0, direct, Mathf.Abs(distX) + DEVIATION, GroundMask);
        if (hit)
        //if (hit && (hit.normal + direct).magnitude < 0.05f)
        {
            //如果发生碰撞,则移动距离
            moved += direct * Mathf.Max((hit.distance - DEVIATION), 0);
        }
        else
        {
            moved += Vector2.right * distX;
        }
        return moved.x;
    }

    private bool CorrectX(float distX)
    {
        Vector2 origin = this.Position + collider.position;
        Vector2 direct = Math.Sign(distX) > 0 ? Vector2.right : Vector2.left;
        return false;
    }

    private bool CorrectY(float distY)
    {
        Vector2 origin = this.Position + collider.position;
        Vector2 direct = Math.Sign(distY) > 0 ? Vector2.up : Vector2.down;

        if (this.Speed.y > 0)
        {
            //Y轴向上方向的Corner Correction
            {
                if (this.Speed.x <= 0)
                {
                    for (int i = 1; i <= Constants.UpwardCornerCorrection * 5; i++)
                    {
                        RaycastHit2D hit = Physics2D.BoxCast(origin + new Vector2(-i * 0.02f, 0), collider.size, 0, direct, Mathf.Abs(distY) + DEVIATION, GroundMask);
                        if (!hit)
                        {
                            this.Position += new Vector2(-i * 0.02f, 0);
                            return true;
                        }
                    }
                }

                if (this.Speed.x >= 0)
                {
                    for (int i = 1; i <= Constants.UpwardCornerCorrection * 5; i++)
                    {
                        RaycastHit2D hit = Physics2D.BoxCast(origin + new Vector2(i * 0.02f, 0), collider.size, 0, direct, Mathf.Abs(distY) + DEVIATION, GroundMask);
                        if (!hit)
                        {
                            this.Position += new Vector2(i * 0.02f, 0);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
}
    