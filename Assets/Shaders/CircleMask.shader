Shader "Custom/CircleMask"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _CircleRadius ("Circle Radius", Range(0,1)) = 0.5
        _CircleSoftness ("Circle Softness", Range(0,0.5)) = 0.1
        _CircleCenterX ("Circle Center X", Range(0,1)) = 0.5
        _CircleCenterY ("Circle Center Y", Range(0,1)) = 0.5
        
        // UI遮罩支持
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _CircleRadius;
            float _CircleSoftness;
            float _CircleCenterX;
            float _CircleCenterY;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            sampler2D _MainTex;

            fixed4 frag(v2f IN) : SV_Target
            {
                // 获取UI元素的尺寸（通过纹理坐标的梯度）
                float2 ddx_uv = ddx(IN.texcoord);
                float2 ddy_uv = ddy(IN.texcoord);
                float2 textureSize = float2(
                    1.0 / sqrt(dot(ddx_uv, ddx_uv)),
                    1.0 / sqrt(dot(ddy_uv, ddy_uv))
                );
                
                // 计算宽高比以保持圆形
                float aspectRatio = textureSize.x / textureSize.y;
                
                // 将UV坐标转换为以自定义中心点为中心的坐标系
                float2 center = float2(_CircleCenterX, _CircleCenterY);
                float2 uv = IN.texcoord - center;
                
                // 校正UV坐标以考虑宽高比，保持圆形
                uv.x *= aspectRatio;
                
                // 计算当前点到中心的距离（保持圆形）
                float dist = length(uv);
                
                // 创建圆形遮罩（内部透明，外部不透明）
                // 这里我们调整半径以适应宽高比
                float adjustedRadius = _CircleRadius / max(1.0, aspectRatio);
                float adjustedSoftness = _CircleSoftness / max(1.0, aspectRatio);
                
                // 距离中心_CircleRadius以内的区域透明
                // 距离在_CircleRadius和_CircleRadius+_CircleSoftness之间的区域半透明
                // 距离中心_CircleRadius+_CircleSoftness以外的区域不透明
                float alpha = smoothstep(adjustedRadius, adjustedRadius + adjustedSoftness, dist);
                
                // 应用裁剪矩形和采样原纹理
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                color.a *= alpha;
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                return color;
            }
            ENDCG
        }
    }
}