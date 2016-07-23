Shader "Custom/Skydome"
{
    Properties
    {
        _Tint ("Tint Color", Color) = (0.5, 0.5, 0.5, 0.5)
        _Blend ("Blend", Range(0.0, 1.0)) = 0.5
        _Tex0 ("Tex at 0", 2D) = "white" {}
        _Tex1 ("Tex at 1", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Background" }
        Fog { Mode Off }
        Cull Off
        ZWrite Off
        Lighting Off
        Color [_Tint]
        Pass
        {
            SetTexture [_Tex0] { combine texture }
            SetTexture [_Tex1] { constantColor (0,0,0,[_Blend]) combine texture lerp(constant) previous }
            SetTexture [_Tex1] { combine previous +- primary, previous * primary }
        }
    }

}