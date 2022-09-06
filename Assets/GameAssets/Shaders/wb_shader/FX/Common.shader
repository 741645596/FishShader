
Shader "WB/Common" {
    Properties {
		
        [Enum(AlphaBlend or Additive, 5, Multiply, 0)] _SrcBlend ("混合SRC", Float) = 5

        [Enum(AlphaBlend, 10,Additive, 1, Multiply, 3)] _DstBlend ("混合DST", Float) = 10

        [Enum(UnityEngine.Rendering.CullMode)] _CullMode ("单双面", Float) = 2

		[Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("深度测试", Float) = 4
        [Enum(Off, 0, On, 1)] _ZWrite ("写深度", Float) = 0

        
		[HDR]_Color ("主贴图颜色", Color) = (1, 1, 1, 1)
        _MainTex ("主贴图", 2D) = "white" {}
        _MainTexRotate("主贴图旋转", Float) = 0

		_MainTexSpeed_U("主贴图UV流动速度U", Float) = 0

		_MainTexSpeed_V("主贴图UV流动速度U", Float) = 0

		
		_MainTwistScale("主贴图扰动强度", Float) = 0

		[Space(30)]

		_TwistTex("扭曲图", 2D) = "black"{}

		_TwistScrollingX("扭曲UV流动U", Float) = 0

		_TwistScrollingY("扭曲UV流动V", Float) = 0


		

		[Space(30)]

		_DisTex("溶解图", 2D) = "white"{}

		_DisTwistScale("溶解强度", Float) = 0

		_DisScrollingX("溶解图UV流动U", Float) = 0

		_DisScrollingY("溶解图UV流动V", Float) = 0

		_DisValue("溶解值", Range(0, 1)) = 0

		_DisEdgeValue("溶解边缘", Range(0, 1)) = 0

		[HDR] _DisEdgeColor("溶解边缘颜色", Color) = (1,1,1,1)

		[Toggle] _DisEdgeSmooth("溶解边缘软度", Float) = 0

		
		[Space(30)]

		_MaskTex("遮罩图", 2D) = "white" {}

		_MaskTexSpeed_U("遮罩图UV流动速度U", Float) = 0

		_MaskTexSpeed_V("遮罩图UV流动速度V", Float) = 0
		


		[Space(30)]
		_VertexAnimateTex("顶点动画图",2D) = "black"{}
		_VertexScrollingX("顶点动画流动U", Float) = 0
		_VertexScrollingY("顶点动画流动Y", Float) = 0
		_VertexStrength("顶点动画强度", Float) = 0
		
		[Space(30)]

		_FresnelColor("菲尼尔颜色", Color) = (1, 1, 1, 1)

		_FresnelPower("菲尼尔宽度", Float) = 30

		_FresnelStrenght("菲尼尔强度", Float) = 0
		
    }
    SubShader {
        Tags {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
			"PreviewType" = "Plane"  "RenderPipeline" = "UniversalPipeline"
        }

		Blend[_SrcBlend][_DstBlend]
		Cull[_CullMode]
		
		ZWrite[_ZWrite]
		ZTest[_ZTest]

		//ZTest LEqual
        //ZWrite Off

        Pass {
            

			HLSLPROGRAM

			//Pragmas 
			#pragma vertex vert
			#pragma fragment frag 

			//Include
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
           
			
			TEXTURE2D(_MainTex);	
			SAMPLER(sampler_MainTex);
			
			TEXTURE2D(_MaskTex);	
			SAMPLER(sampler_MaskTex);

			TEXTURE2D(_TwistTex);
			SAMPLER(sampler_TwistTex);

			TEXTURE2D(_DisTex);
			SAMPLER(sampler_DisTex);

			TEXTURE2D(_VertexAnimateTex);
			SAMPLER(sampler_VertexAnimateTex);

			CBUFFER_START(UnityPerMaterial)

			uniform float4 _MainTex_ST;
			uniform float4 _MaskTex_ST;
			uniform float4 _TwistTex_ST;
			uniform float4 _DisTex_ST;

			uniform float4 _Color;

            uniform float _MainTexSpeed_U;
            uniform float _MainTexSpeed_V;
			uniform float _MainTexRotate;
			uniform float _MainTwistScale;

			uniform float _MaskTexSpeed_U;
            uniform float _MaskTexSpeed_V;

			uniform float4 _FresnelColor;
			uniform float _FresnelPower;
			uniform float _FresnelStrenght;

			uniform half _TwistScrollingX;
			uniform half _TwistScrollingY;

			uniform half _DisValue;
			uniform half _DisEdgeValue;
			uniform half4 _DisEdgeColor;
			uniform float _DisEdgeSmooth;
			uniform half _DisTwistScale;
			uniform half _DisScrollingX;
			uniform half _DisScrollingY;

			float4 _VertexAnimateTex_ST;
			half _VertexScrollingX;
			half _VertexScrollingY;
			half _VertexStrength;

			CBUFFER_END

		
			struct Attributes
			{
				float4 positionOS		:		POSITION;
				float3 normalOS 		:		NORMAL;
				float4 Color	 		: 		COLOR;
				float2 uv				:		TEXCOORD0;
				

			};
			struct Varyings
			{
				float4 positionCS		:		SV_POSITION;
				float4 Color			:		COLOR;
				float2 uv           	:       TEXCOORD0;
				float3 viewDirWS		:		TEXCOORD1;
				half3  normalWS			:		TEXCOORD2;
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT;

				//VertexPositionInputs posInput = GetVertexPositionInputs(IN.positionOS.xyz);
				VertexNormalInputs norInput = GetVertexNormalInputs(IN.normalOS);

				OUT.normalWS = norInput.normalWS;

				float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);

				float2 vertexAnimateTexUV = TRANSFORM_TEX(IN.uv, _VertexAnimateTex) + half2(_VertexScrollingX, _VertexScrollingY) * _Time.g;

				half4 vertexAnimateMask = SAMPLE_TEXTURE2D_LOD(_VertexAnimateTex, sampler_VertexAnimateTex, vertexAnimateTexUV, 0);

				positionWS += _VertexStrength *  vertexAnimateMask.r * OUT.normalWS;

				OUT.positionCS = TransformWorldToHClip(positionWS);

				OUT.uv = IN.uv;

				OUT.viewDirWS = GetCameraPositionWS() - positionWS;

				OUT.Color = IN.Color;

				

				return OUT;
			}

			void Unity_Rotate_Degrees_float(float2 UV, float2 Center, float Rotation, out float2 Out)
			{
				Rotation = Rotation * (3.1415926f / 180.0f);
				UV -= Center;
				float s = sin(Rotation);
				float c = cos(Rotation);
				float2x2 rMatrix = float2x2(c, -s, s, c);
				rMatrix *= 0.5;
				rMatrix += 0.5;
				rMatrix = rMatrix * 2 - 1;
				UV.xy = mul(UV.xy, rMatrix);
				UV += Center;
				Out = UV;
			}

			half4 frag(Varyings IN, half facing : VFACE) : SV_Target{
                
				float3 normalWS =  (facing > 0) ? normalize(IN.normalWS) : normalize(-IN.normalWS) ;

				float2 uv = IN.uv;

				float2 twistUvST = TRANSFORM_TEX(uv, _TwistTex) + half2(_TwistScrollingX, _TwistScrollingY) * _Time.g;

				half4 twistColor = SAMPLE_TEXTURE2D(_TwistTex, sampler_TwistTex, twistUvST);

				half twist = twistColor.a;


				float2 uvMainLoop = TRANSFORM_TEX(uv, _MainTex) + float2(_MainTexSpeed_U, _MainTexSpeed_V) * _Time.g + twist * _MainTwistScale;

				Unity_Rotate_Degrees_float(uvMainLoop, float2(0.5, 0.5), _MainTexRotate, uvMainLoop);

				
				float4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvMainLoop) * _Color * IN.Color;

				float2 uvMask = TRANSFORM_TEX(uv, _MaskTex) + float2(_MaskTexSpeed_U, _MaskTexSpeed_V) * _Time.g;

				float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvMask);

				half mask = maskTexColor.r;

				float2 disUvST = TRANSFORM_TEX(IN.uv, _DisTex) + half2(_DisScrollingX, _DisScrollingY) * _Time.g;

				half2 disUv = disUvST + twist * _DisTwistScale;

				half4 disTexColor = SAMPLE_TEXTURE2D(_DisTex, sampler_DisTex, disUv);

				half disMax = disTexColor.r;

				half disMin = saturate(disMax - _DisEdgeValue);

				half disFactorSmooth = 1 - smoothstep(disMin, disMax, _DisValue);

				half disFactorHard = ceil(disFactorSmooth);

				half disFactorMask = ceil(disFactorHard - disFactorSmooth);

				half disFactor = lerp(disFactorHard, disFactorSmooth, _DisEdgeSmooth);

				half alpha = mainTexColor.a * disFactor * mask;


				float4 finishColor = float4(mainTexColor.rgb, alpha);

				float ndv_inv = 1 - saturate(dot( normalize(IN.viewDirWS), normalWS ) );

				float3 fresnel = _FresnelColor.rgb * pow(ndv_inv, _FresnelPower) * _FresnelStrenght;

				half3 color = mainTexColor.rgb + fresnel.rgb;

				half3 colorSmooth = color;

				half3 colorHard = lerp(color.rgb, _DisEdgeColor.rgb, disFactorMask);

				half3 colorRes = lerp(colorHard, colorSmooth, _DisEdgeSmooth);

				return float4(colorRes, alpha);

            }
			ENDHLSL
        }
    }
	CustomEditor "FoldoutShaderGUI"
}