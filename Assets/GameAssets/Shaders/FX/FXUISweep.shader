Shader "FX/FXUISweep" {
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(One, 1 , OneMinusSrcAlpha, 10 )] _DstBlend ("BlendDestination", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4

        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1

        _MainSpeed("MainTex Speed", Vector) = (0,0,0,0)
        [ToggleUI] _CustomUV("自定义主贴图UV偏移曲线TEXCOORD1.xy", Float) = 1.0

        [Space(20)]
        _MaskTex("Mask ( R Channel )", 2D) = "white" {}
        _MaskSpeed("MaskTex Speed", Vector) = (0,0,0,0)
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
            Name "ForwardLit"
            Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWriteMode]
            Lighting Off
            ZTest [_ZTest]
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _MainSpeed;
            half _CustomUV;
            float4 _ColorTex_ST;

            half4 _MaskTex_ST;

            // half4 _UVNoiseTex_ST;

            // half _UseCutoutTex;
            // float4 _CutoutTex_ST;
            // half _CustomCutOut;
            // half4 _CutoutColor;

            half _GlowScale;
            half _AlphaScale;
            half4 _MaskSpeed;

            float4 _ClipRect;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);


            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            // TEXTURE2D(_UVNoiseTex);
            // SAMPLER(sampler_UVNoiseTex);

            // TEXTURE2D(_CutoutTex);
            // SAMPLER(sampler_CutoutTex);

            struct AttributesParticle
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                half4 color : COLOR;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoordCst : TEXCOORD1;
            };

            struct VaryingsParticle
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                // half2 texcoordColor : TEXCOORD2;
                // float4 texcoordNoise: TEXCOORD1;
                half2 texcoordMask : TEXCOORD4;
                float3 CustData : TEXCOORD3;

                #ifdef UNITY_UI_CLIP_RECT
                half4  clipmask : TEXCOORD5;
                #endif
            };


            void roateUV(float2 _UVRotate, half2 pivot, inout float2 uv)
            {
                half cosAngle = cos(_UVRotate.x + _Time.y * _UVRotate.y);
                half sinAngle = sin(_UVRotate.x + _Time.y * _UVRotate.y);
                half2x2 roation = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                uv.xy = mul(roation, uv.xy -= pivot) + pivot;
            }

            /*
            void distorUVbyTex(float2 noiseUV, inout float2 uvMain)
            {
                float2 scrollDir = _UVScrollDir.xy;
                float2 distStr = _UVScrollDir.zw;
                noiseUV += half2(scrollDir * _Time.y);
                
                float2 uvOffset = SAMPLE_TEXTURE2D(_UVNoiseTex, sampler_UVNoiseTex, noiseUV).xy
                * _UVDistortion * distStr
                ;
                uvMain += uvOffset;
            }
            */
            
            #include "clipRect.cginc"
            #include "../wb_Shader/ColorCore.hlsl"
            
            VaryingsParticle vertParticleUnlit(AttributesParticle input)
            {
                VaryingsParticle output = (VaryingsParticle)0;
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;
                output.texcoord.xy = TRANSFORM_TEX(input.texCoord0, _BaseMap);

                output.CustData.xy = _CustomUV ? input.texCoordCst.xy : float2(0, 0);
                output.CustData.z = input.texCoordCst.z;

                // output.texcoordColor = TRANSFORM_TEX(input.texCoord0, _ColorTex);
                output.texcoordMask = TRANSFORM_TEX(input.texCoord0, _MaskTex);
                // output.texcoordNoise.xy = TRANSFORM_TEX(input.texCoord0, _UVNoiseTex);
                // output.texcoordNoise.zw = TRANSFORM_TEX(input.texCoord0, _CutoutTex);

                PASS_CLIP_MASK(output);
                
                return output;
            }


            half4 fragParticleUnlit(VaryingsParticle In) : SV_Target
            {
                half4 vertColor = In.color;
                vertColor = Gamma22(vertColor);
                float2 uvMain = In.texcoord.xy;
                uvMain += _MainSpeed.xy * _Time.y;
                half2 pivot = 0.5; //_UVRotate.xy;
                roateUV(_MainSpeed.zw, pivot, uvMain);
                uvMain += In.CustData.xy;
                // distorUVbyTex(fInput.texcoordNoise.xy, uvMain);

                float4 mainTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvMain);
                mainTexColor = Gamma22(mainTexColor);
                _BaseColor = Gamma22(_BaseColor);
                half2 uvMask = In.texcoordMask;
                uvMask.xy += _MaskSpeed.xy * _Time.y;
                roateUV(_MaskSpeed.zw, pivot, uvMask);

                half4 color = mainTexColor * _BaseColor;
                color *= vertColor;
                color.rgb *= _GlowScale;
		
                // float4 mapColor = SAMPLE_TEXTURE2D(_ColorTex, sampler_ColorTex, fInput.texcoordColor);		
                // col.rgb += mapColor;
                //
                // {
                //     //CutOut
                //     float cutout = _CustomCutOut ? fInput.CustData.z : _CutOut;
                //
                //     cutout = lerp(cutout, (1.001 - vertColor.a + cutout), _UseParticlesAlphaCutout);
                //
                //     float2 cutoutUV = fInput.texcoordNoise.zw + _UVCutOutScroll.xy * _Time.y;
                //     float mask = SAMPLE_TEXTURE2D(_CutoutTex, sampler_CutoutTex, cutoutUV).r;
                //     mask = _UseCutoutTex ? mask : mainTexColor.a;
                //
                //     float diffMask = mask - cutout;
                //     float alphaMask = lerp(
                //         saturate(diffMask * 10000) * col.a,
                //         saturate(diffMask * 2) * col.a,
                //         _UseSoftCutout);
                //
                //     float alphaMaskThreshold = saturate((diffMask - _CutoutThreshold) * 10000) * col.a;
                //     float3 col2 = lerp(col.rgb, _CutoutColor.rgb, saturate((1 - alphaMaskThreshold) * alphaMask));
                //     col.rgb = lerp(col.rgb, col2, step(0.01, _CutoutThreshold));
                //     col.a = alphaMask;
                // }

                float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvMask);
                color.a = saturate(color.a * _AlphaScale);
                color.a = saturate(color.a * maskTexColor.r);
                
                DO_CLIP_RECT_FRAG(color, In);
                color = Gamma045(color);
                return color;
            }
            #pragma vertex vertParticleUnlit
            #pragma fragment fragParticleUnlit
            ENDHLSL
        }
    }
}