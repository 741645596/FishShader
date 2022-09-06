#ifndef COLOR_CORE_INCLUDED
#define COLOR_CORE_INCLUDED


float3 hsv2rgb(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float3 rgb2hsv(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 CalcFinalColor(float3 resultColor, float _ContrastScale)
{
	//float3 mulColor = _OverlayColor.xyz * _OverlayMultiple;
	float3 rimedColor = resultColor;
	float3 linearRgb = rimedColor;
	float Luminance = dot(linearRgb.xyz, float3(0.212500006, 0.715399981, 0.0720999986));    //LinearRgbToLuminance
	float4 col = 0;
	col.rgb = (rimedColor.xyz - Luminance) * 1.35000002 + Luminance;
	col.w = 1;
	col.rgb = (col.rgb - _ContrastScale * 0.5) * 1.10000002 + _ContrastScale * 0.5;
	return col.rgb;

}
float3 CalcFinalColor(float3 resultColor, float3 _OverlayColor, float _OverlayMultiple, float _ContrastScale)
{
    float3 mulColor = _OverlayColor.xyz * _OverlayMultiple;
	float3 rimedColor = mulColor * resultColor;
	float3 linearRgb = rimedColor;
	float Luminance = dot(linearRgb.xyz, float3(0.212500006, 0.715399981, 0.0720999986));    //LinearRgbToLuminance
	float4 col=0;
	col.rgb = (rimedColor.xyz - Luminance) * 1.35000002 + Luminance;
	col.w = 1;
	col.rgb = (col.rgb - _ContrastScale * 0.5) * 1.10000002 + _ContrastScale * 0.5;
	return col.rgb;
	
}

inline float GammaToLinearSpaceExact(float value)
{
	if (value <= 0.04045F)
		return value / 12.92F;
	else if (value < 1.0F)
		return pow((value + 0.055F) / 1.055F, 2.4F);
	else
		return pow(value, 2.2F);
}

half3 GammaToLinearSpace3(half3 col)
{
	col.r = GammaToLinearSpaceExact(col.r);
	col.g = GammaToLinearSpaceExact(col.g);
	col.b = GammaToLinearSpaceExact(col.b);
	return col;
}


// 2.2
inline float Gamma22(float value)
{
	return pow(value, 2.2f);
}

inline float4 Gamma22(float4 sRGB)
{
	sRGB.r = Gamma22(sRGB.r);
	sRGB.g = Gamma22(sRGB.g);
	sRGB.b = Gamma22(sRGB.b);
	return sRGB;
	//return float4(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b),sRGB.a);

}

// 0.45
inline float Gamma045(float value)
{
		return pow(value, 0.45f);
}

inline float4 Gamma045(float4 sRGB)
{
	sRGB.r = Gamma045(sRGB.r);
	sRGB.g = Gamma045(sRGB.g);
	sRGB.b = Gamma045(sRGB.b);
	return sRGB;
	//return float4(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b), sRGB.a);

}


#endif // COLOR_CORE_INCLUDED