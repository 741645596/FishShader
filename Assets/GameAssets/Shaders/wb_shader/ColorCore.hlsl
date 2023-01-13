#ifndef COLOR_CORE_INCLUDED
#define COLOR_CORE_INCLUDED


half3 hsv2rgb(half3 c)
{
	half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	half3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

half3 rgb2hsv(half3 c)
{
	half4 K = half4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	half4 p = lerp(half4(c.bg, K.wz), half4(c.gb, K.xy), step(c.b, c.g));
	half4 q = lerp(half4(p.xyw, c.r), half4(c.r, p.yzx), step(p.x, c.r));

	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return half3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

half3 CalcFinalColor(half3 resultColor, half _ContrastScale)
{
	//float3 mulColor = _OverlayColor.xyz * _OverlayMultiple;
	half3 rimedColor = resultColor;
	half3 linearRgb = rimedColor;
	half Luminance = dot(linearRgb.xyz, half3(0.212500006, 0.715399981, 0.0720999986));    //LinearRgbToLuminance
	half4 col = 0;
	col.rgb = (rimedColor.xyz - Luminance) * 1.35000002 + Luminance;
	// 等价
	// col.rgb = lerp(Luminance, rimedColor.xyz, 1.35f);
	col.w = 1;
	col.rgb = (col.rgb - _ContrastScale * 0.5) * 1.10000002 + _ContrastScale * 0.5;
	// 等价
	// col.rgb = lerp(_ContrastScale * 0.5, col.rgb, 1.1f);
	return col.rgb;
}



half3 CalcFinalColor(half3 resultColor, half3 _OverlayColor, half _OverlayMultiple, half _ContrastScale)
{
	half3 mulColor = _OverlayColor.xyz * _OverlayMultiple;
	half3 rimedColor = mulColor * resultColor;
	half3 linearRgb = rimedColor;
	half Luminance = dot(linearRgb.xyz, half3(0.212500006, 0.715399981, 0.0720999986));    //LinearRgbToLuminance
	half4 col=0;
	col.rgb = (rimedColor.xyz - Luminance) * 1.35000002 + Luminance;
	// 等价
	// col.rgb = lerp(Luminance, rimedColor.xyz, 1.35f);
	col.w = 1;
	col.rgb = (col.rgb - _ContrastScale * 0.5) * 1.10000002 + _ContrastScale * 0.5;
	// 等价
    // col.rgb = lerp(_ContrastScale * 0.5, col.rgb, 1.1f);
	return col.rgb;
	
}
//  上述函数来源于：
//  https://zhuanlan.zhihu.com/p/84313625 
// 然后加入了对高光颜色的，对比度和饱和度的单独调节，来减少这部分的差异
half3 colorAdjust(half3 Color, half _Saturation, half _Contrast)
{
	half3 finalColor = Color;
	half gray = 0.2125 * Color.r + 0.7154 * Color.g + 0.0721 * Color.b;
	half3 grayColor = half3(gray, gray, gray);
	finalColor = lerp(grayColor, finalColor, _Saturation);
	half3 avgColor = half3(0.5, 0.5, 0.5);
	finalColor = lerp(avgColor, finalColor, _Contrast);

	return finalColor;
}

//#include "../wb_Shader/ColorCore.hlsl"
				// 旋转uv 计算
void roateUV(float2 _UVRotate, float calcTime, half2 pivot, inout float2 uv)
{
	half cosAngle = cos(_UVRotate.x + calcTime * _UVRotate.y);
	half sinAngle = sin(_UVRotate.x + calcTime * _UVRotate.y);
	half2x2 roation = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
	uv.xy = mul(roation, uv.xy -= pivot) + pivot;
}
/*
inline float GammaToLinearSpaceExact(float value)
{
	if (value <= 0.04045F)
		return value / 12.92F;
	else if (value < 1.0F)
		return pow(abs((value + 0.055F) / 1.055F), 2.4F);
	else
		return pow(abs(value), 2.2F);
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
	return pow(abs(value), 2.2f);
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
		return pow(abs(value), 0.45f);
}

inline float4 Gamma045(float4 sRGB)
{
	sRGB.r = Gamma045(sRGB.r);
	sRGB.g = Gamma045(sRGB.g);
	sRGB.b = Gamma045(sRGB.b);
	return sRGB;
	//return float4(GammaToLinearSpaceExact(sRGB.r), GammaToLinearSpaceExact(sRGB.g), GammaToLinearSpaceExact(sRGB.b), sRGB.a);

}
*/

#endif // COLOR_CORE_INCLUDED