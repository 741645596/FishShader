Shader "FX/UVMask" {
    // 纹理流动效果，UV速度调整，Mask遮罩，Mask 速度调整
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1
        _BaseColorSpeed("Base Color Speed", Vector) = (1,1,0,0)

        _Mask("Mask", 2D) = "white" {}
        _MaskSpeed("mask speed", Vector) = (0,0,0,0)
        
        _ClipRect ("Clip Rect", Vector) = (-1, -1, 0, 0)
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
            #include "../wb_Shader/ColorCore.hlsl"
            CBUFFER_START(UnityPerMaterial)

            float4 _BaseMap_ST;
            float4 _BaseColor;
            float _GlowScale;
            float _AlphaScale;
            float4 _BaseColorSpeed;
            float4 _Mask_ST;
            float4 _MaskSpeed;

            float4 _ClipRect;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            struct Attributes
            {
                float3 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord :TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord :TEXCOORD0;
                float2 texcoordMask: TEXCOORD1;

                #ifdef UNITY_UI_CLIP_RECT
                half4  clipmask : TEXCOORD4;
                #endif
            };
#include "clipRect.cginc"

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.texcoordMask = TRANSFORM_TEX(input.texcoord, _Mask);
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;

                PASS_CLIP_MASK(output);
                
                return output;
            }

            float4 frag(Varyings in_f) : SV_TARGET
            {
                float4 vertColor = in_f.color;
                // vertColor = Gamma22(vertColor);
                float2 pivot = 0.5; //_UVRotate.xy;

                float2 uv = in_f.texcoord;
                float t = abs(frac(_Time.y * 0.05));
                float calcTime = t * 20;
                uv += _BaseColorSpeed.xy * calcTime;
                roateUV(_BaseColorSpeed.zw, calcTime, pivot, uv);
                float4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                // baseCol = Gamma22(baseCol);
                // _BaseColor = Gamma22(_BaseColor);

                float2 uvMask = in_f.texcoordMask;
                uvMask += _MaskSpeed.xy * calcTime;
                roateUV(_MaskSpeed.zw, calcTime, pivot, uvMask);
                float4 maskCol = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, uvMask);

                float4 col = vertColor * baseCol * _BaseColor;
                col.rgb *= _GlowScale;
                col.a = saturate(col.a * _AlphaScale);

                col.a = saturate(col.a * maskCol.r * vertColor.a * _BaseColor.a);
                //col.a = col.a * step(0.03, col.a);
                //if (col.a < 0.03)
                //    col.a = 0;

                DO_CLIP_RECT_FRAG(col, in_f);
                
                // col = Gamma045(col);
                return col;
            }
            ENDHLSL
        }
    }
}