using System;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public partial class PlayerController
{
    const float STEP = 0.1f;  //��ײ��ⲽ������POINT�����
    const float DEVIATION = 0.02f;  //��ײ������

    private readonly Rect normalHitbox = new Rect(0, -0.1f, 0.6f, 1.4f);
    private readonly Rect normalHurtbox = new Rect(0f, 0f, 0.55f, 1.2f);

    private new Rect collider;//�˶�ѧ��ײ����position��������ײ������
    private Rect hurtCollider;//������ײ

    public void AdjustPosition(Vector2 adjust)
    {
        UpdateCollideX(adjust.x);
        UpdateCollideY(adjust.y);
        transform.position = Position;
    }

    //��ײ���
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

    //������ײ����X���ϵ������ƶ�����
    protected void UpdateCollideX(float distX)
    {
        if (distX == 0)
        {
            return;
        }
        Vector2 targetPosition = this.Position;
        //ʹ��У��
        float distance = distX;
        int correctTimes = 1;
        while (true)
        {
            float moved = MoveXStepWithCollide(distance);
            //����ײ�˳�ѭ��
            this.Position += Vector2.right * moved;
            if (moved == distance || correctTimes == 0) //����ײ����У������Ϊ0
            {
                break;
            }
            float tempDist = distance - moved;
            correctTimes--;
            if (!CorrectX(tempDist))
            {
                this.Speed = new Vector2(0, Speed.y);//δ���У�������ٶ�����
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
        //ʹ��У��
        float distance = distY;
        int correctTimes = 1; //Ĭ�Ͽ��Ե���λ��10��
        bool collided = true;
        float speedY = Mathf.Abs(this.Speed.y);
        while (true)
        {
            float moved = MoveYStepWithCollide(distance);
            //����ײ�˳�ѭ��
            this.Position += Vector2.up * moved;
            if (moved == distance || correctTimes == 0) //����ײ����У������Ϊ0
            {
                collided = false;
                break;
            }
            float tempDist = distance - moved;
            correctTimes--;
            if (!CorrectY(tempDist))
            {
                this.Speed = new Vector2(Speed.x, 0);//δ���У�������ٶ�����
                break;
            }
            distance = tempDist;
        }

        //���Ч��
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

    //�����ƶ��������ͷ���ֵ�������򣬱�ʾY��
    private float MoveYStepWithCollide(float distY)
    {
        Vector2 moved = Vector2.zero;
        Vector2 direct = Math.Sign(distY) > 0 ? Vector2.up : Vector2.down;
        Vector2 origin = this.Position + collider.position;
        RaycastHit2D hit = Physics2D.BoxCast(origin, collider.size, 0, direct, Mathf.Abs(distY) + DEVIATION, GroundMask);
        if (hit)
        //if (hit && (hit.normal + direct).magnitude < 0.05f)
        {
            //���������ײ,���ƶ�����
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
            //���������ײ,���ƶ�����
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
            //Y�����Ϸ����Corner Correction
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
    #region ��������
    /// <summary>
    /// ����ض����������Ƿ��������ײ
    /// </summary>
    /// <param name="worldRectPos">��������λ�ã��������꣩</param>
    /// <param name="rectSize">���������С</param>
    /// <returns>�Ƿ���ײ</returns>
    public bool IsCollidingWithRect(Vector2 worldRectPos, Vector2 rectSize)
    {
        // ��ȡ�����ײ������������ϵ�е�λ��
        Vector2 playerPos = this.Position;
        Rect playerRect = new Rect(
            playerPos.x + collider.x,
            playerPos.y + collider.y,
            collider.width,
            collider.height
        );
        
        // ����������
        Rect checkRect = new Rect(
            worldRectPos.x - rectSize.x / 2,
            worldRectPos.y - rectSize.y / 2,
            rectSize.x,
            rectSize.y
        );
        
        // ����ص�
        return playerRect.Overlaps(checkRect);
    }
    #endregion
}
    