using UnityEngine;

public class WaveMunController : MonoBehaviour
{
    [Header("WaveMun 控制")]
    [SerializeField] private Material targetMaterial; // 目标材质
    [SerializeField] private float cycleSpeed = 22f; // 循环速度
    [SerializeField] private string propertyName = "_WaveMun"; // 属性名称

    private void Update()
    {
        if (targetMaterial != null)
        {
            // 使用 PingPong 在0到1之间循环
            float waveMun = Mathf.PingPong(Time.time * cycleSpeed, 40f);

            // 或者使用 Sin 在0到1之间循环
            // float waveMun = (Mathf.Sin(Time.time * cycleSpeed) + 1f) * 0.5f;

            targetMaterial.SetFloat(propertyName, waveMun);
        }
    }

    private void OnValidate()
    {
        // 在编辑器中验证材质是否存在
        if (targetMaterial == null)
        {
            Debug.LogWarning("请设置目标材质！");
        }
    }
}