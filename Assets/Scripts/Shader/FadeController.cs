using System;
using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("淡入淡出控制")][SerializeField] private float fadeSpeed = 1f; // 淡入淡出速度

    [SerializeField] private string fadeAmountProperty = "_FadeAmount"; // 淡入淡出属性名

    [SerializeField] private string fadeBurnWidthProperty = "_FadeBurnWidth"; // 燃烧宽度属性名

    [Header("效果联动")][SerializeField] private WaveMunController waveMunController; // 波纹控制器

    [Header("Fade Settings")]
    [SerializeField] private float disappearDuration = 1f; // 添加消失动画持续时间

    private Material material;

    private float fadeAmount;

    private bool isAnimating = false;

    private SpriteRenderer spriteRenderer;

    private Coroutine currentAnimationCoroutine;

    private static readonly int FadeAmount = Shader.PropertyToID("_FadeAmount");
    private static readonly int FadeDirection = Shader.PropertyToID("_FadeDirection");

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        material = spriteRenderer.material;
        fadeAmount = material.GetFloat(fadeAmountProperty);

        if (waveMunController == null)
        {
            waveMunController = GetComponent<WaveMunController>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && waveMunController != null)
        {
            StartDisappearAnimation();
        }
    }

    public void StartDisappearAnimation()
    {
        // 如果当前没有动画在播放，或者当前动画是淡入动画，则开始淡出动画
        if (currentAnimationCoroutine == null || fadeAmount < 1f)
        {
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
            }
            currentAnimationCoroutine = StartCoroutine(DisappearAnimation());
        }
    }

    public void StartAppearAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }
        currentAnimationCoroutine = StartCoroutine(AppearAnimation());
    }

        private IEnumerator DisappearAnimation()
    {
        material.SetFloat(FadeDirection, 1f);

        float elapsedTime = 0f;
        float startValue = fadeAmount;

        while (elapsedTime < disappearDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / disappearDuration;
            fadeAmount = Mathf.Lerp(startValue, 1f, normalizedTime);
            UpdateMaterial();
            yield return null;
        }

        fadeAmount = 1f;
        UpdateMaterial();
        currentAnimationCoroutine = null;
        
        // 动画结束后销毁游戏物体
        Destroy(gameObject);
    }

    private IEnumerator AppearAnimation()
    {
        material.SetFloat(FadeDirection, -1f);

        float elapsedTime = 0f;
        float startValue = fadeAmount;

        while (elapsedTime < disappearDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / disappearDuration;
            fadeAmount = Mathf.Lerp(startValue, 0f, normalizedTime);
            UpdateMaterial();
            yield return null;
        }

        fadeAmount = 0f;
        UpdateMaterial();
        currentAnimationCoroutine = null;
    }

    private void UpdateMaterial()
    {
        material.SetFloat(FadeAmount, fadeAmount);
    }

    private void OnValidate()
    {
        if (material == null && GetComponent<SpriteRenderer>() != null)
        {
            material = GetComponent<SpriteRenderer>().sharedMaterial;
        }
    }
}