using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickAndExit : MonoBehaviour
{
    public bool DontPlaySound = false;
    public GameObject Panel;
    // Start is called before the first frame update
    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(Exit);
    }

    public void Exit()
    {
        if (!DontPlaySound)
        {
            AudioManager.Instance.Play(SFXClip.Cilck3, gameObject.name);
        }
        Panel.SetActive(false);
    }
}
