using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SwitchLimitInUi : MonoBehaviour
{
    public static SwitchLimitInUi instance;
    public GameObject group;

    public TextMeshProUGUI text;

    private int curTarget;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        group.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetTarget(int n)
    {
        curTarget = n;
        text.text = $"0 / {curTarget}";
        group.SetActive(true);
    }

    public void SetSwitchTime(int n)
    {
        text.text = n <= curTarget ? string.Format("{0}/{1}", n, curTarget) : string.Format("<color=#E73CA6>{0}</color>/{1}", n, curTarget);
    }

    public void ShutDown()
    {
        group.SetActive(false);
    }
}
