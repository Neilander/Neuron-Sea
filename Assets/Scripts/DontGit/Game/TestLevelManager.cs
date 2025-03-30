using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Game
{
    public class TestLevelManager : MonoBehaviour
    {
        [Header("玩家设置")]
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private GameObject playerPrefab;

        [Header("UI引用")]
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text instructionText;
        [SerializeField] private Text swapCountText;
        [SerializeField] private GameObject victoryPanel;

        [Header("关卡设置")]
        [SerializeField] private string levelName = "测试关卡";
        [SerializeField] private string levelInstruction = "移动到终点，收集所有交换点";
        [SerializeField] private int maxSwapCount = 5;

        // 游戏状态
        private bool isLevelComplete = false;
        private int currentSwapCount = 0;
        // private PlayerController2 player;
        private SwapManager swapManager;
        private List<CollectibleItem> collectibles = new List<CollectibleItem>();
        private LevelGoal levelGoal;

        void Awake()
        {
            // 获取交换管理器
            swapManager = FindObjectOfType<SwapManager>();

            // 获取所有收集物
            CollectibleItem[] items = FindObjectsOfType<CollectibleItem>();
            collectibles.AddRange(items);

            // 获取关卡目标
            levelGoal = FindObjectOfType<LevelGoal>();

            // 隐藏胜利面板
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(false);
            }
        }

        void Start()
        {
            // 初始化玩家
            SpawnPlayer();

            // 初始化UI
            InitializeUI();

            // 设置交换次数
            if (swapManager != null)
            {
                swapManager.SetMaxSwaps(maxSwapCount);
                swapManager.OnSwapPerformed += HandleSwapPerformed;
            }

            // 设置关卡目标
            if (levelGoal != null)
            {
                levelGoal.OnPlayerReachedGoal += HandleLevelComplete;
            }
        }

        void Update()
        {
            // 检查重启关卡输入
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartLevel();
            }

            // 更新UI
            UpdateUI();
        }

        void SpawnPlayer()
        {
            // 如果没有指定出生点，使用当前位置
            if (playerSpawnPoint == null)
            {
                playerSpawnPoint = transform;
            }

            // 实例化玩家或查找现有玩家
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject == null && playerPrefab != null)
            {
                playerObject = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
            }

            if (playerObject != null)
            {
                // player = playerObject.GetComponent<PlayerController2>();
            }
        }

        void InitializeUI()
        {
            // 设置关卡名称
            if (levelNameText != null)
            {
                levelNameText.text = levelName;
            }

            // 设置关卡说明
            if (instructionText != null)
            {
                instructionText.text = levelInstruction;
            }

            // 更新交换次数UI
            UpdateSwapCountUI();
        }

        void UpdateUI()
        {
            // 更新交换次数UI
            UpdateSwapCountUI();
        }

        void UpdateSwapCountUI()
        {
            if (swapCountText != null && swapManager != null)
            {
                swapCountText.text = $"交换次数: {swapManager.GetCurrentSwaps()} / {maxSwapCount}";
            }
        }

        void HandleSwapPerformed()
        {
            // 增加交换次数
            currentSwapCount++;

            // 检查是否还有剩余交换次数
            if (currentSwapCount >= maxSwapCount && !isLevelComplete)
            {
                // 如果没有剩余交换次数并且关卡未完成，可以选择自动重启关卡或显示失败UI
                // 这里简单地重启关卡
                StartCoroutine(RestartWithDelay(1.5f));
            }
        }

        void HandleLevelComplete()
        {
            if (isLevelComplete) return;

            // 设置关卡为已完成
            isLevelComplete = true;

            // 停止玩家移动
            // if (player != null)
            // {
            //     player.enabled = false;
            // }

            // 显示胜利界面
            if (victoryPanel != null)
            {
                victoryPanel.SetActive(true);
            }

            // 播放胜利音效（如果有）
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.enabled)
            {
                audioSource.Play();
            }
        }

        public void RestartLevel()
        {
            // 重新加载当前场景
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }

        public void LoadNextLevel()
        {
            // 获取当前场景索引
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;

            // 检查下一个场景是否存在
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                // 加载下一个场景
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                // 如果没有下一个场景，返回到主菜单或第一个场景
                SceneManager.LoadScene(0);
            }
        }

        IEnumerator RestartWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            RestartLevel();
        }

        // 检查所有收集物是否已收集
        public bool AreAllCollectiblesCollected()
        {
            foreach (CollectibleItem item in collectibles)
            {
                if (item != null && !item.IsCollected())
                {
                    return false;
                }
            }
            return true;
        }

        // 获取玩家
        // public PlayerController2 GetPlayer()
        // {
        //     return player;
        // }

        // 获取交换管理器
        public SwapManager GetSwapManager()
        {
            return swapManager;
        }

        // 用于检查是否可以完成关卡
        public bool CanCompleteLevel()
        {
            // 检查是否所有收集物都已收集
            return AreAllCollectiblesCollected();
        }
    }
}