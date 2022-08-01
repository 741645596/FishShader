Shader "WB/gaussBlur"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
    }
    SubShader
    { 
        ZTest Always
        Cull Off
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_blur  
            #pragma fragment frag_blur  
            #include "UnityCG.cginc"  
            struct VertexOutput
            {
                float4 pos : SV_POSITION;   
                float2 uv  : TEXCOORD0;      
                float4 uv01 : TEXCOORD1;    
                float4 uv23 : TEXCOORD2;  
            };
            sampler2D _MainTex;
            float4 _MainTex_TexelSize; 
            float4 _offsets;

            VertexOutput vert_blur(appdata_img v)
            {
                VertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex);  
                o.uv = v.texcoord.xy;
                _offsets *= _MainTex_TexelSize.xyxy;
                o.uv01 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1);
                o.uv23 = v.texcoord.xyxy + _offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
                return o;
            }
            fixed4 frag_blur(VertexOutput i) : SV_Target
            {
                fixed4 color = fixed4(0,0,0,0);
                color += 0.4026 * tex2D(_MainTex, i.uv);
                color += 0.2442 * tex2D(_MainTex, i.uv01.xy);
                color += 0.2442 * tex2D(_MainTex, i.uv01.zw);
                color += 0.0545 * tex2D(_MainTex, i.uv23.xy);
                color += 0.0545 * tex2D(_MainTex, i.uv23.zw);
                return color;
            }
            ENDCG
        }
    }  
}

