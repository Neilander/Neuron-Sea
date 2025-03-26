using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_move : MonoBehaviour
{
    // public Color lightColor = new Color(1, 1, 0.5f, 0.3f);
    public int moveSpeed=10;
    private Animator ani;
    private Rigidbody2D rb;
    public float jumpForce=5f;
    public bool isGrounded;
    private void Start(){
        ani = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }
//test git
    public void Update(){
        // GetComponent<SpriteRenderer>().color = lightColor;
        print(Input.GetAxis("Horizontal"));
        transform.Translate(Vector2.right * Input.GetAxis("Horizontal") *moveSpeed* Time.deltaTime);
        PlayerRotate();
        ani.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));
        PlayerJump();
    }

    private void PlayerJump(){
        if (Input.GetKeyDown(KeyCode.Space)) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isGrounded = false;
            // ani.SetTrigger("Jump");
        }
    }

    private void PlayerRotate(){
        if (Input.GetAxis("Horizontal") < 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        
        else if (Input.GetAxis("Horizontal") > 0) {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
    
}
