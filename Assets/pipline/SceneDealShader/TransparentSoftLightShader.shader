Shader "Custom/TransparentSoftLight"
{
    Properties
    {
        _MainTex ("主纹理", 2D) = "white" {} // 主纹理，用于控制混合区域
        _BlendTex ("柔光纹理", 2D) = "white" {} // 柔光纹理，用于产生柔光效果
        _BlendAmount ("柔光强度", Range(0,1)) = 1 // 控制柔光效果的强度
        _Opacity ("不透明度", Range(0,1)) = 1 // 控制整体不透明度
    }

    SubShader
    {
        Tags 
        { 
            "Queue" = "Transparent" // 使用透明队列
            "RenderType" = "Transparent" // 渲染类型为透明
            "PreviewType" = "Plane" // 在预览窗口中显示为平面
            "CanUseSpriteAtlas" = "True" // 支持精灵图集
        }

        Cull Off // 关闭剔除
        Lighting Off // 关闭光照
        ZWrite Off // 关闭深度写入
        Blend SrcAlpha OneMinusSrcAlpha // 使用标准透明混合模式

        // 获取背景
        GrabPass { "_BackgroundTex" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 顶点着色器输入结构
            struct appdata_t
            {
                float4 vertex   : POSITION; // 顶点位置
                float4 color    : COLOR; // 顶点颜色
                float2 texcoord : TEXCOORD0; // 主纹理UV坐标
                float4 grabPos  : TEXCOORD1; // 屏幕空间位置
            };

            // 顶点着色器输出结构
            struct v2f
            {
                float4 vertex   : SV_POSITION; // 裁剪空间位置
                fixed4 color    : COLOR; // 顶点颜色
                float2 texcoord : TEXCOORD0; // 主纹理UV坐标
                float4 grabPos  : TEXCOORD1; // 屏幕空间位置
            };

            sampler2D _MainTex; // 主纹理采样器
            sampler2D _BlendTex; // 柔光纹理采样器
            sampler2D _BackgroundTex; // 背景纹理采样器
            float4 _MainTex_ST; // 主纹理的缩放和偏移
            float4 _BlendTex_ST; // 柔光纹理的缩放和偏移
            fixed _BlendAmount; // 柔光强度
            fixed _Opacity; // 不透明度

            // 柔光混合函数
            fixed3 SoftLight(fixed3 base, fixed3 blend)
            {
                fixed3 result;
                UNITY_UNROLL
                for(int i = 0; i < 3; i++)
                {
                    if(blend[i] <= 0.5)
                    {
                        // 当混合色小于等于0.5时，使用减暗公式
                        result[i] = base[i] - (1 - 2 * blend[i]) * base[i] * (1 - base[i]);
                    }
                    else
                    {
                        // 当混合色大于0.5时，使用增亮公式
                        fixed D = (base[i] <= 0.25) ? 
                            ((16 * base[i] - 12) * base[i] + 4) * base[i] :
                            sqrt(base[i]);
                        result[i] = base[i] + (2 * blend[i] - 1) * (D - base[i]);
                    }
                }
                return result;
            }

            // 顶点着色器
            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex); // 转换到裁剪空间
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex); // 计算主纹理UV
                OUT.color = IN.color; // 传递顶点颜色
                OUT.grabPos = ComputeGrabScreenPos(OUT.vertex); // 计算屏幕空间位置
                return OUT;
            }

            // 片元着色器
            fixed4 frag(v2f IN) : SV_Target
            {
                // 采样背景
                fixed4 bgColor = tex2Dproj(_BackgroundTex, IN.grabPos);
                
                // 采样主纹理和柔光纹理
                fixed4 mainColor = tex2D(_MainTex, IN.texcoord) * IN.color;
                fixed4 blendColor = tex2D(_BlendTex, IN.texcoord);
                
                // 应用柔光混合
                fixed3 softLightColor = SoftLight(bgColor.rgb, blendColor.rgb);
                
                // 在背景颜色和柔光结果之间插值
                fixed3 finalColor = lerp(bgColor.rgb, softLightColor, _BlendAmount * blendColor.a);
                
                // 使用主纹理的alpha通道作为遮罩
                fixed alpha = mainColor.a * _Opacity;
                
                // 预乘alpha
                finalColor *= alpha;
                
                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
} 