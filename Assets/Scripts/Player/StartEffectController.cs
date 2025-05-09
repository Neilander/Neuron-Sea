using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartEffectController : MonoBehaviour
{
    //[Header("目标玩家控制器")]
    private PlayerController playerController;

    [Header("开始位置")]
    public Transform startPosition;

    [Header("启动参数（可调）")]
    [Range(-1f, 1f)] public float controlInput = 1f;
    public float controlDuration = 1f;

    [Header("地面检测设置")]
    public LayerMask groundLayer;
    public float raycastDistance = 5f;

    [Header("淡入图片（可选）")]
    public Image fadeImage;
    public float fadeDuration = 1f;

    public void Start()
    {
        //TriggerStartEffect();
    }

    private void OnEnable()
    {
        PlayerDeathEvent.OnDeathTriggered += StopEffect;
    }

    private void OnDisable()
    {
        PlayerDeathEvent.OnDeathTriggered -= StopEffect;
    }

    public void StopEffect(GameObject obj)
    {
        StopAllCoroutines();
    }

    public void TriggerStartEffect(bool isRight, float? specialTime = null)
    {
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null && startPosition != null)
        {
            Vector3 finalPosition = startPosition.position;
            Debug.Log(playerController.transform.position);
            // 射线向下检测地面
            RaycastHit2D hit = Physics2D.Raycast(startPosition.position, Vector2.down, raycastDistance, groundLayer);
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name);
                // 使用 BoxCollider2D 计算底部偏移
                BoxCollider2D col = playerController.GetComponent<BoxCollider2D>();
                float bottomOffset = 0.5f; // 默认值
            
                if (col != null) {
                    bottomOffset = col.offset.y - col.size.y / 2f;
                }
            
                // 设置玩家最终位置在地面上方（抵消底部偏移）
                finalPosition = hit.point - new Vector2(0f, bottomOffset);
                //Debug.Log(playerController.transform.position);
                //Debug.Log("设置玩家位置：" + finalPosition);
               
            }
            else
            {
                Debug.Log("没有检测到");
            }

            // 设置玩家位置
            playerController.transform.position = finalPosition;
            Debug.Log(playerController.transform.position);
            // 启动控制逻辑
            playerController.StartControl(controlInput, specialTime == null?controlDuration:(float)specialTime, isRight);
            // playerController.ForceGroundCheck();
            Debug.Log(playerController.transform.position);
        }
        else
        {
            Debug.LogWarning("StartEffectController 缺少 playerController 或 startPosition 的引用！");
        }

        if (fadeImage != null)
        {
            StartCoroutine(FadeInImage());
        }
    }

    private IEnumerator FadeInImage()
    {
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            color.a = 1-t;
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
    }
}
