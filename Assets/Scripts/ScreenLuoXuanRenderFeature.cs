using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenLuoXuanRenderFeature : ScriptableRendererFeature
{
    class ScreenPatchPass : ScriptableRenderPass
    {
        private Material intermediateMaterial;
        private Material patchMaterial;

        private Vector2 effectCenter;
        private Vector2 patchBox;
        private float waveMun;
        private float waveStrength;

        private RTHandle source;
        private RTHandle intermediateRT;

        public ScreenPatchPass(Material intermediateMat, Material patchMat)
        {
            intermediateMaterial = intermediateMat;
            patchMaterial = patchMat;
        }

        public void Setup(Vector2 center, Vector2 box, float waveMun, float waveStrength)
        {
            effectCenter = center;
            patchBox = box;
            this.waveMun = waveMun;
            this.waveStrength = waveStrength;

            // 创建一个明确格式的 RT，用于 intermediateRT
            var desc = new RenderTextureDescriptor(
                Screen.width,
                Screen.height,
                RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear)
            };

            intermediateRT = RTHandles.Alloc(desc, name: "_IntermediateRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera) return;

            CommandBuffer cmd = CommandBufferPool.Get("ScreenPatchTwoPass");
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            // 第一步：原图 → intermediateRT（处理材质 1）
            cmd.SetRenderTarget(intermediateRT);
            cmd.ClearRenderTarget(true, true, Color.black);
            intermediateMaterial.SetVector("_EffectCenter", effectCenter);
            intermediateMaterial.SetVector("_PatchBox", patchBox);
            intermediateMaterial.SetFloat("_WaveMun", waveMun);
            intermediateMaterial.SetFloat("_WaveStrength", waveStrength);
            intermediateMaterial.SetTexture("_MainTex", source);
            cmd.Blit(source, intermediateRT, intermediateMaterial);

            // ✅ 执行 Blit 保证 GPU 写入完成，才能读取
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            // 第二步：intermediateRT → Texture2D（便于 Shader Graph 使用）
            RenderTexture.active = intermediateRT;
            Texture2D tex2D = new Texture2D(intermediateRT.rt.width, intermediateRT.rt.height, TextureFormat.RGBA32, false, false);
            tex2D.ReadPixels(new Rect(0, 0, intermediateRT.rt.width, intermediateRT.rt.height), 0, 0);
            tex2D.Apply();
            RenderTexture.active = null;

            // 第三步：tex2D → finalRT（处理材质 2）
            var descriptor = new RenderTextureDescriptor(
                Screen.width,
                Screen.height,
                RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = (QualitySettings.activeColorSpace == ColorSpace.Linear)
            };
            var finalRT = RTHandles.Alloc(descriptor, name: "_FinalRT");

            patchMaterial.SetTexture("_MainTex", source);
            patchMaterial.SetTexture("_TempTex", tex2D); // Shader Graph 读取这个 Texture2D
            patchMaterial.SetVector("_EffectCenter", effectCenter);
            patchMaterial.SetVector("_PatchBox", patchBox);
            patchMaterial.SetFloat("_WaveMun", waveMun);
            patchMaterial.SetFloat("_WaveStrength", waveStrength);

            cmd.Blit(intermediateRT, finalRT, patchMaterial);
            cmd.Blit(finalRT, source); // 写回屏幕

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            // 清理 RTHandle
            RTHandles.Release(finalRT);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (intermediateRT != null)
            {
                RTHandles.Release(intermediateRT);
                intermediateRT = null;
            }
        }
    }

    [Header("材质设置")]
    public Material intermediateMaterial;
    public Material patchMaterial;

    [Header("扭曲区域")]
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
        pass = new ScreenPatchPass(intermediateMaterial, patchMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (intermediateMaterial == null || patchMaterial == null) return;

        pass.Setup(patchCenter, patchBox, waveMun, waveStrength);
        renderer.EnqueuePass(pass);
    }
}
