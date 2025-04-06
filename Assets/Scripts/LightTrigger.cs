using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrigger : MonoBehaviour
{
    [SerializeField] private LightController lightController; // 灯光控制器引用

    private void Start()
    {
        Debug.Log("LightTrigger Start");
        if (lightController == null)
        {
            Debug.LogError("LightController引用未设置！");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"LightTrigger OnTriggerEnter2D: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        if (other.CompareTag("Player") && lightController != null)
        {
            // 通知灯光控制器玩家进入触发器
            lightController.OnTriggerEnter2D(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"LightTrigger OnTriggerExit2D: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        if (other.CompareTag("Player") && lightController != null)
        {
            // 通知灯光控制器玩家离开触发器
            lightController.OnTriggerExit2D(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && lightController != null)
        {
            // 通知灯光控制器玩家在触发器内
            lightController.OnTriggerStay2D(other);
        }
    }

    // 在编辑器中绘制触发器范围（仅用于调试）
    private void OnDrawGizmos()
    {
        if (lightController != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}