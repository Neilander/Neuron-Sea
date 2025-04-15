using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using System.Collections;

/// <summary>
/// 指示牌交互脚本，当玩家靠近时显示文字
/// </summary>
public class SignInteraction : MonoBehaviour
{
    [Header("文本设置")]
    [SerializeField] private string signText = "这是一个指示牌"; // 要显示的文字
    [SerializeField, Multiline(3)] private string[] multiLineTexts; // 多行文本
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果的速度
    [SerializeField] private float displayDuration = 4f; // 显示持续时间
    [SerializeField] private float fadeDuration = 0.5f; // 淡入淡出时间

    [Header("UI设置")]
    [SerializeField] private GameObject textPanel; // 文本面板
    [SerializeField] private TextMeshProUGUI textDisplay; // 文本显示组件
    [SerializeField] private Image panelImage; // 面板图像组件（用于淡入淡出）
    [SerializeField] private Canvas parentCanvas; // 父级Canvas引用

    [Header("触发设置")]
    [SerializeField] private float triggerRadius = 2f; // 触发半径
    [SerializeField] private LayerMask playerLayer; // 玩家层

    // 替换Transform为位置选项
    // [SerializeField] private Transform textDisplayPosition; // 文本显示位置
    public enum TextPosition { Above, Below }
    [SerializeField] private TextPosition textDisplayPosition = TextPosition.Above; // 文本显示位置选项
    [SerializeField] private float textOffsetY = 1.5f; // 文本Y轴偏移量

    [SerializeField] private bool autoActivate = true; // 自动激活（否则需要按键）
    [SerializeField] private KeyCode activationKey = KeyCode.E; // 激活键

    [Header("音效设置")]
    [SerializeField] private AudioSource typingSoundEffect; // 打字声音效果
    [SerializeField] private AudioSource activationSound; // 激活声音

    // 私有变量
    private bool playerInRange = false;
    private bool isDisplaying = false;
    private Coroutine displayCoroutine;
    private int currentTextIndex = 0;
    private RectTransform panelRectTransform;
    private RenderMode canvasRenderMode;
    private Camera canvasCamera;
    private bool isSkippingToNextText = false;

    private void Start()
    {
        // 初始化文本面板
        if (textPanel != null)
        {
            textPanel.SetActive(false);
            panelRectTransform = textPanel.GetComponent<RectTransform>();
        }

        // 获取父级Canvas引用
        if (parentCanvas == null && textPanel != null)
        {
            parentCanvas = textPanel.GetComponentInParent<Canvas>();
        }

        // 检查并记录Canvas渲染模式
        if (parentCanvas != null)
        {
            canvasRenderMode = parentCanvas.renderMode;
            canvasCamera = parentCanvas.worldCamera;

            // 记录Canvas设置
            Debug.Log($"Canvas渲染模式: {canvasRenderMode}, Canvas相机: {(canvasCamera != null ? canvasCamera.name : "无")}");

            // 检查是否为Screen Space - Camera模式但没有设置相机
            if (canvasRenderMode == RenderMode.ScreenSpaceCamera && canvasCamera == null)
            {
                Debug.LogWarning("Canvas设置为Screen Space - Camera模式，但未指定Canvas相机！将尝试使用Camera.main");
                canvasCamera = Camera.main;
            }
        }
        else
        {
            Debug.LogWarning("未找到父级Canvas，文本位置可能不正确！");
        }

        // 日志记录初始设置
        Debug.Log($"指示牌 '{gameObject.name}' 初始化, 文本位置设置为: {textDisplayPosition}, 偏移量: {textOffsetY}");
    }

    private void Update()
    {
        // 检测玩家是否在范围内
        CheckPlayerInRange();

        // 如果玩家在范围内
        if (playerInRange)
        {
            // 如果当前没有显示文本且不是自动激活模式，检测激活键
            if (!isDisplaying && !autoActivate)
            {
                // 检测按键输入
                if (Input.GetKeyDown(activationKey))
                {
                    DisplaySignText();
                }
            }
            // 如果当前正在显示文本，检测是否按下激活键来显示下一段
            else if (isDisplaying && Input.GetKeyDown(activationKey))
            {
                // 如果有多行文本，则跳到下一句
                if (multiLineTexts != null && multiLineTexts.Length > 1)
                {
                    // 检查是否是最后一段文本，如果是最后一段文本，则不直接关闭面板
                    bool isLastText = (currentTextIndex == multiLineTexts.Length - 1);

                    if (isLastText)
                    {
                        // 如果是最后一段文本，按E键时重新开始循环到第一段文本
                        currentTextIndex = -1; // 设为-1，因为SkipToNextText会+1，变成0
                        SkipToNextText();
                        Debug.Log("用户按键循环到第一段文本");
                    }
                    else
                    {
                        SkipToNextText();
                        Debug.Log("用户按键跳到下一句文本");
                    }
                }
            }
        }

        // 如果当前正在显示文本，持续更新面板位置
        if (isDisplaying && textPanel != null && textPanel.activeSelf)
        {
            UpdatePanelPosition();
        }
    }

    private void CheckPlayerInRange()
    {
        // 检查附近是否有玩家
        Collider2D player = Physics2D.OverlapCircle(transform.position, triggerRadius, playerLayer);
        bool wasInRange = playerInRange;
        playerInRange = (player != null);

        // 如果玩家刚进入范围，并且是自动激活模式，则显示文本
        if (playerInRange && !wasInRange && autoActivate && !isDisplaying)
        {
            DisplaySignText();
        }
        // 如果玩家离开范围，并且正在显示文本，则终止显示
        else if (!playerInRange && wasInRange && isDisplaying)
        {
            StopDisplayText();
        }
    }

    private void DisplaySignText()
    {
        // 如果已经在显示，则停止当前显示
        if (isDisplaying && displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        // 播放激活音效
        if (activationSound != null && !isSkippingToNextText) // 只在非跳转模式下播放激活音效
        {
            activationSound.Play();
        }

        // 添加调试信息
        if (multiLineTexts != null && multiLineTexts.Length > 0)
        {
            Debug.Log($"显示文本索引: {currentTextIndex}, 文本内容: {multiLineTexts[currentTextIndex]}");
        }
        else
        {
            Debug.Log($"显示单行文本: {signText}");
        }

        // 开始显示文本
        displayCoroutine = StartCoroutine(DisplayTextSequence());
    }

    private void UpdatePanelPosition()
    {
        if (textPanel == null)
            return;

        // 获取用于屏幕坐标转换的相机
        Camera renderCamera = GetRenderCamera();
        if (renderCamera == null)
            return;

        // 根据设置的位置选项计算文本位置
        Vector3 worldPosition;

        // 根据选择决定文本显示在上方还是下方
        if (textDisplayPosition == TextPosition.Above)
        {
            worldPosition = transform.position + Vector3.up * textOffsetY;
        }
        else // TextPosition.Below
        {
            worldPosition = transform.position - Vector3.up * textOffsetY;
        }

        // 根据Canvas的渲染模式设置面板位置
        if (parentCanvas != null)
        {
            switch (canvasRenderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    // 转换为屏幕坐标
                    Vector3 screenPos = renderCamera.WorldToScreenPoint(worldPosition);
                    textPanel.transform.position = screenPos;
                    break;

                case RenderMode.ScreenSpaceCamera:
                    // 使用WorldToViewportPoint并转换为Canvas空间
                    Vector3 viewportPos = renderCamera.WorldToViewportPoint(worldPosition);
                    if (panelRectTransform != null && parentCanvas != null)
                    {
                        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                        if (canvasRect != null)
                        {
                            Vector2 canvasSize = canvasRect.rect.size;
                            float posX = viewportPos.x * canvasSize.x;
                            float posY = viewportPos.y * canvasSize.y;
                            panelRectTransform.anchoredPosition = new Vector2(posX - canvasSize.x * 0.5f, posY - canvasSize.y * 0.5f);
                        }
                    }
                    break;

                case RenderMode.WorldSpace:
                    // 直接设置世界位置
                    textPanel.transform.position = worldPosition;
                    textPanel.transform.rotation = Quaternion.LookRotation(renderCamera.transform.forward);
                    break;
            }
        }
        else
        {
            // 如果没有Canvas引用，回退到基本方法
            Vector3 screenPos = renderCamera.WorldToScreenPoint(worldPosition);
            textPanel.transform.position = screenPos;
        }

        // 添加额外的日志以便于调试
        if (Time.frameCount % 120 == 0) // 每120帧记录一次，避免日志过多
        {
            Debug.Log($"更新文本位置: 位置选项={textDisplayPosition}, 世界位置={worldPosition}, Canvas模式={canvasRenderMode}");
        }
    }

    // 获取用于渲染的相机
    private Camera GetRenderCamera()
    {
        // 优先使用Canvas相机
        if (canvasRenderMode == RenderMode.ScreenSpaceCamera && canvasCamera != null)
        {
            return canvasCamera;
        }

        // 如果没有设置Canvas相机，使用主相机
        return Camera.main;
    }

    private IEnumerator DisplayTextSequence()
    {
        isDisplaying = true;

        // 获取当前要显示的文本
        string textToDisplay;
        if (multiLineTexts != null && multiLineTexts.Length > 0)
        {
            // 确保索引在有效范围内
            currentTextIndex = Mathf.Clamp(currentTextIndex, 0, multiLineTexts.Length - 1);
            textToDisplay = multiLineTexts[currentTextIndex];
        }
        else
        {
            textToDisplay = signText;
        }

        // 更新文本显示位置
        UpdatePanelPosition();

        // 显示面板
        if (textPanel != null)
        {
            // 只在面板尚未显示或不是通过E键跳转时执行淡入效果
            bool shouldFadeIn = !textPanel.activeSelf && !isSkippingToNextText;

            // 确保面板可见
            textPanel.SetActive(true);

            // 淡入面板 - 只在首次显示时执行，跳转时跳过
            if (shouldFadeIn && panelImage != null)
            {
                float elapsedTime = 0;
                Color startColor = panelImage.color;
                startColor.a = 0;
                Color targetColor = startColor;
                targetColor.a = 1;
                panelImage.color = startColor;

                while (elapsedTime < fadeDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                    panelImage.color = Color.Lerp(startColor, targetColor, t);
                    yield return null;
                }
            }
            else if (isSkippingToNextText && panelImage != null)
            {
                // 如果是跳转，确保面板完全不透明
                Color panelColor = panelImage.color;
                panelColor.a = 1;
                panelImage.color = panelColor;
            }
        }

        // 打字机效果显示文本
        if (textDisplay != null)
        {
            textDisplay.text = "";
            for (int i = 0; i < textToDisplay.Length; i++)
            {
                textDisplay.text += textToDisplay[i];

                // 播放打字声音
                if (typingSoundEffect != null && (i % 2 == 0)) // 每隔两个字符播放一次，可以调整
                {
                    typingSoundEffect.pitch = Random.Range(0.9f, 1.1f);
                    typingSoundEffect.Play();
                }

                // 如果在打字过程中按下激活键，加速完成打字
                if (Input.GetKeyDown(activationKey) && i < textToDisplay.Length - 1)
                {
                    // 直接完成打字
                    textDisplay.text = textToDisplay;
                    Debug.Log("用户加速完成打字");
                    break; // 跳出循环
                }

                yield return new WaitForSeconds(typingSpeed);
            }
        }

        // 等待显示持续时间，但如果按键则可以跳过
        float remainingTime = displayDuration;
        bool nextPageTriggered = false; // 新增标志，检测是否触发了翻页

        while (remainingTime > 0 && !nextPageTriggered)
        {
            remainingTime -= Time.deltaTime;

            // 检测按键事件
            if (Input.GetKeyDown(activationKey))
            {
                // 如果有多行文本，检测是否可以跳转
                if (multiLineTexts != null && multiLineTexts.Length > 1)
                {
                    // 标记已触发翻页
                    nextPageTriggered = true;

                    // 检查当前是否是最后一段文本
                    bool isLastText = (currentTextIndex == multiLineTexts.Length - 1);

                    if (isLastText)
                    {
                        // 如果是最后一段，准备循环到第一段
                        currentTextIndex = -1; // 设为-1，因为SkipToNextText会+1，变成0
                        Debug.Log("触发循环到第一段文本");
                    }

                    // 中断等待时间
                    break;
                }
                else
                {
                    // 如果是单行文本，直接结束等待
                    Debug.Log("单行文本，用户提前结束等待");
                    break;
                }
            }

            yield return null;
        }

        // 处理多行文本的索引
        if (multiLineTexts != null && multiLineTexts.Length > 0 && !nextPageTriggered)
        {
            // 只有在没有手动翻页的情况下才自动更新索引
            // 记录之前的索引，用于调试
            int previousIndex = currentTextIndex;

            // 更新索引以准备下一次显示
            currentTextIndex = (currentTextIndex + 1) % multiLineTexts.Length;

            Debug.Log($"文本循环: 从索引 {previousIndex} 更新到 {currentTextIndex}, 下一段文本: {multiLineTexts[currentTextIndex]}");

            // 检查当前文本是否为空
            if (string.IsNullOrEmpty(multiLineTexts[currentTextIndex]))
            {
                Debug.LogWarning("多行文本中存在空文本，索引: " + currentTextIndex);
            }
        }

        // 检查是否需要继续显示下一段文本
        bool shouldContinueToNextText = false;

        // 判断是通过按键触发继续还是自动循环继续
        if (nextPageTriggered)
        {
            // 如果通过按键触发了翻页，并且玩家还在范围内，则继续显示
            shouldContinueToNextText = playerInRange;
            Debug.Log("通过按键触发继续显示下一段文本");
        }
        else
        {
            // 自动模式下的循环条件
            shouldContinueToNextText = playerInRange && autoActivate && multiLineTexts != null && multiLineTexts.Length > 1;
            Debug.Log("自动模式循环条件评估: " + shouldContinueToNextText);
        }

        // 如果不需要继续下一条文本，则执行淡出效果
        if (!shouldContinueToNextText && textPanel != null && panelImage != null)
        {
            Debug.Log("执行淡出效果");
            // 淡出面板
            float elapsedTime = 0;
            Color startColor = panelImage.color;
            Color targetColor = startColor;
            targetColor.a = 0;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / fadeDuration);
                panelImage.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            textPanel.SetActive(false);
        }

        isDisplaying = false;
        displayCoroutine = null;

        // 如果需要继续显示下一段文本
        if (shouldContinueToNextText)
        {
            if (nextPageTriggered)
            {
                // 如果是通过按键触发的，立即显示下一段
                SkipToNextText();
                Debug.Log("立即显示下一段文本 (按键触发)");
            }
            else
            {
                // 如果是自动模式，等待一小段时间再显示
                yield return new WaitForSeconds(0.5f);
                DisplaySignText();
                Debug.Log("等待后显示下一段文本 (自动模式)");
            }
        }
    }

    private void StopDisplayText()
    {
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        // 立即停止显示，不带淡出效果
        if (textPanel != null)
        {
            textPanel.SetActive(false);
        }

        // 重置状态
        isDisplaying = false;
        isSkippingToNextText = false;
    }

    // 修改SkipToNextText方法，优化协程管理
    private void SkipToNextText()
    {
        // 停止当前显示过程
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
            displayCoroutine = null;
        }

        // 更新文本索引
        if (multiLineTexts != null && multiLineTexts.Length > 0)
        {
            int previousIndex = currentTextIndex;
            currentTextIndex = (currentTextIndex + 1) % multiLineTexts.Length;
            Debug.Log($"手动跳转: 从索引 {previousIndex} 到 {currentTextIndex}");
        }

        try
        {
            // 设置跳转标志为true - 这个会告诉系统不要执行淡入淡出
            isSkippingToNextText = true;

            // 立即显示下一段文本
            DisplaySignText();
        }
        finally
        {
            // 确保在任何情况下都重置跳转标志
            isSkippingToNextText = false;
        }
    }

    // 在编辑器中绘制触发范围（仅在Unity编辑器中可见）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);

        // 绘制文本显示位置
        Gizmos.color = Color.green;
        Vector3 textPos;

        if (textDisplayPosition == TextPosition.Above)
        {
            textPos = transform.position + Vector3.up * textOffsetY;
        }
        else // TextPosition.Below
        {
            textPos = transform.position - Vector3.up * textOffsetY;
        }

        Gizmos.DrawLine(transform.position, textPos);
        Gizmos.DrawSphere(textPos, 0.2f);

        // 添加文本位置标签
#if UNITY_EDITOR
        UnityEditor.Handles.Label(textPos, "文本显示位置");
#endif
    }

    // 添加OnDisable和OnDestroy方法，确保脚本被禁用或销毁时正确清理资源
    private void OnDisable()
    {
        // 确保停止所有协程
        StopAllCoroutines();

        // 清理当前显示状态
        CleanupDisplayState();

        Debug.Log($"SignInteraction on '{gameObject.name}' 已禁用，清理所有协程");
    }

    private void OnDestroy()
    {
        // 确保停止所有协程
        StopAllCoroutines();

        // 清理当前显示状态
        CleanupDisplayState();

        Debug.Log($"SignInteraction on '{gameObject.name}' 已销毁，清理所有协程");
    }

    // 添加辅助方法清理显示状态
    private void CleanupDisplayState()
    {
        // 重置显示状态
        isDisplaying = false;
        displayCoroutine = null;
        isSkippingToNextText = false;

        // 隐藏面板
        if (textPanel != null)
        {
            textPanel.SetActive(false);
        }
    }
}