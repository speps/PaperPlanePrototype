Shader "Custom/Plane"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _SelfIllumMap ("Self-illum (RGB)", 2D) = "black" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        CGPROGRAM
            #pragma surface PlaneMain Plane nolightmap novertexlights noambient noforwardadd

            half4 LightingPlane(SurfaceOutput s, half3 lightDir, half atten)
            {
                half diffuse = saturate(dot(s.Normal, lightDir));
                return half4(lerp(half3(0.0f, 0.0f, 0.0f), diffuse.xxx, diffuse) * s.Albedo, 1.0f);
            }

            struct Input
            {
                float2 uv_MainTex;
                float2 uv_SelfIllumMap;
            };

            sampler2D _MainTex;
            sampler2D _SelfIllumMap;

            void PlaneMain(Input IN, inout SurfaceOutput o)
            {
                o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
                o.Emission = tex2D(_SelfIllumMap, IN.uv_SelfIllumMap).rgb;
            }
        ENDCG
    }
}
