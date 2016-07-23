Shader "Custom/Leaves"
{
    Properties
    {
        _BaseColor ("Main Color", Color) = (0.7,0.7,0.7,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _LightMap ("Lightmap (RGB)", 2D) = "white" {}
        _SplashMap ("Splash (set by code)", 2D) = "white" {}
        _FillTex ("Fill (set by code)", 2D) = "white" {}

        _AlphaCutoff ("Base Alpha cutoff", Range (.5,.9)) = 0.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Cull Off
        
        CGPROGRAM
            #pragma surface LeavesMain Leaves alpha nolightmap novertexlights noambient noforwardadd

            half4 LightingLeaves(SurfaceOutput s, half3 lightDir, half atten)
            {
                return half4(s.Albedo, s.Alpha);
            }

            struct Input {
                float2 uv_MainTex;
                float2 uv2_LightMap;
                float2 uv_SplashMap;
                float2 uv_FillTex;
            };
            
            sampler2D _MainTex;
            sampler2D _LightMap;
            sampler2D _SplashMap;
            sampler2D _FillTex;
            float4 _BaseColor;
            float _AlphaCutoff;

            void LeavesMain(Input IN, inout SurfaceOutput o)
            {
                float4 colMain = tex2D(_MainTex, IN.uv_MainTex);
                float4 colLight = tex2D(_LightMap, IN.uv2_LightMap);
                float4 colSplash = tex2D(_SplashMap, IN.uv_SplashMap);
                float4 colFill = tex2D(_FillTex, IN.uv_FillTex);

                o.Albedo = _BaseColor.rgb * lerp(colMain.rgb, colFill.rgb, 1.0f - Luminance(colSplash.rgb)) * colLight.rgb;
                o.Alpha = _BaseColor.a * step(_AlphaCutoff, colMain.a) * colMain.a;
                o.Emission = half3(0.0f, 0.0f, 0.0f);
            }
        ENDCG
    }
}
