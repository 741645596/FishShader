// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
//https://blog.csdn.net/chenggong2dm/article/details/123506670
Shader "WB/CustomSkybox" {
Properties {
    _Tint ("Tint Color", Color) = (.5, .5, .5, .5)
    [Gamma] _Exposure ("Exposure", Range(0, 8)) = 1.0
    _Rotation ("Rotation", Range(0, 360)) = 0
    [NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
    _SkyDirection("SkyDirection", Vector) = (0,0,1,1)
}

SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off

    Pass {
        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0

        #include "ColorCore.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        samplerCUBE _Tex;
    CBUFFER_START(UnityPerMaterial)
        half4 _Tex_HDR;
        half4 _Tint;
        half _Exposure;
        float _Rotation;
        half3 _SkyDirection;
    CBUFFER_END

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * 3.14159265359f / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }
        // 计算函数 
        float4x4 rotationMatrix(float3 axis, float angle)
        {
            axis = normalize(axis);
            float s = sin(angle);
            float c = cos(angle);
            float oc = 1.0 - c;

            return float4x4(oc * axis.x * axis.x + c,
                oc * axis.x * axis.y - axis.z * s,
                oc * axis.z * axis.x + axis.y * s,
                0.0,
                oc * axis.x * axis.y + axis.z * s,
                oc * axis.y * axis.y + c,
                oc * axis.y * axis.z - axis.x * s,
                0.0,
                oc * axis.z * axis.x - axis.y * s,
                oc * axis.y * axis.z + axis.x * s,
                oc * axis.z * axis.z + c,
                0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        inline half3 DecodeHDR(half4 data, half4 decodeInstructions)
        {
            // Take into account texture alpha if decodeInstructions.w is true(the alpha value affects the RGB channels)
            half alpha = decodeInstructions.w * (data.a - 1.0) + 1.0;

            // If Linear mode is not supported we can skip exponent part
#if defined(UNITY_COLORSPACE_GAMMA)
            return (decodeInstructions.x * alpha) * data.rgb;
#else
#   if defined(UNITY_USE_NATIVE_HDR)
            return decodeInstructions.x * data.rgb; // Multiplier for future HDRI relative to absolute conversion.
#   else
            return (decodeInstructions.x * pow(abs(alpha), decodeInstructions.y)) * data.rgb;
#   endif
#endif
        }

#ifdef UNITY_COLORSPACE_GAMMA
#define unity_ColorSpaceGrey half4(0.5, 0.5, 0.5, 0.5)
#define unity_ColorSpaceDouble half4(2.0, 2.0, 2.0, 2.0)
#define unity_ColorSpaceDielectricSpec half4(0.220916301, 0.220916301, 0.220916301, 1.0 - 0.220916301)
#define unity_ColorSpaceLuminance half4(0.22, 0.707, 0.071, 0.0) // Legacy: alpha is set to 0.0 to specify gamma mode
#else // Linear values
#define unity_ColorSpaceGrey half4(0.214041144, 0.214041144, 0.214041144, 0.5)
#define unity_ColorSpaceDouble half4(4.59479380, 4.59479380, 4.59479380, 2.0)
#define unity_ColorSpaceDielectricSpec half4(0.04, 0.04, 0.04, 1.0 - 0.04) // standard dielectric reflectivity coef at incident angle (= 4%)
#define unity_ColorSpaceLuminance half4(0.0396819152, 0.458021790, 0.00609653955, 1.0) // Legacy: alpha is set to 1.0 to specify linear mode
#endif

        struct appdata_t {
            float4 vertex : POSITION;
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
        };

        v2f vert (appdata_t v)
        {
            v2f o;
            float3 rotated = mul(rotationMatrix(normalize(_SkyDirection.xyz), _Rotation * 3.14159265359f / 180.0), v.vertex).xyz;

            float3 positionWS = TransformObjectToWorld(rotated);
            o.vertex = TransformWorldToHClip(positionWS);
            o.texcoord = v.vertex.xyz;
            return o;
        }

        half4 frag (v2f i) : SV_Target
        {
            half4 tex = texCUBE (_Tex, i.texcoord);
            half3 c = DecodeHDR (tex, _Tex_HDR);
            c = c * _Tint.rgb * unity_ColorSpaceDouble.rgb;
            c *= _Exposure;
            return half4(c, 1);
        }
        ENDHLSL
    }
}
Fallback Off
}
