using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class lightTrigger : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private GameObject lightParent; // 光源父物体
    [SerializeField] private float fadeDuration = 2.0f;
    [SerializeField] private float targetIntensity = 3.0f;
    [SerializeField] private float targetRange = 5.0f;
    [SerializeField] private bool followPlayer = true;

    private Light2D[] childLights;
    private Dictionary<Light2D, Coroutine> activeCoroutines = new Dictionary<Light2D, Coroutine>();

    private void Awake()
    {
        if (lightParent != null)
        {
            childLights = lightParent.GetComponentsInChildren<Light2D>(true);
            InitializeLights();
        }
        else
        {
            Debug.LogError("Light Parent is not assigned!", this);
        }
    }

    private void InitializeLights()
    {
        foreach (var light in childLights)
        {
            light.enabled = false;
            light.intensity = 0;
            light.pointLightOuterRadius = targetRange;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (followPlayer)
        {
            lightParent.transform.position = other.transform.position;
        }

        foreach (var light in childLights)
        {
            if (activeCoroutines.ContainsKey(light))
            {
                StopCoroutine(activeCoroutines[light]);
            }
            activeCoroutines[light] = StartCoroutine(FadeLight(light, targetIntensity));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (var light in childLights)
        {
            if (activeCoroutines.ContainsKey(light))
            {
                StopCoroutine(activeCoroutines[light]);
            }
            activeCoroutines[light] = StartCoroutine(FadeLight(light, 0f));
        }
    }

    private IEnumerator FadeLight(Light2D light, float targetValue)
    {
        float startIntensity = light.intensity;
        float elapsedTime = 0f;

        light.enabled = true;

        while (elapsedTime < fadeDuration)
        {
            light.intensity = Mathf.Lerp(startIntensity, targetValue, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetValue;
        light.enabled = targetValue > 0.01f;

        activeCoroutines.Remove(light);
    }
}

