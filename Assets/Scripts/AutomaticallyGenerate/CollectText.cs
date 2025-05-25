using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectText : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] public RectTransform _rectTransform;
    [SerializeField] private Vector2 _startPosition = new Vector2(-960, 0);
    [SerializeField] private Vector2 showPosition = new Vector2(0, 0);
    [SerializeField] private float showDuration = 0.8f;
    [SerializeField] private float stayDuration = 5f;
    [SerializeField] private float hideDuration = 0.6f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("LevelName")]
    [SerializeField] public TextMeshProUGUI collectText;

    public void Start()
    {
        //设置位置
        _rectTransform.anchoredPosition = _startPosition;
        StartCoroutine(PlayAnimation());
    }
    

    IEnumerator PlayAnimation()
    {
        // 第一阶段：弹出动画
        yield return StartCoroutine(MoveCoroutine(
            _startPosition,
            showPosition,
            showDuration,
            showCurve
        ));

        // 第二阶段：停留等待
        yield return new WaitForSecondsRealtime(stayDuration);

        // 第三阶段：收回动画
        yield return StartCoroutine(MoveCoroutine(
            showPosition,
            _startPosition,
            hideDuration,
            hideCurve
        ));

        // 销毁自身
        Destroy(gameObject);
    }

    IEnumerator MoveCoroutine(Vector2 startPos, Vector2 targetPos, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = curve.Evaluate(t);

            _rectTransform.anchoredPosition = Vector2.Lerp(
                startPos,
                targetPos,
                curvedT
            );

            yield return null;
        }

        _rectTransform.anchoredPosition = targetPos;
    }
}
