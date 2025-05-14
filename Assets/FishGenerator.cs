using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishGenerator : MonoBehaviour
{
    [Header("拖拽4种鱼的预制体到此数组")]
    [SerializeField]
    private GameObject[] fishPrefabs;

    [Header("生成每种鱼的随机间隔的取值范围")]
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
        // 初始等待随机时间，下限为0秒
        yield return new WaitForSeconds(Random.Range(0, spawnRandomInterval[index].y - spawnRandomInterval[index].x));
        while (true)
        {
            // 在镜头当前位置生成鱼
            Instantiate(fishPrefabs[index], Camera.main.transform.position + new Vector3(0, 0, 10), Quaternion.identity);

            // 等待随机时间
            yield return new WaitForSeconds(Random.Range(spawnRandomInterval[index].x, spawnRandomInterval[index].y));
        }
    }
}
