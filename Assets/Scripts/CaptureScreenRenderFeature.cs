using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class CaptureScreenRenderFeature : ScriptableRendererFeature
{
    // 内部类：用于捕捉并处理屏幕图像
    class CaptureScreenPass : ScriptableRenderPass
    {
        private Material blitMaterial;   // 用于处理效果的材质（在 Shader 中实现柔光等效果）
        private RTHandle source;         // 摄像机的颜色目标（RTHandle 类型）
        private RenderTextureDescriptor descriptor;  // 屏幕画面的描述信息
        private RTHandle temporaryRT;    // 临时 RTHandle，用来存储“拍下来的照片”
        private string flipYPropertyName = "_FlipY";

        public CaptureScreenPass(Material material, string flipYPropertyName)
        {
            blitMaterial = material;
            this.flipYPropertyName = flipYPropertyName;
        }

        // 设置需要处理的目标和描述信息
        public void Setup(RenderTextureDescriptor descriptor)
        {
            this.descriptor = descriptor;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // 如果是 SceneView 摄像机，则跳过，不处理
            if (renderingData.cameraData.isSceneViewCamera)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("CaptureScreen");

            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            int width = descriptor.width;
            int height = descriptor.height;

            temporaryRT = RTHandles.Alloc(
                width, height, slices: 1, depthBufferBits: DepthBits.None,
                colorFormat: GraphicsFormat.R8G8B8A8_UNorm, filterMode: FilterMode.Bilinear,
                wrapMode: TextureWrapMode.Clamp, dimension: TextureDimension.Tex2D,
                name: "_TemporaryRT"
            );
            if (blitMaterial.HasProperty(flipYPropertyName))
            {
                blitMaterial.SetFloat(flipYPropertyName, Application.isPlaying ? 1f : 0f);
            }

            // 正常处理 GameView 渲染
            cmd.Blit(source, temporaryRT, blitMaterial);
            cmd.Blit(temporaryRT, source);
            //Blitter.BlitCameraTexture(cmd, source, temporaryRT, blitMaterial, 0);
            //Blitter.BlitCameraTexture(cmd, temporaryRT, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

           
        }

        public void UpdateFlipYName(string name)
        {
            this.flipYPropertyName = name;
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (temporaryRT != null)
            {
                RTHandles.Release(temporaryRT);
                temporaryRT = null;
            }
        }
    }

    public Material blitMaterial;  // 请在 Inspector 中指定你的后处理材质
    public string flipYPropertyName = "_FlipY";
    CaptureScreenPass capturePass;

    public override void Create()
    {
        // 创建自定义 Render Pass，并设置在所有后处理效果之后执行
        capturePass = new CaptureScreenPass(blitMaterial, flipYPropertyName)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (blitMaterial == null)
            return;

        capturePass = new CaptureScreenPass(blitMaterial, flipYPropertyName)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
        //Debug.Log("当前使用的 flipYPropertyName: " + flipYPropertyName);
        capturePass.Setup(renderingData.cameraData.cameraTargetDescriptor);
        renderer.EnqueuePass(capturePass);
    }

    
}