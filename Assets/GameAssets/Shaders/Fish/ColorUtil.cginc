
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

inline float4 LocalGammaToLinear(float4 sRGB)
{
	// Approximate version from http://chilliant.blogspot.com.au/2012/08/srgb-approximations-for-hlsl.html?m=1
	//return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
	return float4(sRGB.xyz * 2.2f, sRGB.a);
	// Precise version, useful for debugging.
	//return half3(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b));
}
