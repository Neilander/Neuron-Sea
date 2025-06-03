using System;
using System.Collections.Generic;
using UnityEngine;


public class ReddotManager
{

    #region 常量

    internal const string ON_REDDOT_PATH_VALUE_CHANGE = "ON_REDDOT_PATH_VALUE_CHANGE";

    #endregion


    #region 字段

    private static List<string> reddotPath = null;

    #endregion

    #region 属性

    /// <summary>
    /// 当前红点系统中所有的红点路径
    /// </summary>
    public static List<string> ReddotPath
    {
        get
        {
            if (reddotPath == null)
                reddotPath = new List<string>();

            return reddotPath;
        } 
    }

    #endregion


    #region 事件

    /// <summary>
    /// 红点路径改变时触发
    /// </summary>
    public static Action onReddotPathChange;

    #endregion


    #region 方法

    [RuntimeInitializeOnLoadMethod]
    private static void InitUITipManager()
    {
        reddotPath = null;
    }


    /// <summary>
    /// 添加红点路径
    /// </summary>
    /// <param name="path"></param>
    public static void AddPath(string path)
    {

        if (string.IsNullOrEmpty(path)) {
            //Debug.Log("为空");
            return;
        }

        // 添加一个结尾符
        path = string.Format("{0}/", path);

        if (ReddotPath.Contains(path)) {
            //Debug.Log("已经有了");
            return; 
        }

        ReddotPath.Add(path);
        RefreshTipPath();
        onReddotPathChange?.Invoke(); 
    }



    /// <summary>
    /// 添加一组红点路径
    /// </summary>
    /// <param name="paths"></param>
    public static void AddPath(IList<string> paths)
    {
        if (paths == null || paths.Count == 0)
            return;

        foreach (string path in paths)
        {
            string p = string.Format("{0}/",path);
            if (ReddotPath.Contains(p))
                continue;
            ReddotPath.Add(p);
        }
         
        RefreshTipPath();
        onReddotPathChange?.Invoke();
    }


    /// <summary>
    /// 移除提示路径(内部使用,直接移除)
    /// </summary>
    /// <param name="path"></param>
    internal static void RemoveTipInternal(string path)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (!ReddotPath.Contains(path))
        {
            return;
        }

        ReddotPath.Remove(path);
        RefreshTipPath();
        onReddotPathChange?.Invoke();


    }

    /// <summary>
    /// 移除红点路径
    /// </summary>
    /// <param name="path"></param>
    public static void RemovePath(string path)
    {
        // 给路径添加一个结尾符(对应 AddTip)
        RemoveTipInternal(string.Format("{0}/", path));
    }

    /// <summary>
    /// 重置红点系统(清空所有提示)
    /// </summary>
    public static void Reset()
    {
        ReddotPath.Clear();
        RefreshTipPath();
    }

    /// <summary>
    /// 是否应该显示
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static bool ShouldBeDisplay(string path)
    {

        if (string.IsNullOrEmpty(path))
            return false;

        if (ReddotPath.Count == 0)
            return false;

        foreach (var item in ReddotPath)
        {
            if (item.StartsWith(path))
                return true;
        }


        return false;
    }

    private static void RefreshTipPath()
    { 
        // 触发事件 
        EventManager.TriggerEvent(ON_REDDOT_PATH_VALUE_CHANGE);
    }

    #endregion

}


/// <summary>
/// 轻量级全局事件管理器：
///     • 支持任意 string 作为事件名  
///     • 每个事件可以挂若干 <see cref="Action"/> 监听器  
///     • 提供 AddEvent / RemoveEvent / TriggerEvent 三个静态方法
/// </summary>
public static class EventManager
{
    /// <summary>事件表：key=事件名，value=委托链</summary>
    private static readonly Dictionary<string, Action> _eventTable =
        new Dictionary<string, Action>();

    /// <summary>
    /// 注册事件监听
    /// </summary>
    /// <param name="eventName">事件名称</param>
    /// <param name="listener">回调（无参）</param>
    public static void AddEvent(string eventName, Action listener){
        if (listener == null) return;

        if (_eventTable.TryGetValue(eventName, out var existing)) {
            // 已存在该事件 ⇒ 追加监听
            existing += listener;
            _eventTable[eventName] = existing;
        }
        else {
            // 新事件 ⇒ 建一个条目
            _eventTable[eventName] = listener;
        }
    }

    /// <summary>
    /// 移除事件监听
    /// </summary>
    public static void RemoveEvent(string eventName, Action listener){
        if (listener == null) return;

        if (_eventTable.TryGetValue(eventName, out var existing)) {
            existing -= listener;
            // 委托链为空就把这个事件清掉，防止字典膨胀
            if (existing == null)
                _eventTable.Remove(eventName);
            else
                _eventTable[eventName] = existing;
        }
    }

    /// <summary>
    /// 触发事件（同步 Invoke）
    /// </summary>
    public static void TriggerEvent(string eventName){
        if (_eventTable.TryGetValue(eventName, out var action)) {
            action?.Invoke();
        }
    }
}


