using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBox : MonoBehaviour, ILDtkImportedFields, IDeathActionOverrider
{
    [SerializeField] private float waitTime;

    [SerializeField] private float explodeDuration;

    [SerializeField] private Animator animator;

    // public WaveMunController waveMunController;

    private bool isInCountDown = false;

    [SerializeField] private SpriteRenderer shineRenderer;

    [SerializeField] private SpriteRenderer baseRenderer;

    [SerializeField] private SpriteRenderer anchorRenderer;

    [SerializeField] private float explosionRadius;

    [SerializeField] private SpriteRenderer radiusVisualRenderer;

    [SerializeField] private GameObject RangeDisplayer;

    public GameObject checker;



    private List<GameObject> triggered;
    
    //自动导入关卡设定数据
    public void OnLDtkImportFields(LDtkFields fields)
    {
        explosionRadius = fields.GetInt("BoomRadius") * 1.5f;
        int iX = fields.GetInt("SizeX");
        int iY = fields.GetInt("SizeY");
        checker.transform.localScale = new Vector3(iX, iY, 1);
    }

    private void Start(){
        PauseEvent.OnPauseTriggered += Pause;
        PauseEvent.OnPauseResumed += Resume;
        Vector3 desiredWorldScale = new Vector3(
        explosionRadius * 2 * GridManager.Instance.gridWidth / RangeDisplayer.GetComponent<SpriteRenderer>().sprite.bounds.size.x,
        explosionRadius * 2 * GridManager.Instance.gridWidth / RangeDisplayer.GetComponent<SpriteRenderer>().sprite.bounds.size.y,
        1f
        );
        Vector3 parentWorldScale = RangeDisplayer.transform.parent != null ? RangeDisplayer.transform.parent.lossyScale : Vector3.one;
        Vector3 newLocalScale = new Vector3(
        desiredWorldScale.x / parentWorldScale.x,
        desiredWorldScale.y / parentWorldScale.y,
        desiredWorldScale.z / parentWorldScale.z
        );
        RangeDisplayer.transform.localScale = newLocalScale;

        InAndOutSwitchEvent.OnInSwitchTriggered += ShowRange;
        InAndOutSwitchEvent.OnOutSwitchTriggered += HideRange;

    }

    private void OnCollisionEnter2D(Collision2D collision){
        if (!isInCountDown && collision.gameObject.GetComponent<PlayerController>()) {
            Debug.Log("检测到玩家触碰");
            isInCountDown = true;
            // // 获取剧情数据资源
            // StoryData storyData = Resources.Load<StoryData>("StoryData/IntroStory");

            // // 进入剧情模式
            // StoryManager.Instance.EnterStoryMode(storyData);
            // if(waveMunController!=null)waveMunController.StartDisappearAnimation();
            //StartCoroutine(ExplodeCountDown(waitTime));
            StartCoroutine(ExplodeCountDownNewWithAnimation());
        }
    }

    public void StartExplode()
    {
        if (!isInCountDown)
        {
            Debug.Log("检测到玩家触碰");
            isInCountDown = true;
            //StartCoroutine(ExplodeCountDown(waitTime));
            StartCoroutine(ExplodeCountDownNewWithAnimation());
        }
    }

    public void StartDirectExplode()
    {
        if (!isInCountDown)
        {
            isInCountDown = true;
            StartCoroutine(DirectExplode());
        }
    }

    public bool DeathAction()
    {
        if (isInCountDown)
            return true;
        StartDirectExplode();
        return false;
    }


    private bool isPaused = false;

    private IEnumerator WaitUnpaused(){
        while (isPaused)
            yield return null;
    }

    public void Pause() => isPaused = false;//现在停用这个
    public void Resume() => isPaused = false;

    IEnumerator ExplodeCountDown(float time){
        
        RangeDisplayer.SetActive(true);
        int totalFlashes = 5;
        float[] flashTimings = new float[5];

        // 前2次占60%，每次30%；后3次各13.33%
        flashTimings[0] = time * 0.3f;
        flashTimings[1] = time * 0.3f;
        flashTimings[2] = time * 0.1333f;
        flashTimings[3] = time * 0.1333f;
        flashTimings[4] = time * 0.1333f;

        float x = 0;

        for (int i = 0; i < totalFlashes; i++) {
            float half = flashTimings[i] / 2f;

            // Alpha 0 → 1
            float t = 0f;
            while (t < half) {
                //yield return WaitUnpaused();

                t += Time.deltaTime;
                x += Time.deltaTime;
                //Debug.Log("当前x"+x);
                float a = Mathf.Lerp(0f, 1f, t / half);
                SetAlpha(a);
                yield return null;
            }

            // Alpha 1 → 0
            t = 0f;
            while (t < half) {
                //yield return WaitUnpaused();

                t += Time.deltaTime;
                x += Time.deltaTime;
                //Debug.Log("当前x" + x);
                float a = Mathf.Lerp(1f, 0f, t / half);
                SetAlpha(a);
                yield return null;
            }
        }
        SetAlpha(0);
        // 设置初始缩放
        radiusVisualRenderer.gameObject.SetActive(true);
        radiusVisualRenderer.transform.localScale = Vector3.zero;
        GetComponent<SwitchableObj>().SwitchEnableSwitchState();

        // 逐渐扩大 radiusVisualRenderer 的 scale
        float expandDuration = explodeDuration;
        float expandTimer = 0f;
        triggered = new List<GameObject>();
        while (expandTimer < expandDuration) {
            //yield return WaitUnpaused();

            expandTimer += Time.deltaTime;
            float t = Mathf.Clamp01(expandTimer / expandDuration);
            float scale = Mathf.Lerp(0f, explosionRadius * 2f, t); // 因为 scale 是直径
            radiusVisualRenderer.transform.localScale = new Vector3(scale, scale, 1f);
            Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, 2 * Mathf.Lerp(0, explosionRadius, t) * Vector2.one, 0);
            foreach (var hit in hits)
            {
                if (triggered.Contains(hit.gameObject))
                    continue;
                if (hit.GetComponent<PlayerController>())
                {
                    triggered.Add(hit.gameObject);
                    PlayerDeathEvent.Trigger(gameObject, DeathType.Explode);
                }
                else if (hit.GetComponent<SwitchableObj>() && hit.gameObject != gameObject)
                {
                    triggered.Add(hit.gameObject);
                    GridManager.Instance.DestroySwitchable(hit.GetComponent<SwitchableObj>());
                }
            }
            yield return null;
        }

        shineRenderer.enabled = false;
        baseRenderer.enabled = false;
        anchorRenderer.enabled = false;

        float fadeTimer = 0f;
        while (fadeTimer < expandDuration) {
            yield return WaitUnpaused();

            fadeTimer += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeTimer / expandDuration);
            Color c = radiusVisualRenderer.color;
            c.a = a;
            radiusVisualRenderer.color = c;
           
            yield return null;
        }


        // 最终检测玩家
       
        GridManager.Instance.DestroySwitchable(GetComponent<SwitchableObj>());
    }

    public IEnumerator ExplodeCountDownNewWithAnimation()
    {
        triggered = new List<GameObject>();
        RangeDisplayer.SetActive(true);
        // Step 1: 播放 "exploding" 动画
        animator.SetTrigger("exploding");
        animator.SetBool("SizeFix",true);

        // 等待 warningDuration 秒（动画播放时间）
        yield return new WaitForSeconds(waitTime);

        // Step 2: 播放 "expand" 动画
        animator.SetTrigger("expand");
        //yield return null;
        animator.speed = 0.2f / explodeDuration;
        // 获取当前动画状态
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        //float originalDuration = stateInfo.length;

        // 根据目标播放时长设置 Animator 速度
        /*
        if (explodeDuration > 0f && originalDuration > 0f)
        {
            animator.speed = originalDuration / explodeDuration;
        }
        else
        {
            animator.speed = 1f; // 保险起见，fallback
        }*/

        float timer = 0f;
        while (timer < explodeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / explodeDuration);
            float currentRadius = Mathf.Lerp(0, explosionRadius, Mathf.Clamp((t-0.75f)*4,0,1));

            // 持续 OverlapCircle 检测
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
            foreach (var hit in hits)
            {
                if (triggered.Contains(hit.gameObject)) continue;

                if (hit.GetComponent<PlayerController>())
                {
                    triggered.Add(hit.gameObject);
                    PlayerDeathEvent.Trigger(gameObject, DeathType.Explode);
                }
                else if (hit.GetComponent<SwitchableObj>() && hit.gameObject != gameObject)
                {
                    triggered.Add(hit.gameObject);
                    GridManager.Instance.DestroySwitchable(hit.GetComponent<SwitchableObj>());
                }
            }

            yield return null;
        }

        // Step 3: 可加后续逻辑，比如消失、销毁等
        GridManager.Instance.DestroySwitchable(GetComponent<SwitchableObj>());
    }

    public IEnumerator DirectExplode()
    {
        triggered = new List<GameObject>();
        RangeDisplayer.SetActive(true);
        // Step 1: 播放 "exploding" 动画
        animator.SetTrigger("exploding");
        animator.SetBool("SizeFix", true);
        yield return null;
        animator.SetTrigger("expand");
        //yield return null;
        animator.speed = 0.2f / explodeDuration;
        float timer = 0f;
        while (timer < explodeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / explodeDuration);
            float currentRadius = Mathf.Lerp(0, explosionRadius, Mathf.Clamp((t - 0.75f) * 4, 0, 1));

            // 持续 OverlapCircle 检测
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
            foreach (var hit in hits)
            {
                if (triggered.Contains(hit.gameObject)) continue;

                if (hit.GetComponent<PlayerController>())
                {
                    triggered.Add(hit.gameObject);
                    PlayerDeathEvent.Trigger(gameObject, DeathType.Explode);
                }
                else if (hit.GetComponent<SwitchableObj>() && hit.gameObject != gameObject)
                {
                    triggered.Add(hit.gameObject);
                    GridManager.Instance.DestroySwitchable(hit.GetComponent<SwitchableObj>());
                }
            }

            yield return null;
        }

        // Step 3: 可加后续逻辑，比如消失、销毁等
        GridManager.Instance.DestroySwitchable(GetComponent<SwitchableObj>());
    }

    private void SetAlpha(float a){
        Color c = shineRenderer.color;
        c.a = a;
        shineRenderer.color = c;
    }

    public void ShowRange()
    {
        RangeDisplayer.SetActive(true);
    }

    public void HideRange()
    {
        if (!isInCountDown) RangeDisplayer.SetActive(false);
    }

    private void OnDestroy()
    {
        InAndOutSwitchEvent.OnInSwitchTriggered -= ShowRange;
        InAndOutSwitchEvent.OnOutSwitchTriggered -= HideRange;
    }
}

public interface IDeathActionOverrider
{
    bool DeathAction();
}