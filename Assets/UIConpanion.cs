using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIConpanion : MonoBehaviour
{
    Animator anim;
    RectTransform rectTransform;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        rectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        int maxScene = levelManager.instance.IsLevelUnlocked(25) ? 3 : levelManager.instance.IsLevelUnlocked(13) ? 2 : 1;
        anim.SetInteger("MaxScene", maxScene);
        rectTransform.localPosition = transform.GetChild(maxScene - 1).GetComponent<RectTransform>().localPosition;
        rectTransform.sizeDelta = transform.GetChild(maxScene - 1).GetComponent<RectTransform>().sizeDelta;
    }
}
