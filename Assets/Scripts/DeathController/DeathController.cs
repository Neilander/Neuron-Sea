using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 用于检测和记录碰撞事件的组件
/// </summary>
public class CollisionListener : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"玩家碰撞检测: 碰到了 {collision.gameObject.name}, 接触点: {collision.contactCount}, 位置: {transform.position}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"玩家触发器检测: 进入了 {other.gameObject.name}, 位置: {transform.position}");
    }
}

// 添加碰撞监听类
// public class CollisionListener : MonoBehaviour
// {
//     private void OnCollisionEnter2D(Collision2D collision)
//     {
//         // Debug.Log($"玩家碰撞检测: 碰到了 {collision.gameObject.name}, 接触点: {collision.contactCount}, 位置: {transform.position}");
//     }
//
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         // Debug.Log($"玩家触发器检测: 进入了 {other.gameObject.name}, 位置: {transform.position}");
//     }
// }

public class DeathController : MonoBehaviour
{
    [SerializeField] private float FLASHdelay = 0.1f;
    [Header("死亡动画设置")]
    public Image deathImg;
    public float cameraRotateAngle = 20f;      // 相机Z轴旋转目标角度
    public float cameraZoomAmount = 2f;        // 相机向前移动的距离
    public float transitionDuration = 0.5f;    // 旋转 + 拉近的时间
    public float fadeDuration = 1.0f;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("花屏效果设置")]
    [SerializeField] private float colorCorrectionPreDelay = 0.2f; // 颜色校正先变化的时间
    [SerializeField] private float colorCorrectionLateDelay = 0.2f; // 颜色校正恢复的时间
    [SerializeField] private float effectTransitionDuration = 0.3f; // 过渡时间
    [SerializeField] private float effectDuration = 0.8f; // 效果持续时间
    [SerializeField] private float colorCorrectionRecoveryDelay = 0f; // 颜色校正和饱和度恢复前的等待时间
    [SerializeField] private Material deathEffectMaterial; // 死亡特效材质

    [Header("重生设置")]
    [SerializeField] private Vector3 respawnPosition = Vector3.zero; // 玩家重生位置
    [SerializeField] private bool useCustomRespawnPosition = false; // 是否使用自定义重生位置
    [SerializeField] public Transform respawnTarget; // 目标物体的Transform组件
    [SerializeField] private bool useTargetPosition = false; // 是否使用目标物体的位置
    [SerializeField] private Material originalMaterial2;
    private PlayerController playerController;
    private leveldata currentLevelData;
    private float deathLineY;
    private ControlEffects controlEffects;
    private bool isEffectActive = false;
    [SerializeField] private Material originalMaterial; // 保存原始材质
    private SpriteRenderer playerSpriteRenderer; // 玩家的SpriteRenderer
    private Animator playerAnimator; // 玩家动画器引用
    private Rigidbody2D playerRigidbody; // 玩家刚体引用

    [Header("闪烁效果设置")]
    [SerializeField] private int flashCount = 2; // 闪烁次数
    [SerializeField] private float flashDuration = 0.15f; // 每次闪烁持续时间
    private bool hasMovedPlayer = false; // 标记是否已经移动了玩家

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
                playerAnimator = playerObject.GetComponent<Animator>();
                playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
                // Debug.Log("已在Update中重新获取玩家引用");
            }
        }

        // // 每5秒输出一次玩家状态和位置，用于调试
        // if (playerController != null && Time.frameCount % 300 == 0)
        // {
        //     // 获取和打印碰撞体信息
        //     Collider2D[] colliders = playerController.GetComponents<Collider2D>();
        //     string colliderInfo = "";
        //     foreach (Collider2D col in colliders)
        //     {
        //         colliderInfo += $"{col.GetType().Name}(启用:{col.enabled},触发器:{col.isTrigger}) ";
        //     }

        //     // 获取和打印刚体信息
        //     string rbInfo = playerRigidbody != null ?
        //         $"刚体:(运动学:{playerRigidbody.isKinematic},模拟:{playerRigidbody.simulated},速度:{playerRigidbody.velocity})" :
        //         "无刚体";

        //     // Debug.Log($"[状态检查] 玩家位置:{playerController.transform.position}, {colliderInfo}, {rbInfo}");
        // }

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
            playerController = FindAnyObjectByType<PlayerController>();
            playerSpriteRenderer = playerController.GetComponent<SpriteRenderer>();
            playerAnimator = playerController.GetComponent<Animator>();
            playerRigidbody = playerController.GetComponent<Rigidbody2D>();

            if (playerController != null && playerSpriteRenderer != null)
            {
                // 使用 PlayerController 禁用输入
                playerController.DisableInput();
                Debug.Log("死亡处理: 已禁用玩家输入");

                // 立即重置所有移动状态
                if (playerRigidbody != null)
                {
                    playerRigidbody.velocity = Vector2.zero;
                    playerRigidbody.isKinematic = true;
                    // 保持碰撞体检测，但防止物理响应
                    playerRigidbody.simulated = true;  // 仍然模拟碰撞
                    Debug.Log($"死亡处理: 已设置玩家刚体 - 运动学: {playerRigidbody.isKinematic}, 模拟: {playerRigidbody.simulated}, 速度: {playerRigidbody.velocity}");
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
        // 使用 PlayerController 禁用输入
        if (playerController != null)
        {
            playerController.DisableInput();
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
        Debug.Log("开始解冻玩家序列 - 恢复跳跃和移动功能");

        // 获取当前玩家（可能是重生后的新玩家）
        playerController = FindObjectOfType<PlayerController>();

        if (playerController != null)
        {
            GameObject playerObject = playerController.gameObject;
            playerRigidbody = playerObject.GetComponent<Rigidbody2D>();
            playerAnimator = playerObject.GetComponent<Animator>();
            Debug.Log($"解冻序列: 已获取玩家组件 - 位置: {playerObject.transform.position}");

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
            Debug.LogError("无法找到PlayerController！解冻操作无法完成");
            yield break;
        }

        // 恢复刚体
        if (playerRigidbody != null)
        {
            // 先将模拟设置为true，再将isKinematic设置为false
            playerRigidbody.simulated = true;
            Debug.Log("解冻序列: 已设置刚体模拟 = true");
            yield return new WaitForFixedUpdate();

            playerRigidbody.isKinematic = false;
            Debug.Log("解冻序列: 已设置刚体运动学 = false");
            yield return new WaitForFixedUpdate();

            playerRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("解冻序列: 已恢复刚体约束为仅冻结旋转");
            yield return new WaitForFixedUpdate();

            // playerRigidbody.AddForce(Vector2.down * 5f, ForceMode2D.Impulse);
            Debug.Log($"已恢复玩家物理系统，当前速度: {playerRigidbody.velocity}");

            // 测试跳跃所需的物理参数是否正确
            Debug.Log($"解冻序列: 检查跳跃相关物理参数 - 重力缩放: {playerRigidbody.gravityScale}, 质量: {playerRigidbody.mass}, 阻力: {playerRigidbody.drag}");

            // 物理更新需要等待下一帧
            yield return new WaitForFixedUpdate();
        }

        // 恢复动画
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            // 检查跳跃相关的动画参数
            try
            {
                playerAnimator.GetParameter(0); // 尝试获取任意参数来判断是否有参数
                Debug.Log("解冻序列: 动画器已启用，准备接收跳跃动画");
            }
            catch (System.Exception)
            {
                Debug.LogWarning("解冻序列: 动画器参数获取失败");
            }

            Debug.Log("已恢复玩家动画");
        }

        // 最后恢复玩家移动和输入控制，同时解锁位置
        if (playerController != null)
        {
            // 获取玩家的地面状态，确保跳跃检测正常
            bool isGrounded = playerController.IsGrounded();
            Debug.Log($"解冻序列: 玩家地面状态检查 - isGrounded: {isGrounded}");

            playerController.EnableMovement();
            Debug.Log("已恢复玩家移动和输入控制，并解锁位置");

            // 测试是否可以接收跳跃输入
            Debug.Log("解冻序列: 玩家现在应该可以跳跃了，请尝试按空格键");


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



        // 注意：移除了重新加载场景的功能
    }

    private IEnumerator ApplyDeathEffectWithTransition()
    {
        // Debug.Log("开始应用死亡特效...");

        // 先启用ScanLineJitterFeature特性
        controlEffects.EnableScanLineJitterFeature();

        // 强制立即更新一次特效参数
        controlEffects.ForceUpdateEffects();

        // 第一步：改变材质和开始颜色校正（快速黑白化）
        if (playerSpriteRenderer != null && deathEffectMaterial != null)
        {
            playerSpriteRenderer.material = deathEffectMaterial;
            playerSpriteRenderer.material.SetFloat("_GlitchFade", 0f);
        }

        // 快速应用黑白效果
        controlEffects.enableBlackAndWhite = true;
        controlEffects.bwEffect = 1.0f;
        controlEffects.saturation = 0f;
        controlEffects.contrast = 1.2f;

        float elapsedTime = 0;

        // 第二步：应用故障特效
        while (elapsedTime < effectTransitionDuration)
        {
            float t = elapsedTime / effectTransitionDuration;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 应用故障特效参数
            controlEffects.jitterIntensity = Mathf.Lerp(0f, 0.195f, smoothT);
            controlEffects.jitterFrequency = Mathf.Lerp(0f, 64.5f, smoothT);
            controlEffects.glitchProbability = Mathf.Lerp(0f, 1f, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(0f, 0.172f, smoothT);

            if (playerSpriteRenderer != null && deathEffectMaterial != null)
            {
                playerSpriteRenderer.material.SetFloat("_GlitchFade", smoothT);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 第三步：执行闪烁效果
        for (int i = 0; i < flashCount; i++)
        {
            // 闪烁开始 - 增强效果
            controlEffects.jitterIntensity = 0.4f;
            controlEffects.glitchProbability = 1f;
            controlEffects.noiseIntensity = 0.3f;
            controlEffects.contrast = 1.5f;
            yield return new WaitForSeconds(flashDuration * 0.5f);

            // 闪烁结束 - 恢复正常
            controlEffects.jitterIntensity = 0.195f;
            controlEffects.glitchProbability = 0.8f;
            controlEffects.noiseIntensity = 0.172f;
            controlEffects.contrast = 1.2f;
            yield return new WaitForSeconds(flashDuration * 0.5f);
        }

        // 第四步：移动玩家到重生点
        if (!hasMovedPlayer)
        {
            // 获取重生点位置逻辑保持不变
            levelManager levelMgr = FindAnyObjectByType<levelManager>();
            Transform targetTransform = null;

            if (levelMgr != null && levelMgr.respawnTarget != null)
            {
                targetTransform = levelMgr.respawnTarget;
            }
            else if (respawnTarget != null)
            {
                targetTransform = respawnTarget;
            }

            if (targetTransform != null)
            {
                playerController.transform.position = targetTransform.position;
                if (playerSpriteRenderer != null && originalMaterial2 != null)
                {
                    playerSpriteRenderer.material = originalMaterial2;
                }
            }
            hasMovedPlayer = true;
        }

        // 第五步：恢复场景颜色
        elapsedTime = 0;
        while (elapsedTime < colorCorrectionLateDelay)
        {
            float t = elapsedTime / colorCorrectionLateDelay;
            float smoothT = Mathf.SmoothStep(0, 1, t);

            // 恢复颜色
            controlEffects.bwEffect = Mathf.Lerp(1f, 0f, smoothT);
            controlEffects.saturation = Mathf.Lerp(0f, 1f, smoothT);
            controlEffects.contrast = Mathf.Lerp(1.2f, 1f, smoothT);

            // 逐渐减弱故障特效
            controlEffects.jitterIntensity = Mathf.Lerp(0.195f, 0f, smoothT);
            controlEffects.glitchProbability = Mathf.Lerp(0.8f, 0f, smoothT);
            controlEffects.noiseIntensity = Mathf.Lerp(0.172f, 0f, smoothT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 完全禁用所有特效
        controlEffects.DisableScanLineJitterFeature();
        controlEffects.enableBlackAndWhite = false;
        controlEffects.bwEffect = 0f;
        controlEffects.saturation = 1f;
        controlEffects.contrast = 1f;
        controlEffects.jitterIntensity = 0f;
        controlEffects.glitchProbability = 0f;
        controlEffects.noiseIntensity = 0f;

        isEffectActive = false;
        UnfreezePlayer();
    }
}
