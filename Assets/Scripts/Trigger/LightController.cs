using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [Header("灯光设置")]
    [SerializeField] private List<Light2D> playerLights = new List<Light2D>(); // 玩家身上的灯光列表
    [SerializeField] private float maxIntensity = 1f; // 最大亮度
    [SerializeField] private float minIntensity = 0f; // 最小亮度
    [SerializeField] private float fadeSpeed = 2f; // 淡入淡出速度

    [Header("触发器设置")]
    [SerializeField] private Transform triggerCenter; // 触发器中心点
    [SerializeField] private float maxDistance = 5f; // 最大影响距离

    private bool isInTrigger = false; // 是否在触发器内
    private float currentIntensity = 0f; // 当前亮度
    private float targetIntensity = 0f; // 目标亮度
    private Transform playerTransform; // 玩家变换组件

    private void Start()
    {
        // Debug.Log("LightController Start");
        // 初始化所有灯光为关闭状态
        foreach (var light in playerLights)
        {
            if (light != null)
            {
                light.intensity = minIntensity;
                // Debug.Log($"初始化灯光: {light.name}, 亮度: {light.intensity}");
            }
            else
            {
                // Debug.LogWarning("发现空的Light2D引用！");
            }
        }
    }

    private void Update()
    {
        // 平滑过渡到目标亮度
        currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * fadeSpeed);

        // 更新所有灯光的亮度
        foreach (var light in playerLights)
        {
            if (light != null)
            {
                light.intensity = currentIntensity;
            }
        }
    }

    // 当进入触发器时调用
    public void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"OnTriggerEnter2D: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        if (other.CompareTag("Player"))
        {
            isInTrigger = true;
            playerTransform = other.transform;
            StartCoroutine(UpdateLightIntensity());
            // Debug.Log("玩家进入触发器区域");
        }
    }

    // 当离开触发器时调用
    public void OnTriggerExit2D(Collider2D other)
    {
        // Debug.Log($"OnTriggerExit2D: {other.gameObject.name}, Tag: {other.gameObject.tag}");
        if (other.CompareTag("Player"))
        {
            isInTrigger = false;
            targetIntensity = minIntensity;
            // Debug.Log("玩家离开触发器区域");
        }
    }

    // 当在触发器内时调用
    public void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isInTrigger)
        {
            playerTransform = other.transform;
            // 计算玩家到触发器中心的距离
            float distance = Vector2.Distance(playerTransform.position, triggerCenter.position);

            // 根据距离计算目标亮度
            if (distance <= maxDistance)
            {
                // 距离越近，亮度越高
                float normalizedDistance = 1f - (distance / maxDistance);
                targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedDistance);
                // Debug.Log($"玩家在触发器内，距离: {distance}, 目标亮度: {targetIntensity}");
            }
            else
            {
                targetIntensity = minIntensity;
            }
        }
    }

    // 持续更新灯光亮度
    private IEnumerator UpdateLightIntensity()
    {
        // Debug.Log("开始更新灯光亮度");
        while (isInTrigger && playerTransform != null)
        {
            // 计算玩家到触发器中心的距离
            float distance = Vector2.Distance(playerTransform.position, triggerCenter.position);

            // 根据距离计算目标亮度
            if (distance <= maxDistance)
            {
                // 距离越近，亮度越高
                float normalizedDistance = 1f - (distance / maxDistance);
                targetIntensity = Mathf.Lerp(minIntensity, maxIntensity, normalizedDistance);
                // Debug.Log($"更新灯光亮度 - 距离: {distance}, 目标亮度: {targetIntensity}");
            }
            else
            {
                targetIntensity = minIntensity;
            }

            yield return new WaitForSeconds(0.1f);
        }
        // Debug.Log("停止更新灯光亮度");
    }

    // 在编辑器中绘制触发器范围（仅用于调试）
    private void OnDrawGizmos()
    {
        if (triggerCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(triggerCenter.position, maxDistance);
        }
    }
}