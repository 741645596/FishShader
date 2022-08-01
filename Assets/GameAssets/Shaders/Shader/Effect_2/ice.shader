Shader "fish/fx/ice"
{
	Properties
	{
		//_Color("Color(RGB)",Color) = (1,1,1,1)

		_Color("Color", Color) = (1,1,1,1)
		_Shininess("Shininess", Range(0.1, 10)) = 1
		_ReflectColor("ReflectColor", Color) = (1,1,1,0.5)
		_MainTex("MainTex", 2D) = "white" {}
		_Refraction("Refraction", 2D) = "bump" {}
		_Cube("Cube", Cube) = "_Skybox" {}
		_RefStrength("RefStrength", Range(0, 20)) = 1
		_LightStrength("LightStrength", Range(0, 20)) = 0.1
		_FrenelPower("FrenelPower", Float) = 0.1
		_TexAlphaAdd("TexAlphaAdd", Float) = 0.1
			// _RefractionStrength ("RefractionStrength", Range(-10, 10)) = 0  
		//_Foce("Foce", Range(-10, 10)) = 1
		_Cutoff("Cutoff", Range(0, 1)) = 1

	}
		SubShader
	{
		Tags
		{

		//"RenderPipeline" = "UniversalPipeline"
		//"RenderType" = "Opaque"
		//"Queue" = "Geometry+0"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
			"Queue" = "Transparent+1"
			"RenderType" = "Transparent"

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
			ZWrite Off

		//// Render State
		//Blend One Zero, One Zero
		//Cull Back

		//ZTest LEqual
		//ZWrite On

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

		sampler2D _MainTex;
		sampler2D _Refraction;
		samplerCUBE _Cube;
		CBUFFER_START(UnityPerMaterial)

		half4 _MainTex_ST;
		half4 _Refraction_ST; 
		half4 _Color;

		half _Shininess;
		half4 _ReflectColor;

		half _RefStrength;
		half _LightStrength;
		half _FrenelPower;
		half _TexAlphaAdd;
		half _Cutoff;
	//	half _Foce;

		CBUFFER_END

			struct Attributes
			{
				float3 vertex : POSITION;
				float2 texcoord0 :TEXCOORD0;

				float3 normal : NORMAL;
				float4 tangent : TANGENT;

			};

			struct Varyings
			{
				float4 pos : SV_POSITION;
				float2 uv0 :TEXCOORD0;

				///////float4 pos : SV_POSITION;
				///////float2 uv0 : TEXCOORD0;
				float4 posWorld : TEXCOORD1;
				float3 normalDir : TEXCOORD2;
				float3 tangentDir : TEXCOORD3;
				float3 bitangentDir : TEXCOORD4;
				//float4 screenPos : TEXCOORD5;

			};

			Varyings vert(Attributes v)
			{
				Varyings o = (Varyings)0;

				//////////VertexOutput o = (VertexOutput)0;
				//////////o.uv0 = v.texcoord0;

				//////o.pos = UnityObjectToClipPos(v.vertex);
				/////////o.screenPos = o.pos;

				o.uv0 = TRANSFORM_TEX(v.texcoord0,_MainTex);

				o.normalDir = TransformObjectToWorldNormal(v.normal);  //CG  UnityObjectToWorldNormal   HLSL TransformObjectToWorldNormal
			   // o.tangentDir = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
				o.tangentDir =  TransformObjectToWorldDir(v.tangent.xyz);

			   /// o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
				o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
			   ///  o.posWorld = mul(unity_ObjectToWorld, v.vertex);        //   HLSL  TransformObjectToWorld

				o.posWorld.xyz  = TransformObjectToWorld(v.vertex.xyz);

				//  float3 lightColor = _LightColor0.rgb;

				o.pos = TransformObjectToHClip(v.vertex);

				return o;
			}

			half4 frag(Varyings i) : SV_TARGET
			{

				float3x3 tangentTransform = float3x3(i.tangentDir, i.bitangentDir, i.normalDir);
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);

				float3 _Refraction_var = UnpackNormal(tex2D(_Refraction,TRANSFORM_TEX(i.uv0, _Refraction)));
				float3 normalLocal = _Refraction_var.rgb;

				float3 normalDirection = normalize(mul(normalLocal, tangentTransform)); // Perturbed normals
				float3 viewReflectDirection = reflect(-viewDirection, normalDirection);

				float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));

				clip((_MainTex_var.a + (2 * _Cutoff - 1)) - 0.5);

				float3 lightDirection = normalize(_MainLightPosition.xyz);
				float3 lightColor = _MainLightColor.rgb;
				float3 halfDirection = normalize(viewDirection + lightDirection);

				half fresnelTerm = Pow4(1.0 - saturate(dot(i.normalDir, lightDirection)- 0.5)) * 2;

				////// Lighting:
				float attenuation = 1;
				float3 attenColor = attenuation * _MainLightColor.rgb;

				///////// Gloss:
				float gloss = _MainTex_var.a;
				float specPow = exp2(gloss * 10.0 + 1.0);

				////// Specular:
				float NdotL = saturate(dot(normalDirection, lightDirection));
				float3 specularColor = float3(_Shininess, _Shininess, _Shininess);
				float3 directSpecular = attenColor * pow(max(0, dot(halfDirection, normalDirection)), specPow)*specularColor;
				float3 specular = directSpecular;

				/////// Diffuse:
				NdotL = max(0.0, dot(normalDirection, lightDirection));
				float3 directDiffuse = pow(max(0.0, NdotL), _MainTex_var.rgb) * attenColor;
				float3 diffuseColor = _Color.rgb;
				float3 diffuse = directDiffuse * diffuseColor;

				//diffuse.x = 0;
				//return float4(diffuse,1);

				//diffuse=0;
				////// Emissive:
				float4 _Cube_var = texCUBE(_Cube, viewReflectDirection);
				float3 emissive = ((_MainTex_var.rgb + (lerp((_MainTex_var.rgb*_Color.rgb), (_Cube_var.rgb*_Cube_var.a), pow(_FrenelPower, lerp(-1, 0, pow(1.0 - max(0, dot(normalDirection, viewDirection)), 0.5*dot(1.0, _Refraction_var.rgb) + 0.5))))*_LightStrength*saturate(_MainTex_var.a * 2)  *_RefStrength))*_ReflectColor.rgb);
				/// Final Color:
				//float3 finalColor = diffuse + specular + emissive;
				emissive = lerp(emissive*0.2, emissive, fresnelTerm );
				float3 finalColor = saturate(diffuse + specular + emissive);
			// 	finalColor *= lerp(0, finalColor,fresnelTerm* _Foce);
				float alpha = lerp(0, (_MainTex_var.a + _TexAlphaAdd), fresnelTerm);
				return   half4(finalColor, alpha);

				//half4 mainTex = tex2D(_MainTex, i.uv0);
				//half4 c = _Color * mainTex;
				//return c;
			}

			ENDHLSL
		}
	}
	FallBack "Diffuse"
		//FallBack "Hidden/Shader Graph/FallbackError"
}