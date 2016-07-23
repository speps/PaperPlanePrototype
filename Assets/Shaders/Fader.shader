// Upgrade NOTE: replaced 'glstate.matrix.mvp' with 'UNITY_MATRIX_MVP'
// Upgrade NOTE: replaced 'glstate.matrix.texture[0]' with 'UNITY_MATRIX_TEXTURE0'
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'
// Upgrade NOTE: replaced 'texRECT' with 'tex2D'

Shader "Custom/Fader" 
{
	Properties {
		_MainTex("Base (RGB)", RECT) = "white" {}
		_Fade("Fade", Float) = 0.65
	}
	SubShader 
	{
		Pass
		{
			ZTest Always Cull Off ZWrite Off Fog {Mode Off}
			
CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct v2f
{
	float4 pos : POSITION;
	float2 uv : TEXCOORD0;
};

uniform sampler2D _MainTex;
uniform float _Fade;

v2f vert (appdata_img v)
{
	v2f o;
	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
	o.uv = MultiplyUV (UNITY_MATRIX_TEXTURE0, v.texcoord);
	return o;
}

half4 frag (v2f i): COLOR
{
	half4 col = tex2D(_MainTex, i.uv);
	col.rgb = lerp(col.rgb, half3(1,1,1),_Fade);
	return col;
}

ENDCG
		}
	}
	Fallback off
}
