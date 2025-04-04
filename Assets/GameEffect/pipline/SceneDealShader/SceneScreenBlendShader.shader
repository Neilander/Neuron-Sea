Shader "Custom/SceneScreenBlend"
{
    Properties
    {
        _ScreenTex ("滤色图层", 2D) = "black" {}
        _ScreenOpacity ("滤色不透明度", Range(0,1)) = 1.0
        _ScreenStrength ("滤色强度", Range(0,2)) = 1.0
        _ScreenScale ("滤色缩放", Range(0.1,2)) = 1
        _ScreenOffset ("滤色位置", Vector) = (0,0,0,0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" "IgnoreProjector"="True" }
        Blend OneMinusDstColor One
        ZWrite Off
        Cull Off
        
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
            
            sampler2D _ScreenTex;
            float4 _ScreenTex_ST;
            float _ScreenOpacity;
            float _ScreenStrength;
            float _ScreenScale;
            float4 _ScreenOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _ScreenTex);
                o.color = v.color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = (i.uv - 0.5) * _ScreenScale + 0.5 + _ScreenOffset.xy;
                screenUV = saturate(screenUV);
                
                fixed4 screenColor = tex2D(_ScreenTex, screenUV);
                
                // 如果alpha值很小，直接返回透明色
                if (screenColor.a < 0.01)
                    return fixed4(0,0,0,0);
                    
                fixed4 finalColor = fixed4(1, 1, 1, screenColor.a);
                finalColor.rgb = lerp(finalColor.rgb, screenColor.rgb * _ScreenStrength, screenColor.a * _ScreenOpacity);
                
                return finalColor * i.color;
            }
            ENDCG
        }
    }
} 