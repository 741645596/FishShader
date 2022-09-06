Shader "FX/AddBlend" {
    // 仅有add blend 模式，颜色调整 已经辉光调整
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendRGB ("BlendSrcRGB", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendRGB ("BlendDstRGB", Float) = 1
        //[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendA ("BlendSrcA", Float) = 1
        //[Enum(UnityEngine.Rendering.BlendMode)] _DstBlendA ("BlendDstA", Float) = 10
        [Space(20)]
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1
        [ToggleUI]_SoftOn("soft on/off", Float) = 0
        _InvFade ("Soft Particles Factor", Range(0.000001,3.0)) = 1.0

        [HideFromInspector]_ClipRect ("Clip Rect", Vector) = (-1, -1, 0, 0)
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
            #include "../wb_Shader/ColorCore.hlsl"
            

            CBUFFER_START(UnityPerMaterial)

            float4 _BaseMap_ST;
            half4 _BaseColor;
            half _GlowScale;
            half _AlphaScale;

            float _InvFade;
            float _SoftOn;

            float4 _ClipRect;
            // float _UIMaskSoftnessX;
            // float _UIMaskSoftnessY;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float3 vertex : POSITION;
                half4 color : COLOR;
                float2 texcoord :TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord :TEXCOORD0;
                float4 projPos: TEXCOORD2;

                #ifdef UNITY_UI_CLIP_RECT
                half4  clipmask : TEXCOORD3;
                #endif
            };
            #include "clipRect.cginc"
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                float4 vPosition = TransformObjectToHClip(input.vertex);
                output.positionCS = vPosition;
                output.color = input.color;
                output.projPos = ComputeScreenPos(output.positionCS);

                PASS_CLIP_MASK(output);

                return output;
            }

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            half4 frag(Varyings In) : SV_TARGET
            {
                float2 uv = In.texcoord;
                float4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                baseCol = Gamma22(baseCol);

                float4 screen_position = In.projPos;
                float fade = 1;
                {
                    //SoftParticles
                    float2 xy = screen_position.xy / screen_position.w;
                    float sceneZ = LinearEyeDepth(SampleSceneDepth(xy), _ZBufferParams);
                    float partZ = In.projPos.z;
                    fade = saturate(_InvFade * (sceneZ - partZ));
                }
                half4 vertColor = In.color;
                vertColor = Gamma22(vertColor);
                _BaseColor = Gamma22(_BaseColor);

                vertColor.a *= _SoftOn ? fade : 1;

                half4 color = vertColor * baseCol * _BaseColor;
                color.rgb *= _GlowScale;

                color.a = saturate(color.a) * _AlphaScale;

                DO_CLIP_RECT_FRAG(color, In);
                color = Gamma045(color);
                return color;
            }
            ENDHLSL
        }
    }
	FallBack Off
}