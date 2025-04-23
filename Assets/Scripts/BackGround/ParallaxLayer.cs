using UnityEngine;

[ExecuteInEditMode]
public class ParallaxLayer : MonoBehaviour
{
    public float parallaxFactor;

    public float parallaxFactorY = 0f;

    public void MoveX(float deltaX)
    {
        Vector3 newPos = transform.localPosition;
        newPos.x -= deltaX * parallaxFactor;
        transform.localPosition = newPos;
    }

    public void MoveY(float deltaY)
    {
        Vector3 newPos = transform.localPosition;
        newPos.y -= deltaY * parallaxFactorY;
        transform.localPosition = newPos;
    }

}
