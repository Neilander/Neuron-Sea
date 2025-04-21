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

    private new Rect collider;//运动学碰撞，其position属性是碰撞体中心
    private Rect hurtCollider;//受伤碰撞

    public void AdjustPosition(Vector2 adjust)
    {
        UpdateCollideX(adjust.x);
        UpdateCollideY(adjust.y);
        transform.position = Position;
    }

    //碰撞检测
    public RaycastHit2D CollideCheck(int layerMask, Vector2 dir = default, float dist = 0)
    {
        return CollideCheck(Position, layerMask, dir, dist);
    }
    public RaycastHit2D CollideCheck(Vector2 position, int layerMask, Vector2 dir = default, float dist = 0)
    {
        Vector2 origin = position + collider.position;
        return Physics2D.BoxCast(origin, collider.size, 0, dir, dist + DEVIATION, layerMask);
    }
    public bool CollideCheck(Rect rect)
    {
        Vector2 origin = Position + collider.position;
        return rect.Overlaps(new(origin - collider.size * 0.5f, collider.size));
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

    private bool CheckGround(Vector2 offset)
    {
        Vector2 origin = this.Position + collider.position + offset;
        RaycastHit2D hit = Physics2D.BoxCast(origin, collider.size, 0, Vector2.down, DEVIATION, GroundMask);
        if (hit)
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
    #region 新增方法
    /// <summary>
    /// 检查特定矩形区域是否与玩家碰撞
    /// </summary>
    /// <param name="worldRectPos">矩形区域位置（世界坐标）</param>
    /// <param name="rectSize">矩形区域大小</param>
    /// <returns>是否碰撞</returns>
    public bool IsCollidingWithRect(Vector2 worldRectPos, Vector2 rectSize)
    {
        // 获取玩家碰撞盒在世界坐标系中的位置
        Vector2 playerPos = this.Position;
        Rect playerRect = new Rect(
            playerPos.x + collider.x,
            playerPos.y + collider.y,
            collider.width,
            collider.height
        );
        
        // 创建检测矩形
        Rect checkRect = new Rect(
            worldRectPos.x - rectSize.x / 2,
            worldRectPos.y - rectSize.y / 2,
            rectSize.x,
            rectSize.y
        );
        
        // 检查重叠
        return playerRect.Overlaps(checkRect);
    }
    #endregion
}
    