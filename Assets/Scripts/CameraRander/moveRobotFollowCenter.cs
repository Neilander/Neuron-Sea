using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class moveRobotFollowCenter : MonoBehaviour, IlerpCompanion
{
    public Vector3 fromLocalPos;
    public Vector3 toLocalPos;
    public Transform tarTrans;
    [Range(0,1)]
    public float limitToReach;
    public float tempSmoothSpeed;
    private bool setted = false;
    public float directSetFloat = 0.8f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DoWhenLerp(float t)
    {
        CameraControl.Instance.RestoreCameraLimit(true, tempSmoothSpeed);
        float newT =Mathf.Clamp01(t / limitToReach);
        tarTrans.transform.localPosition = Vector3.Lerp(fromLocalPos, toLocalPos, newT);
        if (newT > directSetFloat && !setted)
        {
            setted = true;
            CameraControl.Instance.DirectSetPos();
        }
    }
}
