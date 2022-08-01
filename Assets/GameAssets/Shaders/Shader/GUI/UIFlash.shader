Shader "fish/GUI/UIFlash"

{
	Properties 
	{
		//主纹理
		//[PerRendererData]_MainTex("Main Texture", 2D) = "white" {}
	     _MainTex("Main Texture", 2D) = "white" {}

		//流光纹理
		_FlashTex("Flash Texture",2D) = "white"{}
		//遮罩纹理
		_MaskTex("Mask Texture",2D) = "white"{}
		[Toggle]_IsNeedMask("Is Need Mask",Float) = 0
		//流光颜色
		_FlashColor("Flash Color",Color) = (1,1,1,1)
		//流光强度
		_FlashIntensity("Flash Intensity", Range(0, 1)) = 0.6
		//流光区域缩放
		_FlashScale("Flash Scale", Range(0.1, 1)) = 0.5
		//水平流动速度
		_FlashSpeedX("Flash Speed X", Range(-5, 5)) = 0.5
		//垂直流动速度
		_FlashSpeedY("Flash Speed Y", Range(-5, 5)) = 0
		//主纹理凸起值
		_RaisedValue("Raised Value", Range(-0.5, 0.5)) = -0.01
		//流光能见度
		_Visibility("Visibility", Range(0, 1)) = 1

			//MASK SUPPORT ADD
        _StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
			//MASK SUPPORT END

	}
	SubShader
	{
		Tags{ 	
				"Queue"="Transparent"
				"RenderType"="Transparent" 
				"CanUseSpriteAtlas"="True"
			}

			//MASK SUPPORT ADD
		Stencil
		{
			 Ref[_Stencil]
			 Comp[_StencilComp]
			//or Pass[_StencilOp]
			 ReadMask[_StencilReadMask]
			 WriteMask[_StencilWriteMask]
		 }
		 ColorMask[_ColorMask]
			//MASK SUPPORT END

		LOD 100
		
		Blend SrcAlpha OneMinusSrcAlpha

			Stencil {
			//orRef[_StencilRef]
			 //or	Comp[_StencilComp]
				  Pass Keep
			   }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		//	#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			//	UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _FlashTex;
			sampler2D _MaskTex;

			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _FlashTex_ST;
			fixed _IsNeedMask;
			fixed4 _FlashColor;
			fixed _FlashIntensity;
			fixed _FlashScale;
			fixed _FlashSpeedX;
			fixed _FlashSpeedY;
			fixed _RaisedValue;
			fixed _Visibility;
			CBUFFER_END

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = v.uv;
				//UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				//=====================计算流光贴图的uv=====================
				//缩放流光区域
				float2 flashUV = i.uv2*_FlashScale;
				//不断改变uv的x轴，让他往x轴方向移动
				flashUV.x += -_Time.y*_FlashSpeedX;
				//不断改变uv的y轴，让他往y轴方向移动
				flashUV.y += -_Time.y*_FlashSpeedY;

				//=====================计算流光贴图的可见区域=====================
				//取流光贴图的alpha值
				fixed flashAlpha = tex2D(_FlashTex, flashUV).a;
			//	return flashAlpha;

				//return (_IsNeedMask-1);
				//取遮罩贴图的alpha值
				fixed maskAlpha = 1;
				//if(_IsNeedMask > 0)
				//{
				//	maskAlpha = tex2D(_MaskTex, i.uv).a;
				//}
				maskAlpha=lerp(maskAlpha, tex2D(_MaskTex, i.uv).a, _IsNeedMask);

				//最终在主纹理上的可见值（flashAlpha和maskAlpha任意为0则该位置不可见）
				fixed visible = flashAlpha*maskAlpha*_FlashIntensity*_Visibility;

				//=====================计算主纹理的uv=====================
				//被流光贴图覆盖的区域凸起（uv的y值增加）
				float2 mainUV = i.uv;
				mainUV.y += visible*_RaisedValue;

				//=====================最终输出=====================
				//主纹理 + 可见的流光
				fixed4 A = tex2D(_MainTex, mainUV);

				fixed4 col = A + A.a*visible*_FlashColor;

				//UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

