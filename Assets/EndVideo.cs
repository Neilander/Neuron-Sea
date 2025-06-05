using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndVideo : MonoBehaviour
{
    public void OnEnd() 
    {
        AudioManager.Instance.Stop(BGMClip.EndScene);
        SceneManager.LoadScene(0);
    }
}
