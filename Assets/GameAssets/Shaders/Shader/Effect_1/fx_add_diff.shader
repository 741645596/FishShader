Shader "fish/fx/add_G"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}//   没用，避免报错 
        _wenli("wenli", 2D) = "white" {}
        [HideInInspector] _texcoord("", 2D) = "white" {}
        [HideInInspector]_Cutoff("Alpha cutoff", Range(0,1)) = 0.5

    }
        SubShader
        {
            Tags {
                "Queue" = "AlphaTest"
                "RenderType" = "TransparentCutout"
            }
            LOD 100

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                 #pragma multi_compile_fog

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    UNITY_FOG_COORDS(1)
                    float4 vertex : SV_POSITION;
                };

                sampler2D _wenli;
                float4 _wenli_ST;
                half4 _Color;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _wenli);
                    UNITY_TRANSFER_FOG(o,o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {

                    fixed4 wenli = tex2D(_wenli, i.uv);
                    clip(wenli.a - 0.5);
                    float3 Color = (_Color * wenli).rgb;
                    UNITY_APPLY_FOG(i.fogCoord, col);
                    return half4(Color,1);
                }
                ENDCG
            }
        }
}
