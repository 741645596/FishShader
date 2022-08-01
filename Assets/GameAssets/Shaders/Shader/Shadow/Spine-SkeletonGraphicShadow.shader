// This is a premultiply-alpha adaptation of the built-in Unity shader "UI/Default" in Unity 5.6.2 to allow Unity UI stencil masking.

Shader "Spine/SkeletonGraphicShadow"
{
	Properties {
		_Color ("Color and Alpha", Color) = (1,1,1,1)
		_ColorOverlay ("Color Overlay", Range(0, 1)) = 1
		[NoScaleOffset] _MainTex ("MainTex", 2D) = "black" {}
		_Cutoff("Depth alpha cutoff", Range(0,1)) = 0.1
		_ShadowAlphaCutoff("Shadow alpha cutoff", Range(0,1)) = 0.1
		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[HideInInspector] _StencilRef("Stencil Reference", Float) = 1.0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 
	}

	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off
		Lighting Off

		Stencil {
			Ref[_StencilRef]
			Comp[_StencilComp]
			Pass Keep
		}

		Pass {
			Name "Normal"

			ZWrite On
			ZTest Less

			CGPROGRAM
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			float4 _Color;
			float _ColorOverlay;
			fixed _Cutoff;

			struct VertexInput {
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 vertexColor : COLOR;
			};

			VertexOutput vert (VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.uv = v.uv;
				o.vertexColor = v.vertexColor;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				float4 rawColor = tex2D(_MainTex,i.uv);
				float4 baseColor = rawColor * i.vertexColor;
				clip(baseColor.a - _Cutoff);
				float finalAlpha = baseColor.a * _Color.a;

				#if defined(_STRAIGHT_ALPHA_INPUT)
				baseColor.rgb *= rawColor.a;
				#endif

				float3 finalColor = lerp(baseColor.rgb, _Color.rgb, _ColorOverlay) * _Color.a;
				return fixed4(finalColor, finalAlpha);
			}
			ENDCG
		}

		Pass {
			Name "Caster"
			Tags { "LightMode"="ShadowCaster" }
			Offset 1, 1
			ZWrite On
			ZTest LEqual

			Fog { Mode Off }
			Cull Off
			Lighting Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			sampler2D _MainTex;
			fixed _ShadowAlphaCutoff;

			struct VertexOutput {
				V2F_SHADOW_CASTER;
				float4 uvAndAlpha : TEXCOORD1;
			};

			VertexOutput vert (appdata_base v, float4 vertexColor : COLOR) {
				VertexOutput o;
				o.uvAndAlpha = v.texcoord;
				o.uvAndAlpha.a = vertexColor.a;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}

			float4 frag (VertexOutput i) : SV_Target {
				fixed4 texcol = tex2D(_MainTex, i.uvAndAlpha.xy);
				clip(texcol.a * i.uvAndAlpha.a - _ShadowAlphaCutoff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}

