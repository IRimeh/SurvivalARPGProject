Shader "ParticlesMasterShader"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        
        [Toggle(TEXTURE_ON)]_ToggleTexture("Texture On", float) = 0
        _MainTex ("Texture", 2D) = "white" {}

        [Toggle(DISSOLVE_ON)]_ToggleDissolve("Dissolve On", float) = 0
        _DissolveTex("Dissolve Tex", 2D) = "white" {}
        _DissolveEdge("Dissolve Edge", Range(0, 1)) = 0

        [Toggle(EMISSION_ON)]_ToggleEmission("Emission On", float) = 0
        [Toggle(CUSTOM_EMISSION_COLOR_ON)]_ToggleCustomEmissionColor("Custom Emission Color On", float) = 0
        [HDR]_EmissionColor("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity("Intensity", float) = 1
        
        [Toggle(ALPHA_CLIPPING)]_ToggleCutout("Toggle Cutout", float) = 0
        _Cutout("Cutout", Range(0, 1)) = 0.5

        [Toggle(TOGGLE_STENCIL)]_ToggleStencil("Toggle Stencil", float) = 0
        _StencilRef("Stencil Ref", int) = 1

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 2

        [HideInInspector] _SourceBlend("Source blend", float) = 5
        [HideInInspector] _DestBlend("Destination blend", float) = 10
        [HideInInspector] _ZWrite("ZWrite", float) = 0
        [HideInInspector] _SurfaceType("Surface type", float) = 1
        [HideInInspector] _BlendMode("Blend Mode", float) = 0
        [HideInInspector] _StencilOp("Stencil Operation", float) = 0
    }
    CustomEditor "ParticleMasterShaderGUI"
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
        ZWrite [_ZWrite]
        Blend [_SourceBlend] [_DestBlend]
        Cull [_CullMode]
        LOD 100

        Stencil
        {
            Ref [_StencilRef]
            Pass [_StencilOp]
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma shader_feature TEXTURE_ON
            #pragma shader_feature DISSOLVE_ON
            #pragma shader_feature HARD_DISSOLVE_ON
            #pragma shader_feature EMISSION_ON
            #pragma shader_feature CUSTOM_EMISSION_COLOR_ON
            #pragma shader_feature ALPHA_CLIPPING

            struct appdata
            {
                float4 vertex : POSITION;
                float3 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float3 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            half4 _MainTex_ST;

            sampler2D _DissolveTex;
            half4 _DissolveTex_ST;
            half _DissolveEdge;

            half4 _Color;
            half _Cutout;

            half4 _EmissionColor;
            half _EmissionIntensity;
            half _BlendMode;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = i.color * _Color;
                #ifdef TEXTURE_ON
                    half4 tex = tex2D(_MainTex, i.uv.xy * _MainTex_ST.xy);
                    col *= tex;
                #endif

                #ifdef DISSOLVE_ON
                    half4 dissolve = tex2D(_DissolveTex, i.uv.xy * _DissolveTex_ST.xy).r;

                    i.uv.z = (i.uv.z * (1.0f + (_DissolveEdge * 2.0f))) - _DissolveEdge;

                    if(_BlendMode == 1)
                    {
                        col.rgb -= 1.0f - smoothstep(i.uv.z, i.uv.z + _DissolveEdge, dissolve);
                        col = saturate(col);
                    }
                    else
                        col.a -= 1.0f - smoothstep(i.uv.z, i.uv.z + _DissolveEdge, dissolve);
                #endif

                #ifdef EMISSION_ON
                    #if defined(CUSTOM_EMISSION_COLOR_ON)
                        col.rgb *= _EmissionColor;
                    #endif
                    #if !defined(CUSTOM_EMISSION_COLOR_ON)
                        col.rgb *= _EmissionIntensity;
                    #endif
                #endif

                #ifdef ALPHA_CLIPPING
                    if (col.a < _Cutout)
                        discard;
                #endif

                col.a = saturate(col.a);
                return col;
            }
            ENDCG
        }
    }
}
