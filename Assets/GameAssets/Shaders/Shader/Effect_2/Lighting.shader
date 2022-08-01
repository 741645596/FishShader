
Shader "fish/fx/Lighting"
{
	Properties
	{

			    	[HDR]_Color("Main Color", Color) = (1,1,1,1)

					_MainTex("Base (RGB) Emission Tex (A)", 2D) = "white" { }

					_ClolrMultiply("ClolrMultiply", Range(1, 2)) = 1

					_PlaySpeed("�����ٶ�", Range(0.1, 1)) = 0.1

	}

		SubShader
	{

		Tags { "RenderPipeline" = "UniversalPipeline"  "RenderType" = "Transparent"  "Queue" = "Transparent" }

		LOD 250
		Blend One One
		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

		Pass
		{
			Name "Forward"
			Tags { "LightMode" = "UniversalForward" }

			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#pragma multi_compile_instancing

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			sampler2D	_MainTex;
			CBUFFER_START(UnityPerMaterial)

			half4 _MainTex_ST;
			half4 _Color;
			half _ClolrMultiply;
			half _PlaySpeed;
			CBUFFER_END

			struct VertexInput
			{
				float4 vertex : POSITION;

				float4 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;

				float2 uv : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.uv.xy = v.texcoord.xy;

				o.uv.xy= o.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;

					int idx = _Time.w / (1 - _PlaySpeed);

					idx = idx % 8;
					o.uv.y += idx * 0.125;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.clipPos = vertexInput.positionCS;

				return o;
			}

			half4 frag(VertexOutput IN) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				float4 Color = tex2D(_MainTex, IN.uv.xy) ;

				Color = Color * _ClolrMultiply * _Color;

				return half4(Color.rgb*Color.a, 0.1);

			}

			ENDHLSL
		}

	}

}
