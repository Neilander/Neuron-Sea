Shader "Hidden/Fade" {
    Properties {
        _MainTex ("Texture", 2D) = "white" { }

        _FadeTex ("FadeTexture", 2D) = "white" { }//�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�0�0
        _FadeAmount ("FadeAmount", Range(0, 1)) = 0 //�1�7�1�7�1�7�1�7�1�7�1�7�0�5
        _FadeColor ("FadeColor", Color) = (1, 1, 0, 1) //�1�7�1�7�1�7�0�2�1�7�1�7�1�7�1�7�0�2
        _FadeBurnWidth ("Fade Burn Width", Range(0, 1)) = 0.02 //�1�7�1�7�1�7�1�7�0�2�1�7�1�7�1�7�1�7�1�7�0�6�1�7�1�7�1�7�1�7�0�2�1�7�1�7�0�7�1�7�0�3�1�7�1�7�1�7

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
                fixed4 col = tex2D(_MainTex, i.uv);//�0�9�0�3�0�0�0�2�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�0�2
                float originalAlpha = col.a;//�0�9�0�3�1�7�1�7alpha�0�5

                float2 tiledUvFade = TRANSFORM_TEX(i.uv, _FadeTex);//�1�7�0�1�1�7�1�7�1�7�1�7�1�7texture�1�7�1�7UV�1�7�1�7�1�7�1�7

                float fadeTemp = tex2D(_FadeTex, tiledUvFade).r;//�0�5�0�0�1�7�1�7�1�7�1�7texture�1�7�1�7r�0�5�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�0�2�1�7�1�7�1�7�؄1�7
                float fade = step(_FadeAmount, fadeTemp);//�0�0�1�7�1�7texture�1�7�1�7r�0�5�0�5�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7�؄1�7�1�7�1�7�1�7�1�7�1�7�1�7_FadeAmount�0�21�1�7�1�7�1�7�1�7�0�8�0�20
                float fadeBurn = saturate(step(_FadeAmount - _FadeBurnWidth, fadeTemp));//�1�7�1�7�1�7�1�7�0�2�1�7�1�7�1�7�1�7_FadeBurnWidth�1�7�0�2�1�7�0�7�1�7�0�3�1�7�1�7�1�7�1�7�0�5 �0�6�1�7�1�70�1�7�1�71
                col.a *= fade;//�1�7�1�7�0�9�1�7�1�7�1�7�1�7�1�7�1�7�0�2�1�7�1�7alpha�0�5�1�7�1�7fade�1�7�1�7�1�7�1�7�1�7�1�7�1�7�0�5�1�7�1�7�0�2�1�7�0�4�1�7�1�7�1�7
                col += fadeBurn * tex2D(_FadeTex, tiledUvFade) * _FadeColor * originalAlpha * (1 - col.a);//�1�7�1�7�0�5�0�1�1�7�1�7�1�7�1�7�1�7�1�7�1�7�1�7���1�7�1�7

                return col;
            }
            ENDCG
        }
    }
}