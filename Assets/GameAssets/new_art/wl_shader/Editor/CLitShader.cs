using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CLitShader : ShaderGUI
{
    protected MaterialEditor materialEditor { get; set; }

    public bool m_FirstTimeApply = true;
    public enum SurfaceType
    {
        Opaque,
        Transparent
    }
    public enum BlendMode
    {
        Alpha,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
        Premultiply, // Physically plausible transparency mode, implemented as alpha pre-multiply
        Additive,
        Multiply
    }
    public enum RenderFace
    {
        Front = 2,
        Back = 1,
        Both = 0
    }

    public enum ZWriteControl
    {
        Auto = 0,
        ForceEnabled = 1,
        ForceDisabled = 2
    }

    enum ZTestMode  // the values here match UnityEngine.Rendering.CompareFunction
    {
        Disabled = 0,
        Never = 1,
        Less = 2,
        Equal = 3,
        LEqual = 4,     // default for most rendering
        Greater = 5,
        NotEqual = 6,
        GEqual = 7,
        Always = 8,
    }

    private static readonly GUIContent baseMap = EditorGUIUtility.TrTextContent("Base Map", "Specifies the base Material and/or Color of the surface. If you’ve selected Transparent or Alpha Clipping under Surface Options, your Material uses the Texture’s alpha channel or color.");
    private static readonly GUIContent metallicMapText = EditorGUIUtility.TrTextContent("Metallic Map", "Sets and configures the map for the Metallic workflow.");
    private static readonly GUIContent metallicText = EditorGUIUtility.TrTextContent("(R)Metallic", "Controls the spread of highlights and reflections on the surface.");
    private static readonly GUIContent smoothnessText = EditorGUIUtility.TrTextContent("(A)Smoothness", "Controls the spread of highlights and reflections on the surface.");
    private static readonly GUIContent occlusionText = EditorGUIUtility.TrTextContent("(G)Occlusion", "Controls the spread of highlights and reflections on the surface.");
    private static readonly GUIContent normalMapText = EditorGUIUtility.TrTextContent("Normal Map", "Designates a Normal Map to create the illusion of bumps and dents on this Material's surface.");
    private static readonly GUIContent emissionMap = EditorGUIUtility.TrTextContent("Emission Map", "Determines the color and intensity of light that the surface of the material emits.");
    private static readonly GUIContent occlusionSwitchText = EditorGUIUtility.TrTextContent("Use Channel(G) as Occlusion", "");
    private static readonly GUIContent rimColorText = EditorGUIUtility.TrTextContent("Rim Color", "");
    private static readonly GUIContent rimPowerText = EditorGUIUtility.TrTextContent("Rim Power", "");
    private static readonly GUIContent rimStrengthText = EditorGUIUtility.TrTextContent("Rim Strength", "");
    private static readonly GUIContent surfaceType = EditorGUIUtility.TrTextContent("Surface Type", "");
    private static readonly GUIContent blendingMode = EditorGUIUtility.TrTextContent("Blending Mode", "");
    private static readonly GUIContent cullingText = EditorGUIUtility.TrTextContent("Render Face", "");
    private static readonly GUIContent zwriteText = EditorGUIUtility.TrTextContent("Depth Write", "");
    private static readonly GUIContent ztestText = EditorGUIUtility.TrTextContent("Depth Test", "");

    private MaterialProperty surfaceTypeProp;
    private MaterialProperty blendModeProp;
    private MaterialProperty cullingProp;
    private MaterialProperty zwriteProp;
    private MaterialProperty ztestProp;
    private MaterialProperty baseMapProp;
    private MaterialProperty baseColorProp;
    private MaterialProperty metallicGlossMap;
    private MaterialProperty metallic;
    private MaterialProperty smoothness;
    private MaterialProperty occlusion;
    private MaterialProperty bumpMapProp;
    private MaterialProperty bumpScaleProp;
    private MaterialProperty emissionMapProp;
    private MaterialProperty emissionColorProp;
    private MaterialProperty occlusionSwitch;
    private MaterialProperty rimColorProp;
    private MaterialProperty rimPowerProp;
    private MaterialProperty rimStrength;

    //readonly MaterialHeaderScopeList m_MaterialScopeList = new MaterialHeaderScopeList();

    public static readonly GUIContent SurfaceText = EditorGUIUtility.TrTextContent("Surface");
    public static readonly GUIContent PBRBaseText = EditorGUIUtility.TrTextContent("PBR Base");
    public static readonly GUIContent RimText = EditorGUIUtility.TrTextContent("Rim");

    protected enum Expandable
    {
        Surface = 1 << 0,
        PBRBase = 1 << 1,
        Rim = 1 << 2,
    }

    public void MaterialChanged(Material material)
    {

    }

    public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
    {
        if (materialEditorIn == null)
            throw new ArgumentNullException("materialEditorIn");


        EditorGUI.BeginChangeCheck();

        materialEditor = materialEditorIn;
        Material material = materialEditor.target as Material;

        FindProperties(properties);

        DrawSurfaceProperties(material);
        DrawPBRProperties(material);
        DrawRimProperties(material);

        //if (m_FirstTimeApply)
        //{
        //    m_MaterialScopeList.RegisterHeaderScope(SurfaceText, Expandable.Surface, DrawSurfaceProperties);
        //    m_MaterialScopeList.RegisterHeaderScope(PBRBaseText, Expandable.PBRBase, DrawPBRProperties);
        //    m_MaterialScopeList.RegisterHeaderScope(RimText, Expandable.Rim, DrawRimProperties);
        //    m_FirstTimeApply = false;
        //}

        //var surfaceType = EditorGUIUtility.TrTextContent("Surface Type",
        //        "Select a surface type for your texture. Choose between Opaque or Transparent.");
        //var surfaceTypeProp = FindProperty("_Surface", properties, false);

        //DoPopup(surfaceType, surfaceTypeProp, Enum.GetNames(typeof(SurfaceType)));

        //m_MaterialScopeList.DrawHeaders(materialEditor, material);
        if (EditorGUI.EndChangeCheck())
        {
            foreach (var obj in materialEditor.targets)
                SetMaterialKeywords((Material)obj);
        }
    }

    private void FindProperties(MaterialProperty[] properties)
    {
        surfaceTypeProp = FindProperty("_Surface", properties, false);
        blendModeProp = FindProperty("_Blend", properties, false);
        cullingProp = FindProperty("_Cull", properties, false);
        zwriteProp = FindProperty("_ZWrite", properties, false);
        ztestProp = FindProperty("_ZTest", properties, false);

        baseMapProp = FindProperty("_BaseMap", properties, false);
        baseColorProp = FindProperty("_BaseColor", properties, false);
        metallicGlossMap = FindProperty("_MetallicGlossMap", properties);
        metallic = FindProperty("_Metallic", properties, false);
        smoothness = FindProperty("_Smoothness", properties, false);
        occlusion = FindProperty("_Occlusion", properties, false);
        bumpMapProp = FindProperty("_BumpMap", properties, false);
        bumpScaleProp = FindProperty("_BumpScale", properties, false);
        emissionMapProp = FindProperty("_EmissionMap", properties, false);
        emissionColorProp = FindProperty("_EmissionColor", properties, false);
        occlusionSwitch = FindProperty("_OcclusionSwitch", properties, false);

        rimColorProp = FindProperty("_RimColor", properties, false);
        rimPowerProp = FindProperty("_RimPower", properties, false);
        rimStrength = FindProperty("_RimStrength", properties, false);
    }

    private void DrawRimProperties(Material material)
    {
        materialEditor.ShaderProperty(rimColorProp, rimColorText);
        materialEditor.ShaderProperty(rimPowerProp, rimPowerText);
        materialEditor.ShaderProperty(rimStrength, rimStrengthText);

        if (rimStrength.floatValue != 0.0)
        {
            material.EnableKeyword("_USE_RIM");
        }
        else
        {
            material.DisableKeyword("_USE_RIM");
        }
    }

    private void DrawSurfaceProperties(Material material)
    {
        DoPopup(surfaceType, surfaceTypeProp, Enum.GetNames(typeof(SurfaceType)));
        if ((surfaceTypeProp != null) && ((SurfaceType)surfaceTypeProp.floatValue == SurfaceType.Transparent))
            DoPopup(blendingMode, blendModeProp, Enum.GetNames(typeof(BlendMode)));

        DoPopup(cullingText, cullingProp, Enum.GetNames(typeof(RenderFace)));
        DoPopup(zwriteText, zwriteProp, Enum.GetNames(typeof(ZWriteControl)));

        //materialEditor.IntPopupShaderProperty(ztestProp, ztestText.text,
        //    Enum.GetNames(typeof(ZTestMode)).Skip(1).ToArray(),
        //    ((int[])Enum.GetValues(typeof(ZTestMode))).Skip(1).ToArray());
    }

    private void DrawPBRProperties(Material material)
    {
        if (baseMapProp != null && baseColorProp != null) // Draw the baseMap, most shader will have at least a baseMap
        {
            materialEditor.TexturePropertySingleLine(baseMap, baseMapProp, baseColorProp);
            EditorGUI.indentLevel += 2;
            materialEditor.TextureScaleOffsetProperty(baseMapProp);
            EditorGUI.indentLevel -= 2;
        }

        materialEditor.TexturePropertySingleLine(metallicMapText, metallicGlossMap, null);
        EditorGUI.indentLevel += 2;
        materialEditor.ShaderProperty(metallic, metallicText);
        materialEditor.ShaderProperty(smoothness, smoothnessText);
        materialEditor.ShaderProperty(occlusionSwitch, occlusionSwitchText);
        if (occlusionSwitch.floatValue == 1f)
        {
            material.EnableKeyword("_USE_OCCLUSION");
            materialEditor.ShaderProperty(occlusion, occlusionText);
        }
        else
        {
            material.DisableKeyword("_USE_OCCLUSION");
        }
        EditorGUI.indentLevel -= 2;
        materialEditor.TexturePropertySingleLine(normalMapText, bumpMapProp, bumpScaleProp);
        CoreUtils.SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap"));

        materialEditor.TexturePropertyWithHDRColor(emissionMap, emissionMapProp, emissionColorProp, false);
        CoreUtils.SetKeyword(material, "_EMISSION", material.GetTexture("_EmissionMap"));
    }

    public void DoPopup(GUIContent label, MaterialProperty property, string[] options)
    {
        //if (property != null)
        //    materialEditor.PopupShaderProperty(property, label, options);

        if (property == null)
            throw new ArgumentNullException("property");

        EditorGUI.showMixedValue = property.hasMixedValue;

        var mode = property.floatValue;
        EditorGUI.BeginChangeCheck();
        mode = EditorGUILayout.Popup(label, (int)mode, options);
        if (EditorGUI.EndChangeCheck())
        {
            materialEditor.RegisterPropertyChangeUndo(label.text);
            property.floatValue = mode;
        }

        EditorGUI.showMixedValue = false;
    }

    //// material changed check
    //public override void ValidateMaterial(Material material)
    //{
    //    SetMaterialKeywords(material);
    //}

    public static void SetupMaterialBlendMode(Material material)
    {
        SetupMaterialBlendModeInternal(material, out int renderQueue);

        // apply automatic render queue
        if (renderQueue != material.renderQueue)
            material.renderQueue = renderQueue;
    }

    internal static void SetupMaterialBlendModeInternal(Material material, out int automaticRenderQueue)
    {
        if (material == null)
            throw new ArgumentNullException("material");

        bool alphaClip = false;
        if (material.HasProperty("_AlphaClip"))
            alphaClip = material.GetFloat("_AlphaClip") >= 0.5;
        CoreUtils.SetKeyword(material, "_ALPHATEST_ON", alphaClip);

        // default is to use the shader render queue
        int renderQueue = material.shader.renderQueue;
        material.SetOverrideTag("RenderType", "");      // clear override tag
        if (material.HasProperty("_Surface"))
        {
            SurfaceType surfaceType = (SurfaceType)material.GetFloat("_Surface");
            bool zwrite = false;
            CoreUtils.SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", surfaceType == SurfaceType.Transparent);
            if (surfaceType == SurfaceType.Opaque)
            {
                if (alphaClip)
                {
                    renderQueue = (int)RenderQueue.AlphaTest;
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                }
                else
                {
                    renderQueue = (int)RenderQueue.Geometry;
                    material.SetOverrideTag("RenderType", "Opaque");
                }

                SetMaterialSrcDstBlendProperties(material,
                    UnityEngine.Rendering.BlendMode.One,
                    UnityEngine.Rendering.BlendMode.Zero);
                zwrite = true;
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else // SurfaceType Transparent
            {
                BlendMode blendMode = (BlendMode)material.GetFloat("_Blend");

                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.DisableKeyword("_ALPHAMODULATE_ON");

                // Specific Transparent Mode Settings
                switch (blendMode)
                {
                    case BlendMode.Alpha:
                        SetMaterialSrcDstBlendProperties(material,
                            UnityEngine.Rendering.BlendMode.SrcAlpha,
                            UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        break;
                    case BlendMode.Premultiply:
                        SetMaterialSrcDstBlendProperties(material,
                            UnityEngine.Rendering.BlendMode.One,
                            UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                        break;
                    case BlendMode.Additive:
                        SetMaterialSrcDstBlendProperties(material,
                            UnityEngine.Rendering.BlendMode.SrcAlpha,
                            UnityEngine.Rendering.BlendMode.One);
                        break;
                    case BlendMode.Multiply:
                        SetMaterialSrcDstBlendProperties(material,
                            UnityEngine.Rendering.BlendMode.DstColor,
                            UnityEngine.Rendering.BlendMode.Zero);
                        material.EnableKeyword("_ALPHAMODULATE_ON");
                        break;
                }

                // General Transparent Material Settings
                material.SetOverrideTag("RenderType", "Transparent");
                zwrite = false;
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                renderQueue = (int)RenderQueue.Transparent;
            }

            // check for override enum
            if (material.HasProperty("_ZWriteControl"))
            {
                var zwriteControl = (ZWriteControl)material.GetFloat("_ZWriteControl");
                if (zwriteControl == ZWriteControl.ForceEnabled)
                    zwrite = true;
                else if (zwriteControl == ZWriteControl.ForceDisabled)
                    zwrite = false;
            }
            SetMaterialZWriteProperty(material, zwrite);
            material.SetShaderPassEnabled("DepthOnly", zwrite);
        }
        else
        {
            // no surface type property -- must be hard-coded by the shadergraph,
            // so ensure the pass is enabled at the material level
            material.SetShaderPassEnabled("DepthOnly", true);
        }

        automaticRenderQueue = renderQueue;
    }

    internal static void SetMaterialZWriteProperty(Material material, bool zwriteEnabled)
    {
        if (material.HasProperty("_ZWrite"))
            material.SetFloat("_ZWrite", zwriteEnabled ? 1.0f : 0.0f);
    }

    internal static void SetMaterialSrcDstBlendProperties(Material material, UnityEngine.Rendering.BlendMode srcBlend, UnityEngine.Rendering.BlendMode dstBlend)
    {
        if (material.HasProperty("_SrcBlend"))
            material.SetFloat("_SrcBlend", (float)srcBlend);

        if (material.HasProperty("_DstBlend"))
            material.SetFloat("_DstBlend", (float)dstBlend);
    }

    public static void SetMaterialKeywords(Material material, Action<Material> shadingModelFunc = null, Action<Material> shaderFunc = null)
    {
        UpdateMaterialSurfaceOptions(material, automaticRenderQueue: true);
    }

    internal static void UpdateMaterialSurfaceOptions(Material material, bool automaticRenderQueue)
    {
        // Setup blending - consistent across all Universal RP shaders
        SetupMaterialBlendModeInternal(material, out int renderQueue);

        // apply automatic render queue
        if (automaticRenderQueue && (renderQueue != material.renderQueue))
            material.renderQueue = renderQueue;
    }
}
