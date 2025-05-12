using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CompanionController : MonoBehaviour
{
    public CameraSequencePlayer BigCamera;

    public bool canFollow = true;

    [Header("跟随设置")]
    [SerializeField] private Transform target; // 跟随目标（玩家）
    [SerializeField] private float followSpeed = 5f; // 跟随速度
    [SerializeField] private Vector3 offset = new Vector3(1f, 3f, 0f); // 相对于目标的偏移量
    [SerializeField] private float smoothTime = 0.3f; // 平滑时间
    [SerializeField] private bool autoAdjustPosition = true; // 是否自动根据玩家朝向调整位置

    [Header("动画设置")]
    [SerializeField] private Animator animator; // 动画控制器
    [SerializeField] private string isMovingParam = "IsMoving"; // 移动状态参数名
    [SerializeField] private string directionParam = "Direction"; // 方向参数名
    [SerializeField] private float moveThreshold = 0.1f; // 移动检测阈值

    [Header("位置预设")]
    [SerializeField]
    private Vector3[] positionPresets = new Vector3[]
    {
        new Vector3(1f, 1f, 0f),  // 右上角
        new Vector3(-1f, 1f, 0f), // 左上角
        new Vector3(0f, 1.5f, 0f), // 正上方
        new Vector3(1f, 0.5f, 0f)  // 右侧
    };

    private Transform oldTrans;
    private Vector3 velocity = Vector3.zero;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer targetSpriteRenderer;
    private int currentPresetIndex = 0;
    private Vector3 currentOffset;
    private Vector3 lastPosition;
    private bool isMoving;

    private CameraSequencePlayer csp;
    
    public bool hasStopped=true;
    private bool _startMode=false;//改成true之后出现报空


    private bool ignoreStory = true;
    public bool StartMode
    {
        get => _startMode;
        set
        {
            if (_startMode != value) {
                _startMode = value;
                Debug.Log("startMode 被改动了，现在是: " + value);
            }
        }
    }

    public void PrepareForLevelStory(int n)
    {
        if (n == 1)
        {
            hasStopped = false;
            //如果第一次进入在右上角出现
            _startMode = true;
            ignoreStory = false;
        }
    }

    private void Awake()
    {
        StoryGlobalLoadManager.instance.RegisterOnStartWithStory(PrepareForLevelStory);
    }

    private void OnDestroy()
    {
        StoryGlobalLoadManager.instance.UnregisterOnStartWithStory(PrepareForLevelStory);
    }

    private void Start(){
        csp = FindObjectOfType<CameraSequencePlayer>();
        
        /* 原来剧情相关
        if (levelManager.instance.currentLevelIndex == 1) {
            hasStopped=false;
            //如果第一次进入在右上角出现
            _startMode=true;
        }*/
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("未找到SpriteRenderer组件！");
        }

        if (target != null)
        {
            targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("未找到Animator组件！");
            }
        }
        if (levelManager.instance.sceneIndex == 2 && levelManager.instance.currentLevelIndex != 13) {
            animator.Play("robot2");
        }
        if (levelManager.instance.sceneIndex == 3)
        {
            animator.Play("robot3");
        }

        lastPosition = transform.position;
    }

    private void Update()
    {
        if (target == null ||!canFollow) return;

        // 检测是否在移动
        float distance = Vector3.Distance(transform.position, lastPosition);
        isMoving = distance > moveThreshold;

        // 更新动画状态
        if (animator != null)
        {
            animator.SetBool(isMovingParam, isMoving);

            // 根据目标位置设置方向参数
            float direction = transform.position.x < target.position.x ? -1f : 1f;
            animator.SetFloat(directionParam, direction);
        }

        // 根据玩家scale.x自动调整位置
        if (autoAdjustPosition)
        {
            //HelperToolkit.PrintBoolStates(()=>CameraControl.Instance.specialStartForScene1, ()=>CameraControl.Instance.hasLoadOnce);
            if (CameraControl.Instance.specialStartForScene1&&!(CameraControl.Instance.hasLoadOnce) ){ //这里泡饭写的是获取注册表，我改了
                // if (csp == null) {
                //     Debug.LogError("没有打开镜头序列");
                // }
                // if(csp != null)
                //     csp.gameObject.SetActive(true);
                _startMode = true;
                // print("我是true");
            }
            else {
                // if (csp != null)
                //     csp.gameObject.SetActive(false);
                _startMode = false;
                // print("我是false");
            }
            // 如果玩家朝左（scale.x = -1）或者是开始情况，跟随物在右上角
            if (target.localScale.x < 0 || _startMode
                )
            {
                currentOffset = new Vector3(1.5f, 2.18f, 0f);
                if (!_startMode) {
                    transform.localScale = new Vector3(-1, 1, 1);
                }

            }
            // 如果玩家朝右（scale.x = 1），跟随物在左上角
            else
            {
                currentOffset = new Vector3(-1.5f,2.18f, 0f);
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            currentOffset = offset;
        }

        // 计算目标位置（目标位置 + 偏移量）
        Vector3 targetPosition = target.position + currentOffset;
        if(canFollow)
        // 使用Mathf.Lerp实现平滑跟随
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime,
            followSpeed
        );
        //HelperToolkit.PrintBoolStates(() => CameraControl.Instance.hasLoadOnce);
        /*原本剧情相关
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f
            &&!hasStopped
            &&levelManager.instance.isStartStory
            && levelManager.instance.currentLevelIndex == 1
            && CameraControl.Instance.specialStartForScene1&& !(CameraControl.Instance.hasLoadOnce)//这里泡饭写的是获取注册表，我改了
            ) {
            hasStopped = true;
            print("我到达目的地了！");
            // startMode = true;
            oldTrans =this.transform;
            
        
            StartCoroutine(StopStartMode());
        }*/

        if (!ignoreStory && Vector3.Distance(transform.position, targetPosition) < 0.01f && !hasStopped
            && StoryGlobalLoadManager.instance.IfThisStartHasLevel())
        {
            hasStopped = true;
            print("我到达目的地了！");
            // startMode = true;
            oldTrans = this.transform;

            ignoreStory = true;
            StartCoroutine(StopStartMode());
        }
        lastPosition = transform.position;
    }

    public void DirectTo()
    {
        if(target != null)
            transform.position = target.position + offset;
    }

    public void CannotMove(){
        this.enabled = false;
    }
    public void CanMove(){
        this.enabled = true;
    }
    private IEnumerator StopStartMode(){
        canFollow = false;
        print("不能跟随了！");
        transform.localScale = new Vector3(-1f, 1f, 1f);
        print("转向了！");
        GetComponent<Animator>().Play("robot_scan");
        print("播放动画了！");
        AudioManager.Instance.Play(SFXClip.Scan);
        // 等待动画状态真正进入 robot_move 状态
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("robot_scan"));

        // 等待动画播放完（normalizedTime >= 1）
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f
            
        );print("播完了！");
        _startMode = false;
        CameraControl.Instance.specialStartForScene1=false;
        transform.GetComponent<Animator>().Play("robot_idle");
        // canFollow = true;
        // transform.localScale = new Vector3(1f, 1f, 1f);
        print("转回去了！");
        
        if (BigCamera != null) {
            Camera.main.transform.GetComponent<CameraControl>().RestoreHorizontalLimit();
            BigCamera.PlaySequence();
        }
    }
    // 设置跟随目标
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        }
    }

    public void SetTargetToPlayer(){
        target = FindAnyObjectByType<PlayerController>().transform;
        _startMode = true;
        if (target != null) {
            targetSpriteRenderer = target.GetComponent<SpriteRenderer>();
        }
    }
    // 切换到下一个位置预设
    public void SwitchToNextPosition()
    {
        currentPresetIndex = (currentPresetIndex + 1) % positionPresets.Length;
        offset = positionPresets[currentPresetIndex];
    }

    // 切换到指定位置预设
    public void SwitchToPosition(int index)
    {
        if (index >= 0 && index < positionPresets.Length)
        {
            currentPresetIndex = index;
            offset = positionPresets[index];
        }
        else
        {
            Debug.LogWarning($"无效的位置预设索引: {index}");
        }
    }

    // 设置自定义偏移量
    public void SetCustomOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }

    // 设置是否自动调整位置
    public void SetAutoAdjustPosition(bool value)
    {
        autoAdjustPosition = value;
    }



    public void StartMoveRightForSeconds(float speed, float duration, GameObject panelToShow){
        StartCoroutine(MoveRightCoroutine(speed, duration, panelToShow));
    }
/// <summary>
/// 
/// </summary>
/// <param name="speed">移动速度</param>
/// <param name="duration">移动时间</param>
/// <param name="panelToShow">打开的面板</param>
/// <returns></returns>
    private IEnumerator MoveRightCoroutine(float speed, float duration, GameObject panelToShow){
        float elapsed = 0f;
        //Rigidbody2D rb = GetComponent<Rigidbody2D>();

        while (elapsed < duration) {
            //rb.velocity = new Vector2(speed, rb.velocity.y);
            transform.position += new Vector3(speed, 0, 0)*Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        //rb.velocity = Vector2.zero;

        // 打开面板
        if (panelToShow != null) {
            panelToShow.SetActive(true);
        }
        VideoPlayer videoPlayer = panelToShow.transform.GetComponent<VideoPlayer>();
        
        if (videoPlayer != null) {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    // 视频播放完后回到主菜单
    private void OnVideoEnd(VideoPlayer vp){
        // 加载主菜单场景
        ActivityGateCenter.ExitState(ActivityState.Story);
        SceneManager.LoadScene("BeginMenu"); // "MainMenu"为主菜单场景的名称
    }
}