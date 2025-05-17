using System;
using System.Collections;
using UnityEngine;

// 负责监听收集事件并触发相关剧情
public class CollectableStoryManager : MonoBehaviour
{
    public static CollectableStoryManager Instance;

    [Header("剧情资源")]
    [SerializeField] private string firstCollectableStoryPath = "StoryData/FirstCollectableStory";
    // [SerializeField] private string thirdCollectableStoryPath = "StoryData/ThirdCollectableStory";
    // 可以添加更多剧情路径...

    // 已触发剧情的Key前缀
    private const string StoryPlayedKeyPrefix = "CollectableStory_";

    // 当前剧情触发器引用
    private GameObject currentStoryTriggerObj;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 订阅CollectableManager的事件
        SubscribeToCollectableEvents();
    }

    private void SubscribeToCollectableEvents()
    {
        if (CollectableManager.Instance != null)
        {
            // 使用自定义事件委托
            CollectableEvents.onCollectableAdded += OnCollectableAdded;
        }
    }

    private void OnDestroy()
    {
        CollectableEvents.onCollectableAdded -= OnCollectableAdded;
    }

    private void OnCollectableAdded(int levelIndex, int totalCount)
    {
        // 根据不同的收集数量触发不同的剧情
        switch (totalCount)
        {
            case 1:
                if (!IsStoryPlayed("First"))
                    StartCoroutine(TriggerStory("First", firstCollectableStoryPath));
                break;
            // case 3:
            //     if (!IsStoryPlayed("Third"))
            //         StartCoroutine(TriggerStory("Third", thirdCollectableStoryPath));
            //     break;
            // 可以添加更多判断...
        }
    }

    private bool IsStoryPlayed(string storyKey)
    {
        return PlayerPrefs.GetInt(StoryPlayedKeyPrefix + storyKey, 0) == 1;
    }

    private void MarkStoryAsPlayed(string storyKey)
    {
        PlayerPrefs.SetInt(StoryPlayedKeyPrefix + storyKey, 1);
        PlayerPrefs.Save();
    }

    private IEnumerator TriggerStory(string storyKey, string storyResourcePath)
    {
        // 标记为已播放
        MarkStoryAsPlayed(storyKey);
        
        // 等待一帧
        yield return null;
        
        // 创建临时剧情触发器
        GameObject triggerObj = new GameObject($"{storyKey}CollectableTrigger");
        
        // 获取玩家位置
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            triggerObj.transform.position = player.transform.position;
        }
        
        // 添加触发器组件
        StoryTrigger trigger = triggerObj.AddComponent<StoryTrigger>();
        
        // 设置剧情资源路径和类型
        trigger.SetStoryResourcePath(storyResourcePath);
        // trigger.SetTriggerId(storyKey);
        trigger.SetStorySourceType(StoryTrigger.StorySourceType.CSVResource);
        // 存储当前触发器引用
        currentStoryTriggerObj = triggerObj;
        
        // 订阅剧情结束事件
        StoryManager.Instance.onDialogueComplete += OnStoryComplete;
        
        // 触发剧情
        trigger.ForceStartStory();
    }

    private void OnStoryComplete()
    {
        // 取消订阅
        StoryManager.Instance.onDialogueComplete -= OnStoryComplete;
        
        // 销毁临时触发器
        if (currentStoryTriggerObj != null)
        {
            Destroy(currentStoryTriggerObj);
            currentStoryTriggerObj = null;
        }
    }
}
// 需要添加到CollectableManager.cs中的事件系统
public static class CollectableEvents
{
    // 收集物品时触发的事件
    public static Action<int, int> onCollectableAdded;
}