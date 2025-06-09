using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Reddot : MonoBehaviour
{ 
    #region 字段

    [SerializeField]
    [Tooltip("路径")] 
    private string _path;

    #endregion

    #region 属性
    public string Path 
    {
        get => _path;
        set 
        {
            _path = value;
            _path = string.Format("{0}/",_path); // 添加一个结尾符
            Refresh();
        } 
    }

    #endregion

    #region 生命周期

    private void Awake()
    {
        if(!_path.EndsWith("/"))
            _path = string.Format("{0}/", _path); // 添加一个结尾符
    }

    private void OnEnable()
    {
        Refresh(); 
        EventManager.AddEvent(ReddotManager.ON_REDDOT_PATH_VALUE_CHANGE, Refresh);
    }

    private void OnDisable()
    { 
        EventManager.RemoveEvent(ReddotManager.ON_REDDOT_PATH_VALUE_CHANGE, Refresh);
    }

    #endregion

    #region 方法

    private void Refresh() 
    { 
        bool show = ReddotManager.ShouldBeDisplay(Path);
        SetChildActive(show);
    }

    /// <summary>
    /// 移除红点
    /// </summary>
    public void Remove() 
    {
        ReddotManager.RemoveTipInternal(Path);
        Refresh();
    }

    private void SetChildActive(bool active) 
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if(child == null) continue;
            child.gameObject.SetActive(active);
        }
    }


    #endregion

}
