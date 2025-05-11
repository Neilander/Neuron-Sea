using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickAndExit : MonoBehaviour
{
    public GameObject Panel;
    // Start is called before the first frame update
    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(Exit);
    }

    public void Exit(){
        AudioManager.Instance.Play(SFXClip.Cilck3);
        Panel.SetActive(false);
    }
}
