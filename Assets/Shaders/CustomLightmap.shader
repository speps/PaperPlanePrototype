Shader "Custom/Lightmapped"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _LightMap ("Lightmap (RGB)", 2D) = "white" {}
        _SplashMap ("Splash (set by code)", 2D) = "white" {}
        _FillTex ("Fill (set by code)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        CGPROGRAM
            #pragma surface LightmappedMain Lightmapped nolightmap novertexlights noambient noforwardadd

            half4 LightingLightmapped(SurfaceOutput s, half3 lightDir, half atten)
            {
                return half4(s.Albedo, 1.0f);
            }

            struct Input
            {
                float2 uv_MainTex;
                float2 uv2_LightMap;
                float2 uv_SplashMap;
                float2 uv_FillTex;
            };
            
            sampler2D _MainTex;
            sampler2D _LightMap;
            sampler2D _SplashMap;
            sampler2D _FillTex;

            void LightmappedMain(Input IN, inout SurfaceOutput o)
            {
                float4 colMain = tex2D(_MainTex, IN.uv_MainTex);
                float4 colLight = tex2D(_LightMap, IN.uv2_LightMap);
                float4 colSplash = tex2D(_SplashMap, IN.uv_SplashMap);
                float4 colFill = tex2D(_FillTex, IN.uv_FillTex);

                o.Albedo = lerp(colMain.rgb, colFill.rgb, 1.0f - Luminance(colSplash.rgb)) * colLight.rgb;
                o.Emission = half3(0.0f, 0.0f, 0.0f);
            }
        ENDCG
    }
}
