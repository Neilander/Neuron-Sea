using UnityEngine;

/// <summary>
/// 定义移动控制器的基本接口
/// </summary>
public interface IMovementController
{
    /// <summary>
    /// 重置所有移动状态
    /// </summary>
    void ResetMovement();

    /// <summary>
    /// 禁用移动功能
    /// </summary>
    void DisableMovement();

    /// <summary>
    /// 启用移动功能
    /// </summary>
    void EnableMovement();
}