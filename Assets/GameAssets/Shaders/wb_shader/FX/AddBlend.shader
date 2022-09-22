Shader "WB/AddBlend" {
    // 仅有add blend 模式，颜色调整 已经辉光调整
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendRGB ("BlendSrcRGB", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendRGB ("BlendDstRGB", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4

        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1
    }

    SubShader {
        Tags {
            "Queue" = "Transparent" "IgnoreProjector" = "True"
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
            half _GlowScale;
            half _AlphaScale;
            half4 _BaseMap_ST;
            half4 _BaseColor;
            CBUFFER_END
            sampler2D _BaseMap;

            struct Attributes
            {
                half3 vertex : POSITION;
                half4 color : COLOR;
                half2 texcoord :TEXCOORD0;
            };

            struct Varyings
            {
                half4 positionCS : SV_POSITION;
                half4 color : COLOR;
                half2 texcoord :TEXCOORD0;
            };


            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                half2 uv = input.texcoord;
                half4 baseCol = tex2D(_BaseMap, uv);
                half4 vertColor = input.color;

                half4 col = vertColor * baseCol * _BaseColor;
                col.rgb *= _GlowScale;
                col.a = saturate(col.a) * _AlphaScale;
                return col;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}