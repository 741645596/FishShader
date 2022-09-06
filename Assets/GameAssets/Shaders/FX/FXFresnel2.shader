// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FX/Fresnel2" {
    Properties {
        _Fresnel("Fresnel", Range( 0 , 1)) = 0.1943072
        [HDR]_Color0("Color 0", Color) = (0.9811321,0.9811321,0.9811321,1)

    }

    SubShader {
        LOD 0

        Tags {
            "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"
        }

        Cull Back
        HLSLINCLUDE
        #pragma target 3.0
        ENDHLSL


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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "../wb_Shader/ColorCore.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _Color0;
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

            VertexOutput vert(VertexInput In)
            {
                VertexOutput Out = (VertexOutput)0;

                float3 positionWS = TransformObjectToWorld(In.vertex);
                float3 viewDirectionWS = (_WorldSpaceCameraPos.xyz - positionWS);
                viewDirectionWS = normalize(viewDirectionWS);
                half3 normalWS = TransformObjectToWorldNormal(In.ase_normal);

                half NdotV = dot(viewDirectionWS, normalWS);
                half fresnel = smoothstep(_Fresnel, 1.0, (1.0 - max(NdotV, 0.0)));

                Out.ase_texcoord1.x = fresnel;

                Out.ase_color = In.ase_color;

                //setting value to unused interpolator channels and avoid initialization warnings
                Out.ase_texcoord1.yzw = 0;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
                #else
                float3 defaultVertexValue = float3(0, 0, 0);
                #endif
                float3 vertexValue = defaultVertexValue;
                #ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
                #else
                In.vertex.xyz += vertexValue;
                #endif
                In.ase_normal = In.ase_normal;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(In.vertex.xyz);
                Out.clipPos = vertexInput.positionCS;
                return Out;
            }

            half4 frag(VertexOutput In) : SV_Target
            {
                half fresnel = In.ase_texcoord1.x;

                half4 FresnelColor = _Color0;

                float3 Color = FresnelColor.rgb;
                float Alpha = In.ase_color.a * FresnelColor.a * fresnel;

                return half4(Color, Alpha);
            }
            ENDHLSL
        }
    }

}