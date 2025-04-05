Shader "Custom/AdvancedSoftLight"
{
    Properties
    {
        [Header(Base Settings)]
        _MainTex ("Base Texture", 2D) = "white" {}

        [Header(Blend Settings)]
        _BlendTex ("Blend Texture", 2D) = "white" {}
        _BlendColor ("Tint Color", Color) = (1,1,1,1)
        [Toggle] _UseTexAlpha ("Use Texture Alpha", Float) = 1
        [Space]
        _Intensity ("Blend Intensity", Range(0, 2)) = 1.0
        _Contrast ("Effect Contrast", Range(0.5, 2.0)) = 1.0
        _Brightness ("Effect Brightness", Range(-0.5, 0.5)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _USETEXALPHA_ON
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Properties
            sampler2D _MainTex;
            sampler2D _BlendTex;
            float4 _MainTex_ST;
            fixed4 _BlendColor;
            float _Intensity;
            float _Contrast;
            float _Brightness;
            float _UseTexAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float3 ApplySoftLight(float3 base, float3 blend)
            {
                float3 case1 = 2.0 * base * blend + base * base * (1.0 - 2.0 * blend);
                float3 case2 = 2.0 * base * (1.0 - blend) + sqrt(base) * (2.0 * blend - 1.0);
                float3 condition = step(0.5, blend);
                return lerp(case1, case2, condition);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ������������
                fixed4 base = tex2D(_MainTex, i.uv);

                // �������������Ӧ��ɫ��
                fixed4 blend = tex2D(_BlendTex, i.uv);
                blend.rgb *= _BlendColor.rgb;

                // ������ǿ��
                #if _USETEXALPHA_ON
                    float alpha = blend.a;
                #else
                float alpha = 1.0;
                #endif
                float finalIntensity = _Intensity * alpha;

                // Ӧ�������
                float3 result = ApplySoftLight(base.rgb, blend.rgb);

                // ���ȶԱȶȵ���
                result = saturate(result + _Brightness); // ����ƫ��
                result = pow(result, _Contrast); // �Աȶȵ���

                // ���ԭʼ��ɫ
                result = lerp(base.rgb, result, finalIntensity);

                return fixed4(result, base.a);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}