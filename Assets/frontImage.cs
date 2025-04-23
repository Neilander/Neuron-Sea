using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class frontImage : MonoBehaviour
{
    public float offsetMultiplier = 50f; // 系数
    private Vector2 initialAnchorPos;
    private RectTransform rect;
    private Image image;

    public Vector3 adjustOffset;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        image = GetComponent<Image>();

        if (rect != null)
        {
            initialAnchorPos = rect.anchoredPosition;
        }

        if (image != null)
        {
            image.enabled = false; // 初始关闭
        }
    }

    private void Update()
    {
        GameObject level = GameObject.Find("Level_1");
        if (level == null)
        {
            if (image != null) image.enabled = false;
            return;
        }

        Transform entities = level.transform.Find("Entities");
        if (entities == null)
        {
            if (image != null) image.enabled = false;
            return;
        }

        Transform respawn = null;
        foreach (Transform child in entities)
        {
            if (child.name.StartsWith("Respawn"))
            {
                respawn = child;
                break;
            }
        }
        if (respawn == null)
        {
            if (image != null) image.enabled = false;
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            if (image != null) image.enabled = false;
            return;
        }

        Vector3 offset = cam.transform.position - respawn.position+ adjustOffset;

        if (image != null) image.enabled = true;
        if (rect != null)
        {
            rect.anchoredPosition = initialAnchorPos - (Vector2)(offset * offsetMultiplier);
        }
    }
}
