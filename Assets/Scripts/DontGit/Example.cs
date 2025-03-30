using UnityEngine;

/// <summary>
/// 示例脚本，展示如何设置和使用死亡事件系统
/// </summary>
public class Example : MonoBehaviour
{
    // 这个示例脚本主要用于展示如何使用死亡事件系统

    void Start()
    {
        Debug.Log("死亡事件系统使用示例");
        Debug.Log("---------------------");
        Debug.Log("1. 确保场景中有一个挂载了DeathEventSystem脚本的游戏对象");
        Debug.Log("2. 为角色添加Character脚本");
        Debug.Log("3. 为陷阱添加Trap脚本");
        Debug.Log("4. 为检查点添加CheckpointManager脚本");
        Debug.Log("5. 为死亡区域添加DeathZone脚本");
    }

    /// <summary>
    /// 场景设置示例
    /// </summary>
    public void SetupExample()
    {
        // 1. 创建死亡事件系统
        // GameObject eventSystem = new GameObject("DeathEventSystem");
        // eventSystem.AddComponent<DeathEventSystem>();

        // 2. 创建角色
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, 0);

        // 添加角色组件
        CharacterDead character = player.AddComponent<CharacterDead>();
        character.isPlayer = true;
        character.respawnDelay = 2f;

        // 3. 创建陷阱
        GameObject trap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trap.name = "SpikeTrap";
        trap.transform.position = new Vector3(3, 0.1f, 0);
        trap.transform.localScale = new Vector3(1, 0.2f, 1);

        // 确保陷阱有触发器
        Collider trapCollider = trap.GetComponent<Collider>();
        if (trapCollider != null)
        {
            trapCollider.isTrigger = true;
        }

        // 添加陷阱组件
        Trap trapComponent = trap.AddComponent<Trap>();
        trapComponent.deathType = DeathEventSystem.DeathType.Trap;
        trapComponent.targetTags = new string[] { "Player" };

        // 4. 创建检查点
        GameObject checkpoint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        checkpoint.name = "Checkpoint";
        checkpoint.transform.position = new Vector3(-3, 0.5f, 0);
        checkpoint.transform.localScale = new Vector3(1, 0.1f, 1);

        // 确保检查点有触发器
        Collider checkpointCollider = checkpoint.GetComponent<Collider>();
        if (checkpointCollider != null)
        {
            checkpointCollider.isTrigger = true;
        }

        // 添加检查点组件
        CheckpointManager checkpointComponent = checkpoint.AddComponent<CheckpointManager>();
        checkpointComponent.targetTags = new string[] { "Player" };

        // 5. 创建死亡区域（如掉落区）
        GameObject deathZone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        deathZone.name = "FallZone";
        deathZone.transform.position = new Vector3(0, -10, 0);
        deathZone.transform.localScale = new Vector3(50, 1, 50);

        // 确保死亡区域有触发器
        Collider deathZoneCollider = deathZone.GetComponent<Collider>();
        if (deathZoneCollider != null)
        {
            deathZoneCollider.isTrigger = true;
        }

        // 添加死亡区域组件
        DeathZone deathZoneComponent = deathZone.AddComponent<DeathZone>();
        deathZoneComponent.deathType = DeathEventSystem.DeathType.Fall;
        deathZoneComponent.targetTags = new string[] { "Player", "Enemy" };

        Debug.Log("示例场景已设置完成！尝试移动角色到陷阱或掉落区域来测试死亡事件系统。");
    }

    // 如果你想在Inspector中添加一个按钮来设置示例场景
    [ContextMenu("设置示例场景")]
    void SetupExampleScene()
    {
        SetupExample();
    }
}