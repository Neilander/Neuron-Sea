using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionReporter : MonoBehaviour
{
    public touchmoveBox box;
    private float stayTimer = 0f;
    private bool playerInside = false;

    private bool canBeTrigger = true;

    /*
    private void Update()
    {
        if (playerTrigger)
        {
            stayTimer += Time.deltaTime;

            if (stayTimer >= 0.2f)
            {
                // 只有触发成功才清零
                if (box.TriggerMove())
                {
                    playerTrigger = false;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null&& canBeTrigger)
        {
            canBeTrigger = false;
            playerTrigger = true;
            stayTimer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            stayTimer = 0f;
            canBeTrigger = true;
        }
    }*/

    private void OnTriggerStay2D(Collider2D collision)
    {
        box.TriggerMove();
    }
}
