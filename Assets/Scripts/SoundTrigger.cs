using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundTrigger : MonoBehaviour
{
    public void ClickSound(int soundID)
    {
        AudioManager.Instance.Play((SFXClip)(soundID + 27),"System");
    }

    public void ScanSound()
    {
        AudioManager.Instance.Play(SFXClip.Scan, "System");
    }
}
