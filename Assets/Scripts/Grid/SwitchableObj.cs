using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering.Universal;

public class SwitchableObj : MonoBehaviour, ILDtkImportedFields
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

    [SerializeField] private Vector2Int ExpectedSize;
    [SerializeField] private Vector2 ExpectedAnchorPos;

    [Header("重构后用到的变量")]
    [SerializeField] private GameObject lockedStateDisplay;
    [SerializeField] private GameObject previewObj;

    [Header("动画器")]
    [SerializeField] private Animator selfAnimator;

    [Header("Y调整")]
    [SerializeField] private bool ifAdjustY = true;
    [SerializeField] private List<float> adjustYAmount;

    [Header("光照调整")]
    [SerializeField] private Transform lightTrans;

    [Header("点光源与大小对应")]
    [SerializeField] private Light2D EnvironmentLight;
    [SerializeField] private List<float> minRangeList;
    [SerializeField] private List<float> maxRangeList;

    [Header("预览材质")]
    [SerializeField] private Material ProjectionWhite;
    [SerializeField] private Material ProjectionRed;

    public Vector3 SelfGridPos
    {
        get { return selfGridPos; }
    }

    private Vector3 recordTempPos;

    //自动导入关卡设定数据
    public void OnLDtkImportFields(LDtkFields fields)
    {
        ExpectedSize.x = fields.GetInt("SizeX");
        ExpectedSize.y = fields.GetInt("SizeY");
        SizeToExpectedSize();
        ExpectedAnchorPos.x = fields.GetInt("PivotX");
        ExpectedAnchorPos.y = fields.GetInt("PivotY");
        SetAnchorToAnchorPos();
    }

    private void Start(){
        SetAnchorToAnchorPos();
        mover = GetComponent<WorldMover>();
        Vector3 _pos = GridManager.Instance.GetClosestGridPoint(anchor.transform.position);
        //renderer = GetComponentInChildren<SpriteRenderer>();
        selfGridPos = _pos;
        recordTempPos = renderer.transform.localPosition;
        anchorSprite.transform.localPosition = anchor.transform.localPosition;
        anchorSprite.transform.SetParent(renderer.transform);
        //previewObj.transform.localScale = renderer.gameObject.transform.localScale;
        
        
    }

    private void OnEnable()
    {
        if (selfAnimator != null)
        {
            selfAnimator.SetInteger("Size", (int)ExpectedSize.x);
        }
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

        Vector3 recordPos = selfGridPos;
        if (ifInPreview)
        {
            previewObj.transform.position = selfGridPos - anchor.transform.localPosition + Vector3.up * adjustYAmount[ExpectedSize.x-1];
        }
        transform.position = gridPos - anchor.transform.localPosition;
        selfGridPos = gridPos;
        if (ifInPreview)
        {
            previewObj.transform.position = recordPos - anchor.transform.localPosition + Vector3.up * adjustYAmount[ExpectedSize.x - 1];
        }
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
        GetComponent<Collider2D>().isTrigger = true;
        dragOffset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        dragOffset.z = 0;
        SetAlpha(0.5f);
    }

    public void OutSwitchState(){
        Debug.Log("OutSwitchState");
        inSwitchState = false;
        SetAlpha(1f);
        GetComponent<Collider2D>().isTrigger = false;
        transform.position = selfGridPos - anchor.transform.localPosition;
    }

    public void OutSwitchState(Vector3 gridPos){
        Debug.Log("OutSwitchState");
        inSwitchState = false;
        SetAlpha(1f);
        GetComponent<Collider2D>().isTrigger = false;
        SetToGridPos(gridPos);
    }


    private Vector3 tempRecordGridPos;
    public void IntoTempMoveState(Vector3 gridPos)
    {
        SetAlpha(0.5f);
        tempRecordGridPos = gridPos;
        GetComponent<Collider2D>().isTrigger = true;
        SetTempToGridPos(gridPos);
    }

    public void OutTempMoveState()
    {
        SetAlpha(1f);
        GetComponent<Collider2D>().isTrigger =false;
        renderer.transform.localPosition = recordTempPos;
    }

    public void ChangeFromTempMoveToNormal()
    {
        SetAlpha(1f);
        SetToGridPos(tempRecordGridPos);
        GetComponent<Collider2D>().isTrigger = false;
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

        if (ExpectedSize.x == 0 || ExpectedSize.y == 0)
        {
            Debug.LogError("Size不能是0");
            return;
        }

        // 直接将物体的缩放设置为 ExpectedSize（1 = 1 unit）
        //renderer.transform.localScale = new Vector3(ExpectedSize.x, ExpectedSize.y, 1f);

        if (ifAdjustY && adjustYAmount.Count>=ExpectedSize.x)
        {
            Debug.Log("调整y的上下");
            renderer.transform.position += Vector3.up * adjustYAmount[ExpectedSize.x - 1];
        }
        var pRenderer = previewObj.GetComponent<SpriteRenderer>();
        if (pRenderer == null || pRenderer.sprite == null)
        {
            Debug.LogError("缺少 SpriteRenderer 或 Sprite");
            return;
        }

        if (lightTrans != null) lightTrans.localScale = new Vector3(ExpectedSize.x, ExpectedSize.y, 1);
        if (EnvironmentLight != null)
        {
            EnvironmentLight.pointLightInnerRadius = minRangeList[ExpectedSize.x-1];
            EnvironmentLight.pointLightOuterRadius = maxRangeList[ExpectedSize.x-1];
        }

        // sprite 的原始世界尺寸（不考虑缩放）
        Vector2 spriteSize = pRenderer.sprite.bounds.size;

        // 计算缩放因子
        Vector3 scale = new Vector3(
            ExpectedSize.x / spriteSize.x,
            ExpectedSize.y / spriteSize.y,
            1f
        );

        //previewObj.transform.localScale = scale;

        // 让 BoxCollider 的大小和 ExpectedSize 保持一致
        Collider2D col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Debug.Log(gameObject.name+"正在适配碰撞体，大小是"+ExpectedSize);
            box.size = ExpectedSize - Vector2.one * 0.041f;
            box.offset = Vector2.zero;
        }
        else if (col is CircleCollider2D circle)
        {
            Debug.Log(gameObject.name + "正在适配碰撞体，大小是" + ExpectedSize);
            circle.radius = (ExpectedSize.x - 0.041f) * 0.5f;
            circle.offset = Vector2.zero;
        }
        else
        {
            Debug.LogError("Only BoxCollider2D is supported.");
        }

        SetAnchorToAnchorPos();

        
    }

    public void SetAnchorToAnchorPos()
    {
        if (anchor != null)
        {
            float gridSize = GridManager.Instance.gridWidth;

            // 1. 计算世界单位下的 ExpectedSize 大小
            Vector3 worldSize = new Vector3(ExpectedSize.x * gridSize, ExpectedSize.y * gridSize, 0f);

            // 2. 当前物体中心点为原点，计算左下角（中心减去一半尺寸）
            Vector3 worldOrigin = transform.position - worldSize * 0.5f;

            // 3. 计算偏移位置
            Vector3 worldOffset = new Vector3(ExpectedAnchorPos.x * gridSize, ExpectedAnchorPos.y * gridSize, 0f);

            // 4. 最终锚点位置 = 左下角 + 偏移
            anchor.transform.position = worldOrigin + worldOffset;
        }

        // anchorSprite 跟随 anchor
        if (anchorSprite != null)
        {
            anchorSprite.transform.position = anchor.transform.position;
        }
    }

    private bool ifEnableSwitch = true;
    public void SwitchEnableSwitchState()
    {
        if (ifEnableSwitch)
        {
            ifEnableSwitch = false;
            anchorSprite.SetActive(false);
            GridManager.Instance.ReleaseSelection(this);
            previewObj.SetActive(false);
        }
        else
        {
            ifEnableSwitch = true;
            anchorSprite.SetActive(true);
        }
    }

    public bool IfCanSwitch() { return ifEnableSwitch; }


    #region 重构后Switch代码
    private bool ifInPreview = false;
    public void SetLockedToSwitch(bool ifLocked, bool ifLegal, bool ifPreview ,Vector3 gridPos)
    {
        lockedStateDisplay.GetComponent<SpriteRenderer>().color = ifLegal ? Color.white : Color.red;
        if(lockedStateDisplay!=null)lockedStateDisplay.SetActive(ifLocked);

        
        if (ifLocked && ifLegal && ifPreview)
        {
            previewObj.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
            previewObj.GetComponent<SpriteRenderer>().material = ProjectionWhite;
            previewObj.transform.position = gridPos - anchor.transform.localPosition + Vector3.up * adjustYAmount[ExpectedSize.x - 1];
            previewObj.SetActive(true);
            ifInPreview = true;
        }
        else if (ifLocked && !ifLegal)
        {
            previewObj.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
            previewObj.GetComponent<SpriteRenderer>().material = ProjectionRed;
            previewObj.transform.position = gridPos - anchor.transform.localPosition + Vector3.up * adjustYAmount[ExpectedSize.x - 1];
            previewObj.SetActive(true);
            ifInPreview = false;
        }
        else
        {
            previewObj.SetActive(false);
            ifInPreview = false;
        }
        
    }
    #endregion
}