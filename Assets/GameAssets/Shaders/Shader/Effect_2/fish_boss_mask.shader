Shader "fish/fx/fish_boss_mask"
{
	//属性  fish_boss_mask_rotate
	Properties
	{

		_MainTex("Base 2D", 2D) = "white"{}

		_MainTex_a_intensity("遮罩alpha的强度", Range(0,5)) = 1
		_Dark("变暗", Range(0,1)) = 0

	    _Scale("缩放遮罩", Range(1,20)) = 1

		Fish_X("Fish_X", Range(-10,10)) = 0
		Fish_Z("Fish_Z", Range(-10,10)) = 0

		_TESTROAT("Fish_Rotation_Y", Range(-360,360)) = 0

	}

	SubShader
	{
		Pass
		{

			Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
			
			Blend SrcAlpha OneMinusSrcAlpha
			ZTest Off
			ZWrite Off

			HLSLPROGRAM

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

			sampler2D _MainTex;

			CBUFFER_START(UnityPerMaterial)
			
			half4 _Diffuse;

			float4 _MainTex_ST;
	
			half _Dark;
			half _Scale;
			half Fish_X;
			half Fish_Z;

			half _TESTROAT;
			half _MainTex_a_intensity;
			CBUFFER_END

			struct a2v
			{
				float4 vertex : POSITION;
				float4 texcoord : TEXCOORD0;

			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD1;

			};

			v2f vert(a2v v)
			{

				v2f o;

				float3 worldpos = TransformObjectToWorld(v.vertex.xyz);

				o.uv.xy = float2(worldpos.x - Fish_X, worldpos.z - Fish_Z);

				_TESTROAT=0- _TESTROAT;
				_TESTROAT = _TESTROAT * 0.017453;

				float cos6 = cos(_TESTROAT);
				float sin6 = sin(_TESTROAT);

			    o.uv.xy = mul(o.uv.xy , float2x2(cos6, -sin6, sin6, cos6)) ;

				o.uv.xy = float2(o.uv.x + 0.5 , o.uv.y + 0.5)* (1 / _Scale) + float2(0.5 / _Scale, 0.5 / _Scale)*(_Scale - 1) ;

				//o.uv.xy = saturate(o.uv.xy);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

				o.pos = vertexInput.positionCS;

				return o;

			}

			half4 frag(v2f i) : SV_Target
			{

				half a = tex2D(_MainTex, i.uv).r;
			    
			//_MainTex_a_intensity
			    a = a* _MainTex_a_intensity;

			    a = saturate( _Dark- a);
				a = smoothstep(0, 1, a);
				//a = a < 0.2 ? 0 : a;
				return half4(0, 0, 0, a);

			}

			#pragma vertex vert
			#pragma fragment frag	

				ENDHLSL

		}
	}

}
