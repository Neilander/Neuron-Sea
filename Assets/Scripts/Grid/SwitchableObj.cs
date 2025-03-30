using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchableObj : MonoBehaviour
{
    [SerializeField] private GameObject anchor;

    [SerializeField] private float flashRedDuration = 0.5f;

    [SerializeField] private GameObject anchorSprite;


    private Vector3 selfGridPos;

    //switch相关变量
    [HideInInspector] public bool inSwitchState = false;

    private Vector3 dragOffset = Vector3.zero; // 拖动时的基础偏移

    private WorldMover mover;

    [SerializeField]private SpriteRenderer renderer;

    [SerializeField] private Vector2 ExpectedSize;

    public Vector3 SelfGridPos
    {
        get { return selfGridPos; }
    }

    private Vector3 recordTempPos;

    private void Start(){
        mover = GetComponent<WorldMover>();
        Vector3 _pos = GridManager.Instance.GetClosestGridPoint(anchor.transform.position);
        //renderer = GetComponentInChildren<SpriteRenderer>();
        selfGridPos = _pos;
        recordTempPos = renderer.transform.localPosition;
        anchorSprite.transform.localPosition = anchor.transform.localPosition;
        anchorSprite.transform.SetParent(renderer.transform);
    }

    public void MoveToGridPos(Vector3 gridPos){
        mover.MoveTo(gridPos - anchor.transform.localPosition);
        selfGridPos = gridPos;
    }

    
    public void SetTempToGridPos(Vector3 gridPos)
    {
        renderer.transform.position = gridPos - anchor.transform.localPosition + renderer.transform.localPosition;
    }

    public void SetToGridPos(Vector3 gridPos){
        transform.position = gridPos - anchor.transform.localPosition;
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
        SetAlpha(1f);
        SetToGridPos(gridPos);
    }


    private Vector3 tempRecordGridPos;
    public void IntoTempMoveState(Vector3 gridPos)
    {
        SetAlpha(0.5f);
        tempRecordGridPos = gridPos;
        SetTempToGridPos(gridPos);
    }

    public void OutTempMoveState()
    {
        SetAlpha(1f);
        renderer.transform.localPosition = recordTempPos;
    }

    public void ChangeFromTempMoveToNormal()
    {
        SetAlpha(1f);
        SetToGridPos(tempRecordGridPos);
        renderer.transform.localPosition = recordTempPos;
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

    private Coroutine flashCor;
    public void ControlFlash(bool ifStart){
        if (ifStart)
        {
            if (flashCor == null)
                flashCor = StartCoroutine(FlashRedCoroutine(flashRedDuration));
        }
        else
        {
            if(flashCor!=null)StopCoroutine(flashCor);
            flashCor = null;
            renderer.color = originalColor;
        }
    }

    private Color originalColor;
    private IEnumerator FlashRedCoroutine(float duration = 0.4f){
        originalColor = renderer.color;
        Color targetColor = Color.red;
        float halfDuration = duration / 2f;

        while (true)
        {
            float timer = 0f;

            // 渐变到红色
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                renderer.color = Color.Lerp(originalColor, targetColor, t);
                yield return null;
            }

            timer = 0f;

            // 渐变回原色
            while (timer < halfDuration)
            {
                timer += Time.deltaTime;
                float t = timer / halfDuration;
                renderer.color = Color.Lerp(targetColor, originalColor, t);
                yield return null;
            }

            renderer.color = originalColor;
        }
    }

    public void SizeToExpectedSize()
    {
        if (GridManager.Instance == null)
        {
            Debug.LogError("GridManager.Instance is null.");
            return;
        }

        if (renderer == null || renderer.sprite == null)
        {
            Debug.LogError("SpriteRenderer or Sprite is missing.");
            return;
        }

        if (ExpectedSize.x == 0 || ExpectedSize.y == 0)
        {
            Debug.LogError("Size不能是0");
            return;
        }
           
        float gridSize = GridManager.Instance.gridWidth;

        Vector2 targetWorldSize = ExpectedSize * gridSize;

        // 获取原始 sprite 世界单位大小
        Vector2 spriteSize = renderer.sprite.bounds.size;

        // 计算缩放比
        Vector3 scale = new Vector3(
            targetWorldSize.x / spriteSize.x,
            targetWorldSize.y / spriteSize.y,
            1f
        );

        // 应用缩放
        renderer.transform.localScale = scale;

        // Collider 处理
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            box.size = targetWorldSize;
            box.offset = Vector2.zero;
        }
        else
        {
            Debug.LogError("Only BoxCollider2D is supported.");
        }

        anchorSprite.transform.position = anchor.transform.position;
        //anchorSprite.transform.SetParent(renderer.transform);
    }
}