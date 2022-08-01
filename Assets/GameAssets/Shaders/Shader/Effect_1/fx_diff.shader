Shader "fish/fx/fx_diff"
{ 
	Properties
	{
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_BaseMap("BaseMap", 2D) = "white" {}
  		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
   		[NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}  
		_Transparency("Transparency", Range(0, 1)) = 1
	}

	SubShader
	{
		Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True"}
		LOD 300
		Pass
		{
			Name "ForwardLit"
			Tags{"LightMode" = "UniversalForward"} 
			Blend SrcAlpha OneMinusSrcAlpha 
			ZWrite ON
			Cull OFF  
			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag
 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
 			half4  _Color;
			half4  _SpecColor; 
 			half   _Transparency;
 
			CBUFFER_END
			TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
			TEXTURE2D(_BumpMap);		    SAMPLER(sampler_BumpMap); 
			struct Attributes
			{
				float4 positionOS				: POSITION;
				float3 normalOS					: NORMAL;
				float4 tangentOS				: TANGENT;
				float2 texcoord					: TEXCOORD0;
 
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct Varyings
			{ 
				float2 uv                       : TEXCOORD0; 
				float3 posWS                    : TEXCOORD1;
				float4 normal                   : TEXCOORD2;
				float4 tangent                  : TEXCOORD3;
				float4 bitangent                : TEXCOORD4;
				float3 viewDir                  : TEXCOORD5;
				float4 positionCS               : SV_POSITION;
 
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes input)
			{
				Varyings output = (Varyings)0;

				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

				output.posWS = TransformObjectToWorld(input.positionOS.xyz);
				output.positionCS = TransformWorldToHClip(output.posWS);

				half3 viewDirWS = GetCameraPositionWS() - output.posWS;
				output.normal = half4(normalInput.normalWS, viewDirWS.x);
				output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
				output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z); 
				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap); 
				return output;
			}

			real4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

 				float2 uv = input.uv;
				half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb; 
				Light mainLight = GetMainLight(); 
				half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation); 
				half3 l = mainLight.direction; 
				half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
				viewDirWS = SafeNormalize(viewDirWS);
 
 				half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
				half3 normalWS_var = TransformTangentToWorld(normalTS,
				half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
				normalWS_var = NormalizeNormalPerPixel(normalWS_var);
  		  
				half fresnelTerm = Pow4(saturate(dot(normalWS_var, viewDirWS))); 
				half3 finalColor = lerp(_Color , _SpecColor , fresnelTerm).rgb;
				return real4(finalColor, _Transparency);
			}
		ENDHLSL
		}

	}

}

