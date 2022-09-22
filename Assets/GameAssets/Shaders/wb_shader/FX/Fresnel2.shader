Shader "WB/Fresnel2" {
    Properties {
        _Fresnel("Fresnel", Range( 0 , 1)) = 0.1943072
        [HDR]_Color0("Color 0", Color) = (0.9811321,0.9811321,0.9811321,1)
    }

    SubShader {
        Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}

        Cull Back
        HLSLINCLUDE
        #pragma target 3.0
        ENDHLSL


        Pass {
            Name "Forward"
            Tags {"LightMode"="UniversalForward"}

            Blend SrcAlpha OneMinusSrcAlpha , One OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Offset 0 , 0
            ColorMask RGBA

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _Color0;
            half _Fresnel;
            CBUFFER_END


            struct VertexInput
            {
                half4 vertex : POSITION;
                half3 ase_normal : NORMAL;
                half4 ase_color : COLOR;
            };

            struct VertexOutput
            {
                half4 clipPos : SV_POSITION;
                half4 ase_color : COLOR;
                half fresnel : TEXCOORD1;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o = (VertexOutput)0;

                half3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                half3 viewDirectionWS = (_WorldSpaceCameraPos.xyz - positionWS);
                viewDirectionWS = normalize(viewDirectionWS);
                half3 normalWS = TransformObjectToWorldNormal(v.ase_normal);

                half NdotV = dot(viewDirectionWS, normalWS);
                o.fresnel = smoothstep(_Fresnel, 1.0, (1.0 - max(NdotV, 0.0)));
                o.ase_color = v.ase_color;


                #ifdef ASE_ABSOLUTE_VERTEX_POS
                half3 defaultVertexValue = v.vertex.xyz;
                #else
                half3 defaultVertexValue = half3(0, 0, 0);
                #endif
                half3 vertexValue = defaultVertexValue;
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
                half4 FresnelColor = _Color0;

                half3 Color = FresnelColor.rgb;
                half Alpha = IN.ase_color.a * FresnelColor.a * IN.fresnel;

                return half4(Color, Alpha);
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}