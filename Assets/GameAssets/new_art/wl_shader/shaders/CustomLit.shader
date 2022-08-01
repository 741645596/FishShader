Shader "CustomPBR/Lit"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        [NoScaleOffset] _MetallicGlossMap("Metallic", 2D) = "white" {}
        _Metallic("Metallic Scale", Range(0.0, 2.0)) = 1
        _Smoothness("Smoothness Scale", Range(0.0, 2.0)) = 1
        [ToggleOff] _OcclusionSwitch("Occlusion Channel", Float) = 1.0
        _Occlusion("Occlusion Scale", Range(0.0, 2.0)) = 1

        [Normal][NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0.0, 2.0)) = 1.0

        [NoScaleOffset] _EmissionMap("Emission", 2D) = "white" {}
        [HDR] _EmissionColor("Emission Color", Color) = (0,0,0)

        [HDR] _RimColor ("Rim Color", Color) = (0.5,0.5,0.5,1)
		_RimPower ("Rim Power", Range(0, 5)) = 1
		_RimStrength ("Rim Strength", Range(0, 5)) = 1

        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _ZTest("__zt", Float) = 4.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True" 
        }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            Cull [_Cull]
            ZTest [_ZTest]

            HLSLPROGRAM

            #include "CCommon.hlsl"
            #include "CLighting.hlsl"

            #pragma multi_compile _ _USE_OCCLUSION
            #pragma multi_compile _ _NORMALMAP
            #pragma multi_compile _ _EMISSION
            #pragma multi_compile _ _USE_RIM
            #pragma multi_compile _ _ALPHATEST_ON
            #pragma multi_compile _ _ALPHAPREMULTIPLY_ON
            #pragma multi_compile _ _SURFACE_TYPE_TRANSPARENT

            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment

            Varyings LitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = vertexInput.positionWS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS.xyz;
                half sign = input.tangentOS.w * GetOddNegativeScale();
                output.bitangentWS = sign * cross(normalInput.normalWS.xyz, normalInput.tangentWS.xyz);
                
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.positionCS = vertexInput.positionCS;
                return output;
            }

            half4 LitPassFragment(Varyings input) : SV_Target
            {
                SurfaceData surfaceData;
                InitializeSurfaceData(input, surfaceData);
                
                BRDFData brdfData;
                InitializeBRDFData(surfaceData, brdfData);
                half3 color = PBRLighting(input.viewDirWS, input.positionWS, surfaceData, brdfData);

#if defined(_USE_RIM)
                // 边缘光
                half NoV = saturate(dot(input.viewDirWS, surfaceData.normalWS));
                half3 rim = pow(1 - NoV, _RimPower) * _RimColor.xyz * _RimStrength;
                color.rgb += rim;
#endif
                return half4(color, OutputAlpha(surfaceData.alpha, _Surface));
            }
            ENDHLSL
        }
    }

    CustomEditor "CLitShader"
}
