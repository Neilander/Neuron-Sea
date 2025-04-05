using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallMovement : MonoBehaviour
{
    public Transform wallCheckRight; // 右侧墙壁检测点

    public bool isRightWall;

    public LayerMask wallLayer;

    public float wallCheckRadius = 0.1f; // 检测半径

    private bool isWallTouching;

    public enum WallState
    {
        WallClimb,

        WallJump,

        WallGrab,

        none,
    }

    Rigidbody2D rb;

    WallState ws;

    // Start is called before the first frame update
    void Start(){
        ws = WallState.none;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame 
    void Update(){
        WallCheck();
        if (isRightWall) {
            isWallTouching = true;
        }
        else {
            isWallTouching = false;
        }

        ws = WallState.none;
        rb.gravityScale = 5f;
    }

    void WallCheck(){
        // 检测右侧墙壁
        //isRightWall = Physics2D.OverlapCircle(wallCheckRight.position, wallCheckRadius, wallLayer);

        // 调试输出
        if (isRightWall) {
            Debug.Log("检测到右侧墙壁");
        }
    }

    // void OnDrawGizmos(){
    //     Gizmos.color = Color.red;
    //     // 绘制右侧检测范围
    //     if(wallCheckRight != null){
    //         Gizmos.DrawWireSphere(wallCheckRight.position, wallCheckRadius);
    //     }
    // }
    void WallGrab(){
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(0, 0);
        ws = WallState.WallGrab;
    }
}