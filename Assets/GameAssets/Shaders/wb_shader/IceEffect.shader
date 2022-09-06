Shader "WB/IceEffect" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.1, 10)) = 1
        _ReflectColor("ReflectColor", Color) = (1,1,1,0.5)
        _IceTex ("IceTex", 2D) = "white" {}
        _NormalMap("NormalMap", 2D) = "bump" {}
        _RefStrength ("RefStrength", Range(0, 0.3)) = 0.1
        _FrenelPower ("FrenelPower", Range(0, 2)) = 0.1
        _TexAlphaAdd ("TexAlphaAdd", Range(0, 1)) = 0.1
        _Cutoff ("Cutoff", Range(0, 1)) = 1
    }
    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+1"}
        LOD 200
        Pass {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "ColorCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct VertexInput {
            float4 vertex : POSITION;
            float3 ase_normal : NORMAL;
            float4 ase_tangent : TANGENT;
            float2 texcoord : TEXCOORD0;
            };
            struct VertexOutput {
                float4 clipPos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 tSpace0 : TEXCOORD2;
                float4 tSpace1 : TEXCOORD3;
                float4 tSpace2 : TEXCOORD4;
            };


            CBUFFER_START(UnityPerMaterial)
             float _Shininess;
             float4 _Color;
             float4 _ReflectColor;
             float _RefStrength;
             float _FrenelPower;
             float _TexAlphaAdd;
             float _Cutoff;
             float4 _NormalMap_ST;
             float4 _IceTex_ST;
            CBUFFER_END

             sampler2D _IceTex;
             sampler2D _NormalMap;


            VertexOutput vert (VertexInput v) 
            {
                VertexOutput o = (VertexOutput)0;
                o.uv = v.texcoord;

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 positionVS = TransformWorldToView(positionWS);
                float4 positionCS = TransformWorldToHClip(positionWS);
                

                VertexNormalInputs normalInput = GetVertexNormalInputs(v.ase_normal, v.ase_tangent);
                o.tSpace0 = float4(normalInput.normalWS, positionWS.x);
                o.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
                o.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);

                o.clipPos = positionCS;

                return o;
            }


            float4 frag(VertexOutput IN) : COLOR {
                float3 WorldNormal = normalize(IN.tSpace0.xyz);
                float3 WorldTangent = IN.tSpace1.xyz;
                float3 WorldBiTangent = IN.tSpace2.xyz;
                float3 WorldPosition = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
                float3x3 tangentTransform = float3x3(WorldTangent, WorldBiTangent, WorldNormal);
                //----------------------
                float2 uv_NormalMap = IN.uv.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
                float2 uv_IceMap = IN.uv.xy * _IceTex_ST.xy + _IceTex_ST.zw;
                ///
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - WorldPosition);
                float3 Normal = UnpackNormal(tex2D(_NormalMap, uv_NormalMap));
                float3 normalLocal = Normal.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals


                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );

                float4 IceColor = tex2D(_IceTex, uv_IceMap);
                clip((IceColor.a+_Cutoff) - 0.5);
                float3 lightDirection = normalize(_MainLightPosition.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float3 attenColor = _MainLightColor.xyz;
///////// Gloss:
                float gloss = IceColor.a;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow) * _Shininess;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = pow(max( 0.0, NdotL), IceColor.rgb) * attenColor;
                float3 diffuseColor = _Color.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
////// Emissive:
                float3 _Cube_var = float3(1, 1, 1);
                float ddd = lerp(-1, 0, pow(max(0, 1.0 - max(0, dot(normalDirection, viewDirection))), 0.5 * dot(1.0, Normal.rgb) + 0.5));
                float3 ccc = lerp((IceColor.rgb * _Color.rgb), _Cube_var, pow(max(0, _FrenelPower), ddd));
                float3 bbb = ccc  * _RefStrength;
                float3 emissiveTT = (IceColor.rgb + bbb) *_ReflectColor.rgb;
/// Final Color:
                float3 finalColor = diffuse + specular + emissiveTT;
                //return fixed4(lerp(sceneColor.rgb, finalColor,(IceColor.a+_TexAlphaAdd)),1);
                return half4(finalColor, IceColor.a + _TexAlphaAdd);
            }
            ENDHLSL
        }
    }
}
