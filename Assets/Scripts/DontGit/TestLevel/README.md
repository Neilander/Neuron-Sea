# 神经海测试关卡

这是"神经海"游戏项目的测试关卡。通过这个关卡，你可以测试游戏的核心系统功能。

## 如何设置测试关卡

1. 打开Unity编辑器
2. 打开SampleScene场景或创建新场景
3. 在场景中添加以下要素：

### 必要组件

- **地面和平台**：使用2D碰撞器的精灵，标记为"Ground"层
- **玩家**：使用PlayerController2脚本
- **测试关卡管理器**：添加TestLevelManager脚本到空物体
- **交换管理器**：添加SwapManager或SimpleSwapManager脚本到空物体
- **可交换物体**：添加SwappableObject或SimpleSwappableObject脚本到物体
- **收集物**：添加SimpleCollectible脚本到场景中
- **关卡目标**：添加SimpleLevelGoal脚本到场景中

### 场景设置

- 确保所有层和标签正确设置
- 玩家对象需要标记为"Player"
- 地面和平台需要在"Ground"层

## 游戏流程

1. 玩家可以使用方向键移动和跳跃
2. 通过点击可交换物体进行选择和交换
3. 收集所有收集物可以增加交换次数
4. 到达关卡目标点完成关卡

## 脚本兼容性说明

测试关卡使用以下两种脚本选项：

1. **完整功能脚本**：SwapManager, SwappableObject等
2. **简化版脚本**：SimpleSwapManager, SimpleSwappableObject等

如果你在使用完整功能脚本时遇到编译错误，可以选择以下方案之一：

1. 使用简化版脚本替代
2. 修复完整功能脚本中的兼容性问题

## 当前兼容性修复

已完成的兼容性修复：

1. 在SwapManager中添加：
   - SetMaxSwaps方法
   - OnSwapPerformed事件
   - OnObjectHover方法及其相关方法

如有其他兼容性问题，请根据错误信息进行相应修复。 