using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundTrigger : MonoBehaviour
{
    public void ClickSound(int soundID)
    {
        AudioManager.Instance.Play((SFXClip)(soundID + 27));
    }

    public void EnterLevelSound()
    {
        AudioManager.Instance.Play(SFXClip.EnterLevel);
    }
}
