using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBox : MonoBehaviour
{
    [SerializeField]private float waitTime;
    [SerializeField] private float explodeDuration;
    

    private bool isInCountDown = false;
    [SerializeField] private SpriteRenderer shineRenderer;
    [SerializeField] private SpriteRenderer baseRenderer;
    [SerializeField] private SpriteRenderer anchorRenderer;
    [SerializeField] private float explosionRadius = 1.5f;
    [SerializeField] private SpriteRenderer radiusVisualRenderer;

    private void Start()
    {
        PauseEvent.OnPauseTriggered += Pause;
        PauseEvent.OnPauseResumed += Resume;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isInCountDown && collision.gameObject.GetComponent<PlayerController>())
        {
            Debug.Log("检测到玩家触碰");
            isInCountDown = true;
            StartCoroutine(ExplodeCountDown(waitTime));
        }
    }

    private bool isPaused = false;
    private IEnumerator WaitUnpaused()
    {
        while (isPaused)
            yield return null;
    }

    public void Pause() => isPaused = true;
    public void Resume() => isPaused = false;

    IEnumerator ExplodeCountDown(float time)
    {
        int totalFlashes = 5;
        float[] flashTimings = new float[5];

        // 前2次占60%，每次30%；后3次各13.33%
        flashTimings[0] = time * 0.3f;
        flashTimings[1] = time * 0.3f;
        flashTimings[2] = time * 0.1333f;
        flashTimings[3] = time * 0.1333f;
        flashTimings[4] = time * 0.1333f;

        for (int i = 0; i < totalFlashes; i++)
        {
            float half = flashTimings[i] / 2f;

            // Alpha 0 → 1
            float t = 0f;
            while (t < half)
            {
                yield return WaitUnpaused();

                t += Time.deltaTime;
                float a = Mathf.Lerp(0f, 1f, t / half);
                SetAlpha(a);
                yield return null;
            }

            // Alpha 1 → 0
            t = 0f;
            while (t < half)
            {
                yield return WaitUnpaused();

                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, t / half);
                SetAlpha(a);
                yield return null;
            }
        }

        // 设置初始缩放
        radiusVisualRenderer.gameObject.SetActive(true);
        radiusVisualRenderer.transform.localScale = Vector3.zero;

        // 逐渐扩大 radiusVisualRenderer 的 scale
        float expandDuration = explodeDuration;
        float expandTimer = 0f;

        while (expandTimer < expandDuration)
        {
            yield return WaitUnpaused();

            expandTimer += Time.deltaTime;
            float t = Mathf.Clamp01(expandTimer / expandDuration);
            float scale = Mathf.Lerp(0f, explosionRadius * 2f, t); // 因为 scale 是直径
            radiusVisualRenderer.transform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        shineRenderer.enabled = false;
        baseRenderer.enabled = false;
        anchorRenderer.enabled = false;

        float fadeTimer = 0f;
        while (fadeTimer < expandDuration)
        {
            yield return WaitUnpaused();

            fadeTimer += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeTimer / expandDuration);
            Color c = radiusVisualRenderer.color;
            c.a = a;
            radiusVisualRenderer.color = c;
            yield return null;
        }


        // 最终检测玩家
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<PlayerController>())
            {
                PlayerDeathEvent.Trigger(gameObject, DeathType.Explode);
            }
            else if (hit.GetComponent<SwitchableObj>()&& hit.gameObject!=gameObject)
            {
                Destroy(hit.gameObject);
            }
        }
        Destroy(gameObject);
    }

    private void SetAlpha(float a)
    {
        Color c = shineRenderer.color;
        c.a = a;
        shineRenderer.color = c;
    }
}
