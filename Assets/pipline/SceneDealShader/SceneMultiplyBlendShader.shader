Shader "Custom/SceneMultiplyBlend"
{
    Properties
    {
        _MultiplyTex ("正片叠底图层", 2D) = "white" {}
        _MultiplyOpacity ("正片叠底不透明度", Range(0,1)) = 1.0
        _MultiplyStrength ("正片叠底强度", Range(0,2)) = 1.0
        _MultiplyScale ("正片叠底缩放", Range(0.1,2)) = 1
        _MultiplyOffset ("正片叠底位置", Vector) = (0,0,0,0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" "IgnoreProjector"="True" }
        Blend DstColor Zero
        
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
            
            sampler2D _MultiplyTex;
            float4 _MultiplyTex_ST;
            float _MultiplyOpacity;
            float _MultiplyStrength;
            float _MultiplyScale;
            float4 _MultiplyOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MultiplyTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 multiplyUV = (i.uv - 0.5) * _MultiplyScale + 0.5 + _MultiplyOffset.xy;
                multiplyUV = saturate(multiplyUV);
                
                fixed4 multiplyColor = tex2D(_MultiplyTex, multiplyUV);
                
                fixed4 finalColor = fixed4(1, 1, 1, 1);
                finalColor.rgb = lerp(finalColor.rgb, multiplyColor.rgb * _MultiplyStrength, multiplyColor.a * _MultiplyOpacity);
                
                return finalColor * i.color;
            }
            ENDCG
        }
    }
}