using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class distort : ScriptableRendererFeature
{
    class DistortPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle source;

        private Vector2 effectCenter;
        private Vector2 patchBox;
        private float waveMun;
        private float waveStrength;

        public DistortPass(Material mat)
        {
            this.material = mat;
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
        }

        public void Setup(Vector2 center, Vector2 box, float waveMun, float waveStrength)
        {
            this.effectCenter = center;
            this.patchBox = box;
            this.waveMun = waveMun;
            this.waveStrength = waveStrength;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera || material == null)
                return;

            CommandBuffer cmd = CommandBufferPool.Get("Screen Region Distort");

            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // 设置材质参数
            material.SetVector("_EffectCenter", effectCenter);
            material.SetVector("_PatchBox", patchBox);
            material.SetFloat("_WaveMun", waveMun);
            material.SetFloat("_WaveStrength", waveStrength);

            cmd.Blit(source, source, material);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [Header("材质")]
    public Material distortMaterial;

    [Header("区域中心")]
    public Vector2 effectCenter = new Vector2(0.5f, 0.5f);

    [Header("区域范围")]
    public Vector2 patchBox = new Vector2(0.3f, 0.3f);

    [Header("波参数")]
    public float waveMun = 20f;
    public float waveStrength = 0.03f;

    private DistortPass pass;

    public override void Create()
    {
        if (distortMaterial == null)
        {
            Debug.LogError("请指定 ScreenRegionDistort 的材质！");
            return;
        }

        pass = new DistortPass(distortMaterial);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (distortMaterial == null)
            return;

        pass.Setup(effectCenter, patchBox, waveMun, waveStrength);
        renderer.EnqueuePass(pass);
    }
}