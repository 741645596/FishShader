Shader "WB/FishShadow"
{
	Properties
	{
		_VeterxOff("_VeterxOff[xyz offset]", Vector) = (1, 1, 1, 0)
		_ShadowColor("ShadowColor", Color) = (0.25, 0.25, 0.25, 0.6)
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-250" }
		Cull Back

		Stencil  // ����ģ�建�壬�����ʾ�ڲ�������·�쳣
		{
			Ref 1
			Comp NotEqual // �����
			Pass replace//���ģ����ԣ�����Ȳ��ԣ�ͨ������δ�������������  �滻ģ�建�����е�����Ϊ1
		}

		Pass
		{
			Name "Shadow"
			Tags { "LightMode" = "SRPDefaultUnlit"  "RenderType" = "Transparent" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA


			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"


			struct VertexInput
			{
				float4 vertex : POSITION;
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _VeterxOff;
			float4 _ShadowColor;

			CBUFFER_END

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				float3 positionWS = TransformObjectToWorld(v.vertex.xyz) + _VeterxOff.xyz;
				o.clipPos = TransformWorldToHClip(positionWS);
				return o;
			}

			half4 frag(VertexOutput IN) : SV_Target
			{
				return _ShadowColor;
			}
			ENDHLSL
		}
	}
}
