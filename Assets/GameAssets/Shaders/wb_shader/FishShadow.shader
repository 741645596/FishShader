Shader "WB/FishShadow"
{
	Properties
	{
		_VeterxOff("_VeterxOff[xyz offset]", Vector) = (1, 1, 1, 0)
		_ShadowColor("ShadowColor", Color) = (0.25, 0.25, 0.25, 0.6)
		[Foldout] _FadeName("淡入淡出控制面板[程序控制]",Range(0,1)) = 0
		[FoldoutItem] _Alpha("_Alpha", float) = 1
	}

	SubShader
	{
		Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent-250" }
		LOD 300
		Cull Back

		Stencil  // 加入模板缓冲，解决显示内部纹理纹路异常
		{
			Ref 1
			Comp NotEqual // 不相等
			Pass replace//如果模板测试（和深度测试）通过，如何处理缓冲区的内容  替换模板缓冲区中的数据为1
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
			float _Alpha;
			CBUFFER_END
            
			
			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float4 orginClips = TransformWorldToHClip(positionWS);
				positionWS += _VeterxOff.xyz;
				o.clipPos = TransformWorldToHClip(positionWS);
				if (o.clipPos.z > orginClips.z)
				{
					positionWS.z = positionWS.z -_VeterxOff.z * 2;
					o.clipPos = TransformWorldToHClip(positionWS);
				}
				return o;
			}

			half4 frag(VertexOutput IN) : SV_Target
			{
				_ShadowColor.a = _ShadowColor.a * _Alpha;
				clip(_ShadowColor.a - 0.1f);
				return _ShadowColor;
			}
			ENDHLSL
		}
	}
	CustomEditor "FoldoutShaderGUI"
}
