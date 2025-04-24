using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionReporter : MonoBehaviour
{
    public touchmoveBox box;
    private float stayTimer = 0f;
    private bool playerInside = false;

    private void Update()
    {
        if (playerInside)
        {
            stayTimer += Time.deltaTime;

            if (stayTimer >= 0.2f)
            {
                // 只有触发成功才清零
                if (box.TriggerMove())
                {
                    stayTimer = 0f;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            playerInside = true;
            stayTimer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerController>() != null)
        {
            playerInside = false;
            stayTimer = 0f;
        }
    }
}
