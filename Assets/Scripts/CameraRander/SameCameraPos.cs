using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameCameraPos : MonoBehaviour
{
    // 通过脚本同步（若使用两个摄像机）
    public Camera mainCamera;

    public Camera renderCamera;

    void Update(){
        renderCamera.transform.position= new Vector3(mainCamera.transform.position.x+2.6f,mainCamera.transform.position.y-1.03f,mainCamera.transform.position.z);
        renderCamera.transform.rotation = mainCamera.transform.rotation;
    }
}
