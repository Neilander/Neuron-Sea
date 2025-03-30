# 死亡事件系统

这是一个基于事件的角色死亡系统，用于解耦陷阱、死亡区域与角色之间的直接依赖关系。通过使用事件机制，陷阱不需要直接引用角色对象，而只需触发事件，从而使代码更加模块化和可维护。

## 核心组件

### DeathEventSystem
事件系统的核心，负责管理死亡事件的发布和订阅。
- `OnCharacterDeath` 事件：当角色死亡时触发
- `TriggerDeath` 方法：用于触发死亡事件
- `DeathType` 枚举：定义不同的死亡类型（陷阱、跌落、敌人等）

### Character
角色组件，负责订阅死亡事件并处理角色的死亡和重生逻辑。
- 自动订阅死亡事件
- 处理不同类型的死亡
- 管理重生逻辑
- 支持自定义死亡特效

### Trap
陷阱组件，用于检测角色碰撞并触发死亡事件。
- 支持不同的死亡类型
- 可自定义触发特效
- 通过标签系统过滤目标

### DeathZone
死亡区域组件，用于特定区域内的死亡检测。
- 支持延迟死亡
- 可设置不同的死亡类型
- 特殊处理如溺水等持续性死亡区域

### CheckpointManager
检查点管理器，用于更新角色的重生点。
- 支持检查点激活特效
- 可设置一次性或重复使用
- 通过标签系统过滤目标

## 使用方法

### 1. 设置死亡事件系统

在场景中创建一个空游戏对象，添加 `DeathEventSystem` 脚本：

```csharp
// 创建死亡事件系统
GameObject eventSystem = new GameObject("DeathEventSystem");
eventSystem.AddComponent<DeathEventSystem>();
```

### 2. 设置角色

为角色对象添加 `Character` 脚本，并根据需要配置参数：

```csharp
// 添加角色组件
Character character = player.AddComponent<Character>();
character.isPlayer = true;
character.respawnDelay = 2f;
character.deathEffectPrefab = yourDeathEffectPrefab; // 可选
```

### 3. 创建陷阱

为陷阱对象添加 `Trap` 脚本，并确保它有适当的碰撞器：

```csharp
// 添加陷阱组件
Trap trap = trapObject.AddComponent<Trap>();
trap.deathType = DeathEventSystem.DeathType.Trap;
trap.trapTriggerEffectPrefab = yourTrapEffectPrefab; // 可选
trap.targetTags = new string[] { "Player" };
```

### 4. 设置检查点

为检查点对象添加 `CheckpointManager` 脚本：

```csharp
// 添加检查点组件
CheckpointManager checkpoint = checkpointObject.AddComponent<CheckpointManager>();
checkpoint.checkpointActivatedEffectPrefab = yourEffectPrefab; // 可选
checkpoint.deactivateAfterUse = false; // 是否使用后禁用
```

### 5. 创建死亡区域

为死亡区域添加 `DeathZone` 脚本：

```csharp
// 添加死亡区域组件
DeathZone deathZone = deathZoneObject.AddComponent<DeathZone>();
deathZone.deathType = DeathEventSystem.DeathType.Fall;
deathZone.delayDeath = true; // 是否延迟死亡
deathZone.deathDelay = 0.5f; // 延迟时间
```

## 自定义死亡事件处理

你可以在任何脚本中监听死亡事件：

```csharp
void Start()
{
    // 订阅死亡事件
    DeathEventSystem.Instance.OnCharacterDeath += HandleCharacterDeath;
}

void OnDestroy()
{
    // 取消订阅
    if (DeathEventSystem.Instance != null)
    {
        DeathEventSystem.Instance.OnCharacterDeath -= HandleCharacterDeath;
    }
}

void HandleCharacterDeath(GameObject victim, DeathEventSystem.DeathType deathType)
{
    // 自定义处理死亡事件
    Debug.Log($"{victim.name} 死亡，死亡类型: {deathType}");
    
    // 例如，播放声音、更新UI、增加死亡计数等
}
```

## 示例

查看 `Example.cs` 脚本以了解如何设置完整的示例场景。你可以使用脚本中的 `SetupExample()` 方法快速创建一个测试环境。

## 扩展

这个系统设计为易于扩展。你可以：

1. 添加新的死亡类型到 `DeathType` 枚举
2. 创建新的触发器类型，如定时陷阱、切换陷阱等
3. 为死亡事件添加更多参数，如伤害值、死亡来源等
4. 实现更复杂的重生逻辑，如有限生命、改变形态等

## 注意事项

- 记得在对象销毁时取消事件订阅，以防内存泄漏
- 为所有相关对象设置适当的标签 