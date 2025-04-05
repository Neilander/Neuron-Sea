Shader "Custom/SceneColorDodgeBlendShader"
{
    Properties
    {
        _MainTex ("主纹理", 2D) = "white" {}
        _BlendTex ("颜色减淡图层", 2D) = "white" {}
        _BlendAmount ("混合强度", Range(0, 1)) = 1
        _Opacity ("不透明度", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        ZWrite Off
        Cull Off

        // 先渲染基础图层
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 获取主纹理颜色
                fixed4 mainColor = tex2D(_MainTex, i.uv) * i.color;
                return mainColor;
            }
            ENDCG
        }

        // 使用自定义混合模式渲染颜色减淡层
        Pass
        {
            Blend One One // 加法混合，我们将在片元着色器中实现颜色减淡

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
            sampler2D _BlendTex;
            float4 _BlendTex_ST;
            float _BlendAmount;
            float _Opacity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BlendTex);
                o.color = v.color;
                return o;
            }

            // 颜色减淡算法：result = base / (1 - blend)
            fixed4 frag(v2f i) : SV_Target
            {
                // 获取主纹理（底层）颜色
                fixed4 baseColor = tex2D(_MainTex, i.uv);

                // 获取混合图层颜色
                fixed4 blendColor = tex2D(_BlendTex, i.uv) * i.color;

                // 如果混合图层完全透明，不做任何操作
                if (blendColor.a < 0.01)
                    return fixed4(0, 0, 0, 0);

                // 实现颜色减淡混合
                fixed3 invBlend = fixed3(1, 1, 1) - blendColor.rgb;
                fixed3 result = fixed3(0, 0, 0);

                // 避免除以0的情况
                result.r = (invBlend.r < 0.00001) ? 1.0 : min(1.0, baseColor.r / invBlend.r);
                result.g = (invBlend.g < 0.00001) ? 1.0 : min(1.0, baseColor.g / invBlend.g);
                result.b = (invBlend.b < 0.00001) ? 1.0 : min(1.0, baseColor.b / invBlend.b);

                // 计算最终颜色(结果 - 基础)，因为我们使用的是加法混合
                fixed3 finalColor = (result - baseColor.rgb) * _BlendAmount * blendColor.a * _Opacity;

                // 输出颜色差值
                return fixed4(finalColor, 0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}