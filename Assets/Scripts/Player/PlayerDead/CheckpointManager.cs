using UnityEngine;

/// <summary>
/// 检查点管理器，用于设置角色的重生点
/// </summary>
public class CheckpointManager : MonoBehaviour
{
    [Header("检查点设置")]
    public GameObject checkpointActivatedEffectPrefab;
    public float effectDestroyDelay = 2f;
    public bool deactivateAfterUse = false;

    [Header("标签设置")]
    [Tooltip("哪些标签的游戏对象可以激活此检查点")]
    public string[] targetTags = { "Player" };

    private bool isActivated = false;

    private void OnTriggerEnter(Collider other)
    {
        CheckActivation(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckActivation(other.gameObject);
    }

    /// <summary>
    /// 检查是否激活检查点
    /// </summary>
    private void CheckActivation(GameObject other)
    {
        // 如果已经激活且设置为一次性，则跳过
        if (isActivated && deactivateAfterUse) return;

        // 检查目标对象是否具有我们关心的标签
        bool isValidTarget = false;
        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                isValidTarget = true;
                break;
            }
        }

        if (!isValidTarget) return;

        // 获取角色组件
        CharacterDead character = other.GetComponent<CharacterDead>();
        if (character == null)
        {
            Debug.LogWarning($"游戏对象 {other.name} 没有Character组件！");
            return;
        }

        // 设置角色的重生点
        character.SetRespawnPoint(transform.position);

        // 播放激活特效
        if (checkpointActivatedEffectPrefab != null)
        {
            GameObject effect = Instantiate(checkpointActivatedEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, effectDestroyDelay);
        }

        // 标记为已激活
        isActivated = true;

        // 检查是否需要禁用
        if (deactivateAfterUse)
        {
            // 禁用渲染器让检查点看起来已被使用
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }

            // 可以选择禁用碰撞器，但这会使检查点不再触发
            // GetComponent<Collider>().enabled = false;
        }

        Debug.Log($"检查点 {gameObject.name} 已激活");
    }
}