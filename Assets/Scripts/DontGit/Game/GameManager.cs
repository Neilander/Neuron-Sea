using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        // 单例实例
        public static GameManager Instance { get; private set; }

        [Header("关卡设置")]
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private int targetSwapCount = 3; // 当前关卡的目标交换次数
        [SerializeField] private string nextLevelName;

        [Header("UI引用")]
        [SerializeField] private GameObject pauseMenu;
        [SerializeField] private GameObject gameOverMenu;
        [SerializeField] private GameObject levelCompleteMenu;

        // 收集品预制体
        [SerializeField] private GameObject collectiblePrefab;

        // 游戏状态
        private bool isGameOver = false;
        private bool isLevelComplete = false;

        // 组件引用
        private SwapManager swapManager;
        private List<SwappableObject> swappableObjects = new List<SwappableObject>();
        private List<Vector3> originalPositions = new List<Vector3>();

        private void Awake()
        {
            // 单例模式设置
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 获取交换管理器
            swapManager = GetComponent<SwapManager>();
            if (swapManager == null)
            {
                swapManager = gameObject.AddComponent<SwapManager>();
            }
        }

        private void Start()
        {
            // 初始化UI
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverMenu != null) gameOverMenu.SetActive(false);
            if (levelCompleteMenu != null) levelCompleteMenu.SetActive(false);

            // 保存所有可交换物体的初始位置
            SaveOriginalPositions();
        }

        private void Update()
        {
            // 检测关卡完成
            CheckLevelCompletion();
        }

        // 保存所有可交换物体的初始位置
        private void SaveOriginalPositions()
        {
            swappableObjects.Clear();
            originalPositions.Clear();

            // 查找所有可交换物体
            SwappableObject[] objects = FindObjectsOfType<SwappableObject>();
            foreach (SwappableObject obj in objects)
            {
                swappableObjects.Add(obj);
                originalPositions.Add(obj.transform.position);
            }
        }

        // 重置所有可交换物体到初始位置
        private void ResetSwappableObjects()
        {
            for (int i = 0; i < swappableObjects.Count; i++)
            {
                if (swappableObjects[i] != null)
                {
                    swappableObjects[i].transform.position = originalPositions[i];
                    swappableObjects[i].ResetState();
                }
            }
        }

        // 重启当前关卡
        public void RestartLevel()
        {
            // 重置玩家位置
            if (respawnPoint != null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    player.transform.position = respawnPoint.position;
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.velocity = Vector2.zero;
                    }
                }
            }

            // 重置所有可交换物体
            ResetSwappableObjects();

            // 重置交换次数
            if (swapManager != null)
            {
                swapManager.ResetSwapCount();
            }

            // 重置游戏状态
            isGameOver = false;
            isLevelComplete = false;

            // 隐藏所有菜单
            if (pauseMenu != null) pauseMenu.SetActive(false);
            if (gameOverMenu != null) gameOverMenu.SetActive(false);
            if (levelCompleteMenu != null) levelCompleteMenu.SetActive(false);

            // 确保游戏时间正常运行
            Time.timeScale = 1f;
        }

        // 加载下一关
        public void LoadNextLevel()
        {
            if (!string.IsNullOrEmpty(nextLevelName))
            {
                SceneManager.LoadScene(nextLevelName);
            }
            else
            {
                // 如果没有指定下一关，重新加载当前关卡
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        // 游戏暂停时的回调
        public void OnGamePaused()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
        }

        // 游戏继续时的回调
        public void OnGameResumed()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
        }

        // 游戏结束
        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;

            if (gameOverMenu != null)
            {
                gameOverMenu.SetActive(true);
            }

            // 暂停游戏
            Time.timeScale = 0f;
        }

        // 检测关卡是否完成
        private void CheckLevelCompletion()
        {
            if (isLevelComplete) return;

            // 这里根据实际游戏逻辑检测关卡是否完成
            // 例如，检测玩家是否到达终点
            GameObject goal = GameObject.FindGameObjectWithTag("Goal");
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (goal != null && player != null)
            {
                // 简单的距离检测示例
                float distance = Vector2.Distance(goal.transform.position, player.transform.position);
                if (distance < 1f) // 到达终点的距离阈值
                {
                    LevelComplete();
                }
            }
        }

        // 完成关卡，可被LevelGoal调用
        public void CompleteLevel()
        {
            if (isLevelComplete) return;

            // 触发关卡完成逻辑
            LevelComplete();
        }

        // 关卡完成内部实现
        private void LevelComplete()
        {
            if (isLevelComplete) return;

            isLevelComplete = true;

            // 创建收集品
            SpawnCollectible();

            // 显示关卡完成菜单
            if (levelCompleteMenu != null)
            {
                levelCompleteMenu.SetActive(true);
            }

            // 暂停游戏
            Time.timeScale = 0f;
        }

        // 创建收集品
        private void SpawnCollectible()
        {
            // 获取当前交换次数
            int currentSwapCount = swapManager != null ? swapManager.GetSwapCount() : 0;

            // 在关卡终点位置创建收集品
            GameObject goal = GameObject.FindGameObjectWithTag("Goal");
            if (goal != null && collectiblePrefab != null)
            {
                GameObject collectible = Instantiate(collectiblePrefab, goal.transform.position, Quaternion.identity);

                // 设置收集品属性（如显示交换次数等）
                CollectibleItem item = collectible.GetComponent<CollectibleItem>();
                if (item != null)
                {
                    item.SetSwapCount(currentSwapCount, targetSwapCount);
                }
            }
        }

        // 获取当前关卡的目标交换次数
        public int GetTargetSwapCount()
        {
            return targetSwapCount;
        }

        // 获取当前交换次数
        public int GetCurrentSwapCount()
        {
            return swapManager != null ? swapManager.GetSwapCount() : 0;
        }
    }
}