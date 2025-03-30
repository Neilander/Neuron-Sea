Shader "Custom/SoftLightBlend"
{
    Properties
    {
        _MainTex ("主图层", 2D) = "white" {}
        _MiddleTex ("中间层", 2D) = "white" {}
        _SoftLightTex ("柔光图层", 2D) = "white" {}
        _UseMiddleLayer ("使用中间层", Float) = 0
        _ShowMiddleLayer ("显示中间层", Float) = 0
        _ShowSoftLightLayer ("显示柔光层", Float) = 1
        _MiddleBlend ("中间层混合度", Range(0,1)) = 0.5
        _SoftLightOpacity ("柔光不透明度", Range(0,1)) = 1.0
        _SoftLightStrength ("柔光强度", Range(0,2)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            sampler2D _MiddleTex;
            sampler2D _SoftLightTex;
            float4 _MainTex_ST;
            float4 _MiddleTex_ST;
            float4 _SoftLightTex_ST;
            float _UseMiddleLayer;
            float _ShowMiddleLayer;
            float _ShowSoftLightLayer;
            float _MiddleBlend;
            float _SoftLightOpacity;
            float _SoftLightStrength;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainColor = tex2D(_MainTex, i.uv);
                fixed4 middleColor = tex2D(_MiddleTex, i.uv);
                fixed4 softLightColor = tex2D(_SoftLightTex, i.uv);
                
                // 增强柔光效果
                fixed4 enhancedSoftLightColor = lerp(softLightColor, fixed4(1,1,1,1), _SoftLightStrength - 1);
                
                // 柔光混合
                fixed4 softLightResult;
                if (enhancedSoftLightColor.r < 0.5)
                    softLightResult.r = mainColor.r - (1 - 2 * enhancedSoftLightColor.r) * mainColor.r * (1 - mainColor.r);
                else
                    softLightResult.r = mainColor.r + (2 * enhancedSoftLightColor.r - 1) * (sqrt(mainColor.r) - mainColor.r);
                    
                if (enhancedSoftLightColor.g < 0.5)
                    softLightResult.g = mainColor.g - (1 - 2 * enhancedSoftLightColor.g) * mainColor.g * (1 - mainColor.g);
                else
                    softLightResult.g = mainColor.g + (2 * enhancedSoftLightColor.g - 1) * (sqrt(mainColor.g) - mainColor.g);
                    
                if (enhancedSoftLightColor.b < 0.5)
                    softLightResult.b = mainColor.b - (1 - 2 * enhancedSoftLightColor.b) * mainColor.b * (1 - mainColor.b);
                else
                    softLightResult.b = mainColor.b + (2 * enhancedSoftLightColor.b - 1) * (sqrt(mainColor.b) - mainColor.b);
                    
                softLightResult.a = mainColor.a;
                
                // 中间层混合
                fixed4 middleResult = lerp(mainColor, middleColor, _MiddleBlend);
                
                // 根据是否使用中间层选择最终颜色
                fixed4 finalColor = lerp(softLightResult, middleResult, _UseMiddleLayer);
                
                // 应用不透明度
                finalColor = lerp(mainColor, finalColor, softLightColor.a * _SoftLightOpacity * _ShowSoftLightLayer);
                
                // 显示中间层
                if (_ShowMiddleLayer > 0.5)
                {
                    finalColor = lerp(finalColor, middleColor, middleColor.a * _MiddleBlend);
                }
                
                return finalColor * i.color;
            }
            ENDCG
        }
    }
} 