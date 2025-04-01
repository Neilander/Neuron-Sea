Shader "Custom/SceneMultiplyBlend"
{
    // 定义材质面板中可调节的属性
    Properties
    {
        _MultiplyTex ("正片叠底图层", 2D) = "white" {}  // 用于正片叠底混合的纹理
        _MultiplyOpacity ("正片叠底不透明度", Range(0,1)) = 1.0  // 控制混合效果的整体不透明度
        _MultiplyStrength ("正片叠底强度", Range(0,2)) = 1.0  // 控制混合效果的强度
        _MultiplyScale ("正片叠底缩放", Range(0.1,2)) = 1  // 控制纹理的缩放比例
        _MultiplyOffset ("正片叠底位置", Vector) = (0,0,0,0)  // 控制纹理的偏移位置
    }
    
    SubShader
    {
        // 设置渲染标签
        Tags { 
            "RenderType"="Transparent"  // 设置为透明渲染
            "Queue"="Overlay"  // 在Overlay队列中渲染，确保在所有物体之后渲染
            "IgnoreProjector"="True"  // 忽略投影器
        }
        // 设置混合模式为DstColor Zero，实现正片叠底效果
        // DstColor表示目标颜色（背景），Zero表示源颜色（当前shader）为0
        Blend DstColor Zero
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert  // 顶点着色器
            #pragma fragment frag  // 片段着色器
            #include "UnityCG.cginc"
            
            // 顶点着色器输入结构
            struct appdata
            {
                float4 vertex : POSITION;  // 顶点位置
                float2 uv : TEXCOORD0;     // UV坐标
                float4 color : COLOR;      // 顶点颜色
            };
            
            // 顶点着色器输出结构
            struct v2f
            {
                float2 uv : TEXCOORD0;     // UV坐标
                float4 vertex : SV_POSITION; // 裁剪空间顶点位置
                float4 color : COLOR;      // 顶点颜色
            };
            
            // 声明变量
            sampler2D _MultiplyTex;        // 正片叠底纹理采样器
            float4 _MultiplyTex_ST;        // 纹理的缩放和偏移参数
            float _MultiplyOpacity;        // 不透明度
            float _MultiplyStrength;       // 强度
            float _MultiplyScale;          // 缩放
            float4 _MultiplyOffset;        // 偏移
            
            // 顶点着色器
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);  // 转换到裁剪空间
                o.uv = TRANSFORM_TEX(v.uv, _MultiplyTex);   // 计算纹理UV
                o.color = v.color;                          // 传递顶点颜色
                return o;
            }
            
            // 片段着色器
            fixed4 frag (v2f i) : SV_Target
            {
                // 计算最终的UV坐标，应用缩放和偏移
                float2 multiplyUV = (i.uv - 0.5) * _MultiplyScale + 0.5 + _MultiplyOffset.xy;
                multiplyUV = saturate(multiplyUV);  // 限制UV在0-1范围内
                
                // 采样正片叠底纹理
                fixed4 multiplyColor = tex2D(_MultiplyTex, multiplyUV);
                
                // 初始化最终颜色为白色
                fixed4 finalColor = fixed4(1, 1, 1, 1);
                
                // 根据纹理的alpha通道和不透明度进行插值混合
                // lerp函数在白色和混合颜色之间进行插值
                finalColor.rgb = lerp(finalColor.rgb, multiplyColor.rgb * _MultiplyStrength, multiplyColor.a * _MultiplyOpacity);
                
                // 应用顶点颜色并返回最终结果
                return finalColor * i.color;
            }
            ENDCG
        }
    }
}