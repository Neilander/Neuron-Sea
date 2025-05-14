using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishGenerator : MonoBehaviour
{
    [Header("��ק4�����Ԥ���嵽������")]
    [SerializeField]
    private GameObject[] fishPrefabs;

    [Header("����ÿ�������������ȡֵ��Χ")]
    [SerializeField]
    private Vector2[] spawnRandomInterval;

    // Start is called before the first frame update
    void Start()
    {
        for(int index = 0; index < fishPrefabs.Length; index++)
        {
            StartCoroutine(SpawnFish(index));
        }
    }

    IEnumerator SpawnFish(int index)
    {
        // ��ʼ�ȴ����ʱ�䣬����Ϊ0��
        yield return new WaitForSeconds(Random.Range(0, spawnRandomInterval[index].y - spawnRandomInterval[index].x));
        while (true)
        {
            // �ھ�ͷ��ǰλ��������
            Instantiate(fishPrefabs[index], Camera.main.transform.position + new Vector3(0, 0, 10), Quaternion.identity);

            // �ȴ����ʱ��
            yield return new WaitForSeconds(Random.Range(spawnRandomInterval[index].x, spawnRandomInterval[index].y));
        }
    }
}
