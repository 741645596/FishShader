Shader "Fish/Common"
{
	Properties
	{
		_MainTex("主贴图(RGB)", 2D) = "White" {}
		_Color("主色调", Color) = (1,1,1,1)
		_SpecColor("高光颜色", Color) = (1,1,1,1)
		_Alpha("透明度", float) = 1
		_AlphaA_Strength("Alpha A Strength", Range(0, 2)) = 1
		_AlphaB_Strength("Alpha B Strength", Range(0, 2)) = 1
		_Albedo("反射率", Range(0, 1)) = 0.5
		_Shininess("亮度", Range(0.01, 3)) = 1.5
		_Gloss("光泽度", Range(0, 1)) = 0.5
		_FrezFalloff("边缘Alpha衰减", Range(0, 10)) = 4
		
		// 遮罩贴图
		[Space(20)]
		[Toggle(_USE_ALPHA_MAP_ON)] _Use_Alpha_Map("使用遮罩贴图", float) = 0
		_AlphaTex("遮罩贴图", 2D) = "White" {}

		// 受击变红
		[Space(20)]
		[HideInInspector]
		_IsHurt("击中颜色显示", Range(0, 1)) = 0
		_HitSaturation("击中颜色饱和", Range(-10, 10)) = 0.2
		_HitColor("击中最终颜色", Color) = (1,0,0,1)
		_HitAniValue("击中动画渐变", Range(0, 1)) = 0
		[HideInInspector]
		_HurtColor("挨打颜色", Color) = (0, 0, 0, 0)

		// 反射贴图
		[Space(30)]
		[Toggle(_USE_CUBE_MAP_ON)] _Use_Cube_Map ("使用CubeMap", float) = 1
		_Cube("反射Cubemap", Cube) = "Black" {}
		_Reflection("反射Cubemap值", Range(0, 1)) = 0.5

		// 法线贴图
		[Space(30)]
		[Toggle(_USE_BUMP_MAP_ON)] _Use_Bump_Map ("使用法线贴图", float) = 1
		[NoScaleOffset] _BumpMap("法线贴图", 2D) = "Bump" {}
		_FishBumpScale("FishBumpScale", Range(0, 2)) = 1
	}

	SubShader
	{
		Tags
		{
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "False"
			"Queue" = "Geometry"
			"RenderType" = "Opaque"
		}

		Pass
		{
			Name "Pass"
			Tags
			{
				"LightMode" = "UniversalForward"
				//	"LightMode" = "ForwardBase"
			}

			Blend SrcAlpha OneMinusSrcAlpha
			Cull Back

			ZTest LEqual
			ZWrite On

			HLSLPROGRAM
			#pragma multi_compile __ _USE_CUBE_MAP_ON
			#pragma multi_compile __ _USE_BUMP_MAP_ON
			#pragma multi_compile __ _USE_ALPHA_MAP_ON

			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_instancing


			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" 

			CBUFFER_START(UnityPerMaterial)

			half4 _Color;
			half4 _SpecColor;
			half _GrayValue;
			half _IsHurt;
			half _HitSaturation;
			half4 _HitColor;
			half4 _HurtColor;
			half _HitAniValue;
			half _Alpha;
			half _AlphaA_Strength;
			half _AlphaB_Strength;
			half _Albedo;
			half _Shininess;
			half _Gloss;
			half _Reflection;
			half _FrezFalloff;

			float4 _MainTex_ST;

		#if _USE_ALPHA_MAP_ON
			float4 _AlphaTex_ST;
		#endif

		#if _USE_BUMP_MAP_ON
			float4 _BumpMap_ST;
			half _FishBumpScale;
		#endif

			CBUFFER_END

			TEXTURE2D(_MainTex);          SAMPLER(sampler_MainTex);

		#if _USE_ALPHA_MAP_ON
			TEXTURE2D(_AlphaTex);		    SAMPLER(sampler_AlphaTex);
		#endif

		#if _USE_CUBE_MAP_ON
			samplerCUBE _Cube;
		#endif

		#if _USE_BUMP_MAP_ON
			TEXTURE2D(_BumpMap);          SAMPLER(sampler_BumpMap);
		#endif


			struct Attributes
			{
				float3 vertex : POSITION;
				float2 texcoord0 :TEXCOORD0;
				float3 normalOS : NORMAL;
			#if _USE_BUMP_MAP_ON
				float4 tangentOS : TANGENT;
			#endif
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 uv0 :TEXCOORD0;
				float3 positionWS : TEXCOORD1;
				float3 normalWS : TEXCOORD2;
				float3 vertexSH: TEXCOORD3;
			#if _USE_BUMP_MAP_ON
				float3 tangentWS : TEXCOORD4;
				float3 bitangentWS : TEXCOORD5;
			#endif
				
			};

			half4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
			{
				return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
			}


			Varyings vert(Attributes input)
			{
				Varyings output;
				output.uv0.xy = TRANSFORM_TEX(input.texcoord0, _MainTex).xy;
				output.positionWS = TransformObjectToWorld(input.vertex.xyz);
				output.positionCS = TransformObjectToHClip(input.vertex);
			#if _USE_BUMP_MAP_ON
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
				output.normalWS = normalInput.normalWS;
				output.tangentWS = normalInput.tangentWS;
				output.bitangentWS = normalInput.bitangentWS;
			#else
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
				output.normalWS = normalInput.normalWS;
			#endif
				output.vertexSH = half3(1.0, 1.0, 1.0);// max(half3(0, 0, 0), SampleSH(output.normalWS));
				return output;
			}


			half4 frag(Varyings in_f) : SV_TARGET
			{
				float3 viewDirection = in_f.positionWS - _WorldSpaceCameraPos.xyz;
				float2 uv0 = in_f.uv0.xy;
				float4 _MainTex_var = SampleTexture_RGBA(uv0, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
				float alpha = _MainTex_var.a;
				clip(alpha - 0.1);

			#if _USE_BUMP_MAP_ON
				float3x3 tangentTransform = float3x3(in_f.tangentWS, in_f.bitangentWS, in_f.normalWS);
				half3 normalLocal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv0)).xyz;
				normalLocal.x *= _FishBumpScale;
				float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
			#else
				float3 normalDirection = normalize(in_f.normalWS);
			#endif

				float3 vertexSH = in_f.vertexSH;
				float3 viewReflectDirection = reflect(-viewDirection, normalDirection);
				float3 lightDirection = normalize(_MainLightPosition.xyz);
				
			#if _USE_ALPHA_MAP_ON
				float4 _AlphaTex_var = SampleTexture_RGBA(uv0, TEXTURE2D_ARGS(_AlphaTex, sampler_AlphaTex));
			#else
				float4 _AlphaTex_var = float4(0, 0, 1, 1);
			#endif
				Light mainLight = GetMainLight();
				float3 lightColor = mainLight.color;


				float3 fc_1;
				float3 fc_2;
				float3 fc_5;
				float3 fc_6;
				float3 fc_7;
				float3 fc_8;
				float3 fc_9;
				float3 fc_10;
				float3 fc_11;
				float3 fc_12;

				float2 fb_1;
				float2 fb_2;

				float fa_1;
				float fa_2;
				float fa_3;

				float ua_1;
				float ua_3;
				float ua_4;
				float ua_5;

				

				// 高光计算
				fa_1 = dot(viewDirection, normalDirection);
				fa_1 = ((-abs(fa_1)) + 1);
				fa_1 = log2(fa_1);
				fa_1 = (fa_1 * _FrezFalloff);
				fa_1 = exp2(fa_1);
				fa_1 = (fa_1 + _Gloss);
				fa_1 = (fa_1 * _Alpha);

				fa_2 = (_AlphaTex_var.z * _AlphaB_Strength);
				fc_6.y = (fa_1 * fa_2);
				fc_6.x = (fa_2 * _Shininess);
				fb_1.xy = fc_6.xy * _AlphaA_Strength;
				ua_3 = step(_AlphaTex_var.x, 0);
				fc_7.xy = lerp(fb_1.xy, fc_6.xy, ua_3);

				ua_1 = step(0.01, _IsHurt);
				fc_7.x = lerp(fc_7.x, _Shininess, ua_1);
				fa_3 = lerp(fc_7.y, fa_1, ua_1);

				fc_7.x = (fc_7.x * 128);
				fb_2.x = dot(normalDirection, viewDirection);
				fb_2.y = dot(normalDirection, lightDirection) * 0.5 + 0.5;
				fb_2.xy = max(fb_2.xy, float2(0, 0.1));
				fb_2.x = log2(fb_2.x);
				fc_7.x = (fb_2.x * fc_7.x);
				fc_7.x = exp2(fc_7.x);
				fc_7.x = (fa_3 * fc_7.x);
				fc_8.xyz = lightColor.xyz * _SpecColor.xyz;
				fc_7.x = saturate(fc_7.x);
				fc_7.xyz = fc_7.x * fc_8.xyz;
				// 是否金属或非金属
				ua_4 = step(0.5, _AlphaTex_var.z);

				fc_11 = fa_2 * _Color.xyz;
				fc_11 = fc_11 * _MainTex_var.xyz;
				fc_8 = lerp(_MainTex_var.xyz, fc_11, ua_4);

				fc_9 = fc_8 * _Albedo;
				fc_8 = _AlphaTex_var.y * fc_8;
				fc_10 = fc_9 * lightColor.xyz;

				// 高光 + 兰伯特反射
				fc_7 = fc_10 * fb_2.y + fc_7;
				fc_7 = fc_7 + fc_7;

				// 加上环境光
				fc_7 = fc_9 * vertexSH + fc_7;

#if _USE_CUBE_MAP_ON
				//float3 _Cube_var = SAMPLE_TEXTURECUBE(_Cube, sampler_Cube, viewReflectDirection).rgb;
				float3 _Cube_var = texCUBE(_Cube, viewReflectDirection).xyz;
				// cube反射
				fc_12 = _Cube_var * _Reflection;
				fc_2 = fa_2 * fc_12;
				fc_9 = fc_8 * fc_12 - fc_12;
				fc_1 = fc_8 * fc_2 - fc_2;
				fc_5.x = dot(fc_12, float3(0.219999999, 0.707000017, 0.0710000023));
				fc_5.x = clamp(fc_5.x, 0, 1);
				fc_5.x = (((-fc_5.x) * fc_5.x) + 1);
				fc_12 = fc_5.x * fc_9 + fc_12;
				fc_1 = fc_5.x * fc_1 + fc_2;
				fc_2 = fc_12 * _AlphaA_Strength;
				fc_6 = fc_1 * _AlphaA_Strength;
				fc_5 = lerp(fc_6, fc_1, ua_3);
				fc_5 = lerp(fc_5, fc_2, ua_1);
#else
				fc_5 = float3(0, 0, 0);
#endif
				// 受击变红
				_HitAniValue = _HurtColor.b;
				fc_8 = _HitColor;
				fc_1 = fc_8 * _HitAniValue;
				fc_1 = fc_5 * _HitSaturation + fc_1;
				fc_8 = fc_1 * _IsHurt;
				fc_5 = lerp(fc_5, fc_8, ua_1);
				float3 color = fc_5 + fc_7;

				//color = _Cube_var;
				//color = vertexSH * _MainTex_var.xyz * _Albedo;
				return float4(color, _MainTex_var.a * _Alpha);
			}

			ENDHLSL
		}
	}
	FallBack Off
}