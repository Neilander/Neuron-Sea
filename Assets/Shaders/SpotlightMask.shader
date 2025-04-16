Shader "Custom/SpotlightMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Center ("Center", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius", Range(0, 10)) = 1.0
        _Smoothness ("Smoothness", Range(0.001, 10)) = 1.0
        _MaskColor ("Mask Color", Color) = (0, 0, 0, 1)
    }
    
    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
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
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _Center;
            float _Radius;
            float _Smoothness;
            float4 _MaskColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag (v2f i) : SV_Target
            {
                // 计算当前像素到中心的距离
                float dist = distance(i.uv, _Center);
                
                // 使用光滑步进函数创建一个从中心辐射的渐变
                // 此处smoothstep的工作原理是：
                // 1. 当dist小于_Radius - _Smoothness/2时，返回0（完全透明）
                // 2. 当dist大于_Radius + _Smoothness/2时，返回1（完全显示遮罩）
                // 3. 在中间区域平滑过渡
                float alpha = smoothstep(_Radius - _Smoothness/2, _Radius + _Smoothness/2, dist);
                
                // 将计算出的alpha与遮罩颜色的alpha相乘
                float4 finalColor = _MaskColor;
                finalColor.a *= alpha;
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    Fallback "UI/Default"
}