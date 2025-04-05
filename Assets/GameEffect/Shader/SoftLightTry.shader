Shader "Unlit/SoftLightTry"
{
    Properties
    {
        _BaseTex ("Base Texture", 2D) = "white" {} // 底图：MainCamera 渲染的图
        _BlendTex ("Blend Texture", 2D) = "white" {} // 上层图：你叠加的效果
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "SoftLightBlend"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

            sampler2D _BaseTex;
            sampler2D _BlendTex;
            float4 _BaseTex_ST;
            float4 _BlendTex_ST;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float SoftLight(float base, float blend)
            {
                return (blend < 0.5) ? (2.0 * base * blend + base * base * (1.0 - 2.0 * blend)) : (sqrt(base) * (2.0 * blend - 1.0) + 2.0 * base * (1.0 - blend));
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 baseCol = tex2D(_BaseTex, i.uv);
                float4 blendCol = tex2D(_BlendTex, i.uv);

                float3 result;
                result.r = SoftLight(baseCol.r, blendCol.r);
                result.g = SoftLight(baseCol.g, blendCol.g);
                result.b = SoftLight(baseCol.b, blendCol.b);

                return float4(result, 1.0);
            }
            ENDHLSL
        }
    }
}