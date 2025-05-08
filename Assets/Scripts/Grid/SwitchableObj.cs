using LDtkUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

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

    [Header("材质")]
    [SerializeField] private Material ProjectionWhite;
    [SerializeField] private Material ProjectionRed;
    [SerializeField] private Material switchMaterial;
    [SerializeField] private Material lockedMaterial;
    [SerializeField] private Material defaultMaterial;

    [Header("是否允许交换")]
    [SerializeField] private bool IfBanSwitch_SetWhenStart;

    [Header("是否启用特殊的边界检测机制")]
    public bool IfSpecialEdgeChecker;
    [Header("这应该是一个collider")]
    public SpriteRenderer SpecialEdgeChecker;

    [Header("场景替换")]
    public bool IfDoSwitchBasedOnScene = false;
    public List<GameObject> switchPrefabs;

    [Header("是否替换preview参考物体")]
    public bool IfSubstituePreview = false;
    public SpriteRenderer substitueRenderer;
    public Collider2D substitueCollider;

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
        ExpectedAnchorPos.x = fields.GetInt("PivotX");
        ExpectedAnchorPos.y = fields.GetInt("PivotY");
        SizeToExpectedSize();
        foreach(INeilLDTkImportCompanion companion in GetComponents<INeilLDTkImportCompanion>())
        {
            companion.OnAfterImport(this, fields);
        }
    }

    int GetLastDigitOfSceneName()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Length == 0) return -1;

        char lastChar = sceneName[sceneName.Length - 1];

        if (char.IsDigit(lastChar))
        {
            return (int)char.GetNumericValue(lastChar);
        }

        return -1; // 最后一个不是数字
    }

    public void MimicLDtkImportWithNoCompanion(int x, int y, float anchorX, float anchorY)
    {
        ExpectedSize.x = x;
        ExpectedSize.y = y;
        ExpectedAnchorPos.x = anchorX;
        ExpectedAnchorPos.y = anchorY;
        //Debug.Log("在模仿时出错,xy是"+ ExpectedSize);
        SizeToExpectedSize();
        IfDoSwitchBasedOnScene = false;
    }

    private void Start(){
        if (IfDoSwitchBasedOnScene)
        {
            int sceneIndex = levelManager.instance.sceneIndex;
            if (sceneIndex <= 0 || sceneIndex > switchPrefabs.Count)
            {
                Debug.LogWarning(gameObject.name+":场景编号非法或 prefab 未设置");
                return;
            }

            GameObject prefab = switchPrefabs[sceneIndex - 1];
            if (prefab == null)
            {
                Debug.LogWarning("Prefab 丢失: " + (sceneIndex - 1));
                return;
            }
            //Debug.Log(gameObject.name + "的Parent是"+transform.parent.name+ "触发序列1");
            GameObject gmo = Instantiate(prefab, transform.parent);
            gmo.transform.position = transform.position;
            //Debug.Log("gmo parent is: " + gmo.transform.parent?.name);

            var sw = gmo.GetComponent<SwitchableObj>();
            if (sw == null)
            {
                Debug.LogWarning("Prefab 上缺少 SwitchableObj 组件");
                return;
            }
            //Debug.Log(gameObject.name+"当前的大小是"+ExpectedSize);
            sw.MimicLDtkImportWithNoCompanion(ExpectedSize.x, ExpectedSize.y, ExpectedAnchorPos.x, ExpectedAnchorPos.y);
            gameObject.SetActive(false);
        }
        else
        {
            //Debug.Log(gameObject.name + "在"+transform.parent.name);
            SetAnchorToAnchorPos();
            mover = GetComponent<WorldMover>();
            Vector3 _pos = GridManager.Instance.GetClosestGridPoint(anchor.transform.position);
            //renderer = GetComponentInChildren<SpriteRenderer>();
            selfGridPos = _pos;
            recordTempPos = renderer.transform.localPosition;
            anchorSprite.transform.localPosition = anchor.transform.localPosition;
            anchorSprite.transform.SetParent(renderer.transform);
            //previewObj.transform.localScale = renderer.gameObject.transform.localScale;
            if (IfBanSwitch_SetWhenStart)
                SwitchEnableSwitchState();

            defaultMaterial = renderer.material;
        }

        if (IfSpecialEdgeChecker)
            Debug.Log("我用特殊检查，我是"+gameObject.name);
    }

    public SpriteRenderer GetRenderer()
    {
        return renderer;
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
        transform.position = gridPos - anchor.transform.localPosition;
        selfGridPos = gridPos;
        if (ifInPreview)
        {
            Color c = Color.white;
            c.a = 0;
            previewObj.transform.position = recordPos - anchor.transform.localPosition +
                (ifAdjustY ? Vector3.up * adjustYAmount[ExpectedSize.x - 1] : Vector3.zero)+
                (IfSubstituePreview ? substitueRenderer.transform.parent.localPosition : Vector3.zero);
            previewObj.GetComponent<SpriteRenderer>().color = c;
            StartCoroutine(WhatCanISay(renderer.material));
            renderer.material = switchMaterial;
            renderer.material.SetFloat("_KaiShiShiJian", Time.unscaledTime);
            renderer.material.SetVector("_MoXingDaXiaoWangGeZuoBiao", (Vector2)ExpectedSize);
            renderer.material.SetVector("_MaoDianWangGeZuoBiao", ExpectedAnchorPos);
            renderer.material.SetVector("_MaoDianShiJieZuoBiao", recordPos);
            renderer.material.SetVector("_MuBiaoMaoDianShiJieZuoBiao", anchor.transform.position);
        }
    }

    IEnumerator WhatCanISay(Material originMaterial)
    {
        yield return new WaitForSecondsRealtime(GridManager.Instance.waitTime);
        previewObj.GetComponent<SpriteRenderer>().color = Color.white;
        yield return new WaitForSecondsRealtime(renderer.material.GetFloat("_ZongShiJian") - GridManager.Instance.waitTime);
        renderer.material = originMaterial;

    }

    public void SetToClosestGridPoint(){
        Vector3 _pos = GridManager.Instance.GetClosestGridPoint(anchor.transform.position);
        transform.position = _pos - anchor.transform.localPosition;
        selfGridPos = _pos;
    }

    private Bounds? lastCheckBounds = null;
    public bool CheckIfCanMoveTo(Vector3 gridPos, GameObject ignoreObject){
        Collider2D col = IfSubstituePreview? substitueCollider: GetComponent<Collider2D>();
        if (col == null) {
            Debug.LogWarning("缺少 Collider2D 组件！");
            return false;
        }

        Vector2 checkPosition = gridPos - anchor.transform.localPosition+ (IfSubstituePreview?substitueCollider.transform.localPosition:Vector3.zero);

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.SetLayerMask(Physics2D.DefaultRaycastLayers);

        Collider2D[] results = new Collider2D[20];
        int hitCount = 0;

        if (col is BoxCollider2D box) {
            Vector2 center = (Vector2)checkPosition + box.offset;
            Vector2 size = box.size;
            lastCheckBounds = new Bounds(center, size);
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
            //Debug.Log("第"+i+"个是"+results[i].gameObject.name);
            if (results[i] != null && results[i].gameObject != ignoreObject && results[i].gameObject != gameObject&& !results[i].transform.IsChildOf(ignoreObject.transform)) {
                Debug.Log("阻止我们的是" + results[i].gameObject.name);
                return false; // 有碰撞，且不是要忽略的物体
            }
        }

        return true; // 没有碰撞，可以移动
    }

    private void OnDrawGizmos()
    {
        if (lastCheckBounds.HasValue)
        {
            Gizmos.color = Color.cyan;
            Bounds b = lastCheckBounds.Value;
            Gizmos.DrawWireCube(b.center, b.size);
        }
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
        /*
        if (inSwitchState) {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z; // 保持 Z 不变（避免摄像机深度偏移）
            transform.position = mouseWorldPos + dragOffset;
        }*/

        
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

    public void ChangeExpectedSize(int x, int y)
    {
        ExpectedSize.x = x;
        ExpectedSize.y = y;
        SizeToExpectedSize();
    }

    public void SizeToExpectedSize()
    {
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

        if (lightTrans != null && lightTrans.gameObject.activeInHierarchy) lightTrans.localScale = new Vector3(ExpectedSize.x, ExpectedSize.y, 1);
        if (EnvironmentLight != null && EnvironmentLight.gameObject.activeInHierarchy)
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
            Debug.LogError("Only Box And Circle Collider2D is supported.");
        }
        if (selfAnimator != null)
        {
            selfAnimator.SetInteger("Size", (int)ExpectedSize.x);
        }

        SetAnchorToAnchorPos();
    }

    public void SetAnchorToAnchorPos()
    {
        if (anchor != null)
        {
            /*
            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager.Instance is null.");
            }
            */
            float gridSize = GridManager.Instance ? GridManager.Instance.gridWidth : 1f;

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
    private SpriteRenderer previewRenderer;

    /// <summary>
    /// 控制物体的显示，第一个控制锁定，第二个控制预览的合法，第三个控制预览显示
    /// </summary>
    /// <param name="ifLocked">控制是否锁定</param><param name="ifLegal">控制Preview是否为红</param><param name="ifPreview">控制是否显示预览</param>
    public void SetLockedToSwitch(bool ifLocked, bool ifLegal, bool ifPreview ,Vector3 gridPos)
    {
        /*
        lockedStateDisplay.GetComponent<SpriteRenderer>().color = ifLegal ? Color.white : Color.red;
        if(lockedStateDisplay!=null)lockedStateDisplay.SetActive(ifLocked);*/


        if (ifLocked)
        {
            renderer.material = lockedMaterial;
            
        }
        else
        {
            renderer.material = defaultMaterial;
        }

        if (ifPreview && ifLocked)
        {
            previewRenderer = previewObj.GetComponent<SpriteRenderer>();
            previewRenderer.sprite = IfSubstituePreview? substitueRenderer.sprite: renderer.sprite;

            if (ifLegal)
            {
                previewObj.GetComponent<SpriteRenderer>().material = ProjectionWhite;
            }
            else
            {
                previewObj.GetComponent<SpriteRenderer>().material = ProjectionRed;
            }
            previewObj.transform.position = gridPos - anchor.transform.localPosition +
                (ifAdjustY ?Vector3.up * adjustYAmount[ExpectedSize.x - 1]:Vector3.zero)+
                (IfSubstituePreview ? substitueRenderer.transform.parent.localPosition :Vector3.zero);
            Debug.Log("Preview移动到了"+previewObj.transform.position);
            previewObj.SetActive(true);
            ifInPreview = true;
        }
        else
        {
            previewObj.SetActive(false);
            ifInPreview = false;
        }

        /*
        if (ifLocked)
        {
            if (ifLegal && ifPreview)
            {
                previewObj.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
                previewObj.GetComponent<SpriteRenderer>().material = ProjectionWhite;
                previewObj.transform.position = gridPos - anchor.transform.localPosition + Vector3.up * adjustYAmount[ExpectedSize.x - 1];
                previewObj.SetActive(true);
                ifInPreview = true;
            }
            else if(!ifLegal)
            {
                previewObj.GetComponent<SpriteRenderer>().sprite = renderer.sprite;
                previewObj.GetComponent<SpriteRenderer>().material = ProjectionRed;
                previewObj.transform.position = gridPos - anchor.transform.localPosition + Vector3.up * adjustYAmount[ExpectedSize.x - 1];
                previewObj.SetActive(true);
                ifInPreview = true;
            }
            renderer.material = lockedMaterial;
        }
        else
        {
            previewObj.SetActive(false);
            ifInPreview = false;
            renderer.material = defaultMaterial;
        }*/

    }

    public bool IsSpriteVisibleOnScreen()
    {
        Camera cam = Camera.main;

        // 自动选择使用的碰撞器源（优先 SpecialEdgeChecker）
        GameObject source = IfSpecialEdgeChecker ? SpecialEdgeChecker.gameObject : gameObject;

        // 尝试获取 BoxCollider2D 或 CircleCollider2D
        Collider2D col = source.GetComponent<Collider2D>();

        if (col == null)
        {
            Debug.LogWarning("No Collider2D found on the source object.");
            return false;
        }


        Bounds bounds = col.bounds;

        // 获取四个角点（世界坐标）
        Vector3[] worldCorners = new Vector3[4];
        worldCorners[0] = new Vector3(bounds.min.x, bounds.min.y); // 左下
        worldCorners[1] = new Vector3(bounds.min.x, bounds.max.y); // 左上
        worldCorners[2] = new Vector3(bounds.max.x, bounds.min.y); // 右下
        worldCorners[3] = new Vector3(bounds.max.x, bounds.max.y); // 右上

        foreach (Vector3 corner in worldCorners)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(corner);

            // 只判断摄像机前方
            if (screenPos.z < 0) continue;

            if (screenPos.x >= 0 && screenPos.x <= Screen.width &&
                screenPos.y >= 0 && screenPos.y <= Screen.height)
            {
                return true; // 有一个角点在屏幕上
            }
        }

        return false; // 全部角点都不在屏幕范围
    }
    #endregion
}