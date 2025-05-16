using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIConpanion : MonoBehaviour
{
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetInteger("MaxScene", levelManager.instance.IsLevelUnlocked(25) ? 3 : levelManager.instance.IsLevelUnlocked(13) ? 2 : 1);
    }
}
