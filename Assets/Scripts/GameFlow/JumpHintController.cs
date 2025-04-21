using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 跳跃提示控制器
/// 控制跳跃提示图片的显示和隐藏，以及相关动画效果
/// </summary>
public class JumpHintController : MonoBehaviour
{
    [Header("UI引用")]
    [SerializeField] private GameObject hintContainer;      // 提示容器对象
    [SerializeField] private Image hintImage;               // 提示图片
    [SerializeField] private CanvasGroup canvasGroup;       // Canvas组件用于淡入淡出
    
    [Header("设置")]
    [SerializeField] private float fadeInDuration = 0.5f;   // 淡入时间
    [SerializeField] private float fadeOutDuration = 0.5f;  // 淡出时间
    [SerializeField] private float pulseMinScale = 0.9f;    // 脉动最小缩放
    [SerializeField] private float pulseMaxScale = 1.1f;    // 脉动最大缩放
    [SerializeField] private float pulseDuration = 1.0f;    // 脉动周期
    [SerializeField] private bool autoPulse = true;         // 是否自动脉动
    
    private bool isVisible = false;                  // 是否可见
    private Coroutine fadeCoroutine;                 // 淡入淡出协程
    private Coroutine pulseCoroutine;                // 脉动协程
    
    private void Awake()
    {
        // 如果没有指定CanvasGroup，尝试获取
        if (canvasGroup == null && hintContainer != null)
        {
            canvasGroup = hintContainer.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = hintContainer.AddComponent<CanvasGroup>();
            }
        }
        
        // 初始隐藏提示
        HideImmediate();
    }
    
    /// <summary>
    /// 显示提示
    /// </summary>
    public void Show()
    {
        // 如果当前有淡入淡出协程，停止它
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // 开始淡入协程
        fadeCoroutine = StartCoroutine(FadeIn());
        
        // 如果设置了自动脉动，开始脉动
        if (autoPulse && pulseCoroutine == null)
        {
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }
    }
    
    /// <summary>
    /// 隐藏提示
    /// </summary>
    public void Hide()
    {
        // 如果当前有淡入淡出协程，停止它
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        // 开始淡出协程
        fadeCoroutine = StartCoroutine(FadeOut());
        
        // 如果有脉动协程，停止它
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
    }
    
    /// <summary>
    /// 立即显示（无动画）
    /// </summary>
    public void ShowImmediate()
    {
        // 停止所有协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        // 显示容器
        if (hintContainer != null)
        {
            hintContainer.SetActive(true);
        }
        
        // 设置透明度为1
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        isVisible = true;
        
        // 如果设置了自动脉动，开始脉动
        if (autoPulse && pulseCoroutine == null)
        {
            pulseCoroutine = StartCoroutine(PulseAnimation());
        }
    }
    
    /// <summary>
    /// 立即隐藏（无动画）
    /// </summary>
    public void HideImmediate()
    {
        // 停止所有协程
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // 隐藏容器
        if (hintContainer != null)
        {
            hintContainer.SetActive(false);
        }
        
        // 设置透明度为0
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // 重置缩放
        if (hintImage != null)
        {
            hintImage.transform.localScale = Vector3.one;
        }
        
        isVisible = false;
    }
    
    /// <summary>
    /// 淡入协程
    /// </summary>
    private IEnumerator FadeIn()
    {
        // 显示容器
        if (hintContainer != null)
        {
            hintContainer.SetActive(true);
        }
        
        // 如果有CanvasGroup组件
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        isVisible = true;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// 淡出协程
    /// </summary>
    private IEnumerator FadeOut()
    {
        // 如果有CanvasGroup组件
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
        }
        
        // 隐藏容器
        if (hintContainer != null)
        {
            hintContainer.SetActive(false);
        }
        
        isVisible = false;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// 脉动动画协程
    /// </summary>
    private IEnumerator PulseAnimation()
    {
        if (hintImage == null) yield break;
        
        Transform imageTransform = hintImage.transform;
        Vector3 originalScale = imageTransform.localScale;
        
        while (isVisible)
        {
            // 从小到大
            float elapsedTime = 0f;
            while (elapsedTime < pulseDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (pulseDuration / 2);
                float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, t);
                imageTransform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            
            // 从大到小
            elapsedTime = 0f;
            while (elapsedTime < pulseDuration / 2)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (pulseDuration / 2);
                float scale = Mathf.Lerp(pulseMaxScale, pulseMinScale, t);
                imageTransform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }
        
        // 重置缩放
        imageTransform.localScale = originalScale;
        pulseCoroutine = null;
    }
    
    /// <summary>
    /// 设置提示图片
    /// </summary>
    public void SetHintImage(Sprite sprite)
    {
        if (hintImage != null && sprite != null)
        {
            hintImage.sprite = sprite;
        }
    }
    
    /// <summary>
    /// 检查提示是否可见
    /// </summary>
    public bool IsVisible()
    {
        return isVisible;
    }
}