Shader "WB/FXEffect" {
    Properties{
        [Foldout] _BlendName("混合控制",Range(0,1)) = 0
        [FoldoutItem] [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("BlendSource", Float) = 5
        [FoldoutItem] [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("BlendDestination", Float) = 1
        [FoldoutItem] [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [FoldoutItem] [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float) = 0
        [FoldoutItem] [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        // base
        [Foldout] _BaseName("主纹理面板",Range(0,1)) = 0
        [FoldoutItem] _BaseMap("Base Map", 2D) = "white" {}
        [FoldoutItem][ToggleUI] _CustomUV("自定义主贴图UV偏移曲线TEXCOORD0.xy", Float) = 1.0
        [FoldoutItem] [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        [FoldoutItem] _GlowScale("Glow Scale", float) = 1
        [FoldoutItem] _AlphaScale("Alpha Scale", float) = 1
        // 流动
        [Foldout] _ScrollName("流动面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _Scroll("流动控制开关", Float) = 0.0
        [FoldoutItem] _MainSpeed("流动速度及方向", Vector) = (1,1,0,0)
        //  附加色
        [Foldout] _AddColorName("附加色面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _Addcolor("附加色控制开关", Float) = 0.0
        [FoldoutItem] _ColorTex("ColorTex", 2D) = "black" {}
        // mask
        [Foldout] _MaskName("Mask 面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _Mask("Mask开关", Float) = 0.0
        [FoldoutItem] _MaskTex("mask纹理", 2D) = "white" {}
        [FoldoutItem] _MaskSpeed("mask speed", Vector) = (0,0,0,0)
        // 扰动
        [Foldout] _DistortionName("扰动面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _Distortion("扰动开关", Float) = 0.0
        [FoldoutItem] _UVNoiseTex("UVNoiseTex", 2D) = "black" {}
        [FoldoutItem] _UVDistortion("UVDistortion", Float) = 0
        [FoldoutItem] _UVScrollDir("NoiseScroll", Vector) = (0,0,1,1)
        // 溶解
        [Foldout] _DissolveName("溶解面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _Dissolve("溶解开关", Float) = 0.0
        [FoldoutItem]  _CutOut("CutOut", Range(0, 1)) = 0
        [FoldoutItem][ToggleUI] _CustomCutOut("自定义CutOut曲线TEXCOORD1.z", Float) = 0
        [FoldoutItem]  _UseSoftCutout("Use Soft Cutout", Int) = 0
        [FoldoutItem]  _UseParticlesAlphaCutout("Use Particles Alpha", Int) = 0
        [FoldoutItem][ToggleUI] _UseCutoutTex("Use Cutout Texture", Float) = 0
        [FoldoutItem] _CutoutTex("Cutout Tex(R)", 2D) = "white" {}
        [FoldoutItem] [HDR]_CutoutColor("Cutout Color", Color) = (0,0,0,1)
        [FoldoutItem]  _UVCutOutScroll("Cutout UVScroll", Vector) = (0,0,0,0)
        [FoldoutItem]  _CutoutThreshold("Cutout Threshold", Range(0, 1)) = 0
        // 顶点动画
        [Foldout] _VertexAnimateName("顶点动画面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _Vertexani("顶点动画开关", Float) = 0.0
        [FoldoutItem][NoScaleOffset] _VertexAnimateTex("顶点动画纹理",2D) = "black"{}
        [FoldoutItem] _VertexScrollingX("顶点动画流动U", Float) = 0
        [FoldoutItem] _VertexScrollingY("顶点动画流动Y", Float) = 0
        [FoldoutItem] _VertexStrength("顶点动画强度", Float) = 0
    }

        SubShader{
            Tags {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "PerformanceChecks" = "False"
                "RenderPipeline" = "UniversalPipeline"
            }

            Pass {
                Name "FXEffect"
                Blend[_SrcBlend][_DstBlend]
                Cull[_Cull]
                ZWrite[_ZWriteMode]
                Lighting Off
                ZTest[_ZTest]

                HLSLPROGRAM
                #pragma multi_compile __ _SCROLL_ON
                #pragma multi_compile __ _ADDCOLOR_ON
                #pragma multi_compile __ _MASK_ON
                #pragma multi_compile __ _DISTORTION_ON
                #pragma multi_compile __ _DISSOLVE_ON
                #pragma multi_compile __ _VERTEXANI_ON

                #pragma vertex vert
                #pragma fragment frag


                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;
                half _GlowScale;
                half _AlphaScale;
                // 流动
                float4 _MainSpeed;
                // 附加色
                float4 _ColorTex_ST;
                // mask
                half4 _MaskTex_ST;
                float4 _MaskSpeed;
                // 扰动
                half _CustomUV;
                half4 _UVNoiseTex_ST;
                half4 _UVScrollDir;
                half _UVDistortion;
                // 溶解
                half _UseCutoutTex;
                float4 _CutoutTex_ST;
                half _CutOut;
                half _CustomCutOut;
                half _UseSoftCutout;
                half _UseParticlesAlphaCutout;
                half4 _CutoutColor;
                half _CutoutThreshold;
                float2 _UVCutOutScroll;
                // 顶点动画
                float4 _VertexAnimateTex_ST;
                half _VertexScrollingX;
                half _VertexScrollingY;
                half _VertexStrength;

                CBUFFER_END

                TEXTURE2D(_BaseMap);
                SAMPLER(sampler_BaseMap);
#if _MASK_ON
                TEXTURE2D(_MaskTex);
                SAMPLER(sampler_MaskTex);
#endif

#if _DISTORTION_ON
                TEXTURE2D(_UVNoiseTex);
                SAMPLER(sampler_UVNoiseTex);
#endif

#if _DISSOLVE_ON
                TEXTURE2D(_CutoutTex);
                SAMPLER(sampler_CutoutTex);
#endif

#if _ADDCOLOR_ON
                TEXTURE2D(_ColorTex);
                SAMPLER(sampler_ColorTex);
#endif

#if _VERTEXANI_ON
                TEXTURE2D(_VertexAnimateTex);
                SAMPLER(sampler_VertexAnimateTex);
#endif


                struct Attributes
                {
                    float3 vertex : POSITION;
                    float3 normal : NORMAL;
                    half4 color : COLOR;
                    float2 texcoord :TEXCOORD0;
                    float4 texCoordCst : TEXCOORD1; // 粒子特效中自定义的数据
                };

                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    half4 color : COLOR;
                    float2 texcoord :TEXCOORD0;
#if _ADDCOLOR_ON
                    half2 texcoordColor : TEXCOORD1;
#endif
#if _MASK_ON
                    float2 texcoordMask: TEXCOORD2;
#endif
#if _DISTORTION_ON
                    float2 texcoordNoise: TEXCOORD3;
#endif
#if _DISSOLVE_ON
                    float2 texcoordCutOut: TEXCOORD4;
#endif
                    float3 CustData : TEXCOORD5;
                };


                // 旋转uv 计算
                void roateUV(float2 _UVRotate, half2 pivot, inout float2 uv)
                {
                    half cosAngle = cos(_UVRotate.x + _Time.y * _UVRotate.y);
                    half sinAngle = sin(_UVRotate.x + _Time.y * _UVRotate.y);
                    half2x2 roation = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                    uv.xy = mul(roation, uv.xy -= pivot) + pivot;
                }
                // 扰动uv 计算
#if _DISTORTION_ON
                void distorUVbyTex(float2 noiseUV, inout float2 uvMain)
                {
                    noiseUV += half2(_UVScrollDir.xy * _Time.y);
                    float2 uvOffset = SAMPLE_TEXTURE2D(_UVNoiseTex, sampler_UVNoiseTex, noiseUV).xy * _UVDistortion * _UVScrollDir.zw;
                    uvMain += uvOffset;
                }
#endif

                Varyings vert(Attributes input)
                {
                    Varyings output;
                    // 顶点动画
                    float3 positionWS = TransformObjectToWorld(input.vertex.xyz);
#if _VERTEXANI_ON
                    float2 vertexAnimateTexUV = TRANSFORM_TEX(input.texcoord, _VertexAnimateTex) + half2(_VertexScrollingX, _VertexScrollingY) * _Time.y;
                    half4 vertexAnimateMask = SAMPLE_TEXTURE2D_LOD(_VertexAnimateTex, sampler_VertexAnimateTex, vertexAnimateTexUV, 0);
                    positionWS += _VertexStrength * vertexAnimateMask.r * input.normal;
#endif
                    output.positionCS = TransformWorldToHClip(positionWS);
                    // 顶点色
                    output.color = input.color;
                    // 主纹理
                    output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                    // 附加色
#if _ADDCOLOR_ON
                    output.texcoordColor = TRANSFORM_TEX(input.texcoord, _ColorTex);
#endif
                    // mask
#if _MASK_ON
                    output.texcoordMask = TRANSFORM_TEX(input.texcoord, _MaskTex);
#endif
                    // 扰动
                    output.CustData.xy = _CustomUV ? input.texCoordCst.xy : float2(0, 0);
                    output.CustData.z = input.texCoordCst.z;
#if _DISTORTION_ON
                    output.texcoordNoise = TRANSFORM_TEX(input.texcoord, _UVNoiseTex);
#endif
                    // 溶解
#if _DISSOLVE_ON
                    output.texcoordCutOut = TRANSFORM_TEX(input.texcoord, _CutoutTex);
#endif

                    return output;
                }



                half4 frag(Varyings fInput) : SV_TARGET
                {
                    half2 pivot = 0.5; //_UVRotate.xy;
                    half4 vertColor = fInput.color;
                    
                    float2 uvMain = fInput.texcoord;
                    // 流动
#if _SCROLL_ON
                    uvMain += _MainSpeed.xy * _Time.y;
                    roateUV(_MainSpeed.zw, pivot, uvMain);
#endif
                    uvMain += fInput.CustData.xy;
                    // 扰动uv
#if _DISTORTION_ON
                    distorUVbyTex(fInput.texcoordNoise.xy, uvMain);
#endif
                    float4 mainTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvMain);
                    half4 col = mainTexColor * _BaseColor * vertColor;
                    col.rgb *= _GlowScale;
                    // 附加色
#if _ADDCOLOR_ON
                    col.rgb += SAMPLE_TEXTURE2D(_ColorTex, sampler_ColorTex, fInput.texcoordColor).rgb;
#endif
                    // 溶解
#if _DISSOLVE_ON
                    float cutout = _CustomCutOut ? fInput.CustData.z : _CutOut;
                    cutout = lerp(cutout, (1.001 - vertColor.a + cutout), _UseParticlesAlphaCutout);
                    float2 cutoutUV = fInput.texcoordCutOut.xy + _UVCutOutScroll.xy * _Time.y;
                    float mask = SAMPLE_TEXTURE2D(_CutoutTex, sampler_CutoutTex, cutoutUV).r;
                    mask = _UseCutoutTex ? mask : mainTexColor.a;

                    float diffMask = mask - cutout;
                    float alphaMask = lerp(saturate(diffMask * 10000) * col.a, saturate(diffMask * 2) * col.a, _UseSoftCutout);
                    float alphaMaskThreshold = saturate((diffMask - _CutoutThreshold) * 10000) * col.a;
                    float3 col2 = lerp(col.rgb, _CutoutColor.rgb, saturate((1 - alphaMaskThreshold) * alphaMask));
                    col.rgb = lerp(col.rgb, col2, step(0.01, _CutoutThreshold));
                    col.a = alphaMask;
#endif
                    // mask 部分
#if _MASK_ON
                    float2 uvMask = fInput.texcoordMask;
                    uvMask += _MaskSpeed.xy * _Time.y;
                    roateUV(_MaskSpeed.zw, pivot, uvMask);
                    col.a *= SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvMask).r;
#endif
                    col.a = saturate(col.a) * _AlphaScale;
                    return col;
                }
                ENDHLSL
            }
        }
            CustomEditor "FoldoutShaderGUI"
}