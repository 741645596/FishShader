Shader "WB/UVMask" {
    // 纹理流动效果，UV速度调整，Mask遮罩，Mask 速度调整
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1
        _BaseColorSpeed("Base Color Speed", Vector) = (1,1,0,0)

        _Mask("Mask", 2D) = "white" {}
        _MaskSpeed("mask speed", Vector) = (0,0,0,0)
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
            Name "UVMask"
            Blend[_SrcBlend][_DstBlend]
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
            half4 _BaseColorSpeed;
            half4 _Mask_ST;
            half4 _MaskSpeed;
            CBUFFER_END

            sampler2D _BaseMap;
            sampler2D _Mask;

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
                half2 texcoordMask: TEXCOORD1;
            };


            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.texcoordMask = TRANSFORM_TEX(input.texcoord, _Mask);
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;
                return output;
            }

            void roateUV(half2 _UVRotate, half2 pivot, inout half2 uv)
            {
                half cosAngle = cos(_UVRotate.x + _Time.y * _UVRotate.y);
                half sinAngle = sin(_UVRotate.x + _Time.y * _UVRotate.y);
                half2x2 roation = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                uv.xy = mul(roation, uv.xy -= pivot) + pivot;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                half2 pivot = 0.5; //_UVRotate.xy;

                half2 uv = input.texcoord;
                uv += _BaseColorSpeed.xy * _Time.y;
                roateUV(_BaseColorSpeed.zw, pivot, uv);
                float4 baseCol = tex2D(_BaseMap, uv);

                half2 uvMask = input.texcoordMask;
                uvMask += _MaskSpeed.xy * _Time.y;
                roateUV(_MaskSpeed.zw, pivot, uvMask);
                half maskAlpha = tex2D(_Mask, uvMask).r;

                half4 col = input.color * baseCol * _BaseColor;
                col.rgb *= _GlowScale;
                col.a = saturate(col.a * maskAlpha) * _AlphaScale;
                return col;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}