Shader "fish/Effect/Effect_add"
{
	Properties
	{
		[HDR] _Color("Color", Color) = (1,1,1,1)
 		_BaseMap("BaseMap", 2D) = "white" {}
		_Transparency("Transparency", Range(0, 1)) = 1
	}

	SubShader
	{
 		Tags{"RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" "Queue" = "Transparent"}
		LOD 300
		Pass
		{
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"}

		//	Blend SrcAlpha OneMinusSrcAlpha
			Blend One One, One OneMinusSrcAlpha
 		//  Blend[_SrcBlend][_DstBlend]
			Cull Back
		    ZWrite off
		// 	Offset -1,-1
 		  	ZTest LEqual
		 	ColorMask RGBA
			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag
 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST,_Color;
			float  _Transparency;
 			CBUFFER_END
			TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);

			struct Attributes
			{
				float4 positionOS				: POSITION; 
				float2 texcoord					: TEXCOORD0;
				half4  colorOS					: COLOR;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct Varyings
			{ 
				float2 uv                       : TEXCOORD0; 
				float3 posWS                    : TEXCOORD1; 
				float3 viewDir                  : TEXCOORD2;
				float4 positionCS               : SV_POSITION;
				float4 colorWS					: COLOR;

 				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 3);

				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);				
				
		//		VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

				output.posWS = TransformObjectToWorld(input.positionOS.xyz); 
				output.positionCS = TransformWorldToHClip(output.posWS);
   
				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
				output.colorWS = input.colorOS;

 				return output;
			}

			real4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);		
 
				float2 uv = input.uv;
  
				real4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);

				real3 albedo_var = (input.colorWS * _Color * albedo * input.colorWS.a * _Color.a * albedo.a).rgb;

				real alpha = albedo.a* _Color.a* input.colorWS.a;

				//	clip(alpha - 0.5);
    			return real4(albedo_var, alpha);
 			}
		ENDHLSL
		}

	}

}
