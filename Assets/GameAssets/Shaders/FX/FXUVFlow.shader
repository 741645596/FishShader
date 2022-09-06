Shader "FX/UVFlow"
{
	// 主贴图+uv
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("BlendDestination", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
		_GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1

		_FlowMap("Flow Map", 2D) = "white" {}
		[HDR] _FlowColor("Flow Color", Color) = (1,1,1,1)
		_FlowSpeed("Flow Speed", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags 
		{
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
            "RenderPipeline" = "UniversalPipeline"
        }

		Pass
		{
			Name "UVFlow"
			Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWriteMode]
            Lighting Off
            ZTest [_ZTest]

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag



			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "../wb_Shader/ColorCore.hlsl"
			CBUFFER_START(UnityPerMaterial)

			float4 _BaseMap_ST;
            half4 _BaseColor;
			half _GlowScale;
            half _AlphaScale;

            half4 _FlowMap_ST;
			half4 _FlowColor;
			float2 _FlowSpeed;

			CBUFFER_END

			TEXTURE2D(_BaseMap);          SAMPLER(sampler_BaseMap);
			TEXTURE2D(_FlowMap);          SAMPLER(sampler_FlowMap);

			struct Attributes
			{
				float3 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord :TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				half4 color : COLOR;
				float2 texcoord :TEXCOORD0;
				float2 texcoordMask: TEXCOORD1;
			};


			Varyings vert(Attributes input)
			{
				Varyings output;
				output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
				output.texcoordMask = TRANSFORM_TEX(input.texcoord, _FlowMap);
				output.positionCS = TransformObjectToHClip(input.vertex);
				output.color = input.color;
				return output;
			}


			half4 frag(Varyings in_f) : SV_TARGET
			{
				half4 vertColor = in_f.color;
				vertColor = Gamma22(vertColor);
				float2 uv = in_f.texcoord;
				float4 baseCol = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
				baseCol = Gamma22(baseCol);
				_BaseColor = Gamma22(_BaseColor);
                half4 col = vertColor * baseCol * _BaseColor;
				col.rgb *= _GlowScale;

				float2 uvMask = in_f.texcoordMask + _Time.y * _FlowSpeed;
				half4 uvCol = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, uvMask);
				uvCol = Gamma22(uvCol);
				float node_5670 = (dot(uvCol.rgb, float3(0.3, 0.59, 0.11))*uvCol.a);
				_FlowColor = Gamma22(_FlowColor);
				float3 emissive = (_FlowColor.rgb * saturate((node_5670 - 0.25)) * vertColor.rgb);

				col.rgb += emissive;

				col.a = saturate(col.a * vertColor.a * _BaseColor.a * _AlphaScale);
				col = Gamma045(col);
				return col;
			}

			ENDHLSL
		}
	}
}