using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeReporter : MonoBehaviour
{
    public ExplosiveBox box;
    private float stayTimer = 0f;
    public float waitTime = 0f;
    private bool playerInside = false;

    private void Update()
    {
        if (playerInside)
        {
            stayTimer += Time.deltaTime;

            if (stayTimer >= waitTime)
            {
                box.StartExplode();
                stayTimer = 0;
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
