Shader "Fish/Mask"
{
	Properties
	{
		_MainTex("主贴图(RGB)", 2D) = "White" {}
		_Alpha("透明度", Range(0, 1)) = 0.5
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "False"
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
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
			Cull Back

			ZTest LEqual
			ZWrite Off

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_instancing


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			CBUFFER_START(UnityPerMaterial)

			
			float4 _MainTex_ST;
			float _Alpha;
			CBUFFER_END
			TEXTURE2D(_MainTex);          SAMPLER(sampler_MainTex);
			

			struct Attributes
			{
				float3 vertex : POSITION;
				float2 texcoord0 :TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv :TEXCOORD0;
				
			};

			float4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
			{
				return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
			}

			Varyings vert(Attributes input)
			{
				Varyings output;
				output.uv = TRANSFORM_TEX(input.texcoord0, _MainTex).xy;
				output.positionCS = TransformObjectToHClip(input.vertex);
				return output;
			}

			float4 frag(Varyings in_f) : SV_TARGET
			{
				float4 col = SampleTexture_RGBA(in_f.uv, TEXTURE2D_ARGS(_MainTex, sampler_MainTex)) * 1.1;
				float a = lerp(-0.30, 1.00, col.a) * (1.0 - _Alpha);
				return float4(0, 0, 0, a);
			}
			ENDHLSL
		}
	}
	FallBack Off
}