Shader "WB/BG"
{
    Properties
    {
        _BaseMap("BaseMap", 2D) = "White" { }
        _BaseColor("BaseColor", Color) = (1.0, 1.0, 1.0, 1.0)

        _DistortMap("DistortMap", 2D) = "black" { }
        _DistortSpeed("DistortSpeed", Float) = 0

        _CausticsMap("CausticsMap", 2D) = "black" { }
        _CausticsMaskMap("CausticsMaskMap", 2D) = "white" { }
        _CausticsSpeed("CausticsSpeed", Float) = 0
        _CausticsContrast("CausticsContrast", Range(0.5, 2)) = 1
        [HDR] _CausticsColor("CausticsColor", Color) = (1, 1, 1, 1)

    }

    SubShader
    {
        Tags
        {
            "RanderPipline" = "UniversalPipeline"
            "RanderType" = "Opaque"
        }

        HLSLINCLUDE

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 将不占空间的材质相关变量放在CBUFFER中，为了兼容SRP Batcher
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;

                float _DistortSpeed;

                float4 _CausticsMap_ST;
                float _CausticsSpeed;
                float _CausticsContrast;
                float4 _CausticsColor;
            CBUFFER_END

            
            TEXTURE2D (_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            TEXTURE2D (_DistortMap);
            SAMPLER(sampler_DistortMap);

            TEXTURE2D (_CausticsMap);
            SAMPLER(sampler_CausticsMap);
            
            TEXTURE2D (_CausticsMaskMap);
            SAMPLER(sampler_CausticsMaskMap);
            



            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

        ENDHLSL

        Pass
        {
            Name "ForwardUnlit"
            Tags {"LightMode" = "UniversalForward"}

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            Varyings vert(Attributes input)
            {
                const VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                Varyings output;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float2 distortUv  = _Time.y * 0.05 * _DistortSpeed + uv;
                
                float distort = SAMPLE_TEXTURE2D(_DistortMap, sampler_DistortMap, distortUv).r;

                float2 uvNew = distort * 0.006 + (uv);
                
                float4 finishColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvNew) * _BaseColor;


                float causticsMask = SAMPLE_TEXTURE2D(_CausticsMaskMap, sampler_CausticsMaskMap, uv).r;

                float causticsUvOffset = _Time.y * _CausticsSpeed;

                float2 causticsUv = TRANSFORM_TEX(uv, _CausticsMap);                

                float2 causticsUvForward = causticsUvOffset * float2(0.1, 0.1) + causticsUv;
                float2 causticsUvBack = causticsUvOffset * float2(-0.1, -0.1)  + causticsUv + float2(0.41800001, 0.35499999);

                float causticForward = SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, causticsUvForward).x;
                float causticback = SAMPLE_TEXTURE2D(_CausticsMap, sampler_CausticsMap, causticsUvBack).x;

                float caustic = min(causticForward, causticback);

                caustic = pow(max(0, caustic), _CausticsContrast) * causticsMask;

                float3 causticColor = caustic * _CausticsColor.rgb * finishColor.rgb;

                finishColor.rgb += causticColor;

                return finishColor;
            }
            
            ENDHLSL
        }
    }
}