Shader "Fish/Ice"
{
	Properties
	{
	  _Color("主色调", Color) = (1,1,1,1)
	  _SpecColor("高光颜色", Color) = (0.5,0.5,0.5,1)
	  _ReflectColor("Reflection Color", Color) = (1,1,1,0.5)
	  _ReflectionStrength("ReflectionStrength", Range(1, 20)) = 1
	  _MainTex("Base (RGB) Emission Tex (A)", 2D) = "white" {}
	  _Opacity("Material opacity", Range(-1, 1)) = 0.5
	  _Cube("Reflection Cubemap", Cube) = "" {}
	  _BumpMap("Normalmap", 2D) = "bump" {}
	  _FPOW("FPOW Fresnel", float) = 5
	  _R0("R0 Fresnel", float) = 0.05
	  _Cutoff("Cutoff", Range(0, 1)) = 0.5
	  _LightStr("Light strength", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
		}

		Pass
		{
			Name "Pass"
			Tags
			{
				"LightMode" = "UniversalForward"
				//	"LightMode" = "ForwardBase"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			ColorMask RGB


			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			//#pragma target 2.0
			#pragma target 3.0
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

			CBUFFER_START(UnityPerMaterial)

			float4 _MainTex_ST;
			float4 _BumpMap_ST;

			half4 _Color;
			half4 _SpecColor;
			half4 _ReflectColor;
			half _ReflectionStrength;
			half _Opacity;

			half _FPOW;
			half _R0;
			half _Cutoff;
			half _LightStr;

			CBUFFER_END

			TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
			TEXTURE2D(_BumpMap);		    SAMPLER(sampler_BumpMap);
			samplerCUBE _Cube;

			struct Attributes
			{
				float3 vertex : POSITION;
				float2 texcoord0 :TEXCOORD0;
				float3 normalOS : NORMAL;
				float4 tangentOS : TANGENT;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 uv :TEXCOORD0;
				float4 positionWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float3 tangentWS : TEXCOORD3;
				float3 bitangentWS : TEXCOORD4;
				float3 vertexSH: TEXCOORD5;
			};

			half4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
			{
				return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
			}


			Varyings vert(Attributes input)
			{
				Varyings output;
				output.uv.xy = TRANSFORM_TEX(input.texcoord0, _MainTex);
				output.uv.zw = TRANSFORM_TEX(input.texcoord0, _BumpMap);


				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
				output.normalWS = normalInput.normalWS;
				output.tangentWS = normalInput.tangentWS;
				output.bitangentWS = normalInput.bitangentWS;
				output.vertexSH = max(half3(0, 0, 0), SampleSH(output.normalWS));

				output.positionWS.xyz = TransformObjectToWorld(input.vertex.xyz)
				//+output.normalWS*0.11
				;
				output.positionCS = TransformWorldToHClip(output.positionWS);
				return output;
			}

			half4 frag(Varyings i) : SV_TARGET
			{
				float4 diffuse = SampleTexture_RGBA(i.uv.xy, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
				float alpha = diffuse.a;
				float lerpVal = step(alpha, _Cutoff);
				alpha = lerp(0, alpha + _Opacity, lerpVal);
				clip(alpha);
				float3x3 tangentTransform = float3x3(i.tangentWS, i.bitangentWS, i.normalWS);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.positionWS.xyz);
				half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv.zw)).xyz;
				//float3 normalDirection = normalize(mul(normalTS, tangentTransform)); // Perturbed normals
				float3 normalDirection = TransformTangentToWorld(normalTS, tangentTransform);
				
				float3 viewReflectDirection = reflect(-viewDirection, normalDirection);

				float3 _Cube_var = texCUBE(_Cube, viewReflectDirection).xyz;
				// 环境光
				float3 ambient = i.vertexSH * _Color.xyz * 1.5;

				Light mainLight = GetMainLight();
				float3 lightDirection = mainLight.direction;
				float3 lightColor = mainLight.color;

				// 菲尼尔反射系数
				float fresnel = 1-dot(viewDirection, normalDirection);
				fresnel = clamp(fresnel, 0, 1);
				
				fresnel = log2(fresnel);
				fresnel = (fresnel * _FPOW);
				fresnel = exp2(fresnel);
				
				fresnel = _R0+ ( 1-_R0) * fresnel;

				float3 u_xlat1_d = diffuse.rgb * _Color.xyz;
				float3 rgb = _Cube_var * diffuse.a - u_xlat1_d;
				rgb = fresnel * rgb + u_xlat1_d;
				float3 u_xlat0_d = rgb * _ReflectColor.xyz * _ReflectionStrength * _LightStr;

				rgb = lightColor * _Color.xyz;

				// light dot normal
				float LDotN = max(dot(lightDirection, normalDirection), 0);
				rgb = rgb * LDotN + ambient;

				return saturate(float4(rgb + u_xlat0_d, alpha));
			}
			

			ENDHLSL
		}
	}
	FallBack Off
}