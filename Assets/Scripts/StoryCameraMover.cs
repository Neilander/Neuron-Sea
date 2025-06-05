using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoryCameraMover : MonoBehaviour
{
    public CameraRegionTrigger region;

    public 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<GameObject> cameraBoundObjects;
    public void MoveCameraTo(bool IfStart)
    {
        if (IfStart)
        {
            Transform entitiesRoot = levelManager.instance.currentLevelGO.transform.Find("Entities");

            if (entitiesRoot == null)
            {
                Debug.LogError("未找到 Entities 节点！");
                return;
            }

            cameraBoundObjects = new List<GameObject>();

            foreach (Transform child in entitiesRoot)
            {
                if (child.name.StartsWith("CameraBound") && !child.gameObject.activeSelf)
                {
                    cameraBoundObjects.Add(child.gameObject);
                }
            }

            Debug.Log($"找到 {cameraBoundObjects.Count} 个未激活的 CameraBound 开头物体");

            foreach (GameObject go in cameraBoundObjects)
            {
                go.SetActive(true);
            }
        }
        else
        {
            if (cameraBoundObjects != null)
            {
                foreach (GameObject gmo in cameraBoundObjects)
                    gmo.SetActive(false);
            }
        }
       
    }

    public void PlayBGMForSceneThree()
    {
        AudioManager.Instance.Stop(BGMClip.Scene3);
        AudioManager.Instance.Play(BGMClip.EndScene);
    }
}
