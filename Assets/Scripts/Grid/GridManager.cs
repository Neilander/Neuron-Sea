using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{

    public static GridManager Instance;

    [Header("格子数据调整")]
    [SerializeField]
    private int gridWidth = 1;

    [SerializeField]
    [Range(0, 1)]
    private float offsetX = 0;
    [SerializeField]
    [Range(0, 1)]
    private float offsetY = 0;

    [SerializeField]
    private Vector2 displayAmount = new Vector2(10,10);

    [SerializeField]
    private bool displayInGizmos = true;
    [SerializeField]
    private GameObject displayCenter;


    //这部分是在编辑器中绘制网格
    private void OnDrawGizmos() {
        if(!displayInGizmos) return;
        if(displayCenter == null) return;

        // 设置网格线的颜色
        Gizmos.color = Color.red;
        
        // 获取displayCenter的位置
        Vector3 centerPos = displayCenter.transform.position;
        
        // 计算网格线的间距
        float gridSpacing = gridWidth;
        
        // 计算起始位置（向左和向下偏移5个格子）
        float startX = centerPos.x - (gridSpacing * displayAmount.x/2);
        float startY = centerPos.y - (gridSpacing * displayAmount.y/2);

        Vector3 displayCenterOffset = new Vector3(centerPos.x%gridWidth,centerPos.y%gridWidth,0);
        Vector3 inputOffset = new Vector3(offsetX,offsetY,0);
        
        // 绘制10x10的网格
        for(int i = 0; i <= displayAmount.x; i++) {
            // 绘制垂直线
            Vector3 startPos = new Vector3(startX + (gridSpacing * i), startY, 0)-displayCenterOffset+inputOffset;
            Vector3 endPos = new Vector3(startX + (gridSpacing * i), startY + (gridSpacing * displayAmount.y), 0)-displayCenterOffset+inputOffset;
            Gizmos.DrawLine(startPos, endPos);
        }

        for(int i = 0; i <= displayAmount.y; i++) 
        {
            // 绘制水平线
            Vector3 startPos = new Vector3(startX, startY + (gridSpacing * i), 0)-displayCenterOffset+inputOffset;
            Vector3 endPos = new Vector3(startX + (gridSpacing * displayAmount.x), startY + (gridSpacing * i), 0)-displayCenterOffset+inputOffset;
            Gizmos.DrawLine(startPos, endPos);

        }

    }

    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }else{
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
