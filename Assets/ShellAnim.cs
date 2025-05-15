using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellAnim : MonoBehaviour
{
    private Animator shellAnimator;
    [Header("���Ǵ򿪵���������ȡֵ��Χ")]
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
            // �ȴ����ʱ��
            yield return new WaitForSeconds(Random.Range(shellOpenRandomInterval.x, shellOpenRandomInterval.y));

            // �򿪱���
            shellAnimator.SetTrigger("Open");
        }
    }
}
