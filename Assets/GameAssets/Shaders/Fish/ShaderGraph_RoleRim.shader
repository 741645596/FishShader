Shader "Fish/ShaderGraph_RoleRim" {
    Properties {
        //[ToggleUI]_IsHurt("挨打状态", Float) = 0
        //_HurtColor("挨打颜色", Color) = (1, 0, 0, 1)
        //_HurtMultiple ("HurtMultiple程序控制", Float) = 1
        //_HurtMultipleCust ("HurtMultiple美术控制", Float) = 0.45
        [Foldout] _HurtName("Hurt plane",Range(0,1)) = 0
        [FoldoutItem] [KeywordEnum(Rim,Albedo)] _HitColorChannel("HitColorType", Float) = 1.0
        [FoldoutItem][Toggle] _TestHitColor("美术测试受击变红颜色开关", Float) = 0.0
        [FoldoutItem] _HitColor("HitColor[美术控制]", Color) = (1,1,1,1)
        [FoldoutItem] _HitMultiple("HitMultiple[美术控制]", Range(0,1)) = 1
        [FoldoutItem] _HitRimPower("HitRim Power[美术控制]", Range(0.01, 10)) = 0.01
        [FoldoutItem] _HitRimSpread("Hit Rim Spread[美术控制]", Range(-15, 4.99)) = 0.01
        [FoldoutItem] _OverlayColor("_OverlayColor[程序控制]", Color) = (1,1,1,1)
        [FoldoutItem] _OverlayMultiple("_OverlayMultiple[程序控制]", Range(0,1)) = 1

        _MainColor("主颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MainTex("主贴图", 2D) = "white" {}
        _AlbedoIntensity("亮度", Range(0, 10)) = 1
        [NoScaleOffset]_BumpMap("法线贴图", 2D) = "white" {}
        _NormalStr("法线强度", Range(1, 5)) = 1
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

        _Fresnel("Fresnel", Range( 0 , 1)) = 0.1943072
        [HDR]_FresnelColor("FresnelColor", Color) = (0.9811321,0.9811321,0.9811321,1)
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
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
            ZWrite On

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
            #pragma multi_compile _HITCOLORCHANNEL_RIM _HITCOLORCHANNEL_ALBEDO
            #pragma multi_compile __ _TESTHITCOLOR_ON
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
                float3 viewDirectionWS;
                float4 uv0;
            };

            // --------------------------------------------------
            // Graph

            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
            
            //#include "hurt_dec.cginc"
            #include "../wb_shader/HitRed_dec.hlsl"  

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
            }



            void SG_MatCapUV(half fix, float4 normalVS, out float4 SphereUV)
            {
                half fixFactor = (1 - clamp(fix, 0, 0.99)) * 0.5; //mul
                SphereUV = normalVS * fixFactor + 0.5;
            }

            void Unity_Remap_half(half In, half2 InMinMax, half2 OutMinMax, out half Out)
            {
                Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float Unity_FresnelEffect_half(half3 Normal, half3 ViewDir, half Power)
            {
                half v = 1.0 - saturate(dot(normalize(Normal), normalize(ViewDir)));
                v =  v != 0 ? v : 0.001;
                return pow(v, Power);
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
            //#include "hurt_fun.cginc"
            #include "../wb_shader/HitRed_fun.hlsl"
            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescriptionInputs nodeParams = IN;

                half3 surf_color;
                half surf_alpha;

                {
                    half4 texMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, nodeParams.uv0.xy);
                    half4 mainColor = _MainColor * texMain; //mul

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

                    float4 normalWS_ = NormalTS2WS((normalTs.xyz),
                                                   IN.WorldSpaceTangent,
                                                   IN.WorldSpaceBiTangent,
                                                   IN.WorldSpaceNormal);

                    float4 normalVS = mul(UNITY_MATRIX_V, normalWS_);

                    float4 _MatCapUV1;
                    SG_MatCapUV(_MatCapFix1, normalVS, _MatCapUV1);

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
                    SG_MatCapUV(_MatCapFix2, normalVS, _MatCapUV2);

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

                    {
                        float3 viewDirWS = nodeParams.viewDirectionWS;
                        //clrBlendRslt.rgb = hurt(clrBlendRslt, normalWS_, viewDirWS);
                        clrBlendRslt.rgb = HitRed(clrBlendRslt.rgb, float3(0,0,0), normalWS_, viewDirWS);
                    }

                    // half rimStrFromTexOrVal = _EnableRim ? texRimStr : _RimStr;
                    half rimStrFromTexOrVal = lerp(_RimStr, texRimStr, _EnableRim);

                    half Fresnel = Unity_FresnelEffect_half(normalWS_, nodeParams.viewDirectionWS, 1);
                    half4 rimColor = _RimColor * Fresnel; //mul                

                    half4 clrRim = rimColor
                                    * max(0, normalWS_.y)
                                    * rimStrFromTexOrVal; //mul

                    half4 albedo = clrBlendRslt + clrRim;

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
                const float renormFactor = 1.0 / length(unnormalizedNormalWS);

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

                output.viewDirectionWS = input.viewDirectionWS;
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
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
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


Pass {
            Name "Forward"
            Tags {
                "LightMode"="UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha , One OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Offset 0 , 0
            ColorMask RGBA

            HLSLPROGRAM
            #define ASE_SRP_VERSION 999999

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _FresnelColor;
            half _Fresnel;
            CBUFFER_END


            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
                half4 ase_color : COLOR;
            };

            struct VertexOutput
            {
                float4 clipPos : SV_POSITION;
                float4 ase_color : COLOR;
                float4 ase_texcoord1 : TEXCOORD1;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;

                float3 positionWS = TransformObjectToWorld(v.vertex);
                float3 viewDirectionWS = (_WorldSpaceCameraPos.xyz - positionWS);
                viewDirectionWS = normalize(viewDirectionWS);
                half3 normalWS = TransformObjectToWorldNormal(v.ase_normal);

                half NdotV = dot(viewDirectionWS, normalWS);
                half fresnel = smoothstep(_Fresnel, 1.0, (1.0 - max(NdotV, 0.0)));

                o.ase_texcoord1.x = fresnel;

                o.ase_color = v.ase_color;

                //setting value to unused interpolator channels and avoid initialization warnings
                o.ase_texcoord1.yzw = 0;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
                #else
                float3 defaultVertexValue = float3(0, 0, 0);
                #endif
                float3 vertexValue = defaultVertexValue;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
                #else
                v.vertex.xyz += vertexValue;
                #endif
                v.ase_normal = v.ase_normal;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.clipPos = vertexInput.positionCS;
                return o;
            }

            half4 frag(VertexOutput IN) : SV_Target
            {
                half fresnel = IN.ase_texcoord1.x;

                float3 Color = _FresnelColor.rgb;
                float Alpha = IN.ase_color.a * _FresnelColor.a * fresnel;

                return half4(Color, Alpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}