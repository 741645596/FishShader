Shader "WB/Fresnel" {
    // 模型边缘光fresnel效果 调整光的颜色 宽度 
    Properties {

        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendRGB ("BlendSrcRGB", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendRGB ("BlendDstRGB", Float) = 10
        [Space(20)]
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1

        [Space]
        [Header(Fresnel)]

        [HDR]_FresnelColor("Fresnel Color", Color) = (1,1,1,1)

        _FresnelR0("Fresnel R0", Float) = 0.00
        _FresnelScale("Fresnel Scale", Float) = 1
        _FresnelPow("Fresnel Pow", Float) = 5

    }

    SubShader {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass {
            Name "AddBlend"
            Blend[_SrcBlendRGB][_DstBlendRGB]//, [_SrcBlendA][_DstBlendA]
            Cull[_Cull]
            ZWrite[_ZWriteMode]
            Lighting Off
            ZTest [_ZTest]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)

            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _GlowScale;
            half _AlphaScale;

            half4 _FresnelColor;

            half _FresnelR0;
            half _FresnelScale;
            half _FresnelPow;

            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float3 vertex : POSITION;
                float3 normal : NORMAL;
                half4 color : COLOR;
                float2 texcoord :TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord :TEXCOORD0;
                float fresnel : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;

                float3 camOS = TransformWorldToObject(GetCameraPositionWS());
                half3 viewDirOS = normalize(camOS - input.vertex);
                half NdotV = abs(dot(input.normal, viewDirOS));


                half bias = _FresnelR0;
                float fresnelScale = (1.0 - bias);
                fresnelScale = _FresnelScale;
                
                half invDot = 1 - NdotV;
                half rimPower = pow(abs(invDot) , _FresnelPow);
                float fresnel = saturate(fresnelScale * rimPower);
                output.fresnel = fresnel;

                return output;
            }

            half4 frag(Varyings in_f) : SV_TARGET
            {
                float2 uv = in_f.texcoord;
                float4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half4 col = in_f.color * baseCol * _BaseColor;
                col.rgb *= _GlowScale;
                col.rgb = lerp(col.rgb, _FresnelColor.xyz * col.a, in_f.fresnel);
                col.a = saturate(col.a) * _AlphaScale;
                return col;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}