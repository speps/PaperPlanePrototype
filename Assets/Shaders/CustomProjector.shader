Shader "Custom/Projector" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowTex ("Cookie", 2D) = "gray" { TexGen ObjectLinear }
        _FalloffTex ("FallOff", 2D) = "white" { TexGen ObjectLinear	}
    }

    Subshader {
        Tags { "RenderType"="Transparent" }
        Pass {
            ZWrite Off
            Offset -1, -1

            Fog { Color (1, 1, 1) }
            AlphaTest Greater 0
            ColorMask RGB
            Blend Zero OneMinusSrcAlpha
            SetTexture [_ShadowTex] {
                combine texture, ONE - texture
                Matrix [_Projector]
            }
            SetTexture [_ShadowTex] {
                constantColor [_Color]
                combine previous * constant, previous * constant
                Matrix [_Projector]
            }
            SetTexture [_FalloffTex] {
                constantColor (1,1,1,0)
                combine previous lerp (texture) constant
                Matrix [_ProjectorClip]
            }
        }
    }
}
