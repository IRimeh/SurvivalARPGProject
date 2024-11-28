using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class ParticleMasterShaderGUI : ShaderGUI
{
    bool textureBool;
    bool dissolveBool;
    bool emissionBool;
    bool stencilBool;

    public enum SurfaceType
    {
        Opaque,
        Transparent
    }

    public enum BlendingMode
    {
        Alpha,
        Additive,
        PreMultiply,
        Multiply
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        Material material = materialEditor.target as Material;

        MaterialProperty _SurfaceType = FindProperty("_SurfaceType", properties);
        MaterialProperty _BlendMode = FindProperty("_BlendMode", properties);

        MaterialProperty _Color = FindProperty("_Color", properties);

        MaterialProperty _ToggleTexture = FindProperty("_ToggleTexture", properties);
        MaterialProperty _MainTex = FindProperty("_MainTex", properties);

        MaterialProperty _ToggleEmission = FindProperty("_ToggleEmission", properties);
        MaterialProperty _ToggleCustomEmissionColor = FindProperty("_ToggleCustomEmissionColor", properties);
        MaterialProperty _EmissionColor = FindProperty("_EmissionColor", properties);
        MaterialProperty _EmissionIntensity = FindProperty("_EmissionIntensity", properties);

        MaterialProperty _ToggleDissolve = FindProperty("_ToggleDissolve", properties);
        MaterialProperty _DissolveTex = FindProperty("_DissolveTex", properties);
        MaterialProperty _DissolveEdge = FindProperty("_DissolveEdge", properties);

        MaterialProperty _ToggleCutout = FindProperty("_ToggleCutout", properties);
        MaterialProperty _Cutout = FindProperty("_Cutout", properties);

        MaterialProperty _CullMode = FindProperty("_CullMode", properties);
        MaterialProperty _StencilRef = FindProperty("_StencilRef", properties);

        MaterialProperty _ToggleStencil = FindProperty("_ToggleStencil", properties);

        
        EditorGUI.BeginChangeCheck();
        _SurfaceType.floatValue = (int)(SurfaceType)EditorGUILayout.EnumPopup("Surface Type", (SurfaceType)_SurfaceType.floatValue);
        if(EditorGUI.EndChangeCheck())
            UpdateSurfaceType(material);

        EditorGUI.BeginChangeCheck();
        _BlendMode.floatValue = (int)(BlendingMode)EditorGUILayout.EnumPopup("Blend Mode", (BlendingMode)_BlendMode.floatValue);
        if(EditorGUI.EndChangeCheck())
            UpdateBlendMode(material);
        
        materialEditor.ShaderProperty(_Color, _Color.displayName);
        EditorGUILayout.Space(10);

        textureBool = EditorGUILayout.BeginFoldoutHeaderGroup(textureBool, "Texture");
        if (textureBool)
        {
            materialEditor.ShaderProperty(_ToggleTexture, _ToggleTexture.displayName);
            if (_ToggleTexture.floatValue > 0)
            {
                materialEditor.ShaderProperty(_MainTex, _MainTex.displayName);
            }
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        dissolveBool = EditorGUILayout.BeginFoldoutHeaderGroup(dissolveBool, "Dissolve");
        if (dissolveBool)
        {
            materialEditor.ShaderProperty(_ToggleDissolve, _ToggleDissolve.displayName);
            if (_ToggleDissolve.floatValue > 0)
            {
                materialEditor.ShaderProperty(_DissolveTex, _DissolveTex.displayName);
                materialEditor.ShaderProperty(_DissolveEdge, _DissolveEdge.displayName);
            }
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        emissionBool = EditorGUILayout.BeginFoldoutHeaderGroup(emissionBool, "Emission");
        if (emissionBool)
        {
            materialEditor.ShaderProperty(_ToggleEmission, _ToggleEmission.displayName);
            if (_ToggleEmission.floatValue > 0)
            {
                materialEditor.ShaderProperty(_ToggleCustomEmissionColor, _ToggleCustomEmissionColor.displayName);
                if (_ToggleCustomEmissionColor.floatValue > 0)
                    materialEditor.ShaderProperty(_EmissionColor, _EmissionColor.displayName);
                else
                    materialEditor.ShaderProperty(_EmissionIntensity, _EmissionIntensity.displayName);
            }
            EditorGUILayout.Space(10);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUI.BeginChangeCheck();
        stencilBool = EditorGUILayout.BeginFoldoutHeaderGroup(stencilBool, "Stencil");
        if(stencilBool)
        {
            materialEditor.ShaderProperty(_ToggleStencil, _ToggleStencil.displayName);
            if(_ToggleStencil.floatValue > 0)
                materialEditor.ShaderProperty(_StencilRef, _StencilRef.displayName);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        if(EditorGUI.EndChangeCheck())
        {
            if(_ToggleStencil.floatValue > 0)
                material.SetFloat("_StencilOp", (float)StencilOp.Replace);
            else
                material.SetFloat("_StencilOp", (float)StencilOp.Keep);
        }


        EditorGUILayout.Space(10);
        materialEditor.ShaderProperty(_ToggleCutout, _ToggleCutout.displayName);
        if (_ToggleCutout.floatValue > 0)
        {
            materialEditor.ShaderProperty(_Cutout, _Cutout.displayName);
        }

        materialEditor.ShaderProperty(_CullMode, _CullMode.displayName);
        materialEditor.RenderQueueField();
    }

    private void UpdateStencilPass(Material material)
    {
        StencilOp stencilOp = (StencilOp)material.GetFloat("_StencilOp");
        switch(stencilOp)
        {

        }

    }

    private void UpdateSurfaceType(Material material)
    {
        SurfaceType surface = (SurfaceType)material.GetFloat("_SurfaceType");
        switch(surface)
        {
            case SurfaceType.Opaque:
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_ZWrite", 1);
                material.SetShaderPassEnabled("ShadowCaster", true);
                break;
            case SurfaceType.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_ZWrite", 0);
                material.SetShaderPassEnabled("ShadowCaster", false);
                break;
        }
    }

    private void UpdateBlendMode(Material material)
    {
        BlendingMode blendMode = (BlendingMode)material.GetFloat("_BlendMode");
        switch(blendMode)
        {
            case BlendingMode.Alpha:
                material.SetInt("_SourceBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DestBlend", (int)BlendMode.OneMinusSrcAlpha);
                break;
            case BlendingMode.Additive:
                material.SetInt("_SourceBlend", (int)BlendMode.One);
                material.SetInt("_DestBlend", (int)BlendMode.One);
                break;
            case BlendingMode.PreMultiply:
                material.SetInt("_SourceBlend", (int)BlendMode.One);
                material.SetInt("_DestBlend", (int)BlendMode.OneMinusSrcAlpha);
                break;
            case BlendingMode.Multiply:
                material.SetInt("_SourceBlend", (int)BlendMode.DstColor);
                material.SetInt("_DestBlend", (int)BlendMode.Zero);
                break;
        }
    }
}