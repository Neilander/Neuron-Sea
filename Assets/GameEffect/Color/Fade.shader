Shader "Hidden/Fade" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }

        _FadeTex ("FadeTexture", 2D) = "white" { }//�ܽ������ͼ
        _FadeAmount ("FadeAmount", Range(0, 1)) = 0 //�ܽ����ֵ
        _FadeColor ("FadeColor", Color) = (1, 1, 0, 1) //�ܽ�ʱ����ɫ
        _FadeBurnWidth ("Fade Burn Width", Range(0, 1)) = 0.02 //����ɫ�����˶��ٵ�ʱ��ſ�ʼ�ܽ�

    }
    SubShader {
        Tags { "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _FadeTex;
            float4 _FadeTex_ST;
            float _FadeAmount, _FadeBurnWidth;
            fixed4 _FadeColor;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);//ԭʼͼƬ��������ɫ
                float originalAlpha = col.a;//ԭʼ��alphaֵ

                float2 tiledUvFade = TRANSFORM_TEX(i.uv, _FadeTex);//�õ�����texture��UV����

                float fadeTemp = tex2D(_FadeTex, tiledUvFade).r;//ѡȡ����texture��rֵ��������ʧ���ж�
                float fade = step(_FadeAmount, fadeTemp);//ͨ��texture��rֵֵ�������ܽ��ж�������_FadeAmountΪ1����֮Ϊ0
                float fadeBurn = saturate(step(_FadeAmount - _FadeBurnWidth, fadeTemp));//����ɫ����_FadeBurnWidth�Ժ�ſ�ʼ�ܽ��ֵ Ҳ��0��1
                col.a *= fade;//��ԭ������ɫ��alphaֵ��fade�������ʾ��ʧ�Ĳ���
                col += fadeBurn * tex2D(_FadeTex, tiledUvFade) * _FadeColor * originalAlpha * (1 - col.a);//��˵õ������ܽ�Ч��

                return col;
            }
            ENDCG
        }
    }
}