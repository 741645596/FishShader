float uFresnel(half3 Normal, half3 ViewDir, half Power)
{
    float x = (1.0 - saturate(dot(normalize(Normal), normalize(ViewDir))));
    return pow(x, Power);
}

float3 hurt(float3 clrInput, float3 normalWS, float3 viewDirWS)
{
    float3 override_hurtclr = _HurtColor.xyz;
    //override_hurtclr = float3(1, 0, 0);
    float hit_multiple = _HurtMultiple;
    half Fresnel = 1;//uFresnel(normalWS, viewDirWS, 5.0 - _HurtRimSpread) * _HurtRimPower;

    hit_multiple = _HurtMultiple * _HurtMultipleCust;
    // Fresnel = 1;

    float3 hitColorAdd = override_hurtclr * hit_multiple - clrInput.xyz;
    float3 hurt_color = hitColorAdd * saturate(Fresnel * hit_multiple) + clrInput.xyz;

    //float fmod1 = step(sin(_Time.y*_HurtSpeed), _SinTime.w);
    float3 clrOut = lerp(clrInput.xyz, hurt_color, _IsHurt);

    return clrOut;
}


