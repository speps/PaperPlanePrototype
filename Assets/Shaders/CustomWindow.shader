Shader "Custom/Window"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _LightMap ("Lightmap (RGB)", 2D) = "white" {}
        _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
        _SpecularColor1 ("Specular Color 1", Color) = (0.5, 0.5, 0.5, 1)
        _SpecularFactor1 ("Specular Factor 1", Range (0.01, 2)) = 0.078125
        _SpecularColor2 ("Specular Color 2", Color) = (0.5, 0.5, 0.5, 1)
        _SpecularFactor2 ("Specular Factor 2", Range (0.01, 2)) = 0.078125
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        CGPROGRAM
            #pragma surface WindowMain Window nolightmap novertexlights noambient noforwardadd
            
            float4 _Color;
            float _Shininess;
            float4 _SpecularColor1;
            float _SpecularFactor1;
            float4 _SpecularColor2;
            float _SpecularFactor2;

            half4 LightingWindow(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
            {
                half3 h = normalize(lightDir + viewDir);
                float nh = saturate(dot(h, s.Normal));
                float spec = pow(nh, _Shininess * 128) * s.Specular;

                half3 color = _Color * s.Albedo;
                half3 finalSpec = spec * _SpecularFactor1 * _SpecularColor1.rgb + spec * _SpecularFactor2 * _SpecularColor2.rgb;

                return half4(color + finalSpec, 1.0f);
            }

            struct Input
            {
                float2 uv_MainTex;
                float2 uv2_LightMap;
            };

            sampler2D _MainTex;
            sampler2D _LightMap;

            void WindowMain(Input IN, inout SurfaceOutput o)
            {
                float4 colMain = tex2D(_MainTex, IN.uv_MainTex);
                float4 colLight = tex2D(_LightMap, IN.uv2_LightMap);

                o.Albedo = colMain.rgb * colLight.rgb;
                o.Alpha = colMain.a;
                o.Specular = Luminance(colLight.rgb);
                o.Emission = half3(0.0f, 0.0f, 0.0f);
            }
        ENDCG
    }
}
