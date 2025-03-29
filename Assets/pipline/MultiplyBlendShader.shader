Shader "Custom/MultiplyBlend"
{
    Properties
    {
        _MainTex ("主图层", 2D) = "white" {}
        _MiddleTex ("中间层", 2D) = "white" {}
        _MultiplyTex ("正片叠底图层", 2D) = "white" {}
        _UseMiddleLayer ("使用中间层", Float) = 0
        _ShowMiddleLayer ("显示中间层", Float) = 0
        _ShowMultiplyLayer ("显示正片叠底层", Float) = 1
        _MiddleBlend ("中间层混合度", Range(0,1)) = 0.5
        _MultiplyOpacity ("正片叠底不透明度", Range(0,1)) = 1.0
        _MultiplyStrength ("正片叠底强度", Range(0,2)) = 1.0
        _MainScale ("主图层缩放", Range(0.1,2)) = 1
        _MiddleScale ("中间层缩放", Range(0.1,2)) = 1
        _MultiplyScale ("正片叠底缩放", Range(0.1,2)) = 1
        _MainOffset ("主图层位置", Vector) = (0,0,0,0)
        _MiddleOffset ("中间层位置", Vector) = (0,0,0,0)
        _MultiplyOffset ("正片叠底位置", Vector) = (0,0,0,0)
        _BackgroundMultiply ("背景正片叠底", Range(0,1)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }
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
            sampler2D _MultiplyTex;
            float4 _MainTex_ST;
            float4 _MiddleTex_ST;
            float4 _MultiplyTex_ST;
            float _UseMiddleLayer;
            float _ShowMiddleLayer;
            float _ShowMultiplyLayer;
            float _MiddleBlend;
            float _MultiplyOpacity;
            float _MultiplyStrength;
            float _MainScale;
            float _MiddleScale;
            float _MultiplyScale;
            float4 _MainOffset;
            float4 _MiddleOffset;
            float4 _MultiplyOffset;
            float _BackgroundMultiply;
            
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
                // 计算缩放和偏移后的UV坐标
                float2 mainUV = (i.uv - 0.5) * _MainScale + 0.5 + _MainOffset.xy;
                float2 middleUV = (i.uv - 0.5) * _MiddleScale + 0.5 + _MiddleOffset.xy;
                float2 multiplyUV = (i.uv - 0.5) * _MultiplyScale + 0.5 + _MultiplyOffset.xy;
                
                // 确保UV坐标在[0,1]范围内
                mainUV = saturate(mainUV);
                middleUV = saturate(middleUV);
                multiplyUV = saturate(multiplyUV);
                
                // 采样纹理
                fixed4 mainColor = tex2D(_MainTex, mainUV);
                fixed4 middleColor = tex2D(_MiddleTex, middleUV);
                fixed4 multiplyColor = tex2D(_MultiplyTex, multiplyUV);
                
                // 正片叠底混合
                fixed4 multiplyResult = fixed4(mainColor.rgb * multiplyColor.rgb, mainColor.a);
                
                // 中间层混合
                fixed4 middleResult = lerp(mainColor, middleColor, _MiddleBlend);
                
                // 根据是否使用中间层选择最终颜色
                fixed4 finalColor = lerp(multiplyResult, middleResult, _UseMiddleLayer);
                
                // 应用不透明度
                finalColor = lerp(mainColor, finalColor, multiplyColor.a * _MultiplyOpacity * _ShowMultiplyLayer);
                
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
    
    // 第二个Pass用于处理背景
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Stencil
        {
            Ref 1
            Comp Equal
        }
        
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
            float4 _MainTex_ST;
            float _BackgroundMultiply;
            
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
                fixed4 color = tex2D(_MainTex, i.uv);
                // 应用正片叠底效果
                color.rgb = lerp(color.rgb, color.rgb * fixed3(0.5, 0.5, 0.5), _BackgroundMultiply);
                return color * i.color;
            }
            ENDCG
        }
    }
} 