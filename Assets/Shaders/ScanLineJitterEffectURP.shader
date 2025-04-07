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
        
        // 波浪效果参数
        _WaveIntensity ("Wave Intensity", Range(0, 1)) = 0.2
        _WaveFrequency ("Wave Frequency", Range(0, 50)) = 10
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 2
        
        // 黑白效果参数
        _BWEffect ("黑白效果强度", Range(0, 1)) = 0
        _BWNoiseScale ("黑白噪点尺寸", Range(1, 50)) = 10
        _BWNoiseIntensity ("黑白噪点强度", Range(0, 1)) = 0.2
        _BWFlickerSpeed ("黑白闪烁速度", Range(0, 20)) = 5
        
        // 颜色调整参数
        _ColorCorrection ("颜色校正", Range(0, 1)) = 0
        _HueShift ("色相偏移", Range(-180, 180)) = 0
        _Saturation ("饱和度", Range(0, 2)) = 1
        _Brightness ("亮度", Range(0, 2)) = 1
        _Contrast ("对比度", Range(0, 2)) = 1
        _RedOffset ("红色偏移", Range(-1, 1)) = 0
        _GreenOffset ("绿色偏移", Range(-1, 1)) = 0
        _BlueOffset ("蓝色偏移", Range(-1, 1)) = 0
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
            float _WaveIntensity;
            float _WaveFrequency;
            float _WaveSpeed;
            float _BWEffect;
            float _BWNoiseScale;
            float _BWNoiseIntensity;
            float _BWFlickerSpeed;
            float _ColorCorrection;
            float _HueShift;
            float _Saturation;
            float _Brightness;
            float _Contrast;
            float _RedOffset;
            float _GreenOffset;
            float _BlueOffset;
            
            // 简单噪声函数
            float RandomValue(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }
            
            // 柏林噪声简化版本（只用于黑白噪点效果）
            float PerlinNoise(float2 st)
            {
                float2 i = floor(st);
                float2 f = frac(st);
                
                // 四个角的随机值
                float a = RandomValue(i);
                float b = RandomValue(i + float2(1.0, 0.0));
                float c = RandomValue(i + float2(0.0, 1.0));
                float d = RandomValue(i + float2(1.0, 1.0));
                
                // 平滑插值
                float2 u = f * f * (3.0 - 2.0 * f);
                
                return lerp(a, b, u.x) + 
                       (c - a) * u.y * (1.0 - u.x) + 
                       (d - b) * u.x * u.y;
            }
            
            // RGB转HSV
            float3 rgb2hsv(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }
            
            // HSV转RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
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
                float time = _Time.y;
                
                // 基于时间产生随机值，用于控制故障效果
                float randomVal = RandomValue(float2(floor(time * 5.0), floor(time * 5.0)));
                
                // 持续的波浪效果
                float waveX = sin(uv.y * _WaveFrequency + time * _WaveSpeed) * _WaveIntensity;
                float waveY = sin(uv.x * _WaveFrequency * 0.5 + time * _WaveSpeed * 0.7) * _WaveIntensity * 0.5;
                
                // 将波浪效果应用于UV坐标
                uv.x += waveX;
                uv.y += waveY;
                
                // 扫描线效果 - 水平抖动
                float jitterAmount = 0;
                if (randomVal < _GlitchProbability || sin(time * 50.0) > 0.95)
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
                
                // 添加颜色偏移效果 - 增强蓝绿色调
                col = lerp(col, colorShift, _ColorShiftIntensity + abs(waveX) * 0.5);
                
                // 给蓝绿色调添加更强的权重
                col.b = min(1.0, col.b * (1.0 + abs(waveX) * 0.5));
                col.g = min(1.0, col.g * (1.0 + abs(waveY) * 0.3));
                
                // 添加噪声
                float noiseVal = RandomValue(uv + time) * _NoiseIntensity;
                
                // 在扫描线处应用更多噪声和颜色偏移
                if (frac(uv.y * 30.0 + time) < 0.5)
                {
                    col = lerp(col, colorShift, _ColorShiftIntensity * abs(jitterAmount) * 10.0);
                    col.rgb += noiseVal * 2.0 * (randomVal < _GlitchProbability ? 1.0 : 0.2);
                }
                
                // 扫描线调整亮度
                col.rgb *= (0.95 + 0.05 * scanLine);
                
                // 随机像素闪烁
                if (randomVal < _GlitchProbability * 0.1 && noiseVal > 0.8)
                {
                    col.rgb = 1.0 - col.rgb; // 颜色反相
                }
                
                // 添加噪声
                col.rgb += noiseVal;
                
                // 黑白效果处理
                if (_BWEffect > 0.0)
                {
                    // 转换为黑白
                    float bwValue = dot(col.rgb, float3(0.299, 0.587, 0.114));
                    
                    // 黑白闪烁效果
                    float flicker = sin(time * _BWFlickerSpeed) * 0.1 + 0.9;
                    
                    // 黑白噪点效果
                    float bwNoise = PerlinNoise(uv * _BWNoiseScale + time) * _BWNoiseIntensity;
                    
                    // 创建黑白效果
                    half4 bwCol = half4(bwValue, bwValue, bwValue, col.a) * flicker;
                    bwCol.rgb += bwNoise;
                    
                    // 有噪点的区域可以出现更极端的黑白
                    if (bwNoise > 0.8 * _BWNoiseIntensity)
                    {
                        bwCol.rgb = step(0.5, bwCol.rgb);
                    }
                    
                    // 按强度混合黑白效果
                    col = lerp(col, bwCol, _BWEffect);
                }
                
                // 应用颜色校正
                if (_ColorCorrection > 0.0)
                {
                    // 应用颜色偏移
                    col.r += _RedOffset * _ColorCorrection;
                    col.g += _GreenOffset * _ColorCorrection;
                    col.b += _BlueOffset * _ColorCorrection;
                    
                    // 转换为HSV
                    float3 hsv = rgb2hsv(col.rgb);
                    
                    // 应用色相偏移 (0-360范围)
                    hsv.x += _HueShift / 360.0 * _ColorCorrection;
                    hsv.x = frac(hsv.x); // 确保在0-1范围内循环
                    
                    // 应用饱和度
                    hsv.y *= lerp(1.0, _Saturation, _ColorCorrection);
                    
                    // 应用亮度
                    hsv.z *= lerp(1.0, _Brightness, _ColorCorrection);
                    
                    // 转回RGB
                    col.rgb = hsv2rgb(hsv);
                    
                    // 应用对比度
                    float3 gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                    col.rgb = lerp(col.rgb, lerp(gray, col.rgb, _Contrast), _ColorCorrection);
                }
                
                // 确保颜色在有效范围内
                col.rgb = saturate(col.rgb);
                
                return col;
            }
            ENDHLSL
        }
    }
} 