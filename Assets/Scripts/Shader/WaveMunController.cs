using UnityEngine;
using System.Collections;

public class WaveMunController : MonoBehaviour
{
    [Header("WaveMun 控制")]
    [SerializeField] private Material targetMaterial; // 目标材质
    [SerializeField] private float cycleSpeed = 22f; // 循环速度
    [SerializeField] private string propertyName = "_WaveMun"; // 属性名称
 
    [Header("动画设置")]
    [SerializeField] private float appearDuration = 1f; // 出现动画持续时间
    [SerializeField] private float disappearDuration = 1f; // 消失动画持续时间
    [SerializeField] private float maxWaveValue = 20f; // 最大波浪值

    [Header("效果联动")]
    [SerializeField] private FadeController fadeController; // 淡入淡出控制器

    private bool isAnimating = false;
    private SpriteRenderer spriteRenderer;
    private UnityEngine.UI.Image imageComponent; // 添加Image组件引用
    public Canvas canvas;
    private Material originalMaterial; // 存储原始材质

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        if (fadeController == null)
        {
            fadeController = GetComponent<FadeController>();
        }

        // 获取Image组件
        imageComponent = GetComponent<UnityEngine.UI.Image>();

        // 获取Canvas组件
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = -2; // 设置初始排序层级为-2
        }

        // 保存原始材质
        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material;
        }

        StartAppearAnimation();
    }

    private void Update()
    {
        // 不在动画中时保持当前值
        if (targetMaterial != null && !isAnimating)
        {
            // 保持当前值，不做任何变化
        }
    }

    public void StartAppearAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(DelayedLayerChange());
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

    private IEnumerator DelayedLayerChange()
    {
        yield return new WaitForSeconds(1f); // 等待1秒
        if (canvas != null)
        {
            canvas.sortingOrder = 5; // 将排序层级设置为5
        }
    }

    private IEnumerator AppearAnimation()
    {
        isAnimating = true;
        float elapsedTime = 0f;
        float startValue = maxWaveValue; // 从最大值开始
        float targetValue = 0f; // 到0结束

        if (fadeController != null)
        {
            fadeController.StartAppearAnimation();
        }

        while (elapsedTime < appearDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / appearDuration;
            float currentValue = Mathf.Lerp(startValue, targetValue, normalizedTime);
            targetMaterial.SetFloat(propertyName, currentValue);
            yield return null;
        }

        targetMaterial.SetFloat(propertyName, targetValue);

        // 动画结束，波浪值为0时禁用Image组件
        if (imageComponent != null)
        {
            imageComponent.enabled = false;
        }

        isAnimating = false;
    }

    private IEnumerator DisappearAnimation()
    {
        isAnimating = true;
        float elapsedTime = 0f;
        float startValue = 0f; // 从0开始
        float targetValue = maxWaveValue; // 到最大值结束

        // 重新启用Image组件
        if (imageComponent != null)
        {
            imageComponent.enabled = true;
        }

        if (fadeController != null)
        {
            fadeController.StartDisappearAnimation();
        }

        while (elapsedTime < disappearDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / disappearDuration;
            float currentValue = Mathf.Lerp(startValue, targetValue, normalizedTime);
            targetMaterial.SetFloat(propertyName, currentValue);
            yield return null;
        }

        targetMaterial.SetFloat(propertyName, targetValue);
        isAnimating = false;
        gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("请设置目标材质！");
        }
    }
}