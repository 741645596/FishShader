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
            half4 _BaseMap_ST;
            half4 _BaseColor;
            half _GlowScale;
            half _AlphaScale;

            half4 _FresnelColor;
            half _FresnelScale;
            half _FresnelPow;
            CBUFFER_END
            sampler2D _BaseMap;

            struct Attributes
            {
                half3 vertex : POSITION;
                half3 normal : NORMAL;
                half4 color : COLOR;
                half2 texcoord :TEXCOORD0;
            };

            struct Varyings
            {
                half4 positionCS : SV_POSITION;
                half4 color : COLOR;
                half2 texcoord :TEXCOORD0;
                half fresnel : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;

                half3 camOS = TransformWorldToObject(GetCameraPositionWS());
                half3 viewDirOS = normalize(camOS - input.vertex);
                half NdotV = abs(dot(input.normal, viewDirOS));
                output.fresnel = pow(abs(1 - NdotV) , _FresnelPow) * _FresnelScale;
                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                half4 baseCol = tex2D(_BaseMap, input.texcoord);
                half4 col = input.color * baseCol * _BaseColor;
                col.rgb *= _GlowScale;
                col.rgb = lerp(col.rgb, _FresnelColor.xyz * col.a, input.fresnel);
                col.a = saturate(col.a) * _AlphaScale;
                return col;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}