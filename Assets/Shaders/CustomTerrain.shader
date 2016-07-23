Shader "Custom/Terrain"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _LightMap ("LightMap (RGB)", 2D) = "black" {}
        _SplatMap ("SplatMap (RGB)", 2D) = "white" {}
        _SplatR ("Splat R", 2D) = "white" {}
        _SplatG ("Splat G", 2D) = "white" {}
        _SplatB ("Splat B", 2D) = "white" {}
        _Drawings ("Drawings", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
            #pragma surface TerrainMain Terrain nolightmap novertexlights noambient noforwardadd
            
            half4 LightingTerrain(SurfaceOutput s, half3 lightDir, half atten)
            {
                return half4(s.Albedo, 1.0f);
            }

            struct Input
            {
                float4 color : COLOR;
                float2 uv_MainTex;
                float2 uv2_LightMap;
                float2 uv2_SplatMap;
                float2 uv_SplatR;
                float2 uv_SplatG;
                float2 uv_SplatB;
                float2 uv2_Drawings;
            };
            
            sampler2D _MainTex;
            sampler2D _LightMap;
            sampler2D _SplatMap;
            sampler2D _SplatR;
            sampler2D _SplatG;
            sampler2D _SplatB;
            sampler2D _Drawings;
            
            void TerrainMain(Input IN, inout SurfaceOutput o)
            {
                float4 colMain = tex2D(_MainTex, IN.uv_MainTex);
                float4 colSplat = tex2D(_SplatMap, IN.uv2_SplatMap);
                float4 colR = tex2D(_SplatR, IN.uv_SplatR);
                float4 colG = tex2D(_SplatG, IN.uv_SplatG);
                float4 colB = tex2D(_SplatB, IN.uv_SplatB);
                float4 colLight = tex2D(_LightMap, IN.uv2_LightMap);
                
                float4 colFinal = (colMain * (1.0 - (colSplat.r + colSplat.g + colSplat.b)))
                    + colR * colSplat.r
                    + colG * colSplat.g
                    + colB * colSplat.b
                    ;
                float4 colDrawings = tex2D(_Drawings, IN.uv2_Drawings);
                
                o.Albedo = lerp(colFinal * colLight, colDrawings, 1.0 - IN.color.r * IN.color.a).rgb;
                o.Emission = half3(0.0f, 0.0f, 0.0f);
            }
        ENDCG
    }
}
