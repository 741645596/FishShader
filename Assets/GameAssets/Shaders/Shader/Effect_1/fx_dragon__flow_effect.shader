
Shader "fish/fx/dragon__flow_effect"
{
	Properties
	{

	[HDR]_TintColor("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex("Particle Texture", 2D) = "white" {}
	//[WrapMode] _MainTex_Wrap("Particle Tex Wrap Mode", float) = 0
	_ColorMaskTex("Color Mask Texture", 2D) = "white" {}
	//[WrapMode] _ColorMaskTex("Color Mask Tex Wrap Mode", float) = 0
	_Level("Brightness", float) = 1

	}

		SubShader
					{

						Tags { "RenderPipeline" = "UniversalPipeline"
						
								 "IGNOREPROJECTOR" = "true"
	                           "QUEUE" = "Transparent"
	                            "RenderType" = "Transparent"
	
	                          }

						//LOD 250
						//Blend One One
						//Cull Back
						HLSLINCLUDE
						#pragma target 3.0
						ENDHLSL

						Pass
						{
							Name "Forward"
							Tags { "LightMode" = "UniversalForward" 
			                     "IGNOREPROJECTOR" = "true"
		                         "QUEUE" = "Transparent"
		                         "RenderType" = "Transparent"
	                         }

	                       ZWrite Off
	                      Cull Off
	                    Blend SrcAlpha OneMinusSrcAlpha

							//ZWrite Off
							//ZTest LEqual
							//Offset 0 , 0
							//ColorMask RGBA

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
	                        sampler2D            _ColorMaskTex;
							CBUFFER_START(UnityPerMaterial)

							half4 _MainTex_ST;

							half4 _ColorMaskTex_ST;

							half4 _TintColor;
							//half _ClolrMultiply;

						//	half _MainTex_Wrap;

							half _Level;
							//half4 _TintColor;

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

								float4 uv : TEXCOORD1;
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

								o.uv.xy = o.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw; 

								o.uv.zw = v.texcoord.xy * _ColorMaskTex_ST.xy + _ColorMaskTex_ST.zw; 

								VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
								o.clipPos = vertexInput.positionCS;

								return o;
							}

							half4 frag(VertexOutput IN) : SV_Target
							{
								UNITY_SETUP_INSTANCE_ID(IN);
								UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

								float4 Color = tex2D(_MainTex, IN.uv.xy);
								float4 mask = tex2D(_ColorMaskTex, IN.uv.zw);
					
								return  Color * _Level* _TintColor *mask;

							}

							ENDHLSL
						}

					}

}
