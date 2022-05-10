
Shader "WB/PBR"
{
	Properties
	{
		
		_BaseMap("BaseMap", 2D) = "white" {}
		_BaseColor("BaseColor", Color) = (1, 1, 1, 1)

		_NormalMap("NormalMap", 2D) = "bump" {}
		_MixMap("MixMap( [ R : 金属]  [G : AO]  [B : 自发光]  [A : 光滑] )", 2D) = "white" {}
		//_EmissionMap("EmissionMap", 2D) = "black" {}

		_MetallicRemapMin("MetallicRemapMin", Range(0, 1)) = 0
		_MetallicRemapMax("MetallicRemapMax", Range(0, 1)) = 1

		_SmoothnessRemapMin("SmoothnessRemapMin", Range(0, 1)) = 0
		_SmoothnessRemapMax("SmoothnessRemapMax", Range(0, 1)) = 1
		
		_BumpStrength("_BumpStrength", Range(0, 10)) = 1
		_OcclusionStrength("_OcclusionStrength", Range(0, 10)) = 1

		[HDR]_EmissionColor("Emission Color", Color) = (0, 0, 0, 0)

		[HDR]_RimColor("RimColor", Color) = (0, 0, 0, 0)
		_RimSpread("RimSpread", Range( 0 , 50)) = 1
		_RimOffset("RimOffset", Vector) = (0, 0, 0, 0)

		_HSVHue("色调", Range(0, 1)) = 0.5
		_HSVSat("饱和度", Range(0, 1)) = 1
		_HSVValue("亮度", Range(0, 1)) = 1

		_VeterxOff("_VeterxOff[阴影位置偏移值xyz有效]", Vector) = (0, 0, 0, 0)
		_ShadowColor("ShadowColor[阴影色]", Color) = (0.3, 0.3, 0.3, 0.3)
	}

	SubShader
	{
		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Back
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode" = "UniversalForward" "RenderType" = "Opaque" "Queue" = "Geometry" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "ColorCore.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 vertexLight : TEXCOORD1;
				
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				
				float4 uv : TEXCOORD6;
				
				
			};

			CBUFFER_START(UnityPerMaterial)

			float4 _BaseMap_ST;

			float4 _BaseColor;


			float4 _NormalMap_ST;
			float4 _MixMap_ST;

			float4 _RimColor;
			float _RimSpread;
			float4 _RimOffset;

			float _MetallicRemapMin, _MetallicRemapMax;
			float _SmoothnessRemapMin, _SmoothnessRemapMax;
			float _OcclusionStrength;
			float _BumpStrength;

			float4 _EmissionColor;

			float _HSVHue;
			float _HSVSat;
			float _HSVValue;
			
			CBUFFER_END
			sampler2D _BaseMap;
			sampler2D _NormalMap;
			sampler2D _MixMap;
			sampler2D _EmissionMap;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				o.uv.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.uv.zw = 0;
				
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				
				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				
				o.vertexLight = half4(vertexLight, 0);
				
				o.clipPos = positionCS;
				
				return o;
			}
			
			

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{

				float3 WorldNormal = normalize( IN.tSpace0.xyz );
				float3 WorldTangent = IN.tSpace1.xyz;
				float3 WorldBiTangent = IN.tSpace2.xyz;

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				float3x3 tangentTransform = float3x3(WorldTangent, WorldBiTangent, WorldNormal);
				//----------------------
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );


				float2 uv_BaseMap = IN.uv.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				
				float2 uv_NormalMap = IN.uv.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				
				float2 uv_MixMap = IN.uv.xy * _MixMap_ST.xy + _MixMap_ST.zw;

				float4 tex2DNode11 = tex2D( _MixMap, uv_MixMap );

				float4 emissionMapColor = tex2D(_EmissionMap, IN.uv.xy );


				float4 emission = tex2DNode11.b * _EmissionColor;
				//float4 emission = emissionMapColor.b * _EmissionColor;


				//-----------------------
				
				float3 Albedo = tex2D( _BaseMap, uv_BaseMap ).rgb * _BaseColor.rgb;

				float4 temInputColor = float4(Albedo.x, Albedo.y, Albedo.z, 1);
				float4 temOutputColor = float4(0, 0, 0, 0);
				hue_sat(_HSVHue, _HSVSat, _HSVValue, 1, temInputColor, temOutputColor);
				Albedo.rgb = temOutputColor.rgb;
				

				float3 Normal = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), _BumpStrength );

				float meta = lerp(_MetallicRemapMin, _MetallicRemapMax, tex2DNode11.r);
				float smooth = lerp(_SmoothnessRemapMin, _SmoothnessRemapMax, tex2DNode11.a);
				
				float3 Specular = 0.5;
				float Metallic = meta;
				float Smoothness = smooth;
				float Occlusion = tex2DNode11.g * _OcclusionStrength;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				inputData.normalWS = TransformTangentToWorld(Normal, tangentTransform);


				float ndv = dot( normalize(inputData.normalWS) , WorldViewDirection + _RimOffset.xyz );
				float3 rimColor = ( pow( ( 1.0 - saturate( ndv ) ) , _RimSpread ) * _RimColor ).rgb;
				float3 Emission = rimColor.rgb + emission.rgb;



				inputData.vertexLighting = IN.vertexLight.xyz;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);


				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);
				return color;
			}

			ENDHLSL
		}
		/*
		Pass
		{
			Name "Shadow"
			Tags { "LightMode" = "SRPDefaultUnlit"  "RenderType" = "Transparent" "Queue" = "Transparent" }
			Blend SrcAlpha OneMinusSrcAlpha
			//ZWrite Off
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA


			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "ColorCore.hlsl"
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



			VertexOutput VertexFunction(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				float3 positionWS = TransformObjectToWorld(v.vertex.xyz) +  _VeterxOff.xyz;
				float3 positionVS = TransformWorldToView(positionWS);
				float4 positionCS = TransformWorldToHClip(positionWS);


				o.clipPos = positionCS;

				return o;
			}



			VertexOutput vert(VertexInput v)
			{
				return VertexFunction(v);
			}

			half4 frag(VertexOutput IN) : SV_Target
			{
				return _ShadowColor;
			}

			ENDHLSL
		}
        */
	}
	
	
}
