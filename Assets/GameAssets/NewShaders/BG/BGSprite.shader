Shader "BG/Sprite"
{
	Properties
	{
		_MainTex("主贴图(RGB)", 2D) = "White" {}
		_Color("主色调", Color) = (1,1,1,1)
		_Alpha("透明度", float) = 1
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
			Name "Cannon Base"
			Tags
			{
				"LightMode" = "UniversalForward"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			CBUFFER_START(UnityPerMaterial)

			half4 _Color;
			half _Alpha;
			float4 _MainTex_ST;

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
				float2 uv0 :TEXCOORD0;

			};

			half4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
			{
				return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
			}


			Varyings vert(Attributes input)
			{
				Varyings output;
				output.uv0 = TRANSFORM_TEX(input.texcoord0, _MainTex).xy;
				output.positionCS = TransformObjectToHClip(input.vertex);
				return output;
			}


			half4 frag(Varyings in_f) : SV_TARGET
			{
				float4 _MainTex_var = SampleTexture_RGBA(in_f.uv0, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
				return _Color * _MainTex_var;
			}

			ENDHLSL
		}
	}
	FallBack Off
}