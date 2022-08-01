Shader "Fish/UVFlow"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_R_ColoR("R_ColoR", Color) = (0.5019608,0.5019608,0.5019608,0.5)
		_R_Intensity("R_Intensity", float) = 1
		_Flow_Colpr("Flow_Colpr", Color) = (0.5019608,0.5019608,0.5019608,0.5)
		_Rate("Rate", float) = 2
		_G_UV_Tiling("G_UV_Tiling", Vector) = (1,1,0,0)
		_G_Uv_SpeeD("G_Uv_SpeeD", Vector) = (1,0,0,0)
		_Flow_Intensity("Flow_Intensity", float) = 1
		[HideInInspector] _texcoord("", 2D) = "white" {}
		[HideInInspector] __dirty("", float) = 1
	  
	}
		SubShader
	  {
		  Tags
		  {
			  "RenderPipeline" = "UniversalPipeline"
			  "IgnoreProjector" = "True"
			  "Queue" = "Transparent"
			  "RenderType" = "Transparent"
		  }

		  Pass
		  {
			  Name "ForwardLit"
			  Tags{"LightMode" = "UniversalForward"}


			  Blend SrcAlpha OneMinusSrcAlpha
			  ZWrite Off

		  //// Render State
		  Blend One One
		  Cull Back

		  ZTest LEqual
		  ZWrite Off

		  HLSLPROGRAM
		  #pragma vertex vert
		  #pragma fragment frag
		  #pragma prefer_hlslcc gles
		  #pragma exclude_renderers d3d11_9x
		  //#pragma target 2.0
		  #pragma target 3.0
		  #pragma multi_compile_instancing


		  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		  #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		  #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

		  CBUFFER_START(UnityPerMaterial)

		  float4 _R_ColoR;
		  float _R_Intensity;
		  float _Rate;
		  float4 _Texture0_ST;
		  float2 _G_UV_Tiling;
		  float2 _G_Uv_SpeeD;
		  float _Flow_Intensity;
		  float4 _Flow_Colpr;

		  float4 _MainTex_ST;

		  CBUFFER_END

		  TEXTURE2D(_MainTex);          SAMPLER(sampler_MainTex);

		  struct Attributes
		  {
			  float3 vertex : POSITION;
			  float2 texcoord0 :TEXCOORD0;
			  float3 normalOS : NORMAL;
		  };

		  struct Varyings
		  {
			  float4 positionCS : SV_POSITION;
			  float2 uv0 :TEXCOORD0;
		  };

		  half4 SampleTexture_RGBA(float2 uv, TEXTURE2D_PARAM(albedoAlphaMap, sampler_albedoAlphaMap))
		  {
			  return SAMPLE_TEXTURE2D(albedoAlphaMap, sampler_albedoAlphaMap, uv);
		  }


		  Varyings vert(Attributes input)
		  {
			  Varyings output;
			  output.uv0 = TRANSFORM_TEX(input.texcoord0, _MainTex);
			  output.positionCS = TransformObjectToHClip(input.vertex);
			  return output;
		  }


		  half4 frag(Varyings in_f) : SV_TARGET
		  {
			  float2 uv = in_f.uv0;
			  float3 u_xlat0_d;
			  float2 u_xlat1_d;
			  float2 u_xlat10_1;
			  float3 u_xlat2;
			  float3 u_xlat3;
			  float u_xlat6_d;
			  float u_xlat10_6;

			  u_xlat0_d.x = _Time.y * _Rate;
			  u_xlat0_d.x = sin(u_xlat0_d.x);
			  u_xlat0_d.x = u_xlat0_d.x * 0.5 + 0.5;
			  u_xlat2.xyz = _R_ColoR.xyz * _R_Intensity;
			  u_xlat0_d.xyz = (u_xlat0_d.xxx * u_xlat2.xyz);
			  u_xlat1_d.xy = _Time.y * _G_Uv_SpeeD.xy;
			  u_xlat1_d.xy = uv.xy * _G_UV_Tiling.xy + u_xlat1_d.xy;
			  float4 _MainTex_var = SampleTexture_RGBA(u_xlat1_d, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
			  u_xlat10_6 = _MainTex_var.y;
			  u_xlat1_d.xy = TRANSFORM_TEX(uv, _MainTex);
			  _MainTex_var = SampleTexture_RGBA(u_xlat1_d, TEXTURE2D_ARGS(_MainTex, sampler_MainTex));
			  u_xlat10_1.xy = _MainTex_var.xz;
			  u_xlat6_d = (u_xlat10_6 * u_xlat10_1.y);
			  u_xlat6_d = (u_xlat6_d * _Flow_Intensity);
			  u_xlat3.xyz = u_xlat6_d * _Flow_Colpr.xyz;
			  u_xlat0_d.xyz = ((u_xlat0_d.xyz * u_xlat10_1.xxx) + u_xlat3.xyz);
			  u_xlat0_d.xyz = clamp(u_xlat0_d.xyz, 0, 1);

			  float3 color = u_xlat0_d.xyz;
			  return float4(color, 1);
		  }

		  ENDHLSL
	  }
	  }
		  FallBack Off
}