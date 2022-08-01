Shader "Fish/Background2"
{
    Properties
    {
        [MainTexture] _MainTex ("Base Map", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [Space]
        [NoScaleOffset] _CausticMap("CausticsMap", 2D) = "white" {}
        [NoScaleOffset]_CausticMaskTex ("Caustics MaskMap", 2D) = "white" {}
        _CausticsTiling("Caustics Tiling", Range(0, 5)) = 0.5
        _CausticsSpeed("Caustics Speed", Range(0, 5)) = 0.5
        _CausticsStrength("Caustics Strength", Range(0, 1)) = 1
        _CausticsHardness("Caustics Hardness", Range(0, 1)) = 2
        [Space]
        _DistortionMap ("Distortion Noise Map", 2D) = "white" {}
        _DistortionSpeed("Distortion Speed", Range(0, 2)) = 1
        _DistortionStrength("Distortion Strength", Range(0, 2)) = 1

    }
    SubShader
    {
        ZWrite On

        Pass
        {
            Blend One Zero

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            TEXTURE2D(_CausticMap);     SAMPLER(sampler_CausticMap);
            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_CausticMaskTex);        SAMPLER(sampler_CausticMaskTex);
            TEXTURE2D(_DistortionMap);  SAMPLER(sampler_DistortionMap);

            CBUFFER_START(UnityPerMaterial)
            half4 _MainTex_ST;
            half4 _BaseColor;

            half4 _CausticMap_ST;
            half _CausticsSpeed;
            half _CausticsTiling;
            half _CausticsStrength;
            half _CausticsHardness;

            half _DistortionSpeed;
            half _DistortionStrength;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                
                output.vertex = TransformObjectToHClip(input.positionOS.xyz);
                output.uv.xy = TRANSFORM_TEX(input.uv.xy, _MainTex);

                return output;
            }
            
            real4 frag (Varyings input) : SV_Target
            {
                float4 waveOffset = SAMPLE_TEXTURE2D(_CausticMap, sampler_CausticMap, input.uv).w - 0.5;

                float2 causticUV = input.uv.xy * _CausticsTiling + waveOffset * 0.1 * _CausticsHardness;
                float2 uvA = causticUV * 1.2 + _Time.x * 1.2 * _CausticsSpeed;
                float2 uvB = causticUV * 0.8 - _Time.x * 1.1 * _CausticsSpeed;
                float4 A = SAMPLE_TEXTURE2D(_CausticMap, sampler_CausticMap, uvA);
                float4 B = SAMPLE_TEXTURE2D(_CausticMap, sampler_CausticMap, uvB);
                
                float causticsDriver = (A.z * B.z) * 10 + A.z + B.z;

                half mask = SAMPLE_TEXTURE2D(_CausticMaskTex, sampler_CausticMaskTex, input.uv).x;
                half3 caustics = causticsDriver * half3(A.w * 0.5, B.w * 0.75, B.x) * mask * _CausticsStrength;

                half2 distortion_noise_uv = _Time.yy * float2(0.1 * _DistortionSpeed, 0) + input.uv;
                half distortion_noise = SAMPLE_TEXTURE2D(_DistortionMap, sampler_DistortionMap, distortion_noise_uv).y;
                half2 distortion_uv = distortion_noise * 0.01 * _DistortionStrength - input.uv;

                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortion_uv);
                return color * _BaseColor * real4(caustics + 1, 1.0);
            }
            ENDHLSL
        }
    }
}