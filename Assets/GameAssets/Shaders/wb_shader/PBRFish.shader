
Shader "WB/PBRFish"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("CullMode", float) = 2
		[Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend("Source Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend("Dest Blend Mode", Float) = 10
		[Enum(Off, 0, On, 1)]_ZWriteMode("ZWriteMode", float) = 0

		[Foldout] _BaseName("Base控制面板",Range(0,1)) = 0
		[FoldoutItem] _BaseColor("BaseColor[ RGR : 主色]  [A : 透明]", Color) = (1, 1, 1, 1)
		[FoldoutItem] _ColorScale("_ColorScale", float) = 1
		[FoldoutItem] _ContrastScale("_ContrastScale", Float) = 1
		[FoldoutItem] _OverColor("OverlayColor", Color) = (1,1,1,1)
		[FoldoutItem] _OverMultiple("OverlayMultiple", Range(0,1)) = 1


		[Foldout] _PBRName("PBR控制面板",Range(0,1)) = 0
		[FoldoutItem] [NoScaleOffset]  _BaseMap("BaseMap( [ RGR : 主色]  [A : 透明] )", 2D) = "white" {}
		

		[FoldoutItem] [Normal][NoScaleOffset] _NormalMap("NormalMap", 2D) = "bump" {}
		[FoldoutItem] [NoScaleOffset] _MixMap("MixMap( [ R : 金属]  [G : AO]  [B : 用于mask标示眼球]  [A : 光滑] )", 2D) = "white" {}

		[FoldoutItem]  _MetallicRemapMin("MetallicRemapMin[金属度区间]", Range(0, 1)) = 0
		[FoldoutItem]  _MetallicRemapMax("MetallicRemapMax[金属度区间]", Range(0, 1)) = 1

		[FoldoutItem] _SmoothnessRemapMin("SmoothnessRemapMin[光滑度区间]", Range(0, 1)) = 0
		[FoldoutItem] _SmoothnessRemapMax("SmoothnessRemapMax[光滑度区间]", Range(0, 1)) = 1
		[FoldoutItem] _GlossMapScale("_GlossMapScale", float) = 0.75
		
		[FoldoutItem] _BumpStrength("_BumpStrength[法线强度]", Range(0, 10)) = 1
		[FoldoutItem] _OcclusionStrength("_OcclusionStrength[AO强度]", Range(0, 10)) = 1

		[Foldout] _RimColorName("边缘光控制面板",Range(0,1)) = 0
		[FoldoutItem] [HDR]_RimColor("RimColor[边缘光]", Color) = (0, 0, 0, 0)
		[FoldoutItem] _RimOffset("RimOffset", Vector) = (0, 0, 0, 0)
		[FoldoutItem] _RimSpread("RimSpread[边缘扩展参数]", Float) = 1
		[FoldoutItem] _RimPower("RimPower", Float) = 1

		[Foldout] _HSVName("明亮度控制面板",Range(0,1)) = 0
		[FoldoutItem] _HSVHue("色调", Float) = 1
		[FoldoutItem] _HSVSat("饱和度", Float) = 0.8
		[FoldoutItem] _HSVValue("亮度", Float) = 1

		[Foldout] _SpecularName("高光控制面板",Range(0,1)) = 0
		[FoldoutItem] [ToggleOff] _SpecularHighlights("高光控制开关", Float) = 1.0
		[FoldoutItem]  _SpecularStrength("高光强度", Range(0.95, 1)) = 1.0

        [Foldout] _NonMetalName("非金属度控制面板",Range(0,1)) = 0
		[FoldoutItem] _NonMetalThreshold("非金属阈值", Range(0, 0.2)) = 0.1
		[FoldoutItem] _NonMetalStrength("非金属强度", Range(0, 10)) = 1.0

		[Foldout] _FishEyeName("鱼眼径向透明度面板",Range(0,1)) = 0
		[FoldoutItem] [Toggle] _FishEye("鱼眼径向透明度开关", Float) = 0.0
		[FoldoutItem] _RadiusAlphaStrength("RadiusAlphaStrength[径向透明度强度]", Range(1, 3)) = 1
		[FoldoutItem] _RadiusAlphaMin("_RadiusAlphaMin[径向透明度区间]", Range(0, 0.5)) = 0.3
		[FoldoutItem] _RadiusAlphaMax("_RadiusAlphaMax[径向透明度区间]", Range(0.9, 1)) = 1

        [Foldout] _HurtName("受击面板",Range(0,1)) = 0
		[FoldoutItem][KeywordEnum(Rim,Albedo,None)] _HitColorChannel("HitColorType", Float) = 1.0
		//[FoldoutItem][Toggle] _TestHitColor("美术测试受击变红颜色开关", Float) = 0.0
		[FoldoutItem] _HitColor("HitColor[美术控制]", Color) = (1,1,1,1)
		[FoldoutItem] _HitMultiple("HitMultiple[美术控制]", Range(0,1)) = 1
		[FoldoutItem] _HitRimPower("HitRim Power[美术控制]", Range(0.01, 10)) = 0.01
		[FoldoutItem] _HitRimSpread("Hit Rim Spread[美术控制]", Range(-15, 4.99)) = 0.01
		[FoldoutItem] _OverlayColor("_OverlayColor[程序控制]", Color) = (1,1,1,1)
		[FoldoutItem] _OverlayMultiple("_OverlayMultiple[程序控制]", Range(0,1)) = 1

        [Foldout] _FadeName("淡入淡出控制面板[程序控制]",Range(0,1)) = 0
		[FoldoutItem] _Alpha("_Alpha", float) = 1


		[Foldout] _EmissionName("自发光控制面板",Range(0,1)) = 0
		[FoldoutItem][Toggle] _Emission("自发光控制开关开关", Float) = 0.0
		[FoldoutItem][HDR]_EmissionColor("Emission Color[自发光]", Color) = (0, 0, 0, 0)
		[FoldoutItem][NoScaleOffset] _EmissionMap("EmissionMap[自发光]", 2D) = "black" {}

		[Foldout] _StreamerName("流光控制面板",Range(0,1)) = 0
		[FoldoutItem][Toggle] _Streamer("流光控制开关开关", Float) = 0.0
		[FoldoutItem] _StreamerNoise("StreamerNoise", 2D) = "black" {}
		[FoldoutItem] _StreamerMask("StreamerMask[流光mask]", 2D) = "black" {}
		[FoldoutItem] _StreamerTex("StreamerTex[流光纹理]", 2D) = "black" {}
		[FoldoutItem] _StreamerAlpha("StreamerAlpha[流光Alpha]", Range(0, 1)) = 1
		[FoldoutItem] _StreamerNoiseSpeed("StreamerNoiseSpeed[流光速度]", Range(0, 10)) = 1
		[FoldoutItem] _StreamerScrollX("StreamerScrollX[流光方向]", Range(-10, 10)) = 1
		[FoldoutItem] _StreamerScrollY("StreamerScrollY[流光方向]", Range(-10, 10)) = 1
		[FoldoutItem][HDR]_StreamerColor("StreamerColor", Color) = (0, 0, 0, 0)

	    [Foldout] _LightDirName("灯光方向面板",Range(0,1)) = 0
		[FoldoutItem][Toggle] _LightDirControl("灯光方向控制开关", Float) = 0.0
		[FoldoutItem] _LightDir("LightDir", Vector) = (0, 0, 0, 0)
	}

	SubShader
	{
		Tags {  "RenderPipeline" = "UniversalPipeline"  "Queue" = "Transparent -300" }
		Cull[_CullMode]
		LOD 300

		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend[_SourceBlend][_DestBlend]
			ZWrite[_ZWriteMode]

		    ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			
			//模板测试总是通过，并写入模板缓存区值为1
			Stencil
			{
				Ref 9
				Comp always
				Pass replace
				Fail keep
				ZFail keep
			}

			HLSLPROGRAM
			#pragma multi_compile __ _SPECULARHIGHLIGHTS_OFF
			//#pragma multi_compile __ _FISHEYE_ON
			#pragma multi_compile __ _EMISSION_ON
			#pragma multi_compile __ _STREAMER_ON
			#pragma multi_compile __ _LIGHTDIRCONTROL_ON
		    #pragma multi_compile  _HITCOLORCHANNEL_RIM _HITCOLORCHANNEL_ALBEDO _HITCOLORCHANNEL_NONE
		    //#pragma multi_compile __ _TESTHITCOLOR_ON

			#define _RECEIVE_SHADOWS_OFF 1

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD
			#include "ColorCore.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			//#include "LightingCore.hlsl"
			#include "SimpleLightingCore.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 vertexLight : TEXCOORD1;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				float2 uv : TEXCOORD6;
#if _STREAMER_ON
				float4 streamerUv : TEXCOORD7;
#endif
			};

			CBUFFER_START(UnityPerMaterial)
#include "HitRed_dec.hlsl"

			float4 _BaseColor;
			float _ColorScale;
			float _ContrastScale;
			float3 _OverColor;
			float  _OverMultiple;

			float4 _BaseMap_ST;
			float4 _NormalMap_ST;
			float4 _MixMap_ST;

			float4 _RimColor;
			float _RimSpread;
			float4 _RimOffset;
			float _RimPower;

			float _MetallicRemapMin, _MetallicRemapMax;
			float _SmoothnessRemapMin, _SmoothnessRemapMax;
			float _GlossMapScale;
			float _OcclusionStrength;
			float _BumpStrength;
//#if			_EMISSION_ON
			float4 _EmissionColor;
//#endif

			float _HSVHue;
			float _HSVSat;
			float _HSVValue;
			float _SpecularStrength;

			float _NonMetalThreshold;
			float _NonMetalStrength;

			float _FishEye;
			float _RadiusAlphaStrength;
			float _RadiusAlphaMin, _RadiusAlphaMax;

//#if _STREAMER_ON
			float _StreamerAlpha;
			float _StreamerNoiseSpeed;
			float _StreamerScrollX;
			float _StreamerScrollY;
			float4 _StreamerColor;
			float4 _StreamerTex_ST;
//#endif
			float _Alpha;
			float3 _LightDir;
			CBUFFER_END
			sampler2D _BaseMap;
			sampler2D _NormalMap;
			sampler2D _MixMap;
#if			_EMISSION_ON
			//float4 _EmissionColor;
			sampler2D _EmissionMap;
#endif

#if _STREAMER_ON
			//float _StreamerAlpha;
			//float _StreamerNoiseSpeed;
			//float _StreamerScrollX;
			//float _StreamerScrollY;
			//float4 _StreamerColor;
			//float4 _StreamerTex_ST;
			sampler2D _StreamerNoise;
			sampler2D _StreamerTex;
			sampler2D _StreamerMask;
#endif
			

			VertexOutput vert ( VertexInput v )
			{
				VertexOutput o = (VertexOutput)0;
				o.uv.xy = v.texcoord.xy;
				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float3 positionVS = TransformWorldToView(positionWS);
				float4 positionCS = TransformWorldToHClip(positionWS);

				VertexNormalInputs normalInput = GetVertexNormalInputs(v.ase_normal, v.ase_tangent);

				o.tSpace0 = float4(normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);

				OUTPUT_SH(normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz);
				//half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				//o.vertexLight = half4(vertexLight, 0);

				o.clipPos = positionCS;
#if _STREAMER_ON
				float4 offset = (_Time.xyyx * float4(_StreamerScrollX, _StreamerScrollY, _StreamerScrollY, _StreamerScrollX));
				offset = frac(offset);
				float4 streamerUv = v.texcoord.xyxy * _StreamerTex_ST.xyxy + _StreamerTex_ST.zwzw;
				o.streamerUv = streamerUv + offset;
#endif
				return o;
			}

            #include "HitRed_fun.hlsl"
			half4 frag(VertexOutput IN) : SV_Target
			{
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

				float4 emission = float4(0, 0, 0, 0);
#if			_EMISSION_ON
				float4 emissionMapColor = tex2D(_EmissionMap, IN.uv.xy);
			    emission = emissionMapColor * _EmissionColor ; 
#else
                emission = _EmissionColor * tex2DNode11.b;
#endif
				float4 Albedo = tex2D(_BaseMap, uv_BaseMap).rgba;
				// hsv 处理
				float3 hvs = rgb2hsv(Albedo.rgb);
				hvs.x = fmod(_HSVHue * 0.00277777785 + hvs.x, 1);
				hvs.yz *= float2(_HSVSat, _HSVValue);
				Albedo.rgb = hsv2rgb(hvs);
				Albedo = Albedo.rgba * _BaseColor.rgba;
				Albedo.rgb = Albedo.rgb * _ColorScale;
				float Alpha = Albedo.a;
				

				float3 Normal = UnpackNormalScale( tex2D( _NormalMap, uv_NormalMap ), _BumpStrength );

				float meta = lerp(_MetallicRemapMin, _MetallicRemapMax, tex2DNode11.r);


				float smooth = lerp(_SmoothnessRemapMin, _SmoothnessRemapMax, tex2DNode11.a) * _GlossMapScale;

				//float3 Specular = 0.5;
				float3 Specular = _SpecularStrength;
				float Metallic = meta;
				float Smoothness = smooth;
				float Occlusion = tex2DNode11.g * _OcclusionStrength;
				// 先屏蔽
				if (tex2DNode11.r < _NonMetalThreshold)
				{
					Occlusion = Occlusion * _NonMetalStrength;
				}

				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;


				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));

				float ndv = dot(normalize(inputData.normalWS), normalize(WorldViewDirection + _RimOffset.xyz));
				float3 rimColor = (pow((1.0 - saturate(ndv)), 5.0 - _RimSpread) * _RimColor).rgb * _RimPower;
				float3 Emission = emission.rgb + rimColor;

#if _STREAMER_ON
				float streamNoiseX = tex2D(_StreamerNoise, IN.streamerUv.yx).x;
				float streamNoiseY = tex2D(_StreamerNoise, IN.streamerUv.zw).y;
				float streamNoise = streamNoiseX * streamNoiseY;
				float2 streamerUvNew = streamNoise.xx * _StreamerNoiseSpeed + IN.streamerUv.xy;
				float3 streamer = tex2D(_StreamerTex, streamerUvNew.xy).xyz;
				float3 streamerMask = tex2D(_StreamerMask, IN.uv.xy).xyz;
				streamer *= _StreamerColor.rgb;
				streamer *= streamerMask;
				streamer *= _StreamerAlpha;
				Emission += streamer;
#endif

				//inputData.vertexLighting = IN.vertexLight.xyz;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
//#if _FISHEYE_ON
				// 标识出金鱼眼圈。
				if (tex2DNode11.b < 0.3 && _FishEye == 1.0f)
				{
					float cosAngel = saturate(ndv);
					float RadiusDistance = 1 - cosAngel * cosAngel;
					float RadiusAlpha = pow(RadiusDistance, _RadiusAlphaStrength);
					RadiusAlpha = lerp(_RadiusAlphaMin, _RadiusAlphaMax, RadiusAlpha);
					Alpha = RadiusAlpha;
				}
//#endif
				Alpha = Alpha * _Alpha;
				clip(Alpha - 0.1f);
				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo.rgb , 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					float3(0,0,0),
					Alpha,
					_LightDir
				    );

				color.rgb = HitRed(color.rgb, Emission.rgb, inputData.normalWS, WorldViewDirection);
				color.rgb = CalcFinalColor(color.rgb, _OverColor, _OverMultiple, _ContrastScale);
				return color;
			}
			

			ENDHLSL
		}
	}
	SubShader
	{
		Tags {  "RenderPipeline" = "UniversalPipeline"  "Queue" = "Transparent -300" }
		Cull[_CullMode]
		LOD 150

		Pass
		{

			Name "Forward"
			Tags { "LightMode" = "UniversalForward" }

			Blend[_SourceBlend][_DestBlend]
			ZWrite[_ZWriteMode]

			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

		//模板测试总是通过，并写入模板缓存区值为1
		Stencil
		{
			Ref 9
			Comp always
			Pass replace
			Fail keep
			ZFail keep
		}

		HLSLPROGRAM
		#pragma multi_compile __ _SPECULARHIGHLIGHTS_OFF
		//#pragma multi_compile __ _FISHEYE_ON
		#pragma multi_compile __ _EMISSION_ON
		#pragma multi_compile __ _STREAMER_ON
		#pragma multi_compile __ _LIGHTDIRCONTROL_ON
		#pragma multi_compile  _HITCOLORCHANNEL_RIM _HITCOLORCHANNEL_ALBEDO _HITCOLORCHANNEL_NONE
		//#pragma multi_compile __ _TESTHITCOLOR_ON

		#define _RECEIVE_SHADOWS_OFF 1


		#pragma vertex vert
		#pragma fragment frag

		#define SHADERPASS_FORWARD
		#include "ColorCore.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "SimpleLightingCore.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

		struct VertexInput
		{
			float4 vertex : POSITION;
			float3 ase_normal : NORMAL;
			float4 ase_tangent : TANGENT;
			float4 texcoord : TEXCOORD0;

		};

		struct VertexOutput
		{
			float4 clipPos : SV_POSITION;
			float4 lightmapUVOrVertexSH : TEXCOORD0;
			float4 tSpace0 : TEXCOORD3;
			float4 tSpace1 : TEXCOORD4;
			float4 tSpace2 : TEXCOORD5;
			float2 uv : TEXCOORD6;
#if _STREAMER_ON
				float4 streamerUv : TEXCOORD7;
#endif
			};

			CBUFFER_START(UnityPerMaterial)
#include "HitRed_dec.hlsl"

			float4 _BaseColor;
			float _ColorScale;
			float _ContrastScale;
			float3 _OverColor;
			float  _OverMultiple;

			float4 _BaseMap_ST;
			float4 _NormalMap_ST;
			float4 _MixMap_ST;

			float4 _RimColor;
			float _RimSpread;
			float4 _RimOffset;
			float _RimPower;

			float _MetallicRemapMin, _MetallicRemapMax;
			float _SmoothnessRemapMin, _SmoothnessRemapMax;
			float _GlossMapScale;
			float _OcclusionStrength;
			float _BumpStrength;
//#if			_EMISSION_ON
			float4 _EmissionColor;
//#endif

			float _HSVHue;
			float _HSVSat;
			float _HSVValue;
			float _SpecularStrength;

			float _NonMetalThreshold;
			float _NonMetalStrength;

			float _FishEye;
			float _RadiusAlphaStrength;
			float _RadiusAlphaMin, _RadiusAlphaMax;

//#if _STREAMER_ON
			float _StreamerAlpha;
			float _StreamerNoiseSpeed;
			float _StreamerScrollX;
			float _StreamerScrollY;
			float4 _StreamerColor;
			float4 _StreamerTex_ST;
//#endif
			float _Alpha;
			float3 _LightDir;
			CBUFFER_END
			sampler2D _BaseMap;
			sampler2D _NormalMap;
			sampler2D _MixMap;
#if			_EMISSION_ON
	        //float4 _EmissionColor;
			sampler2D _EmissionMap;
		#endif
		#if _STREAMER_ON
			//float _StreamerAlpha;
			//float _StreamerNoiseSpeed;
			//float _StreamerScrollX;
			//float _StreamerScrollY;
			//float4 _StreamerColor;
			//float4 _StreamerTex_ST;
			sampler2D _StreamerNoise;
			sampler2D _StreamerTex;
			sampler2D _StreamerMask;
		#endif

			VertexOutput vert(VertexInput v)
			{
				VertexOutput o = (VertexOutput)0;
				o.uv.xy = v.texcoord.xy;

				float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
				float3 positionVS = TransformWorldToView(positionWS);
				float4 positionCS = TransformWorldToHClip(positionWS);

				VertexNormalInputs normalInput = GetVertexNormalInputs(v.ase_normal, v.ase_tangent);

				o.tSpace0 = float4(normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4(normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4(normalInput.bitangentWS, positionWS.z);

				OUTPUT_SH(normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz);

				o.clipPos = positionCS;
#if _STREAMER_ON
				float4 offset = (_Time.xyyx * float4(_StreamerScrollX, _StreamerScrollY, _StreamerScrollY, _StreamerScrollX));
				offset = frac(offset);
				float4 streamerUv = v.texcoord.xyxy * _StreamerTex_ST.xyxy + _StreamerTex_ST.zwzw;
				o.streamerUv = streamerUv + offset;
#endif
				return o;
			}

			#include "HitRed_fun.hlsl"
			half4 frag(VertexOutput IN) : SV_Target
			{
				float3 WorldNormal = normalize(IN.tSpace0.xyz);
				float3 WorldTangent = IN.tSpace1.xyz;
				float3 WorldBiTangent = IN.tSpace2.xyz;

				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz - WorldPosition;
				float4 ShadowCoords = float4(0, 0, 0, 0);

				//----------------------

				WorldViewDirection = SafeNormalize(WorldViewDirection);


				float2 uv_BaseMap = IN.uv.xy * _BaseMap_ST.xy + _BaseMap_ST.zw;
				
				float2 uv_NormalMap = IN.uv.xy * _NormalMap_ST.xy + _NormalMap_ST.zw;

				float2 uv_MixMap = IN.uv.xy * _MixMap_ST.xy + _MixMap_ST.zw;

				float4 tex2DNode11 = tex2D(_MixMap, uv_MixMap);

				float4 emission = float4(0, 0, 0, 0);
#if		_EMISSION_ON
				float4 emissionMapColor = tex2D(_EmissionMap, IN.uv.xy);
				emission = emissionMapColor * _EmissionColor;
#else
				emission = _EmissionColor * tex2DNode11.b;
#endif
				
				float4 Albedo = tex2D(_BaseMap, uv_BaseMap).rgba;
				// hsv 处理
				float3 hvs = rgb2hsv(Albedo.rgb);
				hvs.x = fmod(_HSVHue * 0.00277777785 + hvs.x, 1);
				hvs.yz *= float2(_HSVSat, _HSVValue);
				Albedo.rgb = hsv2rgb(hvs);
				Albedo = Albedo.rgba * _BaseColor.rgba;
				Albedo.rgb = Albedo.rgb * _ColorScale;
				float Alpha = Albedo.a;


				float3 Normal = UnpackNormalScale(tex2D(_NormalMap, uv_NormalMap), _BumpStrength);

				float meta = lerp(_MetallicRemapMin, _MetallicRemapMax, tex2DNode11.r);


				float smooth = lerp(_SmoothnessRemapMin, _SmoothnessRemapMax, tex2DNode11.a) * _GlossMapScale;

				//float3 Specular = 0.5;
				float3 Specular = _SpecularStrength;
				float Metallic = meta;
				float Smoothness = smooth;
				float Occlusion = tex2DNode11.g * _OcclusionStrength;
				// 先屏蔽
				if (tex2DNode11.r < _NonMetalThreshold)
				{
					Occlusion = Occlusion * _NonMetalStrength;
				}

				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				inputData.normalWS = TransformTangentToWorld(Normal, half3x3(WorldTangent, WorldBiTangent, WorldNormal));

				float ndv = dot(normalize(inputData.normalWS), normalize(WorldViewDirection + _RimOffset.xyz));
				float3 rimColor = (pow((1.0 - saturate(ndv)), 5.0 - _RimSpread) * _RimColor).rgb * _RimPower;
				float3 Emission = emission.rgb + rimColor;
#if _STREAMER_ON
				float streamNoiseX = tex2D(_StreamerNoise, IN.streamerUv.yx).x;
				float streamNoiseY = tex2D(_StreamerNoise, IN.streamerUv.zw).y;
				float streamNoise = streamNoiseX * streamNoiseY;
				float2 streamerUvNew = streamNoise.xx * _StreamerNoiseSpeed + IN.streamerUv.xy;
				float3 streamer = tex2D(_StreamerTex, streamerUvNew.xy).xyz;
				float3 streamerMask = tex2D(_StreamerMask, IN.uv.xy).xyz;
				streamer *= _StreamerColor.rgb;
				streamer *= streamerMask;
				streamer *= _StreamerAlpha;
				Emission += streamer;
#endif
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS);
//#if _FISHEYE_ON
				// 标识出金鱼眼圈。
				if (tex2DNode11.b < 0.3 && _FishEye == 1.0f)
				{
					float cosAngel = saturate(ndv);
					float RadiusDistance = 1 - cosAngel * cosAngel;
					float RadiusAlpha = pow(RadiusDistance, _RadiusAlphaStrength);
					RadiusAlpha = lerp(_RadiusAlphaMin, _RadiusAlphaMax, RadiusAlpha);
					Alpha = RadiusAlpha;
				}
//#endif
				Alpha = Alpha * _Alpha;
				clip(Alpha - 0.1f);
				half4 color = UniversalFragmentPBR(
					inputData,
					Albedo.rgb ,
					Metallic,
					Specular,
					Smoothness,
					Occlusion,
					float3(0,0,0),
					Alpha,
					_LightDir
					);

				color.rgb = HitRed(color.rgb, Emission.rgb, inputData.normalWS, WorldViewDirection);
				color.rgb = CalcFinalColor(color.rgb, _OverColor, _OverMultiple, _ContrastScale);
				return color;
			}
			ENDHLSL
		}
	}
CustomEditor "FoldoutShaderGUI"
}

