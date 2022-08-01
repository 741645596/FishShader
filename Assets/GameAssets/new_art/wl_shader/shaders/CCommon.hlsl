#ifndef CCOMMON_INCLUDED
#define CCOMMON_INCLUDED

// ----------------------------------------------------------------------------
struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
};

struct Varyings
{
    float2 uv                       : TEXCOORD0;
    float3 positionWS               : TEXCOORD1;
    half3 normalWS                  : TEXCOORD2;
    half3 tangentWS                 : TEXCOORD3;
    half3 bitangentWS               : TEXCOORD4;
    float3 viewDirWS                : TEXCOORD5;
    float4 positionCS               : SV_POSITION;
};
// ----------------------------------------------------------------------------

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
// ----------------------------------------------------------------------------
// UnityInput
// ----------------------------------------------------------------------------
// Time (t = time since current level load) values from Unity
float4 _Time; // (t/20, t, t*2, t*3)
float4 _SinTime; // sin(t/8), sin(t/4), sin(t/2), sin(t)
float4 _CosTime; // cos(t/8), cos(t/4), cos(t/2), cos(t)
float4 unity_DeltaTime; // dt, 1/dt, smoothdt, 1/smoothdt
float4 _TimeParameters; // t, sin(t), cos(t)

#if !defined(USING_STEREO_MATRICES)
float3 _WorldSpaceCameraPos;
#endif

// x = 1 or -1 (-1 if projection is flipped)
// y = near plane
// z = far plane
// w = 1/far plane
float4 _ProjectionParams;

// x = width
// y = height
// z = 1 + 1.0/width
// w = 1 + 1.0/height
float4 _ScreenParams;

// Values used to linearize the Z buffer (http://www.humus.name/temp/Linearize%20depth.txt)
// x = 1-far/near
// y = far/near
// z = x/far
// w = y/far
// or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
// x = -1+far/near
// y = 1
// z = x/far
// w = 1/far
float4 _ZBufferParams;

// x = orthographic camera's width
// y = orthographic camera's height
// z = unused
// w = 1.0 if camera is ortho, 0.0 if perspective
float4 unity_OrthoParams;

// scaleBias.x = flipSign
// scaleBias.y = scale
// scaleBias.z = bias
// scaleBias.w = unused
uniform float4 _ScaleBias;
uniform float4 _ScaleBiasRt;

float4 unity_CameraWorldClipPlanes[6];
// ----------------------------------------------------------------------------

// Block Layout should be respected due to SRP Batcher
CBUFFER_START(UnityPerDraw)
// Space block Feature
float4x4 unity_ObjectToWorld;
float4x4 unity_WorldToObject;
float4 unity_LODFade; // x is the fade value ranging within [0,1]. y is x quantized into 16 levels
real4 unity_WorldTransformParams; // w is usually 1.0, or -1.0 for odd-negative scale transforms

// Render Layer block feature
// Only the first channel (x) contains valid data and the float must be reinterpreted using asuint() to extract the original 32 bits values.
float4 unity_RenderingLayer;

// Light Indices block feature
// These are set internally by the engine upon request by RendererConfiguration.
half4 unity_LightData;
half4 unity_LightIndices[2];

half4 unity_ProbesOcclusion;

// Reflection Probe 0 block feature
// HDR environment map decode instructions
real4 unity_SpecCube0_HDR;
real4 unity_SpecCube1_HDR;

float4 unity_SpecCube0_BoxMax;          // w contains the blend distance
float4 unity_SpecCube0_BoxMin;          // w contains the lerp value
float4 unity_SpecCube0_ProbePosition;   // w is set to 1 for box projection
float4 unity_SpecCube1_BoxMax;          // w contains the blend distance
float4 unity_SpecCube1_BoxMin;          // w contains the sign of (SpecCube0.importance - SpecCube1.importance)
float4 unity_SpecCube1_ProbePosition;   // w is set to 1 for box projection

// Lightmap block feature
float4 unity_LightmapST;
float4 unity_DynamicLightmapST;

// SH block feature
real4 unity_SHAr;
real4 unity_SHAg;
real4 unity_SHAb;
real4 unity_SHBr;
real4 unity_SHBg;
real4 unity_SHBb;
real4 unity_SHC;

// Velocity
float4x4 unity_MatrixPreviousM;
float4x4 unity_MatrixPreviousMI;
//X : Use last frame positions (right now skinned meshes are the only objects that use this
//Y : Force No Motion
//Z : Z bias value
//W : Camera only
float4 unity_MotionVectorsParams;
CBUFFER_END

float4x4 glstate_matrix_projection;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_MatrixInvP;
float4x4 unity_MatrixVP;
float4x4 unity_MatrixInvVP;
float4 unity_StereoScaleOffset;
int unity_StereoEyeIndex;

TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);
TEXTURECUBE(unity_SpecCube1);
SAMPLER(samplerunity_SpecCube1);


float4x4 OptimizeProjectionMatrix(float4x4 M)
{
    // Matrix format (x = non-constant value).
    // Orthographic Perspective  Combined(OR)
    // | x 0 0 x |  | x 0 x 0 |  | x 0 x x |
    // | 0 x 0 x |  | 0 x x 0 |  | 0 x x x |
    // | x x x x |  | x x x x |  | x x x x | <- oblique projection row
    // | 0 0 0 1 |  | 0 0 x 0 |  | 0 0 x x |
    // Notice that some values are always 0.
    // We can avoid loading and doing math with constants.
    M._21_41 = 0;
    M._12_42 = 0;
    return M;
}

///////////////////////////////////////////////////////////////////////////////
//                      Constant Buffers                                     //
///////////////////////////////////////////////////////////////////////////////
float4 _ScaledScreenParams;
half4 _AmbientOcclusionParam;
float4 _MainLightPosition;
half4 _MainLightColor;

#define UNITY_MATRIX_M     unity_ObjectToWorld
#define UNITY_MATRIX_I_M   unity_WorldToObject
#define UNITY_MATRIX_V     unity_MatrixV
#define UNITY_MATRIX_I_V   unity_MatrixInvV
#define UNITY_MATRIX_P     OptimizeProjectionMatrix(glstate_matrix_projection)
#define UNITY_MATRIX_I_P   unity_MatrixInvP
#define UNITY_MATRIX_VP    unity_MatrixVP
#define UNITY_MATRIX_I_VP  unity_MatrixInvVP
#define UNITY_MATRIX_MV    mul(UNITY_MATRIX_V, UNITY_MATRIX_M)
#define UNITY_MATRIX_T_MV  transpose(UNITY_MATRIX_MV)
#define UNITY_MATRIX_IT_MV transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V))
#define UNITY_MATRIX_MVP   mul(UNITY_MATRIX_VP, UNITY_MATRIX_M)
#define UNITY_PREV_MATRIX_M   unity_MatrixPreviousM
#define UNITY_PREV_MATRIX_I_M unity_MatrixPreviousMI

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"


// ----------------------------------------------------------------------------
// Core.hlsl
// ----------------------------------------------------------------------------

// Structs
struct VertexPositionInputs
{
    float3 positionWS; // World space position
    float3 positionVS; // View space position
    float4 positionCS; // Homogeneous clip space position
    float4 positionNDC;// Homogeneous normalized device coordinates
};

struct VertexNormalInputs
{
    real3 tangentWS;
    real3 bitangentWS;
    float3 normalWS;
};

VertexPositionInputs GetVertexPositionInputs(float3 positionOS)
{
    VertexPositionInputs input;
    input.positionWS = TransformObjectToWorld(positionOS);
    input.positionVS = TransformWorldToView(input.positionWS);
    input.positionCS = TransformWorldToHClip(input.positionWS);

    float4 ndc = input.positionCS * 0.5f;
    input.positionNDC.xy = float2(ndc.x, ndc.y * _ProjectionParams.x) + ndc.w;
    input.positionNDC.zw = input.positionCS.zw;

    return input;
}

VertexNormalInputs GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;

    // mikkts space compliant. only normalize when extracting normal at frag.
    float sign = float(tangentOS.w) * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = float3(TransformObjectToWorldDir(tangentOS.xyz));
    tbn.bitangentWS = float3(cross(tbn.normalWS, float3(tbn.tangentWS))) * sign;
    return tbn;
}

// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
//  ShaderVariablesFunctions.hlsl
// ----------------------------------------------------------------------------

bool IsPerspectiveProjection()
{
    return (unity_OrthoParams.w == 0);
}

float3 GetCameraPositionWS()
{
    return _WorldSpaceCameraPos;
}

float3 GetCurrentViewPosition()
{
    return GetCameraPositionWS();
}

float3 GetViewForwardDir()
{
    float4x4 viewMat = GetWorldToViewMatrix();
    return -viewMat[2].xyz;
}

half3 GetWorldSpaceNormalizeViewDir(float3 positionWS)
{
    if (IsPerspectiveProjection())
    {
        // Perspective
        float3 V = GetCurrentViewPosition() - positionWS;
        return half3(normalize(V));
    }
    else
    {
        // Orthographic
        return half3(-GetViewForwardDir());
    }
}

half OutputAlpha(half outputAlpha, half surfaceType = half(0.0))
{
    return surfaceType == 1 ? outputAlpha : half(1.0);
}

// ----------------------------------------------------------------------------

struct SurfaceData
{
    half3 albedo;
    half3 specular;
    half  metallic;
    half  smoothness;
    half3 normalWS;
    half3 emission;
    half  occlusion;
    half  alpha;
};

CBUFFER_START(UnityPerMaterial)
half _Surface;
float4 _BaseMap_ST;
float _Metallic;
float4 _BaseColor;
float _BumpScale;
float _Occlusion;
float _Smoothness;
float4 _EmissionColor;

half4 _RimColor;
half _RimPower;
half _RimStrength;
CBUFFER_END

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
float4 _BaseMap_TexelSize;
float4 _BaseMap_MipInfo;
TEXTURE2D(_BumpMap);
SAMPLER(sampler_BumpMap);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);
TEXTURE2D(_MetallicGlossMap);   
SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_OcclusionMap);       
SAMPLER(sampler_OcclusionMap);

half4 SampleAlbedoAlpha(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return half4(SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv));
}

half Alpha(half albedoAlpha, half4 color, half cutoff)
{
    half alpha = albedoAlpha * color.a;

// #if defined(_ALPHATEST_ON)
//     clip(alpha - cutoff);
// #endif

    return alpha;
}

half4 SampleMetallicSpecGloss(float2 uv, half albedoAlpha)
{
    half4 specGloss = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv);
    specGloss.r *= _Metallic;
    specGloss.a *= _Smoothness;
    return specGloss;
}

half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = half(1.0))
{
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return UnpackNormalScale(n, scale);
}

half3 SampleEmission(float2 uv, half3 emissionColor, TEXTURE2D_PARAM(emissionMap, sampler_emissionMap))
{
    return SAMPLE_TEXTURE2D(emissionMap, sampler_emissionMap, uv).rgb * emissionColor;
}

void InitializeSurfaceData(Varyings input, out SurfaceData outSurfaceData)
{
    half2 uv = input.uv;
    half4 albedoAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap));
    outSurfaceData.alpha = Alpha(albedoAlpha.a, _BaseColor, 1.0);

    half4 specGloss = SampleMetallicSpecGloss(uv, albedoAlpha.a);
    outSurfaceData.albedo = albedoAlpha.rgb * _BaseColor.rgb;
    outSurfaceData.metallic = specGloss.r;
    outSurfaceData.specular = half3(0.0, 0.0, 0.0);
    outSurfaceData.smoothness = specGloss.a;

#if defined(_USE_OCCLUSION)
    outSurfaceData.occlusion = LerpWhiteTo(specGloss.g, _Occlusion);
#else
    outSurfaceData.occlusion = 1.0;
#endif

#if defined(_EMISSION)
    outSurfaceData.emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_ARGS(_EmissionMap, sampler_EmissionMap));
#else
    outSurfaceData.emission = 0.0;
#endif

#if defined(_NORMALMAP)
    half3 normalTS = SampleNormal(uv, TEXTURE2D_ARGS(_BumpMap, sampler_BumpMap), _BumpScale);
    half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz);
    float3 normalWS = TransformTangentToWorld(normalTS, tangentToWorld);
    outSurfaceData.normalWS = SafeNormalize(normalWS);
#else
    outSurfaceData.normalWS = input.normalWS;
#endif
}
// ----------------------------------------------------------------------------

#endif // CCOMMON_INCLUDED
