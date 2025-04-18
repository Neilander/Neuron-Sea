using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRegionTrigger : MonoBehaviour
{
    public bool useHorizontalLimit = true;
    public bool useVerticalLimit = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Debug.Log("设置！");

        var camControl = Camera.main.GetComponent<CameraControl>();
        if (camControl == null) return;

        Bounds bounds = GetComponent<Collider2D>().bounds;

        float? left = useHorizontalLimit ? bounds.min.x : null;
        float? right = useHorizontalLimit ? bounds.max.x : null;
        float? top = useVerticalLimit ? bounds.max.y : null;
        float? bottom = useVerticalLimit ? bounds.min.y : null;

        var region = new CameraLimitRegion(left, right, top, bottom, this);
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
            
        if (camControl == null) return;

        camControl.ClearLimitRegion(this);
    }
}
