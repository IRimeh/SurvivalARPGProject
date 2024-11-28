Shader "Unlit/HullGrass"
{
    Properties
    {
        _TopGrassCol("Top Grass Col", Color) = (1,1,1,1)
        _BottomGrassCol("Bottom Grass Col", Color) = (1,1,1,1)
        
        _MainTex ("Texture", 2D) = "white" {}
        _GrassMaskTex("Grass Mask", 2D) = "white" {}
        _GrassHeightVariationTex("Grass Height Variation", 2D) = "white" {}
        
        _GrassMaxHeight("Grass Max Height", Range(0, 1)) = 1
        _GrassHeightVariationIntensity("Grass Height Variation Intensity", Range(0, 1)) = 1
        _GrassMaskMin("Grass Mask Min", Range(0, 1)) = 0.2
        _GrassMaskMax("Grass Mask Max", Range(0, 1)) = 0.25
        
        [Header(Wind)]
        _WindTex("Wind Tex", 2D) = "white" {}
        _WindCol("Wind Color", Color) = (1,1,1,1)
        _WindStrength("Wind Strength", Range(0, 1)) = 1
        _WindSpeed("Wind Speed", float) = 5
        
        [Header(Normals)]
        _NormalStrength("Normal Strength", Range(0, 2)) = 1
        [NoScaleOffset]_NormalTex("Normal", 2D) = "bump" {}
        
        [Header(Specular)]
        _SpecularIntensity("Specular Color", float) = 3
        _Gloss("Glossiness", Range(0,1)) = 1
        
        [Space(20)]
        _AmbientLightIntensity("Ambient Light Intensity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalRenderPipeline" }
        ZWrite On
        
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float3 normalWS : ATTR1;
                half4 tspace0 : ATTR2; // wpos x in w
                half4 tspace1 : ATTR3; // wpos y in w
                half4 tspace2 : ATTR4; // wpos z in w
                float4 shadowCoords : TEXCOORD1;
            };

            float _GrassHeight0To1;

            float4 _TopGrassCol;
            float4 _BottomGrassCol;
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            TEXTURE2D(_GrassMaskTex);
            SAMPLER(sampler_GrassMaskTex);
            float4 _GrassMaskTex_ST;

            TEXTURE2D(_GrassHeightVariationTex);
            SAMPLER(sampler_GrassHeightVariationTex);
            float4 _GrassHeightVariationTex_ST;

            float _GrassMaxHeight;
            float _GrassHeightVariationIntensity;
            float _GrassMaskMin;
            float _GrassMaskMax;

            TEXTURE2D(_WindTex);
            SAMPLER(sampler_WindTex);
            float4 _WindTex_ST;
            float4 _WindCol;
            float _WindStrength;
            float _WindSpeed;

            TEXTURE2D(_NormalTex);
            SAMPLER(sampler_NormalTex);
            float4 _NormalTex_ST;
            float _NormalStrength;

            float _SpecularIntensity;
            float _Gloss;

            float _AmbientLightIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(v.vertex.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(v.normal.xyz);

                o.tspace0 = half4(normalInputs.tangentWS.x, normalInputs.bitangentWS.x, normalInputs.normalWS.x, positionInputs.positionWS.x);
                o.tspace1 = half4(normalInputs.tangentWS.y, normalInputs.bitangentWS.y, normalInputs.normalWS.y, positionInputs.positionWS.y);
                o.tspace2 = half4(normalInputs.tangentWS.z, normalInputs.bitangentWS.z, normalInputs.normalWS.z, positionInputs.positionWS.z);

                // Normal map
                float3 normalTex = UnpackNormal(SAMPLE_TEXTURE2D_LOD(_NormalTex, sampler_NormalTex, v.uv * _MainTex_ST.xy + _MainTex_ST.zw, 0));
                float3 normal;
                normal.x = dot(o.tspace0.xyz, normalTex);
                normal.y = dot(o.tspace1.xyz, normalTex);
                normal.z = dot(o.tspace2.xyz, normalTex);
                o.normal = normalize(lerp(v.normal.xyz, normal, _NormalStrength));
                
                float4 shadowCoordinates = GetShadowCoord(positionInputs);
                o.shadowCoords = shadowCoordinates;
                o.vertex = positionInputs.positionCS;
                o.normalWS = normalInputs.normalWS;
                o.uv = v.uv;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float3 wPos = half3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
                float2 uv = float2(wPos.x, wPos.z);

                float2 windUVs = uv + _Time.z * _WindSpeed;
                float2 windUVs2 = uv + _Time.z * _WindSpeed * 2.0f;
                float windTex = SAMPLE_TEXTURE2D(_WindTex, sampler_WindTex, windUVs * _WindTex_ST.xy).r;
                float windTex2 = SAMPLE_TEXTURE2D(_WindTex, sampler_WindTex, windUVs2 * _WindTex_ST.xy * 2.0f).r;
                float wind = windTex + windTex2 * .5f;
                uv += _WindTex_ST.zw * wind * _GrassHeight0To1 * _WindStrength * 1.0f;
                
                float grassTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv * _MainTex_ST.xy + _MainTex_ST.zw).r;
                float grassMask = SAMPLE_TEXTURE2D(_GrassMaskTex, sampler_GrassMaskTex, uv * _GrassMaskTex_ST.xy + _GrassMaskTex_ST.zw).r;
                float grassHeightVariation = SAMPLE_TEXTURE2D(_GrassHeightVariationTex, sampler_GrassHeightVariationTex, uv * _GrassHeightVariationTex_ST.xy + _GrassHeightVariationTex_ST.zw).r;
                grassMask = smoothstep(_GrassMaskMin, _GrassMaskMax, grassMask);
                grassHeightVariation = lerp(grassHeightVariation, 1, _GrassHeightVariationIntensity);
                float grass = grassTex * grassMask * grassHeightVariation;

                if(grass <= _GrassHeight0To1 * _GrassMaxHeight)
                    discard;

                float3 col = lerp(_BottomGrassCol, _TopGrassCol, _GrassHeight0To1);

                // Lerp col to wind color
                col = lerp(col, _WindCol, smoothstep(0.5, 1.0f, wind) * _GrassHeight0To1);

                // Normal map
                float3 normalTex = UnpackNormal(SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, uv * _MainTex_ST.xy + _MainTex_ST.zw));
                float3 normal;
                normal.x = dot(i.tspace0.xyz, normalTex);
                normal.y = dot(i.tspace1.xyz, normalTex);
                normal.z = dot(i.tspace2.xyz, normalTex);
                normal = normalize(lerp(i.normalWS.xyz, normal, _NormalStrength));
                
                // Get the value from the shadow map at the shadow coordinates
                Light light = GetMainLight(i.shadowCoords);
                half shadowFade = GetMainLightShadowFade(wPos);
                half shadow = lerp(light.shadowAttenuation, 1, shadowFade);
                half3 diffuse = col * shadow * light.color;

                // Light calculation
                float3 lightDir = light.direction;
                float3 lightReflectDir = reflect(-lightDir, normal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - wPos);
                float NDotL = dot(normal.xyz, lightDir);
                float NDotLNorm = dot(normal, lightDir);
                float fresnel = dot(viewDir, normal.xyz);
                
                // Specular calculation
                float3 specular = saturate(dot(lightReflectDir, viewDir));
                specular = pow(specular, exp2(_Gloss * 12.0f)) * (light.color.rgb * _SpecularIntensity) * step(0.5f, light.shadowAttenuation);
                diffuse += specular * _GrassHeight0To1;

                // Ambient light
                half3 ambient = half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w);
                ambient *= _AmbientLightIntensity * col;
                
                return float4(diffuse + ambient, 1);
            }
            ENDHLSL
        }
    }
}
