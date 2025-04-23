using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslateX;
    public ParallaxCameraDelegate onCameraTranslateY;

    private float oldPositionX;
    private float oldPositionY;

    void Start()
    {
        oldPositionX = transform.position.x;
        oldPositionY = transform.position.y;
    }

    void Update()
    {
        float currentX = transform.position.x;
        float currentY = transform.position.y;

        if (currentX != oldPositionX)
        {
            if (onCameraTranslateX != null)
            {
                float deltaX = oldPositionX - currentX;
                onCameraTranslateX?.Invoke(deltaX);
            }

            oldPositionX = currentX;
        }

        if (currentY != oldPositionY)
        {
            if (onCameraTranslateY != null)
            {
                float deltaY = oldPositionY - currentY;
                onCameraTranslateY?.Invoke(deltaY);
            }

            oldPositionY = currentY;
        }
    }
}