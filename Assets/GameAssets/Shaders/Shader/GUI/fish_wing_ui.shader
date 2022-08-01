Shader "fish/GUI/wing"
{
	Properties
	{
 		_BaseMap("BaseMap", 2D) = "white" {}
		_BaseInt("BaseInt", Range(1,2)) = 1
		_LightInt("LightInt", Range(0, 5)) =0

 		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 0.0
		_SpecGlossMap("SpecMap", 2D) = "white" {}
 		_SpecColor("Specular Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_SpecInt("SpecInt", Range(0.0, 1.0)) = 0
		_Smoothness("Smoothness", Range(0.0, 1.0)) = 0.2		
		_DarkColor("Dark Color", Color) = (0.5, 0.5, 0.5, 0.5)
		_DarkInt("DarkInt", Range(0,16)) = 0
		_DarkSide("DarkSide", Range(0,16)) = 4

		[ToggleOff] _NormalMap("NormalMap No", Float) = 0.0
		[NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
 		[ToggleOff] _CubeMap("CubeMap No", Float) = 0.0
		_SpecCube("SpecCube", CUBE) = "bump" {}
		_SpecCubeInt("SpecCubeInt", Range(0.0, 1.0)) = 0.2
		_Emission("Emission", Range(0, 2)) = 0.715

		_HitColorValue("HitColorShow", Range(0, 1)) = 0
		_HitColor("HitColor", Color) = (1,0.20,0,1)
		_HitColorWeight("HitColorWeight", Range(0, 1)) = 0
		_HitColorInt("HitColorInt", Range(1, 5)) = 1
 
		[ToggleOff] _FlowMap("FlowMap No", Float) = 0.0
		_FlowMask("FlowMask", 2D) = "white" {}
		_FlowColor("FlowColor", Color) = (0.5, 0.5, 0.5, 0.5)
 
		_FlowIn("流光强度", Range(0, 1)) = 0
		_FlowLe("流光菲尼耳强度", Range(0, 1)) = 0
		_FlowSpeed("FlowSpeed", Range(0.0, 1.0)) = 0.1
		_LightVec("LightVec", vector) = (0,1,0,0)

		[HideInInspector] _Cutoff("Cutoff", Range(0, 1)) = 0.5
		[HideInInspector] _EffectShinness("EffectShinness", Range(0, 1)) = 0.5
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

			Blend SrcAlpha OneMinusSrcAlpha,Zero OneMinusSrcAlpha
 
 		//  Blend[_SrcBlend][_DstBlend]
			Cull off
		    ZWrite ON
		// 	Offset -1,-1
 		  	ZTest LEqual
		 	ColorMask RGBA
			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma multi_compile_instancing
			#pragma vertex vert
			#pragma fragment frag
            #pragma shader_feature _SPECULARHIGHLIGHTS_OFF
			#pragma shader_feature _NORMALMAP_OFF
			#pragma shader_feature _CUBEMAP_OFF
		 	#pragma shader_feature _FLOWMAP_OFF
			//#pragma shader_feature _TRANSNO_OFF 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			float4 _FlowMask_ST;
			half4  _BaseColor;
			half4  _SpecColor, _DarkColor;
			half   _SpecInt, _BaseInt, _DarkInt, _DarkSide;
   			half   _Smoothness;
 			half   _HitColorValue;
			half4  _HitColor;
			half   _HitColorWeight;
			half   _HitColorInt;
			half   _SpecCubeInt;
			real   _EffectShinness;

			real   _OffsetFactor;
			real   _OffsetUnits;

  			float4 _FlowColor;
			half   _FlowIn;
			half   _FlowLe;
			half   _LightInt;

			half   _FlowSpeed;
			half   _Transparency;
			half   _Cutoff;
			half4  _LightVec;
			half   _Emission;
 			CBUFFER_END
			TEXTURE2D(_BaseMap);            SAMPLER(sampler_BaseMap);
			TEXTURE2D(_BumpMap);		    SAMPLER(sampler_BumpMap);
 			TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);
			TEXTURE2D(_FlowMask);           SAMPLER(sampler_FlowMask);

		  	TEXTURECUBE(_SpecCube);			SAMPLER(sampler_SpecCube);

			real3 DirectBDRF(half roughness, half3 normalWS, half3 lightDirectionWS, half3 viewDirectionWS)
			{
				half specularTerm = 0.0;
#ifndef _SPECULARHIGHLIGHTS_OFF
				half roughness2 = roughness * roughness;
				half roughness2MinusOne = roughness2 - 1.0h;
				half normalizationTerm = roughness * 4.0h + 2.0h;

				float3 halfDir = SafeNormalize(float3(lightDirectionWS)+float3(viewDirectionWS));
				float NoH = saturate(dot(normalWS, halfDir));
				half LoH = saturate(dot(lightDirectionWS, halfDir));
				float d = NoH * NoH * roughness2MinusOne + 1.00001f;
				half LoH2 = LoH * LoH;
				specularTerm = roughness2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);
#else  
				specularTerm = specularTerm - HALF_MIN;
				specularTerm = clamp(specularTerm, 0.0, 100.0); // Prevent FP16 overflow on mobiles
#endif
				half3 color = specularTerm;
				return color;
			} 

			struct Attributes
			{
				float4 positionOS				: POSITION;
				float3 normalOS					: NORMAL;
				float4 tangentOS				: TANGENT;
				float2 texcoord					: TEXCOORD0;
				float2 lightmapUV				: TEXCOORD1;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct Varyings
			{
#ifndef _FLOWMAP_OFF
				float4 uv                       : TEXCOORD0;
#else
				float2 uv                       : TEXCOORD0;
#endif
				
				float3 posWS                    : TEXCOORD1;
				float4 normal                   : TEXCOORD2;
				float4 tangent                  : TEXCOORD3;
				float4 bitangent                : TEXCOORD4;
				float3 viewDir                  : TEXCOORD5;
				float4 positionCS               : SV_POSITION;
 				DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 6);

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
#ifndef _FLOWMAP_OFF
				output.uv.xy = TRANSFORM_TEX(input.texcoord, _BaseMap);
				output.uv.zw = TRANSFORM_TEX(input.texcoord, _FlowMask)-(_FlowSpeed * _Time.y);
#else
				output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
#endif 

				OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
				OUTPUT_SH(output.normal.xyz, output.vertexSH);
 
 				return output;
			}

			real4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);		

#ifndef _FLOWMAP_OFF
				float2 uv = input.uv.xy;
				float2 uv1 = input.uv.zw;
				float2 flowMap = SAMPLE_TEXTURE2D(_FlowMask, sampler_FlowMask, uv).rb;
				float  flowMap_var = SAMPLE_TEXTURE2D(_FlowMask, sampler_FlowMask, uv1).g;				   
		//		real3 flowColor = _FlowIn * _FlowColor.rgb * lerp(0, 1, flowMap_var)+ _FlowIn * float3(flowMap,0)*2;
				real3 flowColor = _FlowIn * _FlowColor.rgb * lerp(0, 1, flowMap_var);
 #else
				float2 uv = input.uv;
				real3 flowColor = float3(0, 0, 0);
#endif
				real3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _BaseInt;

//#if	_TRANSNO_OFF
//				real alpha = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a;				
//
//#else //_TRANSNO_ON
		//		real alpha = saturate(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a * _Transparency + albedo.r * _Transparency);
			//	real alpha =  SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a * _Transparency;
				//		clip(alpha* _Transparency - _Cutoff);
				real alpha = saturate(1 * _Transparency + _Transparency);

				alpha = saturate(alpha * SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a);

			 	clip(alpha* _Transparency*4 - 0.2);
		 	/*	if (SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).a < _Cutoff)
				{
					discard;
				} */

 			 	half3 bakedGI_var = SAMPLE_GI(input.lightmapUV, input.vertexSH, input.normal.xyz);

				Light mainLight = GetMainLight();
				MixRealtimeAndBakedGI(mainLight, input.normal.xyz, bakedGI_var, half4(0, 0, 0, 0));

				half3 attenuatedLightColor = mainLight.color * (mainLight.distanceAttenuation * mainLight.shadowAttenuation);
		//		half3 direct = float3(1,1,1);
				half3 direct = attenuatedLightColor+bakedGI_var*_LightInt;
				 
 			 	half3 l = mainLight.direction;
				half3 vL = _LightVec.xyz;

				half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
				viewDirWS = SafeNormalize(viewDirWS);

				real4 specGloss = SAMPLE_TEXTURE2D(_SpecGlossMap, sampler_SpecGlossMap, uv);
				half smoothness = specGloss.r * _Smoothness;
			 
#ifndef _NORMALMAP_OFF
				half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv));
				half3 normalWS_var = TransformTangentToWorld(normalTS,
					   half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
				normalWS_var = NormalizeNormalPerPixel(normalWS_var);	  	
#else
				half3 normalWS_var = input.normal.xyz;
			//	half NoL = dot(normalWS_var, l);
#endif	 			
				half NoL = max(dot(normalWS_var, vL), -0.5) * 0.5 + 0.4;
#ifndef _CUBEMAP_OFF
				real3  reflectVector = reflect(-viewDirWS, normalWS_var);
		/*		real3 uvw = BoxProjection(reflectVector, input.posWS, _SpecCube_ProbePosition,_SpecCube_BoxMin, _SpecCube_BoxMax);
				real3 envcolor = SAMPLE_TEXTURECUBE_LOD(_SpecCube, sampler_SpecCube, uvw, mip);*/
				float mip = PerceptualRoughnessToMipmapLevel(PerceptualSmoothnessToPerceptualRoughness(smoothness));
				half3 envcolor = SAMPLE_TEXTURECUBE_LOD(_SpecCube, sampler_SpecCube, reflectVector, mip).rgb;
 				envcolor*= albedo* pow(direct,2);
				 
				albedo = lerp(direct * albedo, envcolor, _SpecCubeInt); 
				half3 diffuseColor = (envcolor + albedo) * NoL;
#else
				half3 diffuseColor = direct * albedo * NoL;
#endif	  
				float3 specularTerm = DirectBDRF(smoothness, normalWS_var, vL, viewDirWS);
				half3 specular = saturate(specularTerm * _SpecColor.rgb);
				specular *= _SpecInt;

				half fresnelTerm = pow(1.0 - saturate(dot(normalWS_var, viewDirWS)), _DarkSide);
				//	flowColor*= pow(abs(specularTerm),_EffectShinness);
				flowColor *= lerp(flowColor * 2, flowColor * fresnelTerm*20 , _FlowLe) ;

				half3 hit = lerp(diffuseColor.rgb * (_HitColor.rgb * 0.5 + 0.5), _HitColor.rgb, _HitColorWeight);

 				half3 hitcolor = lerp(diffuseColor, _HitColorInt* hit,_HitColorValue);
				half3 hitcolor_var = saturate(hitcolor);

				half3 Emission = 0.3 * _Emission * albedo.rgb;
				half3 finalColor = saturate((hitcolor_var + Emission) + specular + flowColor);
				finalColor = lerp(finalColor, saturate(dot(finalColor.rgb, real3(0.22, 0.707, 0.071))* _DarkColor.xyz) , saturate(fresnelTerm * _DarkInt));

    			return real4(finalColor, alpha);
 			}
		ENDHLSL
		}

	}

}
