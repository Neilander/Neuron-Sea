using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class LevelTitle : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Vector2 _startPosition = new Vector2(-320, 0);
    [SerializeField] private Vector2 showPosition = new Vector2(0, 0);
    [SerializeField] private float showDuration = 0.8f;
    [SerializeField] private float stayDuration = 1.5f;
    [SerializeField] private float hideDuration = 0.6f;
    [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve hideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("LevelName")]
    [SerializeField] private TextMeshProUGUI levelTitle;
    [SerializeField] private TextMeshProUGUI levelNumber;
    [TextArea(3, 10)]
    public string fullText; // ����ĳ����֣�ʹ�� / �ָ�

    void Start()
    {
        _rectTransform.anchoredPosition = _startPosition;
        SetTitleText();
        StartCoroutine(PlayAnimation());
    }

    void SetTitleText()
    {
        string[] parts = fullText.Split(' ');
        levelTitle.text = parts[(levelManager.instance.currentLevelIndex - 1) / 2].Trim();
        levelNumber.text = levelManager.instance.currentLevelIndex % 2 == 1 ? "#1" : "#2";
    }

    IEnumerator PlayAnimation()
    {
        // ��һ�׶Σ���������
        yield return StartCoroutine(MoveCoroutine(
            startPos: _startPosition,
            targetPos: showPosition,
            duration: showDuration,
            curve: showCurve
        ));

        // �ڶ��׶Σ�ͣ���ȴ�
        yield return new WaitForSeconds(stayDuration);

        // �����׶Σ��ջض���
        yield return StartCoroutine(MoveCoroutine(
            startPos: showPosition,
            targetPos: _startPosition,
            duration: hideDuration,
            curve: hideCurve
        ));

        // ��������
        Destroy(gameObject);
    }

    IEnumerator MoveCoroutine(Vector2 startPos, Vector2 targetPos, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
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
