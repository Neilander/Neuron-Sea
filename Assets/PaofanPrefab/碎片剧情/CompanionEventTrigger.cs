using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CompanionEventTrigger : MonoBehaviour
{
    public string dialogueText = "你好，我是跟随机器人！";
    public GameObject dialoguePrefab; // 拖入一个UI对话框Prefab
    public string companionAnimation = "robot_scan"; // 需要播放的动画名

    private bool triggered = false;

    public UnityEvent exitConsciousness;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            CompanionController companion = FindObjectOfType<CompanionController>();
            if (companion != null)
            {
                StartCoroutine(CompanionEventSequence(companion));
            }
        }
    }

    IEnumerator CompanionEventSequence(CompanionController companion)
    {
        // 1. 记录原始位置
        Vector3 originalPos = companion.transform.position;
        bool oldCanFollow = companion.canFollow;
        companion.canFollow = false;

        // 2. 移动到Trigger位置
        Vector3 targetPos = transform.position + new Vector3(0, 1f, 0); // 目标点可微调
        float speed = 30f;
        while (Vector3.Distance(companion.transform.position, targetPos) > 0.05f)
        {
            companion.transform.position = Vector3.MoveTowards(companion.transform.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        // 3. 播放动画
        if (companion.GetComponent<Animator>() != null)
        {
            Debug.Log("准备播放动画");
            companion.GetComponent<Animator>().Play("robot_scan");
            yield return null;
            Debug.Log("已调用Play");
        }


        // 4. 等待动画播放完
        Animator anim = companion.GetComponent<Animator>();
        // if (anim != null)
        // {
        //     yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName(companionAnimation));
        //     yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
        // }
        // else
        // {
        //     yield return new WaitForSeconds(1f);
        // }
        // //解析时间
        // yield return new WaitForSeconds(1f);


        if (anim != null) {
            float timer = 0f;

            while (timer < 3f) {
                anim.Play(companionAnimation, 0, 0f); // 从头播放
                yield return new WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f);
                timer += anim.GetCurrentAnimatorStateInfo(0).length;
            }
        }
        else {
            yield return new WaitForSeconds(3f);
        }
        
        
        // 5. 显示对话框
        GameObject dialogueObj = null;
        if (dialoguePrefab != null)
        {
            dialogueObj = Instantiate(dialoguePrefab, companion.transform.position + new Vector3(1.5f, 1.5f, 0), Quaternion.identity, GameObject.Find("CanvasOff").transform);
            dialogueObj.GetComponentInChildren<TMP_Text>().text = dialogueText;
        }
        anim.Play("robot_idle");
        // // 6. 返回原位置（带着对话框）
        // while (Vector3.Distance(companion.transform.position, originalPos+new Vector3(3f, 0, 0)) > 0.05f)
        // {
        //     companion.transform.position = Vector3.MoveTowards(companion.transform.position, originalPos + new Vector3(3f, 0, 0), speed * Time.deltaTime);
        //     if (dialogueObj != null)
        //         dialogueObj.transform.position = companion.transform.position + new Vector3(0f, 1f, 0);
        //     yield return null;
        // }


        if (dialogueObj != null){
            // dialogueObj.transform.position = companion.transform.position + new Vector3(-3f, 3f, 0);
            Vector3 worldOffset = new Vector3(0f, 3f, 0f); // 偏移在头顶
            Vector3 screenPos = Camera.main.WorldToScreenPoint(companion.transform.position + worldOffset);
            dialogueObj.transform.position = screenPos;}


        // float dialogueDuration = 3f;
        // float timer2 = 0f;
        //
        // while (timer2 < dialogueDuration) {
        //     if (dialogueObj != null) {
        //         Vector3 worldOffset = new Vector3(0f, 1.5f, 0f); // 偏移在头顶
        //         Vector3 screenPos = Camera.main.WorldToScreenPoint(companion.transform.position + worldOffset);
        //         dialogueObj.transform.position = screenPos;
        //     }
        //
        //     timer2 += Time.deltaTime;
        //     yield return null;
        // }
        // 7. 恢复CompanionController的canFollow
        companion.canFollow = oldCanFollow;
        
        // 8. 3秒后对话框消失
        yield return new WaitForSeconds(3f);
        if (dialogueObj != null)
            Destroy(dialogueObj);
        exitConsciousness.Invoke();
    }
}