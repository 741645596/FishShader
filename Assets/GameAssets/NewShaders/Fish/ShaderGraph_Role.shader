Shader "Fish/ShaderGraph_Role" {
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendRGB ("BlendSrcRGB", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendRGB ("BlendDstRGB", Float) = 0
        
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        
        [ToggleUI]_IsHurt("挨打状态", Float) = 0
        _HurtColor("挨打颜色", Color) = (0, 0, 0, 0)
        _MainColor("主颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MainTex("主贴图", 2D) = "white" {}
        _AlbedoIntensity("亮度", Range(1, 10)) = 1
        [NoScaleOffset]_BumpMap("法线贴图", 2D) = "white" {}
        _NormalStr("法线强度", Range(0, 5)) = 1
        [NoScaleOffset]_TexMask1("遮罩图1(R:金属度 G:粗糙度-非金属 B:粗糙度-金属 A:透贴)", 2D) = "white" {}
        _Roughness1("粗糙度-非金属", Range(0, 8)) = 0
        [ToggleUI]_EnableRoughnessMask1("遮罩开关:    粗糙度-非金属", Float) = 0
        [NoScaleOffset]_TexMatCap1("MatCap-非金属", 2D) = "white" {}
        _MatCapFix1("误差值-非金属", Range(0, 1)) = 0
        [ToggleUI]_EnableMetal("遮罩开关:    金属度", Float) = 0
        _Metallic("金属度", Range(0, 1)) = 0
        [NoScaleOffset]_TexMatCap2("MatCap-金属", 2D) = "white" {}
        _MatCapFix2("误差值-金属", Range(0, 1)) = 0
        [ToggleUI]_EnableRoughnessMask2("遮罩开关:    粗糙度-金属", Float) = 0
        _Roughness2("粗糙度-金属", Range(0, 8)) = 0
        [ToggleUI]_BlendSwitch("混合开关", Float) = 0
        _BlendStrength("混合强度", Range(0, 10)) = 0
        [ToggleUI]_EnableRim("遮罩开关:    轮廓光强度", Float) = 0
        [NoScaleOffset]_TexMask2("遮罩图2(R:轮廓光强度 G:混合遮罩)", 2D) = "white" {}
        _RimColor("外轮廓颜色", Color) = (1, 1, 1, 0)
        _RimStr("轮廓光强度", Range(0, 5)) = 0
        [ToggleUI]_EnableTransparencyMap("遮罩开关:    透贴", Float) = 0
        _AlphaClipThreshold("透明度裁剪", Range(0, 1)) = 0.5
        _FishAlpha("透明度", Range(0, 1)) = 1

    }
    SubShader {
        Tags {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Geometry"
        }
        Pass {
            Name "Pass"
            Tags {
                // LightMode: <None>
            }

            // Render State
            
            Blend[_SrcBlendRGB][_DstBlendRGB]
            ZTest LEqual
            Cull[_Cull]
            ZWrite[_ZWriteMode]

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM
            // Pragmas

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
            // GraphKeywords: <None>

            // Defines
            #define _AlphaClip 1


            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // --------------------------------------------------
            // Structs and Packing

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float4 uv0 : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:TEXCOORD0;
                float4 tangentWS:TEXCOORD1;
                float4 texCoord0:TEXCOORD2;
                float3 viewDirectionWS:TEXCOORD3;
            };

            struct SurfaceDescriptionInputs
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceTangent;
                float3 WorldSpaceBiTangent;
                float3 WorldSpaceViewDirection;
                float4 uv0;
            };

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            half _IsHurt;
            half4 _HurtColor;
            half4 _MainColor;
            float4 _MainTex_TexelSize;
            half _AlbedoIntensity;
            float4 _BumpMap_TexelSize;
            half _NormalStr;
            float4 _TexMask1_TexelSize;
            half _Roughness1;
            half _EnableRoughnessMask1;
            float4 _TexMatCap1_TexelSize;
            half _MatCapFix1;
            half _EnableMetal;
            half _Metallic;
            float4 _TexMatCap2_TexelSize;
            half _MatCapFix2;
            half _EnableRoughnessMask2;
            half _Roughness2;
            half _BlendSwitch;
            half _BlendStrength;
            half _EnableRim;
            float4 _TexMask2_TexelSize;
            half4 _RimColor;
            half _RimStr;
            half _EnableTransparencyMap;
            half _AlphaClipThreshold;
            half _FishAlpha;
            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);           
            TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);           
            TEXTURE2D(_TexMask1);       SAMPLER(sampler_TexMask1);          
            TEXTURE2D(_TexMatCap1);     SAMPLER(sampler_TexMatCap1);        
            TEXTURE2D(_TexMatCap2);     SAMPLER(sampler_TexMatCap2);        
            TEXTURE2D(_TexMask2);       SAMPLER(sampler_TexMask2);
            // Graph Functions


            void Unity_Blend_LinearDodge_half4(half4 Base, half4 Blend, out half4 Out, half Opacity)
            {
                Out = Base + Blend;
                Out = lerp(Base, Out, Opacity);
            }

            void Unity_NormalStrength_half(half3 In, half Strength, out half3 Out)
            {
                Out = half3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
            }


            float4 NormalTS2WS(float3 normalTS,
                               float3 tangent, //WS
                               float3 bitangent, //WS
                               float3 normal //WS
            )
            {
                float4 normalWS = float4(
                    TransformTangentToWorld(normalTS, half3x3(tangent, bitangent, normal)), 0
                );
                return normalWS;

                // float4 normalWS = half4(
                //     dot(float3(tangent.x, bitangent.x, normal.x), normalTS),
                //     dot(float3(tangent.y, bitangent.y, normal.y), normalTS),
                //     dot(float3(tangent.z, bitangent.z, normal.z), normalTS),
                //     0
                // );
                //
                // return normalWS;
            }

            struct BNT
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceTangent;
                float3 WorldSpaceBiTangent;
            };

            void SG_MatCapUV(half fix,
                             float4 normalVS,
                             BNT IN,
                             out float4 SphereUV)
            {
                half fixFactor = (1 - clamp(fix, 0, 0.99)) * 0.5; //mul
                SphereUV = normalVS * fixFactor + 0.5;
            }

            void Unity_Remap_half(half In, half2 InMinMax, half2 OutMinMax, out half Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            void Unity_FresnelEffect_half(half3 Normal, half3 ViewDir, half Power, out half Out)
            {
                // Out = pow(min((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), 0.01), Power);
                // Out = pow((1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)))), Power);
                half v = (1.0 - saturate(dot(normalize(Normal), normalize(ViewDir))));
                v = v != 0 ? v : 0.001;
                Out = pow(v, Power);
            }

            // Graph Vertex
            struct VertexDescription
            {
                half3 Position;
                half3 Normal;
                half3 Tangent;
            };

            // Graph Pixel
            struct SurfaceDescription
            {
                half3 BaseColor;
                half Alpha;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                // half4 clrHurtOrMain = _IsHurt ? _HurtColor : _MainColor;
                // half4 clrHurtOrMain = lerp(_MainColor, _HurtColor, _IsHurt)
                half4 clrHurtOrMain = lerp(_MainColor, half4(1, 0.8, 0.8, 1) * _HurtColor, _IsHurt);

                SurfaceDescriptionInputs nodeParams = IN;

                half3 surf_color;
                half surf_alpha;

                {
                    half4 texMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, nodeParams.uv0.xy);
                    half4 mainColor = clrHurtOrMain * texMain; //mul
                    // half4 mainColor = lerp(texMain, _HurtColor, _IsHurt);

                    half4 texMask2 = SAMPLE_TEXTURE2D(_TexMask2, sampler_TexMask2, nodeParams.uv0.xy);
                    half texRimStr = texMask2.r;
                    half texBlendMask = texMask2.g;
                    half4 baseBlendWithMask = mainColor * texBlendMask; //mul

                    half4 blendRslt;
                    Unity_Blend_LinearDodge_half4(mainColor,
                                                  baseBlendWithMask,
                                                  blendRslt,
                                                  _BlendStrength);

                    // half4 blendOrMain = _BlendSwitch ? blendRslt : mainColor;
                    half4 blendOrMain = lerp(mainColor, blendRslt, _BlendSwitch);

                    
                    half4 texBump = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, nodeParams.uv0.xy);
                    texBump.rgb = UnpackNormal(texBump);

                    half3 normalTs;
                    Unity_NormalStrength_half((texBump.xyz),
                                              _NormalStr,
                                              normalTs);

                    BNT _MatCapParams;
                    _MatCapParams.WorldSpaceNormal = nodeParams.WorldSpaceNormal;
                    _MatCapParams.WorldSpaceTangent = nodeParams.WorldSpaceTangent;
                    _MatCapParams.WorldSpaceBiTangent = nodeParams.WorldSpaceBiTangent;

                    float4 normalWS_ = NormalTS2WS((normalTs.xyz),
                                                   IN.WorldSpaceTangent,
                                                   IN.WorldSpaceBiTangent,
                                                   IN.WorldSpaceNormal);

                    float4 normalVS = mul(UNITY_MATRIX_V, normalWS_);

                    float4 _MatCapUV1;
                    SG_MatCapUV(_MatCapFix1, normalVS, _MatCapParams, _MatCapUV1);

                    
                    half4 texMask1 = SAMPLE_TEXTURE2D(_TexMask1, sampler_TexMask1, nodeParams.uv0.xy);
                    half texMask1R = texMask1.r;
                    half texMask1G = texMask1.g;
                    half texMask1B = texMask1.b;
                    half texMask1A = texMask1.a;

                    half RoughnessTexG;
                    Unity_Remap_half(texMask1G, half2(0, 1), half2(0, 8), RoughnessTexG);
                    // half Roughness1 = _EnableRoughnessMask1 ? RoughnessTexG : _Roughness1;
                    half Roughness1 = lerp(_Roughness1, RoughnessTexG, _EnableRoughnessMask1);
                    
                    half4 texCap1 = SAMPLE_TEXTURE2D_LOD(_TexMatCap1, sampler_TexMatCap1,
                                                         (_MatCapUV1.xy),
                                                         Roughness1);

                    float4 _MatCapUV2;
                    SG_MatCapUV(_MatCapFix2, normalVS, _MatCapParams, _MatCapUV2);

                    half RoughnessTexB;
                    Unity_Remap_half(texMask1B, half2(0, 1), half2(0, 8), RoughnessTexB);
                    // half Roughness2 = _EnableRoughnessMask2 ? RoughnessTexB : _Roughness2;
                    half Roughness2 = lerp(_Roughness2, RoughnessTexB, _EnableRoughnessMask2);
                    
                    half4 texCap2 = SAMPLE_TEXTURE2D_LOD(_TexMatCap2, sampler_TexMatCap2,
                                                         (_MatCapUV2.xy),
                                                         Roughness2);

                    // half metallic = _EnableMetal ? texMask1R : _Metallic;
                    half metallic = lerp(_Metallic, texMask1R, _EnableMetal);
                    half4 matBlend = lerp(texCap1, texCap2, metallic);

                    half4 clrBlendRslt = blendOrMain * matBlend; //mul

                    // half rimStrFromTexOrVal = _EnableRim ? texRimStr : _RimStr;
                    half rimStrFromTexOrVal = lerp(_RimStr, texRimStr, _EnableRim);

                    // half Fresnel;
                    // Unity_FresnelEffect_half(normalWS_, nodeParams.WorldSpaceViewDirection, 1, Fresnel);
                    // half4 rimColor = _RimColor * Fresnel; //mul                

                    // half4 zero = half4(0, 0, 0, 0);

                    // half4 rim_clr = lerp(zero, rimColor, metallic);

                    // half4 clrRim = rim_clr * max(0, normalWS_.y) * rimStrFromTexOrVal; //mul

                    // half4 albedo = clrBlendRslt + clrRim;
                    half4 albedo = clrBlendRslt;

                    half4 clrRslt = albedo * _AlbedoIntensity; //mul

                    surf_color = (clrRslt.xyz);
                    // surf_alpha = _EnableTransparencyMap ? texMask1A : mainColor.a;
                    surf_alpha = lerp(mainColor.a, texMask1A, _EnableTransparencyMap);
                }


                SurfaceDescription surface = (SurfaceDescription)0;
                surface.BaseColor = surf_color;
                surface.Alpha = surf_alpha;
                return surface;
            }

            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
            {
                SurfaceDescriptionInputs output;
                ZERO_INITIALIZE(SurfaceDescriptionInputs, output);

                // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
                float3 unnormalizedNormalWS = input.normalWS;
                const float renormFactor = min(1.0 / length(unnormalizedNormalWS), 1.0);
				//const float renormFactor = 1.0 / length(unnormalizedNormalWS);
                // use bitangent on the fly like in hdrp
                // IMPORTANT! If we ever support Flip on double sided materials ensure bitangent and tangent are NOT flipped.
                float crossSign = (input.tangentWS.w > 0.0 ? 1.0 : -1.0) * GetOddNegativeScale();
                float3 bitang = crossSign * cross(input.normalWS.xyz, input.tangentWS.xyz);

                output.WorldSpaceNormal = renormFactor * input.normalWS.xyz;
                // we want a unit length Normal Vector node in shader graph

                // to preserve mikktspace compliance we use same scale renormFactor as was used on the normal.
                // This is explained in section 2.2 in "surface gradient based bump mapping framework"
                output.WorldSpaceTangent = renormFactor * input.tangentWS.xyz;
                output.WorldSpaceBiTangent = renormFactor * bitang;

                output.WorldSpaceViewDirection = input.viewDirectionWS;
                //TODO: by default normalized in HD, but not in universal
                output.uv0 = input.texCoord0;

                return output;
            }

            // --------------------------------------------------
            // Main

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                float3 positionWS = TransformObjectToWorld(input.positionOS);

                float3 viewDirectionWS = _WorldSpaceCameraPos - positionWS;

                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

                #ifdef VARYINGS_NEED_SCREENPOSITION
    output.screenPosition = ComputeScreenPos(output.positionCS, _ProjectionParams.x);
                #endif

                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS.xyz = normalWS;
                output.tangentWS.xyzw = tangentWS;
                output.texCoord0.xyzw = input.uv0;
                output.viewDirectionWS.xyz = viewDirectionWS;

                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                Varyings unpacked = input;

                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

                half alpha = surfaceDescription.Alpha;
                clip(alpha - _AlphaClipThreshold);

                #ifdef _ALPHAPREMULTIPLY_ON
    surfaceDescription.BaseColor *= surfaceDescription.Alpha;
                #endif

                return half4(surfaceDescription.BaseColor, alpha * _FishAlpha);
            }
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
    //    CustomEditor "ShaderGraphRoleUI"
    //    FallBack "Hidden/Shader Graph/FallbackError"
}