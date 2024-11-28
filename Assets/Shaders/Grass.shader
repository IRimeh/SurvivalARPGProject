Shader "Unlit/Grass"
{
    Properties
    {
        _GrassInstance("Grass Instance", float) = 0
        _TopGrassCol("Top Grass Col", Color) = (1,1,1,1)
        _BottomGrassCol("Bottom Grass Col", Color) = (1,1,1,1)
        _VeryTipColor("Very Tip Col", Color) = (1,1,1,1)

        [Space(10)]
        _TopGrassCol1("Top Grass Col 1", Color) = (1,1,1,1)
        _BottomGrassCol1("Bottom Grass Col 1", Color) = (1,1,1,1)
        _VeryTipColor1("Very Tip Col 1", Color) = (1,1,1,1)

        [Space(20)]
        _WindColor("Wind Color", Color) = (1,1,1,1)
        _TrampledGrassColor("Trampled Grass Color", Color) = (1,1,1,1)

        [Header(Textures)]
        _MainTex("Texture", 2D) = "white" {}
        _HeightTex("Height Tex", 2D) = "white" {}
        _HeightVariationTex("Height Variation Tex", 2D) = "white" {}
        _HeightGradient("HeightGradient", 2D) = "white" {}

        [Space(20)]
        _HeightImpact("Height Impact", Range(0, 1)) = 0.25
        _MaxGrassOffset("Max Grass Offset", float) = 0.05
        _MinGrassOffset("Min Grass Offset", float) = 0.05
        _WindSpeed("Wind Speed", float) = 0.01
        _WindOffset("Wind Offset", float) = 0.01
        _WindDirection("Wind Direction", Vector) = (1,1,1,1)
        _ShadowAtten("Shadow Attenuation", float) = 0.1

        [Header(Terrain Masking)]
        [Toggle(TERRAIN_MASKING)]
        _TerrainMasking("Terrain Masking", float) = 0
        _Cutoff("Cutoff", float) = 0.05
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalRenderPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float3 wPos : ATTR0;
                float2 depth : ATTR1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_HeightTex);
            SAMPLER(sampler_HeightTex);

            TEXTURE2D(_HeightGradient);
            SAMPLER(sampler_HeightGradient);

            TEXTURE2D(_HeightVariationTex);
            SAMPLER(sampler_HeightVariationTex);

            TEXTURE2D(_SplatMap);
            SAMPLER(sampler_SplatMap);

            float4 _MainTex_ST;
            float4 _HeightGradient_ST;
            float4 _HeightVariationTex_ST;

            float _MaxGrassOffset;
            float _MinGrassOffset;

            int _GrassInstance;
            int _Layers;
            float _ShadowAtten;
            float _CameraFarPlane;

            //Colors
            float4 _TopGrassCol;
            float4 _BottomGrassCol;
            float4 _VeryTipColor;

            float4 _TopGrassCol1;
            float4 _BottomGrassCol1;
            float4 _VeryTipColor1;

            float4 _WindColor;
            float4 _TrampledGrassColor;

            float4 _ShadowColor;

            //Wind
            float2 _WindDirection;
            float _HeightImpact;
            float _WindOffset;
            float _WindSpeed;

            //Terrain masking
            float2 _TerrainSize;
            float3 _TerrainPos;
            float _Cutoff;

            v2f vert(appdata v)
            {
                v2f o;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normal.xyz);

                // float2 heightUvs = float2(1 - v.uv.x, 1 - v.uv.y);
                // float2 depthTrample = SAMPLE_TEXTURE2D_LOD(_HeightTex, sampler_HeightTex, heightUvs, 0);
                // float depth = depthTrample.r;
                // float trample = depthTrample.g;
                // float heightOffset = depth * _CameraFarPlane - (_CameraFarPlane * 0.5f);
                //
                // float perc = ((float)_GrassInstance) / (float)_Layers;
                // o.wPos = positionInputs.positionWS;
                // float offset = SAMPLE_TEXTURE2D_LOD(_HeightVariationTex, sampler_HeightVariationTex, o.wPos.xz * _HeightVariationTex_ST.xy, 0).r;
                // offset = clamp(pow(offset, 0.75f), 0, 0.8);
                // o.wPos.xz += perc * offset;
                //
                // float heightVariation = SAMPLE_TEXTURE2D_LOD(_HeightVariationTex, sampler_HeightVariationTex, o.wPos.xz * _HeightVariationTex_ST.xy, 0).r;
                // float grassOffset = lerp(_MinGrassOffset, _MaxGrassOffset, heightVariation) * (1.1f - trample);
                //
                // o.wPos += float3(0, grassOffset * _GrassInstance + heightOffset, 0);
                // v.vertex = mul(unity_WorldToObject, float4(o.wPos, 1.0f));
                //
                // positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                o.vertex = positionInputs.positionCS;
                // o.normal.xyz = normalInputs.normalWS;
                // o.uv = v.uv;
                // o.depth.x = depth;
                // o.depth.y = perc;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 heightUvs = float2(1 - i.uv.x, 1 - i.uv.y);

                // Wind
                float2 windDir = normalize(_WindDirection);
                float offset = SAMPLE_TEXTURE2D(_HeightVariationTex, sampler_HeightVariationTex, i.wPos.xz * _HeightVariationTex_ST.zw + windDir * _Time.x * _WindSpeed).r;
                offset *= SAMPLE_TEXTURE2D(_HeightVariationTex, sampler_HeightVariationTex, i.wPos.xz * _HeightVariationTex_ST.zw + windDir * float2(-1, 1) * _Time.x * 1.25f * _WindSpeed).r;
                offset *= 0.75f;
                offset += SAMPLE_TEXTURE2D(_HeightVariationTex, sampler_HeightVariationTex, i.wPos.xz * 0.01f + windDir * _Time.x * 1.25f * _WindSpeed).r * 0.25f;
                offset = saturate(offset);

                //Trample
                float4 heightTex = SAMPLE_TEXTURE2D(_HeightTex, sampler_HeightTex, heightUvs);
                float trample = heightTex.g;
                float3 wPos = i.wPos;

                // Grass Tex
                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.wPos.xz * _MainTex_ST.xy + offset * windDir * _WindOffset * i.depth.y);
                float3 normal = i.normal;

                float perc = ((float)_GrassInstance) / (float)_Layers;
                if (perc > baseTex.r) discard;

                // light
                float4 shadowCoord = TransformWorldToShadowCoord(wPos.xyz);
                Light light = GetMainLight(shadowCoord);
                Light light2 = GetAdditionalLight(0, wPos);

                // lightning calculation
                float NdotL = saturate(saturate(dot(normal, light.direction)) + _ShadowAtten);
                float NdotL2 = saturate(saturate(dot(normal, light2.direction)) + _ShadowAtten);
                float4 diffuse = NdotL;
                float3 diffuse2 = NdotL2;

                float shadowTint = (1 - light.shadowAttenuation);
                float shadowTint2 = (1 - light2.shadowAttenuation);

                // Output color
                float4 output;
                output.rgb = (diffuse * light.color);
                output.rgb += (diffuse2 * light2.color);


                float colorVariationTex = SAMPLE_TEXTURE2D(_HeightVariationTex, sampler_HeightVariationTex, i.wPos.xz * _HeightVariationTex_ST.xy).r;
                float4 topGrass = lerp(_TopGrassCol, _TopGrassCol1, colorVariationTex);
                float4 bottomGrass = lerp(_BottomGrassCol, _BottomGrassCol1, colorVariationTex);
                float4 tipGrass = lerp(_VeryTipColor, _VeryTipColor1, colorVariationTex);


                float4 heightGradientCol = SAMPLE_TEXTURE2D(_HeightGradient, sampler_HeightGradient, float2(i.depth.x * _HeightGradient_ST.x + _HeightGradient_ST.z, 0));
                float4 grassCol = lerp(bottomGrass, topGrass, perc);
                grassCol = lerp(grassCol, tipGrass, saturate(pow(perc + 0.1f, 8)));
                grassCol = lerp(0.0f, grassCol, heightGradientCol * _HeightImpact + (1.0f - _HeightImpact));

                grassCol.rgb = lerp(grassCol.rgb, _WindColor.rgb, _WindColor.a * offset);
                grassCol.rgb = lerp(grassCol.rgb, _TrampledGrassColor.rgb, min(trample, _TrampledGrassColor.a));

                
                grassCol.rgb *= 1.0f - (trample * 0.5f);
                output.rgb *= grassCol;
                output.rgb = lerp(output.rgb, _ShadowColor.rgb, (1 - light.shadowAttenuation * light.distanceAttenuation) * _ShadowColor.a);
                //output.rgb = lerp(output.rgb, _ShadowColor.rgb, (1 - light2.shadowAttenuation * light2.distanceAttenuation) * _ShadowColor.a * max(light2.color, 0.2f));
                output.a = 1;

                return float4(1,1,1,1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}