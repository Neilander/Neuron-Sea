Shader "Custom/ScenePinLightBlendShader"
{
    Properties
    {
        _BlendTex ("点光", 2D) = "white" {}
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
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        //GrabPass { "_BackgroundTexture" }

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
                float4 grabPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _BlendTex;
            float4 _BlendTex_ST;
            float _BlendAmount;
            float _Opacity;
            sampler2D _BackgroundTexture;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BlendTex);
                o.grabPos = ComputeGrabScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            // 点光混合模式实现
            // 如果blend < 0.5，取较暗的颜色（类似darken）
            // 如果blend >= 0.5，取较亮的颜色（类似lighten）
            float3 pinLight(float3 base, float3 blend)
            {
                float3 result;

                // 对每个通道分别计算
                result.r = (blend.r > 0.5) ? max(base.r, (2.0 * (blend.r - 0.5))) : min(base.r, (2.0 * blend.r));
                result.g = (blend.g > 0.5) ? max(base.g, (2.0 * (blend.g - 0.5))) : min(base.g, (2.0 * blend.g));
                result.b = (blend.b > 0.5) ? max(base.b, (2.0 * (blend.b - 0.5))) : min(base.b, (2.0 * blend.b));

                return result;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 获取背景
                fixed4 background = tex2Dproj(_BackgroundTexture, i.grabPos);

                // 获取混合图层
                fixed4 blendColor = tex2D(_BlendTex, i.uv);

                // 如果混合图层完全透明，直接返回背景
                if (blendColor.a < 0.01)
                    return background;

                // 应用点光混合模式
                float3 blendedColor = pinLight(background.rgb, blendColor.rgb);

                // 混合强度，考虑图层的透明度
                float blendFactor = _BlendAmount * blendColor.a * _Opacity;
                float3 finalColor = lerp(background.rgb, blendedColor, blendFactor);

                // 保留原始alpha，不要在完全透明区域产生灰色边缘
                return fixed4(finalColor, background.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}