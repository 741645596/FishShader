Shader "Fish/ShadowTex"
{
	// 鱼的阴影
	Properties
	{
		_ShadowProjDir("_ShadowProjDir", Vector) = (0,0,0,0)
		_FishAlpha("_FishAlpha", float) = 1
		_Scale("_Scale", Range(0, 2)) = 1
		[NoScaleOffset]_MainTex("主贴图", 2D) = "white" {}
		_AlphaClipThreshold("透明度裁剪", Range(0, 1)) = 0.5
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
			Stencil
			{
				Ref 0
				Comp equal
				Pass incrWrap
				Fail keep
				ZFail keep
			}
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
			
            CBUFFER_START(UnityPerMaterial)

			float4 _ShadowProjDir;
			float _Scale;
			float _FishAlpha;
			
			float4 _MainTex_TexelSize;
			half _AlphaClipThreshold;
			CBUFFER_END
			
			TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);

			struct Attributes
			{
				float3 vertex : POSITION;
				float4 texCoord0 : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 uv0 :TEXCOORD0;
				float4 shadowColor :TEXCOORD1;
				
			};


			half4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
			{
				return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
			}

			Varyings vert(Attributes input)
			{
				Varyings output;
				float3 u_xlat1 = normalize(_ShadowProjDir.xyz);
				float3 vertex = input.vertex * _Scale;
				float3 positionWS = TransformObjectToWorld(vertex);
				positionWS += u_xlat1 * _ShadowProjDir.w;
				output.positionCS = TransformWorldToHClip(positionWS);
				output.shadowColor = float4(0, 0, 0, 0.45 * _FishAlpha);
				output.uv0 = input.texCoord0;
				return output;
			}
			half4 frag(Varyings i) : SV_TARGET
			{
				float4 _MainTex_var = SampleTexture_RGBA(i.uv0, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
				half alpha = _MainTex_var.a;
                clip(alpha - _AlphaClipThreshold);
				
				return i.shadowColor;
			}

			ENDHLSL
		}
	}
	FallBack Off
}