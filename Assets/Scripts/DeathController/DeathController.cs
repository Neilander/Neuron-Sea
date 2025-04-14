using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

// 添加碰撞监听类
public class CollisionListener : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log($"玩家碰撞检测: 碰到了 {collision.gameObject.name}, 接触点: {collision.contactCount}, 位置: {transform.position}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug.Log($"玩家触发器检测: 进入了 {other.gameObject.name}, 位置: {transform.position}");
    }
}

public class DeathController : MonoBehaviour
{
    [Header("死亡动画设置")]
    public Image deathImg;
    public float cameraRotateAngle = 20f;      // 相机Z轴旋转目标角度
    public float cameraZoomAmount = 2f;        // 相机向前移动的距离
    public float transitionDuration = 0.5f;    // 旋转 + 拉近的时间
    public float fadeDuration = 1.0f;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("花屏效果设置")]
    [SerializeField] private float colorCorrectionPreDelay = 0.3f; // 颜色校正先变化的时间
    [SerializeField] private float effectTransitionDuration = 0.5f; // 过渡时间
    [SerializeField] private float effectDuration = 1.0f; // 效果持续时间
    [SerializeField] private float colorCorrectionRecoveryDelay = 3.0f; // 颜色校正和饱和度恢复前的等待时间
    [SerializeField] private Material deathEffectMaterial; // 死亡特效材质

    [Header("重生设置")]
    [SerializeField] private Vector3 respawnPosition = Vector3.zero; // 玩家重生位置
    [SerializeField] private bool useCustomRespawnPosition = false; // 是否使用自定义重生位置
    [SerializeField] private Transform respawnTarget; // 目标物体的Transform组件
    [SerializeField] private bool useTargetPosition = false; // 是否使用目标物体的位置

    private PlayerController playerController;
    private leveldata currentLevelData;
    private float deathLineY;
    private ControlEffects controlEffects;
    private bool isEffectActive = false;
    private Material originalMaterial; // 保存原始材质
    private SpriteRenderer playerSpriteRenderer; // 玩家的SpriteRenderer
    private Animator playerAnimator; // 玩家动画器引用
    private Rigidbody2D playerRigidbody; // 玩家刚体引用
    private PlayerInput playerInput; // 玩家输入组件引用

    [System.Serializable]
    public class EffectParameters
    {
        [Header("扫描线参数")]
        public float jitterIntensity = 0f;
        public float jitterFrequency = 0f;
        public float scanLineThickness = 0f;
        public float scanLineSpeed = 0f;

        [Header("颜色和噪点参数")]
        public float colorShiftIntensity = 0f;
        public float noiseIntensity = 0f;
        public float glitchProbability = 0f;

        [Header("波浪效果参数")]
        public float waveIntensity = 0f;
        public float waveFrequency = 0f;
        public float waveSpeed = 0f;

        [Header("黑白效果参数")]
        public float bwEffect = 0f;
        public float bwNoiseScale = 0f;
        public float bwNoiseIntensity = 0f;
        public float bwFlickerSpeed = 0f;

        [Header("颜色校正参数")]
        public float colorCorrection = 0f;
        public float hueShift = 0f;
        public float saturation = 1f; // 默认值为1，避免变绿
        public float brightness = 1f;
        public float contrast = 1f;
        public float redOffset = 0f;
        public float greenOffset = 0f;
        public float blueOffset = 0f;

        [Header("效果开关")]
        public bool enableScanLineJitter = false;
        public bool enableColorShift = false;
        public bool enableNoise = false;
        public bool enableGlitch = false;
        public bool enableWaveEffect = false;
        public bool enableBlackAndWhite = false;
    }

    // 保存原始值的备份
    private EffectParameters originalValues;

    [Header("初始效果参数")]
    [SerializeField]
    private EffectParameters initialValues = new EffectParameters
    {
        // 确保默认饱和度为1
        saturation = 1f,
        brightness = 1f,
        contrast = 0f
    };

    [Header("结束效果参数")]
    [SerializeField]
    private EffectParameters targetValues = new EffectParameters
    {
        jitterIntensity = 0.195f,
        jitterFrequency = 64.5f,
        scanLineThickness = 0f,
        scanLineSpeed = 4.2f,
        colorShiftIntensity = 0f,
        noiseIntensity = 0.172f,
        glitchProbability = 1f,
        waveIntensity = 0f,
        waveFrequency = 27f,
        waveSpeed = 10f,
        bwEffect = 0f,
        bwNoiseScale = 15f,
        bwNoiseIntensity = 0.2f,
        bwFlickerSpeed = 8f,
        colorCorrection = 1f,
        hueShift = 0f,
        saturation = 0f,
        brightness = 1f,
        contrast = 1f,
        redOffset = 0f,
        greenOffset = 0f,
        blueOffset = 0f,
        enableScanLineJitter = true,
        enableColorShift = true,
        enableNoise = true,
        enableGlitch = true,
        enableWaveEffect = true,
        enableBlackAndWhite = true
    };

    private void Start()
    {
        // 获取玩家控制器
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            // Debug.LogWarning("未找到PlayerController组件！");
        }

        // 获取当前关卡数据
        currentLevelData = FindObjectOfType<leveldata>();
        if (currentLevelData != null)
        {
            // 设置死亡线为关卡边界的最低点
            deathLineY = currentLevelData.levelBound.yMin;
            // Debug.Log($"当前关卡死亡线高度: {deathLineY}");
        }
        else
        {
            // Debug.LogWarning("未找到leveldata组件！");
        }

        // 获取ControlEffects组件
        controlEffects = FindObjectOfType<ControlEffects>();
        if (controlEffects == null)
        {
            // Debug.LogError("场景中没有找到ControlEffects组件！");
        }
        else
        {
            // 创建原始值的备份
            originalValues = new EffectParameters();
            BackupOriginalValues();

            // 确保初始状态是正确的
            ApplyParameters(initialValues);

            // 确保ScanLineJitterFeature是禁用状态
            controlEffects.DisableScanLineJitterFeature();
        }

        // 检查死亡特效材质是否已设置
        if (deathEffectMaterial == null)
        {
            // Debug.LogError("请在Inspector中设置死亡特效材质！将材质拖拽到DeathEffectMaterial字段中。");
        }
    }

    private void OnEnable()
    {
        PlayerDeathEvent.OnDeathTriggered += HandleDeath;
    }

    private void OnDisable()
    {
        PlayerDeathEvent.OnDeathTriggered -= HandleDeath;
    }

    private void Update()
    {
        // 持续检查和更新玩家引用
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
            if (playerController != null)
            {
                // 更新其他玩家相关组件
                GameObject playerObject = playerController.gameObject;
                playerSpriteRenderer = playerObject.GetComponent<SpriteRenderer>();
                playerAnimator = playerObject.GetComponent<Animator>();
                playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
                playerInput = playerObject.GetComponent<PlayerInput>();
                // Debug.Log("已在Update中重新获取玩家引用");
            }
        }

        // 每5秒输出一次玩家状态和位置，用于调试
        if (playerController != null && Time.frameCount % 300 == 0)
        {
            // 获取和打印碰撞体信息
            Collider2D[] colliders = playerController.GetComponents<Collider2D>();
            string colliderInfo = "";
            foreach (Collider2D col in colliders)
            {
                colliderInfo += $"{col.GetType().Name}(启用:{col.enabled},触发器:{col.isTrigger}) ";
            }

            // 获取和打印刚体信息
            string rbInfo = playerRigidbody != null ?
                $"刚体:(运动学:{playerRigidbody.isKinematic},模拟:{playerRigidbody.simulated},速度:{playerRigidbody.velocity})" :
                "无刚体";

            // Debug.Log($"[状态检查] 玩家位置:{playerController.transform.position}, {colliderInfo}, {rbInfo}");
        }

        // 检测玩家是否低于死亡线
        if (playerController != null && playerController.transform.position.y < deathLineY)
        {
            // Debug.Log($"检测到玩家({playerController.name})低于死亡线! 当前位置Y: {playerController.transform.position.y}, 死亡线Y: {deathLineY}");
            HandleDeath(playerController.gameObject);
        }
    }

    public void HandleDeath(GameObject obj)
    {
        // 获取玩家组件
        if (obj != null)
        {
            playerController = obj.GetComponent<PlayerController>();
            playerSpriteRenderer = obj.GetComponent<SpriteRenderer>();
            playerAnimator = obj.GetComponent<Animator>();
            playerRigidbody = obj.GetComponent<Rigidbody2D>();
            playerInput = obj.GetComponent<PlayerInput>();

            if (playerController != null && playerSpriteRenderer != null)
            {
                // 立即禁用输入系统
                if (playerInput != null)
                {
                    playerInput.enabled = false;
                }

                // 立即重置所有移动状态
                if (playerRigidbody != null)
                {
                    playerRigidbody.velocity = Vector2.zero;
                    playerRigidbody.isKinematic = true;
                    // 保持碰撞体检测，但防止物理响应
                    playerRigidbody.simulated = true;  // 仍然模拟碰撞
                    // Debug.Log("玩家刚体设置为运动学，但保持碰撞检测");
                }

                // 保存原始材质
                originalMaterial = playerSpriteRenderer.material;

                // 检查材质是否有GlitchFade属性
                if (deathEffectMaterial != null && !deathEffectMaterial.HasProperty("_GlitchFade"))
                {
                    // Debug.LogError("死亡特效材质缺少_GlitchFade属性！");
                }

                // 记录玩家当前位置
                // Debug.Log($"死亡时玩家位置: {obj.transform.position}");

                // 完全冻结玩家
                FreezePlayer();

                // 开始死亡序列
                StartCoroutine(DeathSequence());
            }
            else
            {
                // Debug.LogError("玩家对象缺少必要的组件！请确保玩家有SpriteRenderer组件。");
            }
        }
    }

    // 备份原始值
    private void BackupOriginalValues()
    {
        if (controlEffects == null) return;

        originalValues.jitterIntensity = controlEffects.jitterIntensity;
        originalValues.jitterFrequency = controlEffects.jitterFrequency;
        originalValues.scanLineThickness = controlEffects.scanLineThickness;
        originalValues.scanLineSpeed = controlEffects.scanLineSpeed;
        originalValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        originalValues.noiseIntensity = controlEffects.noiseIntensity;
        originalValues.glitchProbability = controlEffects.glitchProbability;
        originalValues.waveIntensity = controlEffects.waveIntensity;
        originalValues.waveFrequency = controlEffects.waveFrequency;
        originalValues.waveSpeed = controlEffects.waveSpeed;
        originalValues.bwEffect = controlEffects.bwEffect;
        originalValues.bwNoiseScale = controlEffects.bwNoiseScale;
        originalValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        originalValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        originalValues.colorCorrection = controlEffects.colorCorrection;
        originalValues.hueShift = controlEffects.hueShift;
        originalValues.saturation = controlEffects.saturation;
        originalValues.brightness = controlEffects.brightness;
        originalValues.contrast = controlEffects.contrast;
        originalValues.redOffset = controlEffects.redOffset;
        originalValues.greenOffset = controlEffects.greenOffset;
        originalValues.blueOffset = controlEffects.blueOffset;
        originalValues.enableScanLineJitter = controlEffects.enableScanLineJitter;
        originalValues.enableColorShift = controlEffects.enableColorShift;
        originalValues.enableNoise = controlEffects.enableNoise;
        originalValues.enableGlitch = controlEffects.enableGlitch;
        originalValues.enableWaveEffect = controlEffects.enableWaveEffect;
        originalValues.enableBlackAndWhite = controlEffects.enableBlackAndWhite;

        // 确保饱和度不会变成0(这会导致图像变绿)
        if (originalValues.saturation <= 0.01f)
            originalValues.saturation = 1.0f;

        // Debug.Log($"备份的原始饱和度值: {originalValues.saturation}");
    }

    // 冻结玩家的所有移动和动画
    private void FreezePlayer()
    {
        // 立即禁用输入系统
        if (playerInput != null)
        {
            playerInput.enabled = false;
            // Debug.Log("已禁用输入系统");
        }

        // 停止动画
        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
            // Debug.Log("已禁用玩家动画");
        }

        // 冻结刚体，但保留碰撞检测功能
        if (playerRigidbody != null)
        {
            playerRigidbody.velocity = Vector2.zero;
            playerRigidbody.isKinematic = true;
            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            playerRigidbody.simulated = true;  // 仍然进行碰撞检测
            // Debug.Log("已冻结玩家物理系统，但保留碰撞检测");
        }

        // 确保所有碰撞体都保持启用状态
        if (playerController != null)
        {
            Collider2D[] colliders = playerController.GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = true;
                // 使用触发器模式，可以检测但不会产生物理反应
                col.isTrigger = true;
                // Debug.Log($"将碰撞体 {col.GetType().Name} 设置为触发器模式");
            }
        }
    }

    // 解除玩家的冻结状态
    private void UnfreezePlayer()
    {
        StartCoroutine(UnfreezePlayerSequence());
    }

    private IEnumerator UnfreezePlayerSequence()
    {
        // 等待一帧确保所有特效和材质更改都已完成
        yield return new WaitForEndOfFrame();

        // 获取当前玩家（可能是重生后的新玩家）
        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            GameObject playerObject = playerController.gameObject;
            playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
            playerAnimator = playerObject.GetComponent<Animator>();
            playerInput = playerObject.GetComponent<PlayerInput>();
            playerSpriteRenderer = playerObject.GetComponent<SpriteRenderer>();

            // 为玩家添加碰撞监听组件
            CollisionListener collisionListener = playerObject.GetComponent<CollisionListener>();
            if (collisionListener == null)
            {
                collisionListener = playerObject.AddComponent<CollisionListener>();
                // Debug.Log("为玩家添加了碰撞监听组件");
            }

            // 获取所有碰撞体并启用
            Collider2D[] colliders = playerObject.GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = true;
                col.isTrigger = false; // 恢复为非触发器模式
                // Debug.Log($"启用玩家碰撞体: {col.GetType().Name}, 触发器状态: {col.isTrigger}");
            }

            // 检查玩家是否有BoxCollider2D
            BoxCollider2D boxCollider = playerObject.GetComponent<BoxCollider2D>();
            if (boxCollider != null)
            {
                boxCollider.enabled = true;
                boxCollider.isTrigger = false;
                // Debug.Log($"确认BoxCollider2D已启用，大小: {boxCollider.size}, 偏移: {boxCollider.offset}, 触发器状态: {boxCollider.isTrigger}");
            }

            // Debug.Log($"在解冻序列中重新获取了玩家引用，位置: {playerObject.transform.position}");
        }
        else
        {
            // Debug.LogError("无法找到PlayerController！解冻操作无法完成");
            yield break;
        }

        // 恢复刚体
        if (playerRigidbody != null)
        {
            // 先将模拟设置为true，再将isKinematic设置为false
            playerRigidbody.simulated = true;
            yield return new WaitForFixedUpdate();

            playerRigidbody.isKinematic = false;
            yield return new WaitForFixedUpdate();

            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            yield return new WaitForFixedUpdate();

            // 添加一个明显的向下力，帮助重新检测地面
            playerRigidbody.AddForce(Vector2.down * 5f, ForceMode2D.Impulse);
            // Debug.Log($"已恢复玩家物理系统，当前速度: {playerRigidbody.velocity}，向下施加了力");

            // 物理更新需要等待下一帧
            yield return new WaitForFixedUpdate();
        }

        // 恢复动画
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            // Debug.Log("已恢复玩家动画");
        }

        // 恢复输入系统
        if (playerInput != null)
        {
            playerInput.enabled = true;
            // Debug.Log("已恢复输入系统");
        }

        // 最后恢复玩家移动和输入控制，同时解锁位置
        if (playerController != null)
        {
            playerController.EnableMovement();
            // Debug.Log("已恢复玩家移动和输入控制，并解锁位置");
        }
    }

    private void ApplyParameters(EffectParameters parameters)
    {
        if (controlEffects == null) return;

        // 设置所有参数
        controlEffects.jitterIntensity = parameters.jitterIntensity;
        controlEffects.jitterFrequency = parameters.jitterFrequency;
        controlEffects.scanLineThickness = parameters.scanLineThickness;
        controlEffects.scanLineSpeed = parameters.scanLineSpeed;
        controlEffects.colorShiftIntensity = parameters.colorShiftIntensity;
        controlEffects.noiseIntensity = parameters.noiseIntensity;
        controlEffects.glitchProbability = parameters.glitchProbability;

        controlEffects.waveIntensity = parameters.waveIntensity;
        controlEffects.waveFrequency = parameters.waveFrequency;
        controlEffects.waveSpeed = parameters.waveSpeed;

        controlEffects.bwEffect = parameters.bwEffect;
        controlEffects.bwNoiseScale = parameters.bwNoiseScale;
        controlEffects.bwNoiseIntensity = parameters.bwNoiseIntensity;
        controlEffects.bwFlickerSpeed = parameters.bwFlickerSpeed;

        controlEffects.colorCorrection = parameters.colorCorrection;
        controlEffects.hueShift = parameters.hueShift;
        controlEffects.saturation = parameters.saturation;
        controlEffects.brightness = parameters.brightness;
        controlEffects.contrast = parameters.contrast;
        controlEffects.redOffset = parameters.redOffset;
        controlEffects.greenOffset = parameters.greenOffset;
        controlEffects.blueOffset = parameters.blueOffset;

        // 设置所有开关
        controlEffects.enableScanLineJitter = parameters.enableScanLineJitter;
        controlEffects.enableColorShift = parameters.enableColorShift;
        controlEffects.enableNoise = parameters.enableNoise;
        controlEffects.enableGlitch = parameters.enableGlitch;
        controlEffects.enableWaveEffect = parameters.enableWaveEffect;
        controlEffects.enableBlackAndWhite = parameters.enableBlackAndWhite;
    }

    private IEnumerator DeathSequence()
    {
        // 应用死亡效果
        yield return StartCoroutine(ApplyDeathEffectWithTransition());

        // 特效恢复完成后解除玩家冻结状态
        UnfreezePlayer();

        // 注意：移除了重新加载场景的功能
    }

    private IEnumerator ApplyDeathEffectWithTransition()
    {
        // Debug.Log("开始应用死亡特效...");

        // 先启用ScanLineJitterFeature特性
        controlEffects.EnableScanLineJitterFeature();

        // 检查特性是否成功启用
        if (!controlEffects.IsFeatureActive())
        {
            // Debug.LogError("无法启用ScanLineJitterFeature特性！特效可能无法正常显示");
        }
        else
        {
            // Debug.Log("ScanLineJitterFeature特性已启用");
        }

        // 强制立即更新一次特效参数
        controlEffects.ForceUpdateEffects();

        // 第一步：改变材质和开始颜色校正
        if (playerSpriteRenderer != null && deathEffectMaterial != null)
        {
            playerSpriteRenderer.material = deathEffectMaterial;
            // 设置初始GlitchFade值
            playerSpriteRenderer.material.SetFloat("_GlitchFade", 0f);
            // Debug.Log($"初始化 GlitchFade 值: 0");
        }

        float elapsedTime = 0;
        float initialColorCorrection = controlEffects.colorCorrection;
        float initialSaturation = controlEffects.saturation;

        // 同时变化颜色校正、饱和度和GlitchFade
        while (elapsedTime < colorCorrectionPreDelay)
        {
            float t = elapsedTime / colorCorrectionPreDelay;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            controlEffects.colorCorrection = Mathf.Lerp(initialColorCorrection, targetValues.colorCorrection, smoothT);
            controlEffects.saturation = Mathf.Lerp(initialSaturation, targetValues.saturation, smoothT);

            if (playerSpriteRenderer != null && deathEffectMaterial != null)
            {
                float currentFade = Mathf.Lerp(0f, 1f, smoothT);
                playerSpriteRenderer.material.SetFloat("_GlitchFade", currentFade);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保颜色校正和饱和度达到目标值，GlitchFade达到1
        controlEffects.colorCorrection = targetValues.colorCorrection;
        controlEffects.saturation = targetValues.saturation;
        if (playerSpriteRenderer != null && deathEffectMaterial != null)
        {
            playerSpriteRenderer.material.SetFloat("_GlitchFade", 1f);
            // Debug.Log("GlitchFade 达到最大值: 1");
        }
        // Debug.Log($"第一阶段完成：颜色校正={controlEffects.colorCorrection}, 饱和度={controlEffects.saturation}");

        // 第二步：开始其他所有参数的过渡
        elapsedTime = 0;
        // Debug.Log("开始第二阶段：其他参数的过渡");

        // 记录当前参数(目标效果值)
        EffectParameters endValues = new EffectParameters();
        endValues.jitterIntensity = controlEffects.jitterIntensity;
        endValues.jitterFrequency = controlEffects.jitterFrequency;
        endValues.scanLineThickness = controlEffects.scanLineThickness;
        endValues.scanLineSpeed = controlEffects.scanLineSpeed;
        endValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        endValues.noiseIntensity = controlEffects.noiseIntensity;
        endValues.glitchProbability = controlEffects.glitchProbability;
        endValues.waveIntensity = controlEffects.waveIntensity;
        endValues.waveFrequency = controlEffects.waveFrequency;
        endValues.waveSpeed = controlEffects.waveSpeed;
        endValues.bwEffect = controlEffects.bwEffect;
        endValues.bwNoiseScale = controlEffects.bwNoiseScale;
        endValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        endValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        endValues.colorCorrection = controlEffects.colorCorrection;
        endValues.hueShift = controlEffects.hueShift;
        endValues.saturation = controlEffects.saturation;
        endValues.brightness = controlEffects.brightness;
        endValues.contrast = controlEffects.contrast;
        endValues.redOffset = controlEffects.redOffset;
        endValues.greenOffset = controlEffects.greenOffset;
        endValues.blueOffset = controlEffects.blueOffset;

        bool hasMovedPlayer = false; // 标记是否已经移动了玩家

        // 开始其他效果的过渡
        while (elapsedTime < effectTransitionDuration)
        {
            float t = elapsedTime / effectTransitionDuration; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 平滑插值所有其他参数，保持颜色校正不变
            controlEffects.jitterIntensity = Mathf.Lerp(endValues.jitterIntensity, targetValues.jitterIntensity, smoothT);
            controlEffects.jitterFrequency = Mathf.Lerp(endValues.jitterFrequency, targetValues.jitterFrequency, smoothT);
            controlEffects.scanLineThickness = Mathf.Lerp(endValues.scanLineThickness, targetValues.scanLineThickness, smoothT);
            controlEffects.scanLineSpeed = Mathf.Lerp(endValues.scanLineSpeed, targetValues.scanLineSpeed, smoothT);
            controlEffects.colorShiftIntensity = Mathf.Lerp(endValues.colorShiftIntensity, targetValues.colorShiftIntensity, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(endValues.noiseIntensity, targetValues.noiseIntensity, smoothT);
            // glitchProbability直接设置为目标值，不进行平滑过渡
            controlEffects.glitchProbability = targetValues.glitchProbability;
            controlEffects.waveIntensity = Mathf.Lerp(endValues.waveIntensity, targetValues.waveIntensity, smoothT);
            controlEffects.waveFrequency = Mathf.Lerp(endValues.waveFrequency, targetValues.waveFrequency, smoothT);
            controlEffects.waveSpeed = Mathf.Lerp(endValues.waveSpeed, targetValues.waveSpeed, smoothT);
            controlEffects.bwEffect = Mathf.Lerp(endValues.bwEffect, targetValues.bwEffect, smoothT);
            controlEffects.bwNoiseScale = Mathf.Lerp(endValues.bwNoiseScale, targetValues.bwNoiseScale, smoothT);
            controlEffects.bwNoiseIntensity = Mathf.Lerp(endValues.bwNoiseIntensity, targetValues.bwNoiseIntensity, smoothT);
            controlEffects.bwFlickerSpeed = Mathf.Lerp(endValues.bwFlickerSpeed, targetValues.bwFlickerSpeed, smoothT);
            // 颜色校正已经设置好了，不需要再变化
            controlEffects.hueShift = Mathf.Lerp(endValues.hueShift, targetValues.hueShift, smoothT);
            controlEffects.saturation = Mathf.Lerp(endValues.saturation, targetValues.saturation, smoothT);
            controlEffects.brightness = Mathf.Lerp(endValues.brightness, targetValues.brightness, smoothT);
            controlEffects.contrast = Mathf.Lerp(endValues.contrast, targetValues.contrast, smoothT);
            controlEffects.redOffset = Mathf.Lerp(endValues.redOffset, targetValues.redOffset, smoothT);
            controlEffects.greenOffset = Mathf.Lerp(endValues.greenOffset, targetValues.greenOffset, smoothT);
            controlEffects.blueOffset = Mathf.Lerp(endValues.blueOffset, targetValues.blueOffset, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保达到精确的目标值
        ApplyParameters(targetValues);
        // Debug.Log("已应用目标值，所有参数过渡完成");

        // 保持效果一段时间
        // Debug.Log($"效果将保持 {effectDuration} 秒");
        float effectElapsedTime = 0;
        hasMovedPlayer = false;

        // 在效果保持阶段移动玩家
        while (effectElapsedTime < effectDuration)
        {
            // 在效果保持阶段的一半时间点移动玩家
            if (!hasMovedPlayer && effectElapsedTime >= effectDuration * 0.5f)
            {
                // 根据设置决定使用哪种重生位置
                if (useTargetPosition && respawnTarget != null)
                {
                    playerController.transform.position = respawnTarget.position;
                    // Debug.Log($"已在效果保持阶段中间时间点将玩家移动到目标物体位置：{respawnTarget.position}");
                }
                else if (useCustomRespawnPosition)
                {
                    playerController.transform.position = respawnPosition;
                    // Debug.Log($"已在效果保持阶段中间时间点将玩家移动到自定义重生位置：{respawnPosition}");
                }
                hasMovedPlayer = true;
            }

            effectElapsedTime += Time.deltaTime;
            yield return null;
        }

        // Debug.Log("开始恢复参数");

        // 先恢复除颜色校正和GlitchFade之外的所有参数
        elapsedTime = 0;
        while (elapsedTime < effectTransitionDuration)
        {
            float t = elapsedTime / effectTransitionDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 平滑插值所有参数回到原始值，但保持颜色校正、饱和度和GlitchFade不变
            controlEffects.jitterIntensity = Mathf.Lerp(endValues.jitterIntensity, originalValues.jitterIntensity, smoothT);
            controlEffects.jitterFrequency = Mathf.Lerp(endValues.jitterFrequency, originalValues.jitterFrequency, smoothT);
            controlEffects.scanLineThickness = Mathf.Lerp(endValues.scanLineThickness, originalValues.scanLineThickness, smoothT);
            controlEffects.scanLineSpeed = Mathf.Lerp(endValues.scanLineSpeed, originalValues.scanLineSpeed, smoothT);
            controlEffects.colorShiftIntensity = Mathf.Lerp(endValues.colorShiftIntensity, originalValues.colorShiftIntensity, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(endValues.noiseIntensity, originalValues.noiseIntensity, smoothT);
            // glitchProbability直接恢复为原始值，不进行平滑过渡
            controlEffects.glitchProbability = originalValues.glitchProbability;
            controlEffects.waveIntensity = Mathf.Lerp(endValues.waveIntensity, originalValues.waveIntensity, smoothT);
            controlEffects.waveFrequency = Mathf.Lerp(endValues.waveFrequency, originalValues.waveFrequency, smoothT);
            controlEffects.waveSpeed = Mathf.Lerp(endValues.waveSpeed, originalValues.waveSpeed, smoothT);
            controlEffects.bwEffect = Mathf.Lerp(endValues.bwEffect, originalValues.bwEffect, smoothT);
            controlEffects.bwNoiseScale = Mathf.Lerp(endValues.bwNoiseScale, originalValues.bwNoiseScale, smoothT);
            controlEffects.bwNoiseIntensity = Mathf.Lerp(endValues.bwNoiseIntensity, originalValues.bwNoiseIntensity, smoothT);
            controlEffects.bwFlickerSpeed = Mathf.Lerp(endValues.bwFlickerSpeed, originalValues.bwFlickerSpeed, smoothT);
            // 颜色校正和饱和度保持不变
            controlEffects.hueShift = Mathf.Lerp(endValues.hueShift, originalValues.hueShift, smoothT);
            controlEffects.brightness = Mathf.Lerp(endValues.brightness, originalValues.brightness, smoothT);
            controlEffects.contrast = Mathf.Lerp(endValues.contrast, originalValues.contrast, smoothT);
            controlEffects.redOffset = Mathf.Lerp(endValues.redOffset, originalValues.redOffset, smoothT);
            controlEffects.greenOffset = Mathf.Lerp(endValues.greenOffset, originalValues.greenOffset, smoothT);
            controlEffects.blueOffset = Mathf.Lerp(endValues.blueOffset, originalValues.blueOffset, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 等待一小段时间再开始恢复GlitchFade
        yield return new WaitForSeconds(0.2f);
        // Debug.Log("开始恢复GlitchFade...");

        // 恢复GlitchFade
        elapsedTime = 0;
        while (elapsedTime < colorCorrectionPreDelay)
        {
            float t = elapsedTime / colorCorrectionPreDelay;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            if (playerSpriteRenderer != null && deathEffectMaterial != null)
            {
                float currentFade = Mathf.Lerp(1f, 0f, smoothT);
                playerSpriteRenderer.material.SetFloat("_GlitchFade", currentFade);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保GlitchFade完全恢复到0
        if (playerSpriteRenderer != null && deathEffectMaterial != null)
        {
            playerSpriteRenderer.material.SetFloat("_GlitchFade", 0f);
            // Debug.Log("GlitchFade 完全恢复: 0");
        }

        // 恢复原始材质
        if (playerSpriteRenderer != null && originalMaterial != null)
        {
            playerSpriteRenderer.material = originalMaterial;
            // Debug.Log("已恢复原始材质");
        }

        // 平滑过渡回初始值
        // 先恢复除颜色校正外的所有参数
        elapsedTime = 0;

        // 记录当前参数(目标效果值)
        endValues = new EffectParameters();
        endValues.jitterIntensity = controlEffects.jitterIntensity;
        endValues.jitterFrequency = controlEffects.jitterFrequency;
        endValues.scanLineThickness = controlEffects.scanLineThickness;
        endValues.scanLineSpeed = controlEffects.scanLineSpeed;
        endValues.colorShiftIntensity = controlEffects.colorShiftIntensity;
        endValues.noiseIntensity = controlEffects.noiseIntensity;
        endValues.glitchProbability = controlEffects.glitchProbability;
        endValues.waveIntensity = controlEffects.waveIntensity;
        endValues.waveFrequency = controlEffects.waveFrequency;
        endValues.waveSpeed = controlEffects.waveSpeed;
        endValues.bwEffect = controlEffects.bwEffect;
        endValues.bwNoiseScale = controlEffects.bwNoiseScale;
        endValues.bwNoiseIntensity = controlEffects.bwNoiseIntensity;
        endValues.bwFlickerSpeed = controlEffects.bwFlickerSpeed;
        endValues.colorCorrection = controlEffects.colorCorrection;
        endValues.hueShift = controlEffects.hueShift;
        endValues.saturation = controlEffects.saturation;
        endValues.brightness = controlEffects.brightness;
        endValues.contrast = controlEffects.contrast;
        endValues.redOffset = controlEffects.redOffset;
        endValues.greenOffset = controlEffects.greenOffset;
        endValues.blueOffset = controlEffects.blueOffset;

        // 使用备份的原始值而不是初始值
        while (elapsedTime < effectTransitionDuration)
        {
            float t = elapsedTime / effectTransitionDuration; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 平滑插值所有参数回到原始值，但保持颜色校正和饱和度不变
            controlEffects.jitterIntensity = Mathf.Lerp(endValues.jitterIntensity, originalValues.jitterIntensity, smoothT);
            controlEffects.jitterFrequency = Mathf.Lerp(endValues.jitterFrequency, originalValues.jitterFrequency, smoothT);
            controlEffects.scanLineThickness = Mathf.Lerp(endValues.scanLineThickness, originalValues.scanLineThickness, smoothT);
            controlEffects.scanLineSpeed = Mathf.Lerp(endValues.scanLineSpeed, originalValues.scanLineSpeed, smoothT);
            controlEffects.colorShiftIntensity = Mathf.Lerp(endValues.colorShiftIntensity, originalValues.colorShiftIntensity, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(endValues.noiseIntensity, originalValues.noiseIntensity, smoothT);
            // glitchProbability直接恢复为原始值，不进行平滑过渡
            controlEffects.glitchProbability = originalValues.glitchProbability;
            controlEffects.waveIntensity = Mathf.Lerp(endValues.waveIntensity, originalValues.waveIntensity, smoothT);
            controlEffects.waveFrequency = Mathf.Lerp(endValues.waveFrequency, originalValues.waveFrequency, smoothT);
            controlEffects.waveSpeed = Mathf.Lerp(endValues.waveSpeed, originalValues.waveSpeed, smoothT);
            controlEffects.bwEffect = Mathf.Lerp(endValues.bwEffect, originalValues.bwEffect, smoothT);
            controlEffects.bwNoiseScale = Mathf.Lerp(endValues.bwNoiseScale, originalValues.bwNoiseScale, smoothT);
            controlEffects.bwNoiseIntensity = Mathf.Lerp(endValues.bwNoiseIntensity, originalValues.bwNoiseIntensity, smoothT);
            controlEffects.bwFlickerSpeed = Mathf.Lerp(endValues.bwFlickerSpeed, originalValues.bwFlickerSpeed, smoothT);
            // 颜色校正和饱和度保持不变
            controlEffects.hueShift = Mathf.Lerp(endValues.hueShift, originalValues.hueShift, smoothT);
            controlEffects.brightness = Mathf.Lerp(endValues.brightness, originalValues.brightness, smoothT);
            controlEffects.contrast = Mathf.Lerp(endValues.contrast, originalValues.contrast, smoothT);
            controlEffects.redOffset = Mathf.Lerp(endValues.redOffset, originalValues.redOffset, smoothT);
            controlEffects.greenOffset = Mathf.Lerp(endValues.greenOffset, originalValues.greenOffset, smoothT);
            controlEffects.blueOffset = Mathf.Lerp(endValues.blueOffset, originalValues.blueOffset, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 恢复其他参数后，等待指定时间
        // Debug.Log($"所有其他参数已恢复，颜色校正和饱和度将等待 {colorCorrectionRecoveryDelay} 秒后恢复");
        yield return new WaitForSeconds(colorCorrectionRecoveryDelay);
        // Debug.Log("开始恢复颜色校正和饱和度");

        // 最后再平滑恢复颜色校正和饱和度
        elapsedTime = 0;
        float finalColorCorrection = controlEffects.colorCorrection;
        float finalSaturation = controlEffects.saturation;

        // 确保目标饱和度至少为1，避免绿色效果
        float targetSaturation = originalValues.saturation <= 0.01f ? 1.0f : originalValues.saturation;

        while (elapsedTime < colorCorrectionPreDelay)
        {
            float t = elapsedTime / colorCorrectionPreDelay; // 归一化时间，从0到1
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 同时平滑变化颜色校正和饱和度参数
            float targetCorrection = originalValues.colorCorrection;
            controlEffects.colorCorrection = Mathf.Lerp(finalColorCorrection, targetCorrection, smoothT);
            controlEffects.saturation = Mathf.Lerp(finalSaturation, targetSaturation, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 确保颜色校正和饱和度达到目标值
        controlEffects.colorCorrection = originalValues.colorCorrection;
        controlEffects.saturation = targetSaturation;

        // 完全恢复到原始状态（开关设置）
        controlEffects.enableScanLineJitter = originalValues.enableScanLineJitter;
        controlEffects.enableColorShift = originalValues.enableColorShift;
        controlEffects.enableNoise = originalValues.enableNoise;
        controlEffects.enableGlitch = originalValues.enableGlitch;
        controlEffects.enableWaveEffect = originalValues.enableWaveEffect;
        controlEffects.enableBlackAndWhite = originalValues.enableBlackAndWhite;

        // Debug.Log($"恢复后的饱和度值: {controlEffects.saturation}");

        // 禁用ScanLineJitterFeature特性
        controlEffects.DisableScanLineJitterFeature();
        // Debug.Log("特效已完全禁用，效果结束");

        // 等待一帧确保ScanLineJitterFeature完全禁用
        yield return new WaitForEndOfFrame();

        isEffectActive = false;
    }
}
