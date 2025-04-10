Shader "Custom/ScreenRegionDistort"
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _EffectCenter ("Effect Center", Vector) = (0.5, 0.5, 0, 0)
        _PatchBox ("Patch Box Size", Vector) = (0.3, 0.3, 0, 0)
        _WaveMun ("Wave Count", Float) = 20
        _WaveStrength ("Wave Strength", Float) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "FullScreenPass"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _EffectCenter;
            float4 _PatchBox;
            float _WaveMun;
            float _WaveStrength;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert (Attributes input)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }

            float4 Frag (Varyings i) : SV_Target
            {
                float2 uv = i.uv;

                float2 center = _EffectCenter.xy;
                float2 box = _PatchBox.xy;
                float2 halfBox = box * 0.5;

                float2 delta = uv - center;

                // ✅ 区域判断（保持原样）
                float2 inside = step(abs(delta), halfBox);
                float mask = inside.x * inside.y;

                float dist = length(delta / halfBox);

                // ✅ soft falloff
                float falloff = 1.0 - smoothstep(0.9, 1.0, dist);
                float wave = sin(dist * _WaveMun) * _WaveStrength * falloff;

                // ✅ 安全 normalize
                float2 direction = length(delta) > 1e-4 ? normalize(delta) : float2(0, 0);
                float2 offset = direction * wave * mask;

                float2 finalUV = uv + offset;

                // ✅ 只调整这里：Clamp 边界防止采样出黑边
                finalUV = clamp(finalUV, 0.001, 0.999);

                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}