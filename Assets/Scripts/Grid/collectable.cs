using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class collectable : MonoBehaviour
{
    [SerializeField]
    private int restrictedTime = 5;

    private bool unlocked = true;
    [SerializeField] private SpriteRenderer renderer;

    [SerializeField] private Transform floatingTarget; // 设置为 renderer.transform，或单独物体

    [SerializeField] private float floatAmplitude = 0.1f; // 上下移动的幅度
    [SerializeField] private float floatSpeed = 1.5f;     // 上下移动的速度

    private Vector3 initialLocalPos;

    [SerializeField] private TextMeshPro DisplayText;

    // Start is called before the first frame update
    void Start()
    {
        if (floatingTarget == null)
            floatingTarget = renderer.transform;

        initialLocalPos = floatingTarget.localPosition;
        StartCoroutine(FloatCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        if(GridManager.Instance!=null)UpdateState(GridManager.Instance.SwitchTime);
    }

    void UpdateState(int curSTime)
    {
        
        if (curSTime <= restrictedTime && !unlocked)
        {
            //可接触
            Color c = renderer.color;
            c.a = 1f;
            renderer.color = c;
            unlocked = true;
        }
        else if (unlocked && curSTime > restrictedTime)
        {

            //不可接触
            Color c = renderer.color;
            c.a = 0.5f;
            renderer.color = c;
            unlocked = false;
        }

        DisplayText.text = string.Format("{0}/{1}", curSTime, restrictedTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (unlocked && collision.GetComponent<PlayerController>())
        {
            GetCollected(); 
        }
    }

    void GetCollected()
    {
        Destroy(gameObject);
    }

    IEnumerator FloatCoroutine()
    {
        float timer = 0f;
        while (true)
        {
            timer += Time.deltaTime * floatSpeed;
            float offsetY = Mathf.Sin(timer) * floatAmplitude;
            Vector3 offset = new Vector3(0f, offsetY, 0f);
            floatingTarget.localPosition = initialLocalPos + offset;
            yield return null;
        }
    }
}
