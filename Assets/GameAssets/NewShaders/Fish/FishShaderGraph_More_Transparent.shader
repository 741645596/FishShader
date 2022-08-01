Shader "Fish/FishShaderGraph_More_Transparent" {
    Properties {
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 1
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        
        [ToggleUI]_IsHurt("挨打状态", Float) = 0
        _HurtColor("挨打颜色", Color) = (0, 0, 0, 0)
        _MainColor("主颜色", Color) = (1, 1, 1, 1)
        [NoScaleOffset]_MainTex("主贴图", 2D) = "white" {}
        _AlbedoIntensity("亮度", Range(0, 5)) = 1

        [NoScaleOffset]_BlendTex("混合图", 2D) = "white" {}
        [ToggleUI]_EnableMatCap1("启动MatCap法线/不启用法线贴图作为颜色输出", Float) = 0
        [NoScaleOffset]_BumpMap("法线贴图", 2D) = "white" {}
        _NormalStr("法线强度", Range(0, 5)) = 1

        [NoScaleOffset]_TexMatCap1("MatCap贴图", 2D) = "white" {}
        [ToggleUI]_EnableRim("启动外轮廓光", Float) = 0
        [NoScaleOffset]_TexMatCapRim("外轮廓光贴图", 2D) = "white" {}
        _RimStr("外轮廓光强度", Range(0, 2)) = 0
        _RimColor("外轮廓光颜色", Color) = (0, 0, 0, 0)
        [NoScaleOffset]_TexTransparency("透明通道图", 2D) = "white" {}
        _TransparencyStr("透明通道强度", Range(1, 2)) = 1
        [ToggleUI]_EnableTransparency("启动透明通道", Float) = 0
        _FishAlpha("透明度", Range(0, 1)) = 1

        [ToggleUI]_BlendSwitch("启动混合", Float) = 0
        _BlendStrength("混合程度", Range(0, 5)) = 0
    }
    SubShader {
        Tags {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
        }
        Pass {
            Name "Pass"
            Tags {
                // LightMode: <None>
            }

            // Render State
            
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            ZTest LEqual
            
            Cull[_Cull]
            ZWrite[_ZWriteMode]
//            Cull Back
//            ZWrite On

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
                // float3 viewDirectionWS:TEXCOORD3;
            };

            struct SurfaceDescriptionInputs
            {
                float3 WorldSpaceNormal;
                float3 WorldSpaceTangent;
                float3 WorldSpaceBiTangent;
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
            float _AlbedoIntensity;
            float4 _BlendTex_TexelSize;
            float4 _BumpMap_TexelSize;
            float _NormalStr;
            float _EnableMatCap1;
            float4 _TexMatCap1_TexelSize;
            float _BlendStrength;
            float _BlendSwitch;
            float4 _TexMatCapRim_TexelSize;
            float _EnableRim;
            float _RimStr;
            float4 _RimColor;

            float _TransparencyStr;
            float _EnableTransparency;
            float4 _TexTransparency_TexelSize;
            half _FishAlpha;


            CBUFFER_END

            // Object and Global properties
            SAMPLER(SamplerState_Linear_Repeat);
            TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);           
            TEXTURE2D(_BlendTex);           SAMPLER(sampler_BlendTex);          
            TEXTURE2D(_BumpMap);            SAMPLER(sampler_BumpMap);           
            TEXTURE2D(_TexMatCap1);         SAMPLER(sampler_TexMatCap1);        
            TEXTURE2D(_TexMatCapRim);       SAMPLER(sampler_TexMatCapRim);      
            TEXTURE2D(_TexTransparency);    SAMPLER(sampler_TexTransparency);

            // Graph Functions

            void Unity_Blend_LinearDodge_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
            {
                Out = Base + Blend;
                Out = lerp(Base, Out, Opacity);
            }


            void Unity_NormalStrength_float(float3 In, float Strength, out float3 Out)
            {
                Out = float3(In.rg * Strength, lerp(1, In.b, saturate(Strength)));
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


            void color_float(float4 MainColor,
                             float4 Color,
                             float3 Lambert,
                             float Shininess,
                             float3 MatCap,
                             float3 Fresnel,
                             out float3 NewColor)
            {
                NewColor = MainColor.xyz
                    * Color.xyz
                    * Shininess
                    * MatCap
                    * Lambert
                    + Fresnel;
            }

            // Graph Pixel
            struct SurfaceDescription
            {
                float3 BaseColor;
                float Alpha;
            };

            SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
            {
                SurfaceDescription surface = (SurfaceDescription)0;
                float4 texMain = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv0.xy);
                float4 texBlend = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, IN.uv0.xy);

                float4 blendClr;
                Unity_Blend_LinearDodge_float4(texMain, texBlend, blendClr, _BlendStrength);
                float4 texMainBld = _BlendSwitch ? blendClr : texMain;

                half4 hurtOrMainClr = _IsHurt ? _HurtColor : _MainColor;
                
                float4 texBump = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv0.xy);
                texBump.rgb = UnpackNormal(texBump);
                float3 normalTS;
                Unity_NormalStrength_float((texBump.xyz), _NormalStr, normalTS);

                float4 normal_ts2_ws = NormalTS2WS(normalTS, IN.WorldSpaceTangent, IN.WorldSpaceBiTangent,
                                                   IN.WorldSpaceNormal);

                float4 normalVS = mul(UNITY_MATRIX_V, normal_ts2_ws);

                float4 MatCap1UV = normalVS * 0.45 + 0.5;

                float4 texMatCap1 = SAMPLE_TEXTURE2D(_TexMatCap1, sampler_TexMatCap1, (MatCap1UV.xy));
                float4 clrMatCap1 = texMatCap1 * 2;

                float4 texBump1 = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv0.xy);
                float3 normalAsColor;
                Unity_NormalStrength_float((texBump1.xyz), _NormalStr, normalAsColor);
                float3 clrMat = _EnableMatCap1 ? (clrMatCap1.xyz) : normalAsColor;
                float4 texMatCapRim = SAMPLE_TEXTURE2D(_TexMatCapRim, sampler_TexMatCapRim, (MatCap1UV.xy));

                float4 black = float4(0, 0, 0, 0);
                float4 rimClr = _EnableRim ? _RimColor * texMatCapRim * _RimStr : black;
                float3 _FishColor;
                color_float(texMainBld, hurtOrMainClr, 1, _AlbedoIntensity,
                            (float4(clrMat, 1.0)),
                            (rimClr.xyz),
                            _FishColor);

                float4 texTransparency = SAMPLE_TEXTURE2D(_TexTransparency, sampler_TexTransparency,
                                                          IN.uv0.xy);

                float alpha = _EnableTransparency
                                  ? _TransparencyStr * texTransparency.r
                                  : hurtOrMainClr.a;

                surface.BaseColor = _FishColor;
                surface.Alpha = alpha;
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

                output.uv0 = input.texCoord0;

                return output;
            }

            // --------------------------------------------------
            // Main

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                float3 positionWS = TransformObjectToWorld(input.positionOS);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);

                #ifdef VARYINGS_NEED_SCREENPOSITION
    output.screenPosition = ComputeScreenPos(output.positionCS, _ProjectionParams.x);
                #endif

                output.positionCS = TransformWorldToHClip(positionWS);
                output.normalWS.xyz = normalWS;
                output.tangentWS.xyzw = tangentWS;
                output.texCoord0.xyzw = input.uv0;

                // float3 viewDirectionWS = GetWorldSpaceNormalizeViewDir(positionWS);
                // output.viewDirectionWS.xyz = viewDirectionWS;

                return output;
            }

            half4 frag(Varyings input) : SV_TARGET
            {
                Varyings unpacked = input;

                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(unpacked);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);

                half alpha = surfaceDescription.Alpha;
                // clip(alpha - _AlphaClipThreshold);

                #ifdef _ALPHAPREMULTIPLY_ON
    surfaceDescription.BaseColor *= surfaceDescription.Alpha;
                #endif

                return half4(surfaceDescription.BaseColor, alpha * _FishAlpha
                );
            }
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}