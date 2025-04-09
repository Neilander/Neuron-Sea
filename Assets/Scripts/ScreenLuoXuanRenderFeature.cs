using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenLuoXuanRenderFeature : ScriptableRendererFeature
{
    class ScreenPatchPass : ScriptableRenderPass
    {
        private Material material;
        private Vector2 effectCenter;
        private Vector2 patchBox;

        private float waveMun;
        private float waveStrength;

        private RTHandle source;

        public ScreenPatchPass(Material mat)
        {
            material = mat;
        }

        public void Setup(Vector2 center, Vector2 box, float waveMun, float waveStrength)
        {
            effectCenter = center;
            patchBox = box;
            this.waveMun = waveMun;
            this.waveStrength = waveStrength;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera) return;

            Debug.Log("执行了螺旋");

            CommandBuffer cmd = CommandBufferPool.Get("ScreenPatchEffect");

            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            material.SetVector("_EffectCenter", effectCenter);
            material.SetVector("_PatchBox", patchBox);
            material.SetFloat("_WaveMun", waveMun);
            material.SetFloat("_WaveStrength", waveStrength);
            material.SetTexture("_MainTex", source);

            cmd.Blit(source, source, material);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [Header("材质和参数")]
    public Material patchMaterial;

    [Range(0f, 1f)]
    public float patchBoxX = 0.15f;
    [Range(0f, 1f)]
    public float patchBoxY = 0.15f;
    public Vector2 patchBox => new Vector2(patchBoxX, patchBoxY);

    public Vector2 patchCenter = new Vector2(0.5f, 0.5f);

    [Header("螺旋参数")]
    public float waveMun = 20f;
    public float waveStrength = 0.05f;

    private ScreenPatchPass pass;

    public override void Create()
    {
        pass = new ScreenPatchPass(patchMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (patchMaterial == null) return;

        pass.Setup(patchCenter, patchBox, waveMun, waveStrength);
        renderer.EnqueuePass(pass);
    }
}
