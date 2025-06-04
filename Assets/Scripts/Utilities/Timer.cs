using UnityEngine;

public class Timer : MonoBehaviour
{
    private float waitTime = 5f; // 等待时间（秒）
    private float elapsedTime = 0f; // 已经过的时间
    private bool isReady = true; // 准备完毕状态

    // 设置等待时间
    public void SetWaitTime(float time)
    {
        waitTime = time;
    }

    // 触发计时器，设置为未准备完毕
    public void Trigger()
    {
        isReady = false;
        elapsedTime = 0f; // 重置计时
    }

    // 更新函数，传入更新的时间
    public void UpdateTimer(float deltaTime)
    {
        if (!isReady)
        {
            elapsedTime += deltaTime; // 增加经过的时间

            // 检查是否超过等待时间
            if (elapsedTime >= waitTime)
            {
                isReady = true; // 设置为准备完毕
                elapsedTime = 0f; // 重置计时
            }
        }
    }

    // 获取准备状态
    public bool IsReady()
    {
        return isReady;
    }
}
