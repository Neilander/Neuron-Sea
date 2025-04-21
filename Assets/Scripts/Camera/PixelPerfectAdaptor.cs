using UnityEngine;
using System.Reflection;

/// <summary>
/// 像素完美相机适配器
/// 通过反射实现对不同Unity版本PixelPerfectCamera组件的访问
/// </summary>
public class PixelPerfectAdaptor : MonoBehaviour
{
    // 目标组件
    [SerializeField] private Component pixelPerfectComponent;

    // 缓存的属性信息
    private PropertyInfo assetsPPUProperty;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    /// <summary>
    /// 初始化适配器，查找属性
    /// </summary>
    public void InitializeIfNeeded()
    {
        if (isInitialized) return;

        // 如果未指定组件，尝试在当前游戏对象上查找
        if (pixelPerfectComponent == null)
        {
            // 尝试查找所有可能的PixelPerfectCamera组件类型
            pixelPerfectComponent = TryGetComponent("UnityEngine.Rendering.Universal.PixelPerfectCamera");
            if (pixelPerfectComponent == null)
                pixelPerfectComponent = TryGetComponent("UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera");
            if (pixelPerfectComponent == null)
                pixelPerfectComponent = TryGetComponent("Unity.RenderPipelines.Universal.Runtime.PixelPerfectCamera");
        }

        if (pixelPerfectComponent == null)
        {
            Debug.LogError("未找到PixelPerfectCamera组件，请确保启用了URP并在相机上添加了PixelPerfectCamera组件");
            return;
        }

        // 获取assetsPPU属性
        assetsPPUProperty = pixelPerfectComponent.GetType().GetProperty("assetsPPU");
        if (assetsPPUProperty == null)
        {
            Debug.LogError("无法获取PixelPerfectCamera的assetsPPU属性，请检查Unity版本或URP版本");
            return;
        }

        isInitialized = true;
    }

    /// <summary>
    /// 尝试按类型名称获取组件
    /// </summary>
    private Component TryGetComponent(string typeName)
    {
        System.Type type = System.Type.GetType(typeName);
        if (type != null)
        {
            return GetComponent(type) as Component;
        }
        return null;
    }

    /// <summary>
    /// 获取当前的AssetsPPU值
    /// </summary>
    public int GetAssetsPPU()
    {
        InitializeIfNeeded();
        if (!isInitialized || assetsPPUProperty == null) return 0;
        
        return (int)assetsPPUProperty.GetValue(pixelPerfectComponent, null);
    }

    /// <summary>
    /// 设置AssetsPPU值
    /// </summary>
    public void SetAssetsPPU(int value)
    {
        InitializeIfNeeded();
        if (!isInitialized || assetsPPUProperty == null) return;
        
        assetsPPUProperty.SetValue(pixelPerfectComponent, value, null);
    }

    /// <summary>
    /// 判断适配器是否已初始化并可用
    /// </summary>
    public bool IsValid()
    {
        InitializeIfNeeded();
        return isInitialized && pixelPerfectComponent != null && assetsPPUProperty != null;
    }

    /// <summary>
    /// 创建一个适配器实例（用于无法直接在Inspector中设置的情况）
    /// </summary>
    public static PixelPerfectAdaptor CreateAdaptor(Camera camera)
    {
        if (camera == null) return null;

        // 检查是否已经有适配器
        PixelPerfectAdaptor adaptor = camera.GetComponent<PixelPerfectAdaptor>();
        if (adaptor != null) return adaptor;

        // 创建新的适配器
        adaptor = camera.gameObject.AddComponent<PixelPerfectAdaptor>();
        adaptor.InitializeIfNeeded();
        return adaptor;
    }
}