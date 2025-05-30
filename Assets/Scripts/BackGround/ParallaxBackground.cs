using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParallaxBackground : MonoBehaviour
{
    public ParallaxCamera parallaxCamera;
    List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();

    void Start()
    {
        if (parallaxCamera == null)
            parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();

        if (parallaxCamera != null)
        {
            parallaxCamera.onCameraTranslateX += MoveX;
            parallaxCamera.onCameraTranslateY += MoveY;
        }
            

        SetLayers();
    }

    void SetLayers()
    {
        parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer != null)
            {
                layer.name = "Layer-" + i;
                parallaxLayers.Add(layer);
            }
        }
    }

    void MoveX(float deltaX)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.MoveX(deltaX);
        }
    }

    void MoveY(float deltaY)
    {
        foreach (ParallaxLayer layer in parallaxLayers)
        {
            layer.MoveY(deltaY);
        }
    }

    private void OnDestroy()
    {
        if (parallaxCamera != null)
        {
            parallaxCamera.onCameraTranslateX -= MoveX;
            parallaxCamera.onCameraTranslateY -= MoveY;
        }
    }
}