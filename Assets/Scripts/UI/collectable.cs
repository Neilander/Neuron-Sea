using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LDtkUnity;
using UnityEngine.Events;

public class collectable : MonoBehaviour, ILDtkImportedFields
{
    [SerializeField]
    private int restrictedTime;

    private bool unlocked = true;
    [SerializeField] private SpriteRenderer renderer;

    [SerializeField] private Transform floatingTarget; // 设置为 renderer.transform，或单独物体

    [SerializeField] private float floatAmplitude = 0.1f; // 上下移动的幅度
    [SerializeField] private float floatSpeed = 1.5f;     // 上下移动的速度

    private Vector3 initialLocalPos;

    [SerializeField] private TextMeshPro DisplayText;

    public UnityEvent DestroyToDo;
    //自动导入关卡设定数据
    public void OnLDtkImportFields(LDtkFields fields)
    {
        restrictedTime = fields.GetInt("SwitchTimeRequire");
    }

    // Start is called before the first frame update
    void Start()
    {
        if (floatingTarget == null)
            floatingTarget = renderer.transform;

        initialLocalPos = floatingTarget.localPosition;
        StartCoroutine(FloatCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if (GridManager.Instance != null) UpdateState(GridManager.Instance.SwitchTime);
    }

    void UpdateState(int curSTime)
    {

        if (curSTime <= restrictedTime && !unlocked)
        {
            //可接触
            Color c = renderer.color;
            c.a = 1f;
            renderer.color = c;
            unlocked = true;
        }
        else if (unlocked && curSTime > restrictedTime)
        {

            //不可接触
            Color c = renderer.color;
            c.a = 0.5f;
            renderer.color = c;
            unlocked = false;
        }

        DisplayText.text = string.Format("{0}/{1}", curSTime, restrictedTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (unlocked && collision.GetComponent<PlayerController>())
        {
            GetCollected();
        }
    }

    void GetCollected()
    {
        CollectableManager.Instance.TryAddCollection(levelManager.instance.currentLevelIndex);
        AudioManager.Instance.Play(SFXClip.PickUpCollectable,gameObject.name);
        Destroy(gameObject);
        DestroyToDo.Invoke();
        if(CollectableManager.Instance.totalCollected == 1) {
            showDialogue(1,"*信息泡在我指尖消融，残破的图像直接浮现：我只能窥看无意义的一角。但这或许能够给我带来崭新的东西……我如此期待。");
        }
        showDialogue(4,1,"*我能看到更多的图景了，这些东西让我觉得熟悉。或许它们并不是崭新的……但我依然抱有期待。");
        showDialogue(8,1,"*我似乎已经能够猜测这份来自于前代首席观测员的礼物是什么了。");
        showDialogue(12,1,"*我不知道她是从什么角度拍摄下了这幅场景，事实上，我从未这样观察过我工作和生活的地方……我相信她爱着这个地方，但是这样的爱能够支撑她的选择吗？");
        showDialogue(4,13,"我没有想到在这个区域也有新的礼物。");
        showDialogue(8,13,"收集意识泡的感觉十分奇特……有时候我感觉我可以听见她的声音，是我的错觉吗？");
        showDialogue(12,13,"……我其实很好奇，她每次都是如何找到这样漂亮的角度的，有些人说爱是无法掩饰的，我对此不置可否。但她对这片海域的爱，的确无法掩饰。");
        showDialogue(4,25,"她真的是一个很喜欢拍照留念的人。");
        showDialogue(8,25,"我想你会比她走得更远……我会像保存她的一切那样，保存你的一切。");
        showDialogue(12,25,"你喜欢这份礼物吗？这是她的礼物，同样也是我的。");
        
    }
    //播过一次不再播放
    private void showDialogue(int collectNum, int levelGroup, string wordToDisplay){
        // 生成唯一ID用于记录此对话是否已播放
        string dialogueId = $"Collectable_Dialogue_{collectNum}_{levelGroup}";

        // 检查是否已播放过此对话
        if (StoryGlobalLoadManager.instance.IsTriggerDisabled(dialogueId)) {
            return; // 如果已播放过，直接返回
        }

        if (CollectableManager.Instance.collectedLevels.Count == collectNum &&
            levelManager.instance.currentLevelIndex >= levelGroup &&
            levelManager.instance.currentLevelIndex <= levelGroup + 11) {

            // 玩家头上显示一个面板
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) {
                Head dialogue = player.GetComponentInChildren<Head>();
                if (dialogue != null) {
                    dialogue.ShowDialogue(wordToDisplay);

                    // 标记此对话已播放
                    StoryGlobalLoadManager.instance.DisableTrigger(dialogueId);
                }
            }
        }
    }
    
    //播过一次不再播放，不限制关卡组
    private void showDialogue(int collectNum, string wordToDisplay){
        // 生成唯一ID用于记录此对话是否已播放
        string dialogueId = $"Collectable_Dialogue_{collectNum}_NoLevelGroup";

        // 检查是否已播放过此对话
        if (StoryGlobalLoadManager.instance.IsTriggerDisabled(dialogueId)) {
            return; // 如果已播放过，直接返回
        }

        if (CollectableManager.Instance.collectedLevels.Count == collectNum) {
            // 玩家头上显示一个面板
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) {
                Head dialogue = player.GetComponentInChildren<Head>();
                if (dialogue != null) {
                    dialogue.ShowDialogue(wordToDisplay);

                    // 标记此对话已播放
                    StoryGlobalLoadManager.instance.DisableTrigger(dialogueId);
                }
            }
        }
    }
    IEnumerator FloatCoroutine()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * floatSpeed;
            float offsetY = Mathf.Sin(timer) * floatAmplitude;
            Vector3 offset = new Vector3(0f, offsetY, 0f);
            floatingTarget.localPosition = initialLocalPos + offset;
            yield return null;
        }
    }
}
