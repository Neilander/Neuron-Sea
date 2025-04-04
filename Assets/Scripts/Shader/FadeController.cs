using System;
using UnityEngine;
using System.Collections;

public class FadeController : MonoBehaviour
{
    [Header("淡入淡出控制")]
    [SerializeField] private float fadeSpeed = 0.4f; // 淡入淡出速度
    [SerializeField] private string fadeAmountProperty = "_FadeAmount"; // 淡入淡出属性名
    [SerializeField] private string fadeBurnWidthProperty = "_FadeBurnWidth"; // 燃烧宽度属性名

    [Header("效果联动")]
    [SerializeField] private WaveMunController waveMunController; // 波纹控制器

    private Material material;
    private float fadeAmount;
    private bool isAnimating = false;
    private SpriteRenderer spriteRenderer;

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

    public void StartAppearAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(AppearAnimation());
        }
    }

    public void StartDisappearAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(DisappearAnimation());
        }
    }

    private IEnumerator AppearAnimation()
    {
        isAnimating = true;
        fadeAmount = 1f; // 从1开始
        material.SetFloat(fadeAmountProperty, fadeAmount);

        while (fadeAmount > 0f)
        {
            fadeAmount -= Time.deltaTime * fadeSpeed;
            fadeAmount = Mathf.Clamp01(fadeAmount);
            material.SetFloat(fadeAmountProperty, fadeAmount);
            yield return null;
        }

        isAnimating = false;
    }

    private IEnumerator DisappearAnimation()
    {
        isAnimating = true;
        fadeAmount = 0f; // 从0开始
        material.SetFloat(fadeAmountProperty, fadeAmount);

        while (fadeAmount < 1f)
        {
            fadeAmount += Time.deltaTime * fadeSpeed;
            fadeAmount = Mathf.Clamp01(fadeAmount);
            material.SetFloat(fadeAmountProperty, fadeAmount);
            yield return null;
        }

        isAnimating = false;
    }

    private void OnValidate()
    {
        if (material == null && GetComponent<SpriteRenderer>() != null)
        {
            material = GetComponent<SpriteRenderer>().sharedMaterial;
        }
    }

    
}
