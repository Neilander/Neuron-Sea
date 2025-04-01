Shader "Unlit/TestPs"
{
    // 定义材质面板中可调节的属性
    Properties
    {
        _MainTexA ("纹理A", 2D) = "white" {}  // 第一个输入纹理
        _MainTexB ("纹理B", 2D) = "white" {}  // 第二个输入纹理
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }  // 设置为不透明渲染
        LOD 100  // 细节层次
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert  // 顶点着色器
            #pragma fragment frag  // 片段着色器
            #pragma multi_compile_fog  // 启用雾效
 
            #include "UnityCG.cginc"
 
            // 顶点着色器输入结构
            struct appdata
            {
                float4 vertex : POSITION;  // 顶点位置
                float2 uv : TEXCOORD0;     // UV坐标
            };
 
            // 顶点着色器输出结构
            struct v2f
            {
                float4 uv : TEXCOORD0;     // UV坐标（xy为纹理A，zw为纹理B）
                float4 vertex : SV_POSITION; // 裁剪空间顶点位置
            };

            // RGB转HSV颜色空间
            float3 RGB2HSV(float3 c)
            {
                float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
 
                float d = q.x - min(q.w, q.y);
                float e = 1.0e-10;
                return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
            }

            // HSV转RGB颜色空间
            float3 HSV2RGB( float3 c ){
                float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6)-3.0)-1.0, 0, 1);
                rgb = rgb*rgb*(3.0-2.0*rgb);
                return c.z * lerp( float3(1,1,1), rgb, c.y);
            }
 
            // 声明纹理采样器和变换矩阵
            sampler2D _MainTexA;
            float4 _MainTexA_ST;
            sampler2D _MainTexB;
            float4 _MainTexB_ST;
 
            // 顶点着色器
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);  // 转换到裁剪空间
                o.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTexA);  // 计算纹理A的UV
                o.uv.zw = TRANSFORM_TEX(v.uv.xy, _MainTexB);  // 计算纹理B的UV
                UNITY_TRANSFER_FOG(o,o.vertex);  // 传递雾效参数
                return o;
            }
 
            // 片段着色器
            half4 frag (v2f i) : SV_Target
            {
                // 采样两个纹理
                float4 colorA = tex2D(_MainTexA, i.uv.xy);
                float4 colorB = tex2D(_MainTexB, i.uv.zw);
                
                // 计算亮度
                float colorALum = Luminance(colorA.xyz);
                float colorBLum = Luminance(colorA.xyz);
                
                // 转换到HSV颜色空间
                float3 colorAHSV = RGB2HSV(colorA.xyz);
                float3 colorBHSV = RGB2HSV(colorB.xyz);
                
                float4 finalRGBA = 0;

                // 各种混合模式实现：
                
                // 1. 正片叠底 (Multiply)
                finalRGBA = float4(colorA.xyz * colorB.xyz,1);
                
                // 2. 滤色 (Screen)
                // finalRGBA = float4(1 - (1 - colorA.xyz) * (1 -colorB.xyz),1);
                
                // 3. 颜色减淡 (Color Dodge)
                // finalRGBA = float4(colorA.xyz * (rcp(1 - colorB.xyz)),1);
                
                // 4. 颜色加深 (Color Burn)
                // finalRGBA = float4(1- rcp(colorB.xyz) * (1 - colorA.xyz),1);
                
                // 5. 线性减淡 (Linear Dodge)
                // finalRGBA = float4(colorB.xyz + colorA.xyz,1);
                
                // 6. 线性加深 (Linear Burn)
                // finalRGBA = float4(colorB.xyz + colorA.xyz - 1,1);
                
                // 7. 叠加 (Overlay)
                // float3 OA = step(colorA.xyz,0.5);
                // float3 c1 = colorA.xyz * colorB.xyz * 2 *OA ;
                // float3 c2 = (1 - 2 * (1 - colorA.xyz) *(1 - colorB.xyz)) * (1 - OA);
                // finalRGBA = float4(c1+ c2,1);
                
                // 8. 强光 (Hard Light)
                // float3 OB = step(colorB.xyz,0.5);
                // float3 c1 = colorA.xyz * colorB.xyz * 2 *OB ;
                // float3 c2 = (1 - 2 * (1 - colorA.xyz) *(1 - colorB.xyz)) * (1 - OB);
                
                // 9. 柔光 (Soft Light)
                // float3 OB = step(colorB.xyz,0.5);
                // float3 c1 = ((2 * colorB - 1) * (colorA - colorA * colorA) + colorA) * OB ;
                // float3 c2 = ((2 * colorB - 1) * (sqrt(colorA)- colorA) + colorA) * (1 - OB);
                
                // 10. 亮光 (Vivid Light)
                // float3 c1 = colorB - (1 - colorB) * (1 - 2 * colorA) / (2 * colorA);
                // float3 c2 = colorB + colorB * (2 * colorA - 1) / (2 * (1 - colorA));
                // float3 OB = step(colorA.xyz,0.5);
                // c1 *= OB;
                // c2 *= (1 - OB);
                // finalRGBA = float4(c1+ c2,1);
                
                // 11. 线性光 (Linear Light)
                // finalRGBA = float4(colorA + 2 * colorB) - 1;
                
                // 12. 点光 (Pin Light)
                // float3 OB = step(colorA,2 *colorB - 1);
                // float3 c1  = (2 * colorB - 1) * OB;
                // float3 OB1 = (1 - OB) * step(colorA , 2 * colorB);
                // float3 c2 = colorA * OB1;
                // float3 OB2 = 1 -  step(colorA ,2 * colorB);
                // float3 c3 = 2 * colorB * OB2;
                // finalRGBA = float4(c1 + c2 + c3,1);
                
                // 13. 实色混合 (Hard Mix)
                // float3 OA = step(colorB, 1 - colorA);
                // float3 c1 = 0 * OA;
                // float3 OA1 = step(1 - colorA,colorB);
                // float3 c2 = 1 * OA1;
                // finalRGBA = float4(c1 + c2,1);
                
                // 14. 差值 (Difference)
                // finalRGBA = float4(abs(colorB.xyz - colorA.xyz),1);
                
                // 15. 排除 (Exclusion)
                // finalRGBA = float4(colorB + colorA - 2 * colorB * colorA);
                
                // 16. 色相 (Hue)
                // float3 c1 = float3(colorBHSV.x,colorAHSV.yz);
                // c1 = HSV2RGB(c1);
                // finalRGBA = half4(c1,1);
 
                return finalRGBA;
            }
            ENDCG
        }
    }
}