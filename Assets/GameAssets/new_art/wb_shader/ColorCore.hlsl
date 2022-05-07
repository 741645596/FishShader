#ifndef COLOR_CORE_INCLUDED
#define COLOR_CORE_INCLUDED

void rgb_to_hsv(float4 rgb, out float4 outcol)
{
	float cmax, cmin, h, s, v, cdelta;
	float3 c;

	cmax = max(rgb[0], max(rgb[1], rgb[2]));
	cmin = min(rgb[0], min(rgb[1], rgb[2]));
	cdelta = cmax - cmin;

	v = cmax;
	if (cmax != 0.0) {
		s = cdelta / cmax;
	}
	else {
		s = 0.0;
		h = 0.0;
	}

	if (s == 0.0) {
		h = 0.0;
	}
	else {
		c = (float3(cmax, cmax, cmax) - rgb.xyz) / cdelta;

		if (rgb.x == cmax) {
			h = c[2] - c[1];
		}
		else if (rgb.y == cmax) {
			h = 2.0 + c[0] - c[2];
		}
		else {
			h = 4.0 + c[1] - c[0];
		}

		h /= 6.0;

		if (h < 0.0) {
			h += 1.0;
		}
	}

	outcol = float4(h, s, v, rgb.w);
}


void hsv_to_rgb(float4 hsv, out float4 outcol)
{
	float i, f, p, q, t, h, s, v;
	float3 rgb;

	h = hsv[0];
	s = hsv[1];
	v = hsv[2];

	if (s == 0.0) {
		rgb = float3(v, v, v);
	}
	else {
		if (h == 1.0) {
			h = 0.0;
		}

		h *= 6.0;
		i = floor(h);
		f = h - i;
		rgb = float3(f, f, f);
		p = v * (1.0 - s);
		q = v * (1.0 - (s * f));
		t = v * (1.0 - (s * (1.0 - f)));

		if (i == 0.0) {
			rgb = float3(v, t, p);
		}
		else if (i == 1.0) {
			rgb = float3(q, v, p);
		}
		else if (i == 2.0) {
			rgb = float3(p, v, t);
		}
		else if (i == 3.0) {
			rgb = float3(p, q, v);
		}
		else if (i == 4.0) {
			rgb = float3(t, p, v);
		}
		else {
			rgb = float3(v, p, q);
		}
	}

	outcol = float4(rgb, hsv.w);
}

void hue_sat(float hue, float sat, float value, float fac, float4 col, out float4 outcol)
{
	float4 hsv;

	rgb_to_hsv(col, hsv);

	hsv[0] = frac(hsv[0] + hue + 0.5);
	hsv[1] = clamp(hsv[1] * sat, 0.0, 1.0);
	hsv[2] = hsv[2] * value;

	hsv_to_rgb(hsv, outcol);

	outcol = lerp(col, outcol, fac);
}


#endif // COLOR_CORE_INCLUDED