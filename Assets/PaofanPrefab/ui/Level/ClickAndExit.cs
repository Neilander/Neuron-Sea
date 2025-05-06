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

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Exit(){
        Panel.SetActive(false);
    }
}
