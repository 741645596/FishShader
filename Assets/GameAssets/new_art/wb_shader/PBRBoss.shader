
Shader "WB/PBRBoss"
{
	Properties
	{
		
		_BaseMap("BaseMap", 2D) = "white" {}
		_BaseColor("BaseColor", Color) = (1, 1, 1, 1)

		_NormalMap("NormalMap", 2D) = "bump" {}
		_MixMap("MixMap( [ R : 金属]  [G : AO]  [B : 无]  [A : 光滑] )", 2D) = "white" {}
		_EmissionMap("EmissionMap", 2D) = "black" {}

		_MetallicRemapMin("MetallicRemapMin", Range(0, 1)) = 0
		_MetallicRemapMax("MetallicRemapMax", Range(0, 1)) = 1

		_SmoothnessRemapMin("SmoothnessRemapMin", Range(0, 1)) = 0
		_SmoothnessRemapMax("SmoothnessRemapMax", Range(0, 1)) = 1
		
		_BumpStrength("BumpStrength", Range(0, 10)) = 1
		_OcclusionStrength("OcclusionStrength", Range(0, 10)) = 1

		[HDR]_EmissionColor("Emission Color", Color) = (0, 0, 0, 0)

		[HDR]_RimColor("RimColor", Color) = (0, 0, 0, 0)
		_RimSpread("RimSpread", Range( 0 , 50)) = 1
		_RimOffset("RimOffset", Vector) = (0, 0, 0, 0)
		_RimPower("RimPower", Float) = 1

		
		_StreamerNoise("StreamerNoise", 2D) = "black" {}
		_StreamerMask("StreamerMask", 2D) = "black" {}
		_StreamerTex("StreamerTex", 2D) = "black" {}

		_StreamerAlpha("StreamerAlpha", Range(0, 1)) = 1

		_StreamerNoiseSpeed("StreamerNoiseSpeed", Range(0, 10)) = 1
		_StreamerScrollX("StreamerScrollX", Range(0, 10)) = 1
		_StreamerScrollY("StreamerScrollY", Range(0, 10)) = 1
		[HDR]_StreamerColor("StreamerColor", Color) = (0, 0, 0, 0)



	}

	SubShader
	{


		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Back
		

		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM

			#define _RECEIVE_SHADOWS_OFF 1
			#define _EMISSION

			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

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
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				float color : COLOR;
				
				
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 vertexLight : TEXCOORD1;
				float4 color : TEXCOORD2;

				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				
				float4 uv : TEXCOORD6;

				float4 streamerUv : TEXCOORD7;
				
				
			};

			CBUFFER_START(UnityPerMaterial)

			float4 _BaseMap_ST;

			float4 _BaseColor;


			float4 _NormalMap_ST;
			float4 _MixMap_ST;

			float4 _RimColor;
			float _RimSpread;
			float4 _RimOffset;
			float _RimPower;

			float _MetallicRemapMin, _MetallicRemapMax;
			float _SmoothnessRemapMin, _SmoothnessRemapMax;
			float _OcclusionStrength;
			float _BumpStrength;

			float4 _EmissionColor;

			float _StreamerAlpha;
			float _StreamerNoiseSpeed;
			float _StreamerScrollX;
			float _StreamerScrollY;
    		float4 _StreamerColor;

			float4 _StreamerTex_ST;
			
			
			CBUFFER_END
			sampler2D _BaseMap;
			sampler2D _NormalMap;
			sampler2D _MixMap;
			sampler2D _EmissionMap;

			sampler2D _StreamerNoise;
			sampler2D _StreamerTex;
			sampler2D _StreamerMask;


			
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

				float4 offset = (_Time.xyyx * float4(_StreamerScrollX, _StreamerScrollY, _StreamerScrollY, _StreamerScrollX));
				offset = frac(offset);

				float4 streamerUv = v.texcoord.xyxy * _StreamerTex_ST.xyxy + _StreamerTex_ST.zwzw;

				o.streamerUv = streamerUv + offset;

				o.color = v.color;
				
				return o;
			}
			
			

			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{

				//return IN.color;

				float3 WorldNormal = normalize( IN.tSpace0.xyz );
				float3 WorldTangent = IN.tSpace1.xyz;
				float3 WorldBiTangent = IN.tSpace2.xyz;

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				
				//----------------------
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );


				float2 uv_BaseMap = IN.uv.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				
				float2 uv_NormalMap = IN.uv.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;
				
				float2 uv_MixMap = IN.uv.xy * _MixMap_ST.xy + _MixMap_ST.zw;

				float4 tex2DNode11 = tex2D( _MixMap, uv_MixMap );

				float4 emissionMapColor = tex2D(_EmissionMap, IN.uv.xy );

				//float4 emission = tex2DNode11.b * _EmissionColor + emissionMapColor * _EmissionColor;
				float4 emission = emissionMapColor * _EmissionColor;


				//-----------------------
				
				float3 Albedo = tex2D( _BaseMap, uv_BaseMap ).rgb * _BaseColor.rgb;
				

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

				inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));


				float ndv = dot( normalize(inputData.normalWS) , WorldViewDirection + _RimOffset );
				float3 rimColor = ( pow( ( 1.0 - saturate( ndv ) ) , _RimSpread ) * _RimColor ).rgb * _RimPower;
				float3 Emission = rimColor + emission;


				float streamNoiseX = tex2D(_StreamerNoise, IN.streamerUv.yx).x;
				float streamNoiseY = tex2D(_StreamerNoise, IN.streamerUv.zw).y;
				float streamNoise = streamNoiseX * streamNoiseY;
				float2 streamerUvNew = streamNoise.xx * _StreamerNoiseSpeed + IN.streamerUv.xy;

				float3 streamer = tex2D(_StreamerTex, streamerUvNew.xy).xyz;
				float3 streamerMask = tex2D(_StreamerMask, IN.uv.xy).xyz;

				streamer *= _StreamerColor.rgb;

				streamer *= streamerMask;

				streamer *= _StreamerAlpha;
				
				//return float4(streamer.xyz, 1);
				Emission += streamer;


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

		

	}
	
	
}
