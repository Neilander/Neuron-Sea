Shader "Custom/LayerBlend"
{
    Properties
    {
        _MainTex ("主图层", 2D) = "white" {}
        _BlendTex ("混合图层", 2D) = "white" {}
        _GradientTex ("渐变纹理", 2D) = "white" {}
        _GradientStrength ("渐变强度", Range(0,1)) = 0.5
        _Opacity ("不透明度", Range(0,1)) = 1.0
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
            sampler2D _BlendTex;
            sampler2D _GradientTex;
            float4 _MainTex_ST;
            float4 _BlendTex_ST;
            float4 _GradientTex_ST;
            float _GradientStrength;
            float _Opacity;
            
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
                fixed4 blendColor = tex2D(_BlendTex, i.uv);
                fixed4 gradientColor = tex2D(_GradientTex, i.uv);
                
                // 应用渐变
                fixed4 finalColor = lerp(blendColor, gradientColor, _GradientStrength);
                
                // 应用不透明度
                finalColor = lerp(mainColor, finalColor, _Opacity);
                
                return finalColor * i.color;
            }
            ENDCG
        }
    }
}