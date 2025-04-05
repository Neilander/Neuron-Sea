Shader "Custom/SceneSoftLightBlend"
{
    Properties
    {
        _MainTex ("主纹理", 2D) = "white" {}
        _BlendTex ("柔光纹理", 2D) = "white" {}
        _BlendAmount ("柔光强度", Range(0,1)) = 1
        _Opacity ("不透明度", Range(0,1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _BlendTex;
            float4 _MainTex_ST;
            float4 _BlendTex_ST;
            fixed _BlendAmount;
            fixed _Opacity;

            // 柔光混合函数
            fixed3 SoftLight(fixed3 base, fixed3 blend)
            {
                fixed3 result;
                UNITY_UNROLL
                for (int i = 0; i < 3; i++)
                {
                    if (blend[i] <= 0.5)
                    {
                        result[i] = base[i] - (1 - 2 * blend[i]) * base[i] * (1 - base[i]);
                    }
                    else
                    {
                        fixed D = (base[i] <= 0.25) ? ((16 * base[i] - 12) * base[i] + 4) * base[i] : sqrt(base[i]);
                        result[i] = base[i] + (2 * blend[i] - 1) * (D - base[i]);
                    }
                }
                return result;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 采样纹理
                fixed4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                fixed4 blendColor = tex2D(_BlendTex, IN.texcoord);

                // 应用柔光混合
                fixed3 softLightColor = SoftLight(mainColor.rgb, blendColor.rgb);

                // 在原始颜色和柔光结果之间插值
                fixed3 finalColor = lerp(mainColor.rgb, softLightColor, _BlendAmount * blendColor.a);

                // 处理透明度
                fixed alpha = mainColor.a * _Opacity;

                // 预乘alpha
                finalColor *= alpha;

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}