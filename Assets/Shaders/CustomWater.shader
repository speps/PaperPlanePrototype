Shader "Custom/CustomWater" {
	Properties {
		_BaseColor ("Main Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			half4 c = tex2D (_MainTex, IN.uv_MainTex);
			o.Albedo = _BaseColor.rgb * c.rgb;
			o.Alpha = _BaseColor.a * c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
