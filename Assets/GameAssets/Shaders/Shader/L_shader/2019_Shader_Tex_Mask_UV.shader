Shader "2019_Shader/Tex_Mask_UV"
{
  Properties
  {
    [WrapMode] _MainTex1_Wrap ("Tex1 Wrap Mode", float) = 0
    _MainTex1 ("Tex1(RGB)", 2D) = "white" {}
    [WrapMode] _MainTex2_Wrap ("Tex2 Wrap Mode", float) = 0
    _MainTex2 ("Tex2(RGB)", 2D) = "white" {}
    _ScrollX ("Tex1 speed X", float) = 1
    _ScrollY ("Tex1 speed Y", float) = 0
    _Scroll2X ("Tex2 speed X", float) = 1
    _Scroll2Y ("Tex2 speed Y", float) = 0
    _Color ("Color", Color) = (1,1,1,1)
    _UVXX ("UVXX", Vector) = (0.3,1,1,1)
    _MMultiplier ("Layer Multiplier", float) = 2
    _SrcBlend ("SrcBlend", float) = 5
    _DestBlend ("DestBlend", float) = 10
  }
  SubShader
  {
    Tags
    { 
      "IGNOREPROJECTOR" = "true"
      "QUEUE" = "Transparent-1"
      "RenderType" = "Transparent"
    }
    LOD 500
    Pass // ind: 1, name: 
    {
      Tags
      { 
        "IGNOREPROJECTOR" = "true"
        "QUEUE" = "Transparent-1"
        "RenderType" = "Transparent"
      }
      LOD 500
      ZWrite Off
      Cull Off
      Blend Zero Zero
      ColorMask RGB
      // m_ProgramMask = 6
      CGPROGRAM
      //#pragma target 4.0
      
      #pragma vertex vert
      #pragma fragment frag
      
      #include "UnityCG.cginc"

      #define CODE_BLOCK_VERTEX
      //uniform float4 _Time;
      //uniform float4x4 unity_ObjectToWorld;
      //uniform float4x4 unity_MatrixVP;
      uniform float4 _MainTex1_ST;
      uniform float4 _MainTex2_ST;
      uniform float _MainTex1_Wrap;
      uniform float _ScrollX;
      uniform float _ScrollY;
      uniform float _Scroll2X;
      uniform float _Scroll2Y;
      uniform float _MMultiplier;
      uniform float4 _Color;
      uniform float _MainTex2_Wrap;
      uniform float4 _UVXX;
      uniform sampler2D _MainTex1;
      uniform sampler2D _MainTex2;
      struct appdata_t
      {
          float4 vertex :POSITION0;
          float4 texcoord :TEXCOORD0;
          float4 color :COLOR0;
      };
      
      struct OUT_Data_Vert
      {
          float4 texcoord :TEXCOORD0;
          float4 texcoord1 :TEXCOORD1;
          float4 vertex :SV_POSITION;
      };
      
      struct v2f
      {
          float4 texcoord :TEXCOORD0;
          float4 texcoord1 :TEXCOORD1;
      };
      
      struct OUT_Data_Frag
      {
          float4 color :SV_Target0;
      };
      
      float4 u_xlat0;
      float4 u_xlat1;
      OUT_Data_Vert vert(appdata_t in_v)
      {
          OUT_Data_Vert out_v;
          out_v.vertex = UnityObjectToClipPos(in_v.vertex);
          u_xlat0.xy = float2((_Time.xx * float2(_ScrollX, _ScrollY)));
          u_xlat0.zw = (_Time.xx * float2(_Scroll2X, _Scroll2Y));
          u_xlat0 = frac(u_xlat0);
          u_xlat1.xy = float2(TRANSFORM_TEX(in_v.texcoord.xy, _MainTex1));
          u_xlat1.zw = TRANSFORM_TEX(in_v.texcoord.xy, _MainTex2);
          u_xlat0 = (u_xlat0 + u_xlat1);
          u_xlat1.xy = float2(u_xlat0.xy);
          u_xlat1.xy = float2(clamp(u_xlat1.xy, 0, 1));
          u_xlat1.xy = float2(((-u_xlat0.xy) + u_xlat1.xy));
          out_v.texcoord.xy = float2(((float2(_MainTex1_Wrap, _MainTex1_Wrap) * u_xlat1.xy) + u_xlat0.xy));
          out_v.texcoord.zw = u_xlat0.zw;
          u_xlat0 = (float4(float4(_MMultiplier, _MMultiplier, _MMultiplier, _MMultiplier)) * _Color);
          u_xlat0 = (u_xlat0 * in_v.color);
          out_v.texcoord1 = u_xlat0;
          return out_v;
      }
      
      #define CODE_BLOCK_FRAGMENT
      float4 u_xlat16_0;
      float4 u_xlat10_0;
      float2 u_xlat1_d;
      float4 u_xlat10_1;
      float2 u_xlat5;
      OUT_Data_Frag frag(v2f in_f)
      {
          OUT_Data_Frag out_f;
          u_xlat10_0 = tex2D(_MainTex1, in_f.texcoord.xy);
          u_xlat1_d.xy = float2(((u_xlat10_0.xx * _UVXX.xx) + in_f.texcoord.zw));
          u_xlat5.xy = float2(u_xlat1_d.xy);
          u_xlat5.xy = float2(clamp(u_xlat5.xy, 0, 1));
          u_xlat5.xy = float2(((-u_xlat1_d.xy) + u_xlat5.xy));
          u_xlat1_d.xy = float2(((float2(float2(_MainTex2_Wrap, _MainTex2_Wrap)) * u_xlat5.xy) + u_xlat1_d.xy));
          u_xlat10_1 = tex2D(_MainTex2, u_xlat1_d.xy);
          u_xlat16_0 = (u_xlat10_0 * u_xlat10_1);
          out_f.color = (u_xlat16_0 * in_f.texcoord1);
          return out_f;
      }

      ENDCG
      
    } // end phase
  }
  FallBack Off
}
