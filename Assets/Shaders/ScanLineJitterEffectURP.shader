Shader "Custom/ScanLineJitterEffectURP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _JitterIntensity ("Jitter Intensity", Range(0, 1)) = 0.1
        _JitterFrequency ("Jitter Frequency", Range(0, 100)) = 10
        _ScanLineThickness ("Scan Line Thickness", Range(0, 10)) = 2
        _ScanLineSpeed ("Scan Line Speed", Range(0, 10)) = 1
        _ColorShiftIntensity ("Color Shift Intensity", Range(0, 1)) = 0.05
        _NoiseIntensity ("Noise Intensity", Range(0, 1)) = 0.1
        _GlitchProbability ("Glitch Probability", Range(0, 1)) = 0.05
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZTest Always
        ZWrite Off
        Cull Off
        
        // URP后处理Pass
        Pass
        {
            Name "ScanLineJitterPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            
            float _JitterIntensity;
            float _JitterFrequency;
            float _ScanLineThickness;
            float _ScanLineSpeed;
            float _ColorShiftIntensity;
            float _NoiseIntensity;
            float _GlitchProbability;
            
            // 简单噪声函数
            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                // 基于时间产生随机值，用于控制故障效果
                float time = _Time.y;
                float randomValue = random(float2(floor(time * 5.0), floor(time * 5.0)));
                
                // 扫描线效果 - 水平抖动
                float jitterAmount = 0;
                if (randomValue < _GlitchProbability || sin(time * 50.0) > 0.95)
                {
                    // 随机水平偏移，创建抖动效果
                    jitterAmount = _JitterIntensity * sin(uv.y * _JitterFrequency + time * 10.0);
                    uv.x += jitterAmount;
                }
                
                // 扫描线效果
                float scanLine = frac(uv.y * _ScanLineThickness + time * _ScanLineSpeed);
                scanLine = step(0.5, scanLine);
                
                // 获取原始颜色
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                
                // 颜色偏移
                float2 colorShiftUV = uv;
                colorShiftUV.x += _ColorShiftIntensity * sin(time * 2.0);
                half4 colorShift = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, colorShiftUV);
                
                // 添加噪声
                float noise = random(uv + time) * _NoiseIntensity;
                
                // 在扫描线处应用更多噪声和颜色偏移
                if (frac(uv.y * 30.0 + time) < 0.5)
                {
                    col = lerp(col, colorShift, _ColorShiftIntensity * abs(jitterAmount) * 10.0);
                    col.rgb += noise * 2.0 * (randomValue < _GlitchProbability ? 1.0 : 0.2);
                }
                
                // 扫描线调整亮度
                col.rgb *= (0.95 + 0.05 * scanLine);
                
                // 随机像素闪烁
                if (randomValue < _GlitchProbability * 0.1 && noise > 0.8)
                {
                    col.rgb = 1.0 - col.rgb; // 颜色反相
                }
                
                // 添加噪声
                col.rgb += noise;
                
                return col;
            }
            ENDHLSL
        }
    }
} 