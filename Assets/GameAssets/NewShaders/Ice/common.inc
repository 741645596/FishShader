half4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
{
    return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
}
half3 SampleNormal(float2 uv, TEXTURE2D_PARAM(bumpMap, sampler_bumpMap), half scale = 1.0h)
{
    half4 n = SAMPLE_TEXTURE2D(bumpMap, sampler_bumpMap, uv);
    return UnpackNormal(n);
}
    
VertexNormalInputs GetVertexNormalInputs_F(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;
    
    real sign = tangentOS.w * GetOddNegativeScale();
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
    
    //tbn.bitangentWS = cross(tbn.normalWS, tbn.tangentWS) * sign;
    tbn.bitangentWS = normalize(cross(tbn.normalWS, tbn.tangentWS) * tangentOS.w);
    
    return tbn;
}