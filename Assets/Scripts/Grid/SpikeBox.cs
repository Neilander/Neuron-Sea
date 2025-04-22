using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeBox : MonoBehaviour
{
    /*
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log("玩家触碰了尖刺");
            PlayerDeathEvent.Trigger(gameObject, DeathType.Spike);
        }
    }*/

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            Debug.Log("玩家触碰了尖刺");
            PlayerDeathEvent.Trigger(gameObject, DeathType.Spike);
        }
    }
}
