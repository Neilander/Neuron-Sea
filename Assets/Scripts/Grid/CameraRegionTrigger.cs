using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LDtkUnity;

public class CameraRegionTrigger : MonoBehaviour, ILDtkImportedFields
{
    public bool useHorizontalLimit = true;
    public bool useVerticalLimit = true;
    // 新增：进入区域时是否忽略所有边界
    public bool ignoreAllLimit = false;
    // 新增：进入区域时是否只忽略左右边界
    public bool ignoreHorizontalOnly = false;
    public float priority = 0;
    public bool ifStory;
    public float extraOffset;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        //Debug.Log($"[CameraRegionTrigger] 玩家进入触发器: {gameObject.name}");

        var camControl = Camera.main.GetComponent<CameraControl>();
        if (camControl == null)
        {
            Debug.LogWarning("[CameraRegionTrigger] 未找到 CameraControl 组件");
            return;
        }

        // 调试：显示当前边界设置
        //Debug.Log($"[CameraRegionTrigger] ignoreAllLimit={ignoreAllLimit}, ignoreHorizontalOnly={ignoreHorizontalOnly}, useHorizontalLimit={useHorizontalLimit}, useVerticalLimit={useVerticalLimit}");

        // // 新增逻辑：根据设置调用摄像机的忽略方法
        // if (ignoreAllLimit)
        // {
        //     camControl.IgnoreCameraLimit();
        //     Debug.Log("[CameraRegionTrigger] 忽略所有边界");
        //     return; // 直接返回，不设置区域
        // }
        // else if (ignoreHorizontalOnly)
        // {
        //     camControl.IgnoreHorizontalLimit();
        //     Debug.Log("[CameraRegionTrigger] 忽略左右边界");
        //     // 继续设置Y轴限制
        // }
        // else
        // {
        //     camControl.RestoreCameraLimit();
        //     camControl.RestoreHorizontalLimit();
        //     Debug.Log("[CameraRegionTrigger] 恢复所有边界");
        // }

        Bounds bounds = GetComponent<Collider2D>().bounds;

        float? left = useHorizontalLimit ? bounds.min.x : null;
        float? right = useHorizontalLimit ? bounds.max.x : null;
        float? top = useVerticalLimit ? bounds.max.y : null;
        float? bottom = useVerticalLimit ? bounds.min.y : null;

        //Debug.Log($"[CameraRegionTrigger] 设置边界: left={left}, right={right}, top={top}, bottom={bottom}");

        var region = new CameraLimitRegion(left, right, top, bottom, this, extraOffset);
        camControl.SetLimitRegion(region);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        CameraControl camControl = null;
        if (Camera.main != null)
        {
            camControl = Camera.main.GetComponent<CameraControl>();
        }
        if (camControl == null)
        {
            Debug.LogWarning("[CameraRegionTrigger] 退出时未找到 CameraControl 组件");
            return;
        }

        //Debug.Log($"[CameraRegionTrigger] 玩家离开触发器: {gameObject.name}");
        // 离开时恢复边界
        camControl.RestoreCameraLimit();
        camControl.RestoreHorizontalLimit();
        camControl.ClearLimitRegion(this);
    }

    //     // 新增：外部可调用，忽略所有边界
    public void IgnoreCameraLimit()
    {
        var camControl = Camera.main.GetComponent<CameraControl>();
        if (camControl == null)
        {
            Debug.LogWarning("[CameraRegionTrigger] IgnoreCameraLimit 未找到 CameraControl 组件");
            return;
        }
        camControl.ClearLimitRegion(this);
        // this.gameObject.SetActive(false);
        //Debug.Log($"[CameraRegionTrigger] 外部调用忽略所有边界: {gameObject.name}");
    }
    //
    // // 新增：外部可调用，忽略左右边界
    // public void IgnoreHorizontalLimit()
    // {
    //     var camControl = Camera.main.GetComponent<CameraControl>();
    //     if (camControl == null) return;
    //     camControl.IgnoreHorizontalLimit();
    // }
    //
    // // 新增：外部可调用，恢复所有边界
    public void RestoreCameraLimit()
    {
        // this.gameObject.SetActive(true);
        //     ignoreAllLimit = false;
        var camControl = Camera.main.GetComponent<CameraControl>();
        if (camControl == null)
        {
            Debug.LogWarning("[CameraRegionTrigger] RestoreCameraLimit 未找到 CameraControl 组件");
            return;
        }
        camControl.RestoreCameraLimit();
        camControl.RestoreHorizontalLimit();
    //     Bounds bounds = GetComponent<Collider2D>().bounds;
    //
    //     float? left = useHorizontalLimit ? bounds.min.x : null;
    //     float? right = useHorizontalLimit ? bounds.max.x : null;
    //     float? top = useVerticalLimit ? bounds.max.y : null;
    //     float? bottom = useVerticalLimit ? bounds.min.y : null;
    //
    //     var region = new CameraLimitRegion(left, right, top, bottom, this);
    //     camControl.SetLimitRegion(region);
    //     Debug.Log($"[CameraRegionTrigger] 外部调用恢复边界: {gameObject.name}");
    }

    public void OnLDtkImportFields(LDtkFields fields)
    {
        priority = fields.GetFloat("Priority");
        ifStory = fields.GetBool("IsStory");
        if (ifStory)
            gameObject.SetActive(false);
        extraOffset = fields.GetFloat("ExtraYOffest");
    }
}
