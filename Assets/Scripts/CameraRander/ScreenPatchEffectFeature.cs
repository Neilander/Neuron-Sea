using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenPatchEffectFeature : ScriptableRendererFeature
{
    class ScreenPatchPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle source;
        private RenderTextureDescriptor descriptor;
        private Vector2 sourceUV;
        private Vector2 targetUV;
        private float patchSize;

        public ScreenPatchPass(Material mat)
        {
            material = mat;
        }

        public void Setup(RenderTextureDescriptor desc, Vector2 sourceUV, Vector2 targetUV, float patchSize)
        {
            descriptor = desc;
            this.sourceUV = sourceUV;
            this.targetUV = targetUV;
            this.patchSize = patchSize;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            if (renderingData.cameraData.isSceneViewCamera)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("ScreenPatchEffect");

            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            material.SetVector("_SourceUV", sourceUV);
            material.SetVector("_TargetUV", targetUV);
            material.SetFloat("_PatchSize", patchSize);

            cmd.Blit(source, source, material);
            //Debug.Log("ScreenPatchEffect Execute called");
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    public Material patchMaterial;
    public Vector2 sourceUV = new Vector2(0.2f, 0.2f); // 源位置（屏幕UV）
    public Vector2 targetUV = new Vector2(0.5f, 0.5f); // 目标位置（屏幕UV）
    public float patchSize = 100f;

    ScreenPatchPass pass;

    public override void Create()
    {
        pass = new ScreenPatchPass(patchMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };

        //Debug.Log("Create 被调用");
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        //Debug.Log("AddRenderPasses 被调用");
        if (patchMaterial == null)
            return;

        pass.Setup(renderingData.cameraData.cameraTargetDescriptor, sourceUV, targetUV, patchSize);
        renderer.EnqueuePass(pass);
    }
}
