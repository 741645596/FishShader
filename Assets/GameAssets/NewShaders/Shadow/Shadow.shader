Shader "Fish/Shadow"
{
	// 鱼的阴影
	Properties
	{
		_ShadowProjDir("_ShadowProjDir", Vector) = (0,0,0,0)
		_FishAlpha("_FishAlpha", float) = 1
		_Scale("_Scale", Range(0, 2)) = 1
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
			Cull Back
			ZWrite Off
			ColorMask RGB
			Stencil
			{
				Ref 0
				Comp Equal
				WriteMask 255
				ReadMask 255
				//Pass IncrSat
				Pass Invert
				Fail Keep
				ZFail Keep
			}


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

			CBUFFER_END

			struct Attributes
			{
				float3 vertex : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 shadowColor :TEXCOORD0;
			};


			Varyings vert(Attributes input)
			{
				Varyings output;
				float3 u_xlat1 = normalize(_ShadowProjDir.xyz);
				float3 vertex = input.vertex * _Scale;
				float3 positionWS = TransformObjectToWorld(vertex);
				positionWS += u_xlat1 * _ShadowProjDir.w;
				output.positionCS = TransformWorldToHClip(positionWS);
				output.shadowColor = float4(0, 0, 0, 0.45 * _FishAlpha);
				return output;
			}

			half4 frag(Varyings i) : SV_TARGET
			{
				return i.shadowColor;
			}

			ENDHLSL
		}
	}
	FallBack Off
}