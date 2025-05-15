using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellAnim : MonoBehaviour
{
    private Animator shellAnimator;
    [Header("贝壳打开的随机间隔的取值范围")]
    [SerializeField]
    private Vector2 shellOpenRandomInterval;

    // Start is called before the first frame update
    void Start()
    {
        shellAnimator = GetComponent<Animator>(); 
        StartCoroutine(ShellOpen());
    }

    IEnumerator ShellOpen()
    {
        while (true)
        {
            // 等待随机时间
            yield return new WaitForSeconds(Random.Range(shellOpenRandomInterval.x, shellOpenRandomInterval.y));

            // 打开贝壳
            shellAnimator.SetTrigger("Open");
        }
    }
}
