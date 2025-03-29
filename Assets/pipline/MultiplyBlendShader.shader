Shader "Custom/MultiplyBlend"
{
    Properties
    {
        _MainTex ("主图层", 2D) = "white" {}
        _MultiplyTex ("正片叠底图层", 2D) = "white" {}
        _MultiplyOpacity ("正片叠底不透明度", Range(0,1)) = 1.0
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
            sampler2D _MultiplyTex;
            float4 _MainTex_ST;
            float4 _MultiplyTex_ST;
            float _MultiplyOpacity;
            
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
                fixed4 multiplyColor = tex2D(_MultiplyTex, i.uv);
                
                // 正片叠底混合，使用正片叠底图层的不透明度
                fixed4 multiplyResult = mainColor * multiplyColor;
                fixed4 finalColor = lerp(mainColor, multiplyResult, multiplyColor.a * _MultiplyOpacity);
                
                return finalColor * i.color;
            }
            ENDCG
        }
    }
} 