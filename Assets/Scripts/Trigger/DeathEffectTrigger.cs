using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathEffectTrigger : MonoBehaviour
{
    private ControlEffects controlEffects;
    private bool isEffectActive = false;

    private void Start()
    {
        // 获取ControlEffects组件
        controlEffects = FindObjectOfType<ControlEffects>();
        if (controlEffects == null)
        {
            Debug.LogError("场景中没有找到ControlEffects组件！");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            print("碰到死亡效果触发器了！！");
            if (!isEffectActive)
            {
                isEffectActive = true;
                StartCoroutine(ApplyDeathEffect());
            }
        }
    }

    private IEnumerator ApplyDeathEffect()
    {
        if (controlEffects != null)
        {
            // 应用图中所示的效果
            controlEffects.jitterIntensity = 0;
            controlEffects.jitterFrequency = 0;
            controlEffects.scanLineThickness = 10;
            controlEffects.scanLineSpeed = 8.8f;
            controlEffects.colorShiftIntensity = 0;
            controlEffects.noiseIntensity = 0.36f;
            controlEffects.glitchProbability = 0;

            controlEffects.waveIntensity = 0.27f;
            controlEffects.waveFrequency = 27;
            controlEffects.waveSpeed = 10;

            controlEffects.bwEffect = 0;
            controlEffects.bwNoiseScale = 15;
            controlEffects.bwNoiseIntensity = 0.2f;
            controlEffects.bwFlickerSpeed = 8;

            controlEffects.colorCorrection = 1;
            controlEffects.hueShift = -60;
            controlEffects.saturation = 0;
            controlEffects.brightness = 1;
            controlEffects.contrast = 1.1f;
            controlEffects.redOffset = -0.2f;
            controlEffects.greenOffset = 0.1f;
            controlEffects.blueOffset = 0.1f;

            controlEffects.enableScanLineJitter = true;
            controlEffects.enableColorShift = true;
            controlEffects.enableNoise = true;
            controlEffects.enableGlitch = true;
            controlEffects.enableWaveEffect = true;
            controlEffects.enableBlackAndWhite = true;

            // 等待3秒
            yield return new WaitForSeconds(3f);

            // 重置所有效果
            controlEffects.ResetAllEffects();
            isEffectActive = false;
        }
    }
}
