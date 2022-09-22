Shader "WB/Dissolve" {
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(One, 1 , OneMinusSrcAlpha, 10 )] _DstBlend ("BlendDestination", Float) = 1

        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 2

        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)

        _UVBaseScroll("Base UVScroll", Vector) = (0,0,0,0)

        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1

        _CutOut("CutOut", Range(0, 1)) = 1
        _UseSoftCutout("Use Soft Cutout", Int) = 0
        _UseParticlesAlphaCutout("Use Particles Alpha", Int) = 0

        [Toggle(USE_CUTOUT_TEX)] _UseCutoutTex("Use Cutout Texture", Int) = 0
        _CutoutTex("Cutout Tex(R)", 2D) = "white" {}
        [HDR]_CutoutColor("Cutout Color", Color) = (0,0,0,1)
        _UVCutOutScroll("Cutout UVScroll", Vector) = (0,0,0,0)
        _CutoutThreshold("Cutout Threshold", Range(0, 1)) = 0.015
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
            Name "ForwardLit"
            Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWriteMode]
            Lighting Off
            ZTest [_ZTest]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #pragma multi_compile __ USE_CUTOUT_TEX

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseMap_ST;
            half4 _BaseColor;
            half2 _UVBaseScroll;

            #ifdef USE_CUTOUT_TEX
            half4 _CutoutTex_ST;
            #endif

            half _UseSoftCutout;
            half _UseParticlesAlphaCutout;

            half _CutOut;
            half4 _CutoutColor;
            half _CutoutThreshold;

            half2 _UVCutOutScroll;

            half _GlowScale;
            half _AlphaScale;

            CBUFFER_END
            sampler2D _BaseMap;
            sampler2D _CutoutTex;

            struct AttributesParticle
            {
                half4 vertex : POSITION;
                half4 color : COLOR;
                half3 texcoord : TEXCOORD0;
            };

            struct VaryingsParticle
            {
                half4 positionCS : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                half4 color : COLOR;
                #if defined (USE_CUTOUT_TEX)
                half4 texcoordNoise : TEXCOORD2;
                #endif
                half age_percent : TEXCOORD3;
            };

            VaryingsParticle vert(AttributesParticle input)
            {
                VaryingsParticle output = (VaryingsParticle)0;

                output.positionCS = TransformObjectToHClip(input.vertex.xyz);

                output.color = input.color;

                output.texcoord.xy = TRANSFORM_TEX(input.texcoord, _BaseMap);

                if (input.texcoord.z == 0)
                    output.age_percent = 1;
                else
                    output.age_percent = input.texcoord.z;

                #ifdef USE_CUTOUT_TEX
                output.texcoordNoise.zw = TRANSFORM_TEX(input.texcoord, _CutoutTex);
                #endif
                return output;
            }

            half4 frag(VaryingsParticle fInput) : SV_Target
            {
                half4 vertColor = fInput.color;
                half2 uv = fInput.texcoord.xy;

                half4 mainTexColor = tex2D(_BaseMap, uv +_Time.y * _UVBaseScroll.xy);
                half4 col = mainTexColor * _BaseColor;

                half cutout = _CutOut * fInput.age_percent;
                cutout = lerp(cutout, (1.001 - vertColor.a + cutout), _UseParticlesAlphaCutout);

                #ifdef USE_CUTOUT_TEX
                half2 cutoutUV = fInput.texcoordNoise.zw + _UVCutOutScroll.xy * _Time.y;
                half mask = tex2D(_CutoutTex, cutoutUV).r;
                #else
                half mask = mainTexColor.a;
                #endif

                half diffMask = mask - cutout;
                half alphaMask = lerp(
                    saturate(diffMask * 10000) * col.a,
                    saturate(diffMask * 2) * col.a,
                    _UseSoftCutout);

                half alphaMaskThreshold = saturate((diffMask - _CutoutThreshold) * 10000) * col.a;
                half3 col2 = lerp(col.rgb, _CutoutColor.rgb, saturate((1 - alphaMaskThreshold) * alphaMask));
                col.rgb = lerp(col.rgb, col2, step(0.01, _CutoutThreshold));
                col.a = alphaMask;

                col *= vertColor;

                col.a = saturate(col.a * _AlphaScale);
                return col;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}