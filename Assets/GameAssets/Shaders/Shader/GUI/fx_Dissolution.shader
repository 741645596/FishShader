Shader "fish/fx/WaveMask"
{

	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("MainTex (RGB)", 2D) = "white" {} 
		_NoiseTex("NoiseTex (R)",2D) = "white"{}
		_EdgeWidth("EdgeWidth",Range(0,0.5)) = 0.1
		[HDR]_EdgeColor("EdgeColor",Color) = (1,1,1,1)
		_EdgeThresholdValue("EdgeThresholdValue",Range(0,1)) = 0.5
		_DissolveSide("DissolveSide",Range(0,1)) = 0 
		_DissolvePercentage("DissolvePercentage",Range(0,1)) = 0

	}

		SubShader
		{
			Pass
			{

				Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
				Blend SrcAlpha  OneMinusSrcAlpha
				ZWrite Off
				HLSLPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
				#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

				CBUFFER_START(UnityPerMaterial)

				half4  _MainTex_ST;
				half4  _Diffuse;

				float _EdgeWidth;
				float4 _EdgeColor;
				float _EdgeThresholdValue;
				float _DissolvePercentage;
	 
				float4 _Color;
				float  _DissolveSide;
				float  _Power;
				float  _Maxset;
				float  _TimeScale;
				float  _Transparent;
				CBUFFER_END
				TEXTURE2D(_MainTex);            SAMPLER(sampler_MainTex);
				TEXTURE2D(_NoiseTex);		    SAMPLER(sampler_NoiseTex);

				struct Attributes
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;

				};

				struct Varyings
				{
					float4 pos	: SV_POSITION;
					float2 uv	: TEXCOORD1;
					float3 posWS: TEXCOORD2;

				};

				//定义顶点shader
				Varyings vert(Attributes input)
				{
					Varyings output = (Varyings)0;

					UNITY_SETUP_INSTANCE_ID(input);
					UNITY_TRANSFER_INSTANCE_ID(input, output);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

					VertexPositionInputs vertexInput = GetVertexPositionInputs(input.vertex.xyz);
 
					output.pos = vertexInput.positionCS;
					output.posWS = vertexInput.positionWS;

					output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);

					return output;
				}

				//定义片元shader
				half4 frag(Varyings input) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID(input);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

					float2 uv = input.uv;
		 
					half4 Albedo     = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
					half4 Emission	 = 0;
					half  Alpha		 = Albedo.a;

					float DissolveFactor = saturate(_DissolvePercentage);
					float noiseValue = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex,input.posWS.rb* _DissolveSide).r;
					if (noiseValue <= DissolveFactor)
					{
						discard;
					}
					float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
					float EdgeFactor = saturate((noiseValue - DissolveFactor) / (_EdgeWidth * DissolveFactor));
					float4 BlendColor = texColor * _EdgeColor;

					if (_EdgeThresholdValue > 0) 
					{
						float HardEdgeFactor = EdgeFactor;
						if (HardEdgeFactor > _EdgeThresholdValue) 
						{
							HardEdgeFactor = 1;
							Emission = 0;
							Albedo = lerp(texColor, BlendColor, 1 - EdgeFactor);
						}
						else 
						{
							HardEdgeFactor = 0;
							Emission = _EdgeColor;

						}
						
					}

					else
					{
						if (_EdgeThresholdValue >= 1)
						{
							Albedo = BlendColor;
							Alpha = 0;
						}
						else 
						{
							Albedo = lerp(texColor, BlendColor, 1 - EdgeFactor);
						}
					}
					return half4(Albedo.xyz, Alpha);
				}

				ENDHLSL

			}
		}
		FallBack "Diffuse"
}
