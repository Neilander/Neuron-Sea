Shader "CS0102/01MiniShader"
{
    Properties
    {
        _MyFloat("浮点数",Float)=0.0
        _Range("Range",Range(0.0,1.0))=0.0
        _Vector("Vector",Vector)=(1,1,1,1)
        _Color("Color",Color)=(0.5,0.5,0.5,0.5)
        _Texture("Texture",2D)="black"{}
    }
    SubShader
    {
        Pass
        {
            
        }
    }
}