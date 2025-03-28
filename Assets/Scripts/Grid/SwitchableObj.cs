using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchableObj : MonoBehaviour
{
    [SerializeField] private GameObject anchor;

    [SerializeField] private float flashRedDuration = 0.5f;


    private Vector3 selfGridPos;

    //switch相关变量
    [HideInInspector] public bool inSwitchState = false;

    private Vector3 dragOffset = Vector3.zero; // 拖动时的基础偏移

    private WorldMover mover;

    private SpriteRenderer renderer;

    public Vector3 SelfGridPos
    {
        get { return selfGridPos; }
    }

    private void Start(){
        mover = GetComponent<WorldMover>();
        Vector3 _pos = GridManager.Instance.GetClosestGridPoint(anchor.transform.position);
        renderer = GetComponentInChildren<SpriteRenderer>();
        selfGridPos = _pos;
    }

    public void MoveToGridPos(Vector3 gridPos){
        mover.MoveTo(gridPos - anchor.transform.localPosition);
        selfGridPos = gridPos;
    }

    public void SetToClosestGridPoint(){
        Vector3 _pos = GridManager.Instance.GetClosestGridPoint(anchor.transform.position);
        transform.position = _pos - anchor.transform.localPosition;
        selfGridPos = _pos;
    }

    public bool CheckIfCanMoveTo(Vector3 gridPos, GameObject ignoreObject){
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) {
            Debug.LogWarning("缺少 Collider2D 组件！");
            return false;
        }

        Vector2 checkPosition = gridPos - anchor.transform.localPosition;

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.DefaultRaycastLayers);

        Collider2D[] results = new Collider2D[20];
        int hitCount = 0;

        if (col is BoxCollider2D box) {
            hitCount = Physics2D.OverlapBox(
                (Vector2)checkPosition + box.offset,
                box.size,
                box.transform.eulerAngles.z,
                filter,
                results
            );
        }
        else if (col is CircleCollider2D circle) {
            hitCount = Physics2D.OverlapCircle(
                (Vector2)checkPosition + circle.offset,
                circle.radius,
                filter,
                results
            );
        }
        else if (col is CapsuleCollider2D capsule) {
            hitCount = Physics2D.OverlapCapsule(
                (Vector2)checkPosition + capsule.offset,
                capsule.size,
                capsule.direction,
                capsule.transform.eulerAngles.z,
                filter,
                results
            );
        }
        else {
            Debug.LogWarning("不支持的 Collider2D 类型：" + col.GetType().Name);
            return false;
        }

        for (int i = 0; i < hitCount; i++) {
            if (results[i] != null && results[i].gameObject != ignoreObject && results[i].gameObject != gameObject) {
                return false; // 有碰撞，且不是要忽略的物体
            }
        }

        return true; // 没有碰撞，可以移动
    }

    public void IntoSwitchState(){
        Debug.Log("IntoSwitchState");
        inSwitchState = true;
        dragOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset.z = 0;
        SetAlpha(0.5f);
    }

    public void OutSwitchState(){
        Debug.Log("OutSwitchState");
        inSwitchState = false;
        SetAlpha(1f);
        transform.position = selfGridPos - anchor.transform.localPosition;
    }

    public void OutSwitchState(Vector3 gridPos){
        Debug.Log("OutSwitchState");
        inSwitchState = false;
        MoveToGridPos(gridPos);
    }

    private void Update(){
        if (inSwitchState) {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z; // 保持 Z 不变（避免摄像机深度偏移）
            transform.position = mouseWorldPos + dragOffset;
        }
    }

    public void SetAlpha(float alpha){
        Color c = renderer.color;
        c.a = alpha;
        renderer.color = c;
    }

    public void BackAlpha(){
        Color c = renderer.color;
        c.a = 1f;
        renderer.color = c;
        GridManager.Instance.CountDown();
    }

    public void FlashRed(){
        StartCoroutine(FlashRedCoroutine(flashRedDuration));
    }

    private IEnumerator FlashRedCoroutine(float duration = 0.4f){
        Color originalColor = renderer.color;
        Color targetColor = Color.red;

        float halfDuration = duration / 2f;
        float timer = 0f;

        // 渐变到红色
        while (timer < halfDuration) {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            renderer.color = Color.Lerp(originalColor, targetColor, t);
            yield return null;
        }

        timer = 0f;

        // 渐变回原色
        while (timer < halfDuration) {
            timer += Time.deltaTime;
            float t = timer / halfDuration;
            renderer.color = Color.Lerp(targetColor, originalColor, t);
            yield return null;
        }

        renderer.color = originalColor; // 确保最终还原
        GridManager.Instance.CountDown();
    }
}