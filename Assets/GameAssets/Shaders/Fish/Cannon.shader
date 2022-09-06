Shader "Fish/Cannon"
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

			Blend One Zero
			Cull Back

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 
			#include "ColorUtil.cginc"
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
				float2 uv0 = in_f.uv0.xy;
				float4 _MainTex_var = SampleTexture_RGBA(uv0, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
				//_MainTex_var.xyz = GammaToLinearSpace3(_MainTex_var.xyz);
				float alpha = _MainTex_var.a;
				clip(alpha - 0.1);
				float3 color = _MainTex_var.rgb;
				//return LocalGammaToLinear(float4(color, _MainTex_var.a * _Alpha));

				return float4(color, _MainTex_var.a * _Alpha);
			}

			ENDHLSL
		}
	}
	FallBack Off
}