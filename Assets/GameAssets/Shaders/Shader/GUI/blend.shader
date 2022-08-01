

Shader "fish/GUI/blend"
{
	Properties
	{
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}//   没用，避免报错 
		_wenli("wenli", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//MASK SUPPORT ADD
  _StencilComp("Stencil Comparison", Float) = 8
  _Stencil("Stencil ID", Float) = 0
  _StencilOp("Stencil Operation", Float) = 0
  _StencilWriteMask("Stencil Write Mask", Float) = 255
  _StencilReadMask("Stencil Read Mask", Float) = 255
  _ColorMask("Color Mask", Float) = 15
  //MASK SUPPORT END

	}

	SubShader
	{
		LOD 0

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

	  //MASK SUPPORT ADD
  Stencil
  {
	  Ref[_Stencil]
	  Comp[_StencilComp]
	 //or Pass[_StencilOp]
	  ReadMask[_StencilReadMask]
	  WriteMask[_StencilWriteMask]
  }
  ColorMask[_ColorMask]
	  //MASK SUPPORT END

		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

			Stencil {
	  //orRef[_StencilRef]
		//or	Comp[_StencilComp]
			Pass Keep
		         }

		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend SrcAlpha OneMinusSrcAlpha , One OneMinusSrcAlpha
			ZWrite Off
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			sampler2D _wenli;
			CBUFFER_START( UnityPerMaterial )
			half4 _wenli_ST;
			half4 _Color;
			CBUFFER_END

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				half4 ase_color : COLOR;

			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;

				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;

				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;

				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_color = v.ase_color;

				o.ase_texcoord1.zw = 0;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.clipPos = vertexInput.positionCS;

				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
			
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float2 uv_wenli = IN.ase_texcoord1.xy * _wenli_ST.xy + _wenli_ST.zw;
				half4 tex2DNode68 = tex2D( _wenli, uv_wenli );

				float3 Color = ( tex2DNode68 * IN.ase_color * _Color ).rgb;
				float Alpha = ( tex2DNode68.a * IN.ase_color.a * _Color.a );

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

	}

}
