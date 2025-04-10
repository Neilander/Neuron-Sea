using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

/// <summary>
/// 标签按钮 - 用于实现设置面板中的标签切换功能
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [Header("引用")]
    [SerializeField] private TabGroup tabGroup;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text tabText;

    [Header("颜色设置")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 0.9f);
    [SerializeField] private Color inactiveColor = new Color(0.8f, 0.8f, 0.8f);

    [Header("文本颜色设置")]
    [SerializeField] private Color activeTextColor = Color.black;
    [SerializeField] private Color hoverTextColor = new Color(0.1f, 0.1f, 0.1f);
    [SerializeField] private Color inactiveTextColor = new Color(0.5f, 0.5f, 0.5f);

    private Button button;

    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // 获取或添加必要组件
        button = GetComponent<Button>();

        if (background == null)
        {
            background = GetComponent<Image>();
        }

        // 检查并初始化TMP_Text组件
        if (tabText == null)
        {
            // 首先尝试在子对象中查找
            tabText = GetComponentInChildren<TMP_Text>();

            // 如果找不到，创建一个新的Text对象
            if (tabText == null)
            {
                GameObject textObj = new GameObject("Text (TMP)");
                textObj.transform.SetParent(transform, false);

                // 添加RectTransform组件
                RectTransform rectTransform = textObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;

                // 添加TMP_Text组件
                tabText = textObj.AddComponent<TextMeshProUGUI>();
                tabText.text = gameObject.name;
                tabText.color = inactiveTextColor;
                tabText.alignment = TextAlignmentOptions.Center;
                tabText.fontSize = 14;
            }
        }

        // 验证组件
        ValidateComponents();
    }

    private void ValidateComponents()
    {
        if (background == null)
        {
            Debug.LogError($"TabButton [{gameObject.name}]: 缺少Image组件！");
            return;
        }

        if (tabText == null)
        {
            Debug.LogError($"TabButton [{gameObject.name}]: 缺少TMP_Text组件！");
            return;
        }

        if (button == null)
        {
            Debug.LogError($"TabButton [{gameObject.name}]: 缺少Button组件！");
            return;
        }

        // 设置初始状态
        SetInactive();

        if (tabGroup == null)
        {
            Debug.LogWarning($"TabButton [{gameObject.name}]: 未分配TabGroup！");
        }
    }

    private void OnEnable()
    {
        if (tabGroup != null)
        {
            tabGroup.Subscribe(this);
        }
    }

    private void OnDisable()
    {
        if (tabGroup != null)
        {
            tabGroup.Unsubscribe(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup?.OnTabEnter(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup?.OnTabSelected(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup?.OnTabExit(this);
    }

    /// <summary>
    /// 设置此按钮为活跃状态
    /// </summary>
    public void SetActive()
    {
        if (background != null)
        {
            background.color = activeColor;
        }

        if (tabText != null)
        {
            tabText.color = activeTextColor;
        }
    }

    /// <summary>
    /// 设置此按钮为悬停状态
    /// </summary>
    public void SetHover()
    {
        if (background != null)
        {
            background.color = hoverColor;
        }

        if (tabText != null)
        {
            tabText.color = hoverTextColor;
        }
    }

    /// <summary>
    /// 设置此按钮为非活跃状态
    /// </summary>
    public void SetInactive()
    {
        if (background != null)
        {
            background.color = inactiveColor;
        }

        if (tabText != null)
        {
            tabText.color = inactiveTextColor;
        }
    }

    public void SetText(string text)
    {
        if (tabText != null)
        {
            tabText.text = text;
        }
    }

    public void SetBackgroundColor(Color color)
    {
        if (background != null)
        {
            background.color = color;
        }
    }

    public void SetTextColor(Color color)
    {
        if (tabText != null)
        {
            tabText.color = color;
        }
    }
}