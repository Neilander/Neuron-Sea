Shader "Custom/RippleDissolve"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _RippleCenter ("Ripple Center", Vector) = (0.5, 0.5, 0, 0) // 波纹中心（UV坐标）
        _RippleSpeed ("Ripple Speed", Range(0.1, 5)) = 1.0 // 波纹扩散速度
        _RippleWidth ("Ripple Width", Range(0.01, 0.5)) = 0.1 // 单次波纹宽度
        _RippleInterval ("Ripple Interval", Range(0.5, 3)) = 1.0 // 波纹间隔时间
        _DissolveProgress ("Dissolve Progress", Range(0, 1)) = 0 // 整体溶解进度
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float2 _RippleCenter;
            float _RippleSpeed;
            float _RippleWidth;
            float _RippleInterval;
            float _DissolveProgress;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 基础颜色采样
                fixed4 col = tex2D(_MainTex, i.uv);

                // 计算到中心的距离
                float2 centerVec = i.uv - _RippleCenter;
                float distance = length(centerVec);

                // 波纹动态计算
                float time = _Time.y * _RippleSpeed;
                float rippleWave = 0;

                // 多层波纹叠加（示例使用2层）
                for (int j = 0; j < 2; j++)
                {
                    float offset = j * _RippleInterval;
                    float rippleRadius = (time - offset) * _RippleSpeed;

                    // 波纹衰减曲线
                    float falloff = 1 - smoothstep(
                        rippleRadius - _RippleWidth,
                        rippleRadius + _RippleWidth,
                        distance
                    );

                    rippleWave += falloff * saturate(1 - (time - offset) / _RippleInterval);
                }

                // 组合透明度
                float baseAlpha = 1 - _DissolveProgress;
                float rippleAlpha = saturate(rippleWave * 0.7); // 波纹强度系数
                col.a = baseAlpha * (1 - rippleAlpha);

                // 边缘增强效果
                float edge = saturate((1 - col.a) * 5);
                col.rgb += fixed3(0.2, 0.5, 1) * edge; // 添加蓝色光边

                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}