#ifndef CLIGHTING_INCLUDED
#define CLIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"

// ----------------------------------------------------------------------------
//  BRDFData
// ----------------------------------------------------------------------------
#define kDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)

struct BRDFData
{
    half3 albedo;
    half3 diffuse;
    half3 specular;
    half reflectivity;
    half perceptualRoughness;
    half roughness;
    half roughness2;
    half grazingTerm;

    // We save some light invariant BRDF terms so we don't have to recompute
    // them in the light loop. Take a look at DirectBRDF function for detailed explaination.
    half normalizationTerm;     // roughness * 4.0 + 2.0
    half roughness2MinusOne;    // roughness^2 - 1.0
};

half OneMinusReflectivityMetallic(half metallic)
{
    half oneMinusDielectricSpec = kDielectricSpec.a;
    return oneMinusDielectricSpec - metallic * oneMinusDielectricSpec;
}

inline void InitializeBRDFDataDirect(half3 albedo, half3 diffuse, half3 specular, half reflectivity, half oneMinusReflectivity, half smoothness, inout half alpha, out BRDFData outBRDFData)
{
    outBRDFData = (BRDFData)0;
    outBRDFData.albedo = albedo;
    outBRDFData.diffuse = diffuse;
    outBRDFData.specular = specular;
    outBRDFData.reflectivity = reflectivity;

    outBRDFData.perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(smoothness);
    outBRDFData.roughness           = max(PerceptualRoughnessToRoughness(outBRDFData.perceptualRoughness), 0.0078125);
    outBRDFData.roughness2          = max(outBRDFData.roughness * outBRDFData.roughness, HALF_MIN);
    outBRDFData.grazingTerm         = saturate(smoothness + reflectivity);
    outBRDFData.normalizationTerm   = outBRDFData.roughness * half(4.0) + half(2.0);
    outBRDFData.roughness2MinusOne  = outBRDFData.roughness2 - half(1.0);

#if defined(_ALPHAPREMULTIPLY_ON)
    outBRDFData.diffuse *= alpha;
    alpha = alpha * oneMinusReflectivity + reflectivity; // NOTE: alpha modified and propagated up.
#endif
}

void InitializeBRDFData(half3 albedo, half metallic, half3 specular, half smoothness, inout half alpha, out BRDFData outBRDFData)
{
    half oneMinusReflectivity = OneMinusReflectivityMetallic(metallic);
    half reflectivity = half(1.0) - oneMinusReflectivity;
    half3 brdfDiffuse = albedo * oneMinusReflectivity;
    half3 brdfSpecular = lerp(kDielectricSpec.rgb, albedo, metallic);
    InitializeBRDFDataDirect(albedo, brdfDiffuse, brdfSpecular, reflectivity, oneMinusReflectivity, smoothness, alpha, outBRDFData);
}

void InitializeBRDFData(inout SurfaceData surfaceData, out BRDFData brdfData)
{
    InitializeBRDFData(surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.alpha, brdfData);
}
// ----------------------------------------------------------------------------


// ----------------------------------------------------------------------------
//  环境光采样 球谐光照 baked GI
// ----------------------------------------------------------------------------

float3 SampleSH(float3 N)
{
    // Linear + constant polynomial terms
    float3 res = SHEvalLinearL0L1(N, unity_SHAr, unity_SHAg, unity_SHAb);

    // Quadratic polynomials
    res += SHEvalLinearL2(N, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);

    return max(half3(0, 0, 0), res);
}

half3 SampleSHPixel(half3 normalWS)
{
    return SampleSH(normalWS);
}
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
// 环境镜面反射
// ----------------------------------------------------------------------------
half3 GlossyEnvironmentReflection(half3 reflectVector, float3 positionWS, half perceptualRoughness)
{
    half mip = PerceptualRoughnessToMipmapLevel(perceptualRoughness);
    half4 encodedIrradiance = half4(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectVector, mip));
    return DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
}
// ----------------------------------------------------------------------------

half3 EnvironmentBRDFSpecular(BRDFData brdfData, half fresnelTerm)
{
    float surfaceReduction = 1.0 / (brdfData.roughness2 + 1.0);
    return half3(surfaceReduction * lerp(brdfData.specular, brdfData.grazingTerm, fresnelTerm));
}

// ----------------------------------------------------------------------------
//  直接光高光项
// ----------------------------------------------------------------------------
half DirectBRDFSpecular(BRDFData brdfData, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
{
    float3 lightDirectionWSFloat3 = float3(lightDirectionWS);
    float3 halfDir = SafeNormalize(lightDirectionWSFloat3 + float3(viewDirectionWS));
    float NoH = saturate(dot(float3(normalWS), halfDir));
    half LoH = half(saturate(dot(lightDirectionWSFloat3, halfDir)));
    float d = NoH * NoH * brdfData.roughness2MinusOne + 1.00001f;
    half d2 = half(d * d);
    half LoH2 = LoH * LoH;
    half specularTerm = brdfData.roughness2 / (d2 * max(half(0.1), LoH2) * brdfData.normalizationTerm);
    specularTerm = specularTerm - HALF_MIN;
    specularTerm = clamp(specularTerm, 0.0, 100.0);
    return specularTerm;
}
// ----------------------------------------------------------------------------

// ----------------------------------------------------------------------------
//  灯光数据
// ----------------------------------------------------------------------------
struct Light
{
    half3   direction;
    half3   color;
    half    distanceAttenuation;
    half    shadowAttenuation;
};

Light GetMainLight(float4 shadowCoord)
{
    Light light;
    light.direction = half3(_MainLightPosition.xyz);
    light.distanceAttenuation = unity_LightData.z;
    light.color = _MainLightColor.rgb;
    light.shadowAttenuation = 1;
    return light;
}

// ----------------------------------------------------------------------------

half3 PBRLighting(half3 viewDirectionWS, half3 positionWS, SurfaceData surfaceData, BRDFData brdfData)
{
    // 环境光漫反射
    half3 indirectDiffuseColor = brdfData.diffuse;
    half3 indirectDiffuseTerm = SampleSHPixel(surfaceData.normalWS);
    half3 indirectDiffuse = indirectDiffuseColor * indirectDiffuseTerm;
    // 环境光高光
    half3 reflectVector = reflect(-viewDirectionWS, surfaceData.normalWS);
    half3 indirectSpecularColor = GlossyEnvironmentReflection(reflectVector, positionWS, brdfData.perceptualRoughness);
    half NoV = saturate(dot(surfaceData.normalWS, viewDirectionWS));
    half fresnelTerm = Pow4(1.0 - NoV);
    half3 indirectSpecularTerm = EnvironmentBRDFSpecular(brdfData, fresnelTerm);
    half3 indirectSpecular = indirectSpecularTerm * indirectSpecularColor;
    // 环境光
    half3 indirectLighting = indirectDiffuse + indirectSpecular;
    indirectLighting *= surfaceData.occlusion;
    // 直接光漫反射
    half3 directDiffse = brdfData.diffuse;
    // 直接光高光
    half3 directSpecularColor = brdfData.specular;
    Light light = GetMainLight(0);
    half3 directSpecularTerm = DirectBRDFSpecular(brdfData, surfaceData.normalWS, light.direction, viewDirectionWS);
    half3 directSpecular = directSpecularTerm * directSpecularColor;
    // 光源辐照
    half NdotL = saturate(dot(surfaceData.normalWS, light.direction));
    half lightAttenuation = light.distanceAttenuation * light.shadowAttenuation;
    half3 radiance = light.color * (lightAttenuation * NdotL);
    // 直接光
    half3 directLighting = (directDiffse + directSpecular) * radiance;
    half3 final = directLighting + indirectLighting;
#if defined(_EMISSION)
    // 自发光
    final += surfaceData.emission;
#endif
    return final;
}
#endif // CLIGHTING_INCLUDED
