Shader "Custom/Flames"
{
    Properties
    {
        _MainTex("Main texture", 2D) = "black" {}
    }
    Category
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        LOD 300
        ZWrite Off
        Cull Off
        SubShader
        {
            Pass
            {
                Blend SrcAlpha OneMinusSrcAlpha
                SetTexture [_MainTex] { combine texture }
            }
        }
    }
}
