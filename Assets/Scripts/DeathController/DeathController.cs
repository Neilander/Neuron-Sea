using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DeathController : MonoBehaviour
{
    [Header("死亡动画设置")]
    public Image deathImg;
    public float cameraRotateAngle = 20f;      // 相机Z轴旋转目标角度
    public float cameraZoomAmount = 2f;        // 相机向前移动的距离
    public float transitionDuration = 0.5f;    // 旋转 + 拉近的时间
    public float fadeDuration = 1.0f;
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private PlayerController playerController;
    private leveldata currentLevelData;
    private float deathLineY;

    private void Start()
    {
        // 获取玩家控制器
        playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning("未找到PlayerController组件！");
        }

        // 获取当前关卡数据
        currentLevelData = FindObjectOfType<leveldata>();
        if (currentLevelData != null)
        {
            // 设置死亡线为关卡边界的最低点
            deathLineY = currentLevelData.levelBound.yMin;
            Debug.Log($"当前关卡死亡线高度: {deathLineY}");
        }
        else
        {
            Debug.LogWarning("未找到leveldata组件！");
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
        // 检测玩家是否低于死亡线
        if (playerController != null && playerController.transform.position.y < deathLineY)
        {
            HandleDeath(playerController.gameObject);
        }
    }

    public void HandleDeath(GameObject obj)
    {
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Step 1: 暂停游戏
        Time.timeScale = 0f;

        // Step 2: 获取主相机并保存初始状态
        Camera cam = Camera.main;
        if (cam == null) yield break;

        Quaternion startRot = cam.transform.rotation;
        Quaternion targetRot = startRot * Quaternion.Euler(0, 0, cameraRotateAngle);

        Vector3 startPos = cam.transform.position;
        Vector3 targetPos = startPos + cam.transform.forward * cameraZoomAmount;

        float t = 0f;

        // Step 3: 执行相机动画（旋转 + 拉近）
        while (t < transitionDuration)
        {
            t += Time.unscaledDeltaTime;
            float normalizedT = Mathf.Clamp01(t / transitionDuration);

            float curvedT = rotationCurve.Evaluate(normalizedT);
            cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, curvedT);

            yield return null;
        }

        // Step 4: 等待 0.5 秒（真实时间）
        yield return new WaitForSecondsRealtime(0.5f);

        // Step 5: 渐隐 UI 图片
        if (deathImg != null)
        {
            Color color = deathImg.color;
            color.a = 0f;
            deathImg.color = color;

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                float fadeT = Mathf.Clamp01(timer / fadeDuration);
                color.a = fadeT;
                deathImg.color = color;
                yield return null;
            }

            // 确保完全不透明
            color.a = 1f;
            deathImg.color = color;
        }

        // Step 6: 重载场景（恢复时间）
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
