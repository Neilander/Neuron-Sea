Shader "Custom/ScreenBlend"
{
    Properties
    {
        _MainTex ("主图层", 2D) = "white" {}
        _MiddleTex ("中间层", 2D) = "white" {}
        _ScreenTex ("滤色图层", 2D) = "white" {}
        _UseMiddleLayer ("使用中间层", Float) = 0
        _ShowMiddleLayer ("显示中间层", Float) = 0
        _ShowScreenLayer ("显示滤色层", Float) = 1
        _MiddleBlend ("中间层混合度", Range(0,1)) = 0.5
        _ScreenOpacity ("滤色不透明度", Range(0,1)) = 1.0
        _ScreenStrength ("滤色强度", Range(0,2)) = 1.0
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
            sampler2D _ScreenTex;
            float4 _MainTex_ST;
            float4 _MiddleTex_ST;
            float4 _ScreenTex_ST;
            float _UseMiddleLayer;
            float _ShowMiddleLayer;
            float _ShowScreenLayer;
            float _MiddleBlend;
            float _ScreenOpacity;
            float _ScreenStrength;
            
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
                fixed4 screenColor = tex2D(_ScreenTex, i.uv);
                
                // 增强滤色效果
                fixed4 enhancedScreenColor = lerp(screenColor, fixed4(1,1,1,1), _ScreenStrength - 1);
                fixed4 screenResult = 1 - (1 - mainColor) * (1 - enhancedScreenColor);
                
                // 中间层混合
                fixed4 middleResult = lerp(mainColor, middleColor, _MiddleBlend);
                
                // 根据是否使用中间层选择最终颜色
                fixed4 finalColor = lerp(screenResult, middleResult, _UseMiddleLayer);
                
                // 应用不透明度
                finalColor = lerp(mainColor, finalColor, screenColor.a * _ScreenOpacity * _ShowScreenLayer);
                
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