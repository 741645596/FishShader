#pragma multi_compile _ UNITY_UI_CLIP_RECT

#ifdef UNITY_UI_CLIP_RECT
    #define PASS_CLIP_MASK(output)                                        \
        float2 pixelSize = output.positionCS.w;                         \
        pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy)); \
        float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);             \
        output.clipmask = half4(                                            \
            input.vertex.xy * 2 - clampedRect.xy - clampedRect.zw,      \
            0.25 / (/*0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) +*/ abs(pixelSize.xy)) \
        )
#else
    #define PASS_CLIP_MASK(output)  
#endif


#ifdef UNITY_UI_CLIP_RECT
    #define DO_CLIP_RECT_FRAG(color, In)    \
    half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(In.clipmask.xy)) * In.clipmask.zw);   \
    color.a *= m.x * m.y
#else
    #define DO_CLIP_RECT_FRAG(color, In) 
#endif