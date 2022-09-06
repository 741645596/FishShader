#ifndef HITRED_FUN
#define HITRED_FUN


float3 HitRed(float3 baseColor, float3 RimColor, float3 normalWS, float3 viewDirWS)
{
    float3 ResultColor = baseColor + RimColor;
#if _HITCOLORCHANNEL_RIM
    float ndv = dot(normalize(normalWS), normalize(viewDirWS));
    float Fresnel = pow(1.0 - saturate(ndv), 5.0 - _HitRimSpread) * _HitRimPower;
    float hitColorScale = 0;
#if _TESTHITCOLOR_ON    
    hitColorScale = saturate(Fresnel * _HitMultiple);
    float3 hitColorAdd = _HitColor * _HitMultiple - RimColor;
#else
    hitColorScale = saturate(Fresnel) * _OverlayMultiple;
    float3 hitColorAdd = _OverlayColor.rgb * _OverlayMultiple - RimColor;
#endif
    ResultColor += hitColorScale * hitColorAdd;

#elif _HITCOLORCHANNEL_ALBEDO
#if _TESTHITCOLOR_ON    
    float3 mulColor = _HitColor * _HitMultiple;
    ResultColor = mulColor * ResultColor;
#else
    float3 mulColor = _OverlayColor * _OverlayMultiple;
    ResultColor = mulColor * ResultColor;
#endif

#endif
    return ResultColor;
}


#endif // HITRED_FUN