Shader "Unlit/OutLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                //float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 vs_TEXCOORD3:TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _MaskTex;
            float4 _MaskTex_ST;
            sampler2D _StreamTex;
            float4 _StreamTex_ST;

            uniform 	float _Alpha;
            uniform 	float4 _Color;
            uniform 	float _EdgeColorFactor;
            uniform 	float4 _EdgeColor;
            uniform 	float _EdgeWidth;
            uniform 	float _SecondEdgeColorFactor;
            uniform 	float4 _SecondEdgeColor;
            uniform 	float _SecondEdgeWidth;
            uniform 	float _StreamColorFactor;
            uniform 	float4 _StreamColor;
            uniform 	float _StreamWidth;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG



            in highp vec2 vs_TEXCOORD0;
            in highp vec2 vs_TEXCOORD1;
            in highp vec3 vs_TEXCOORD2;
            in highp vec3 vs_TEXCOORD3;
            layout(location = 0) out highp vec4 SV_Target0;
            vec4 u_xlat0;
            vec4 u_xlat1;
            vec3 u_xlat2;
            vec2 u_xlat3;
            mediump vec3 u_xlat16_3;
            vec3 u_xlat4;
            vec3 u_xlat5;
            float u_xlat12;
            void main()
            {
                u_xlat0.x = dot(vs_TEXCOORD3.xyz, vs_TEXCOORD3.xyz);
                u_xlat0.x = inversesqrt(u_xlat0.x);
                u_xlat0.xyz = u_xlat0.xxx * vs_TEXCOORD3.xyz;
                u_xlat12 = dot(vs_TEXCOORD2.xyz, vs_TEXCOORD2.xyz);
                u_xlat12 = inversesqrt(u_xlat12);
                u_xlat1.xyz = vec3(u_xlat12) * vs_TEXCOORD2.xyz;
                u_xlat0.x = dot(u_xlat0.xyz, u_xlat1.xyz);
                u_xlat0.x = max(u_xlat0.x, 0.0);
                u_xlat0.x = min(u_xlat0.x, 0.999000013);
                u_xlat0.x = (-u_xlat0.x) + 1.0;
                u_xlat0.x = log2(u_xlat0.x);
                u_xlat4.x = u_xlat0.x * (-_EdgeWidth);
                u_xlat4.x = exp2(u_xlat4.x);
                u_xlat4.xyz = u_xlat4.xxx * _EdgeColor.xyz;
                u_xlat4.xyz = u_xlat4.xyz * vec3(_EdgeColorFactor);
                u_xlat1.x = u_xlat0.x * (-_StreamWidth);
                u_xlat0.x = u_xlat0.x * (-_SecondEdgeWidth);
                u_xlat0.x = exp2(u_xlat0.x);
                u_xlat5.xyz = u_xlat0.xxx * _SecondEdgeColor.xyz;
                u_xlat0.x = exp2(u_xlat1.x);
                u_xlat2.xyz = u_xlat0.xxx * _StreamColor.xyz;
                u_xlat16_3.xyz = texture(_MaskTex, vs_TEXCOORD1.xy).xyz;
                u_xlat3.xy = u_xlat16_3.xy + vs_TEXCOORD0.xy;
                u_xlat0.x = u_xlat16_3.z * _Color.w;
                u_xlat0.x = u_xlat0.x * _Alpha;
                u_xlat16_3.xyz = texture(_StreamTex, u_xlat3.xy).xyz;
                u_xlat2.xyz = u_xlat2.xyz * u_xlat16_3.xyz;
                u_xlat4.xyz = u_xlat2.xyz * vec3(vec3(_StreamColorFactor, _StreamColorFactor, _StreamColorFactor)) + u_xlat4.xyz;
                u_xlat4.xyz = u_xlat5.xyz * vec3(vec3(_SecondEdgeColorFactor, _SecondEdgeColorFactor, _SecondEdgeColorFactor)) + u_xlat4.xyz;
                u_xlat1.xyz = u_xlat4.xyz * _Color.xyz;
                u_xlat1.w = 1.0;
                u_xlat0 = u_xlat0.xxxx * u_xlat1;
                SV_Target0.w = u_xlat0.w;
#ifdef UNITY_ADRENO_ES3
                SV_Target0.w = min(max(SV_Target0.w, 0.0), 1.0);
#else
                SV_Target0.w = clamp(SV_Target0.w, 0.0, 1.0);
#endif
                SV_Target0.xyz = u_xlat0.xyz;
                return;
            }
        }
    }
}
