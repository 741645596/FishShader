Shader "WB/SimpleBlur" {
    Properties{
        [NoScaleOffset] _MaskTex("Mask Texture", 2D) = "white" {}
        _BlurSize("BlurSize", Range(0, 3)) = 1
    }

    SubShader{
        Tags{"RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent+100"}
        Pass {
            Tags{"LightMode" = "UniversalForward"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord: TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 uvgrab : TEXCOORD1;
            };
            sampler2D _CameraColorAttachmentA;
            float4 _CameraColorAttachmentA_TexelSize;
            sampler2D _MaskTex;
            CBUFFER_START(UnityPerMaterial) 
            float _BlurSize;
            float4 _MaskTex_ST;
            CBUFFER_END

            v2f vert(appdata_t v) {
                v2f o;
                o.uv.xy = v.texcoord.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                #if UNITY_UV_STARTS_AT_TOP
                float scale = -1.0;
                #else
                float scale = 1.0;
                #endif
                o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                o.uvgrab.zw = o.vertex.zw;
                return o;
            }
            


            half4 frag(v2f i) : COLOR 
            {
                    half4 blurColor = half4(0,0,0,0);
                    #define GRABPIXEL(weight,kernelx) tex2Dproj( _CameraColorAttachmentA, UNITY_PROJ_COORD(float4(i.uvgrab.x + _CameraColorAttachmentA_TexelSize.x * kernelx*_BlurSize, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w))) * weight
                    blurColor += GRABPIXEL(0.05, -4.0);
                    blurColor += GRABPIXEL(0.09, -3.0);
                    blurColor += GRABPIXEL(0.12, -2.0);
                    blurColor += GRABPIXEL(0.12, -2.0);
                    blurColor += GRABPIXEL(0.15, -1.0);
                    blurColor += GRABPIXEL(0.18, 0.0);
                    blurColor += GRABPIXEL(0.15, +1.0);
                    blurColor += GRABPIXEL(0.12, +2.0);
                    blurColor += GRABPIXEL(0.09, +3.0);
                    blurColor += GRABPIXEL(0.05, +4.0);

                    half4 color = GRABPIXEL(1.0f, 0.0);

                    return lerp(color, blurColor, tex2D(_MaskTex, i.uv.xy).r);
            }
            ENDCG
        }
        Pass {
            Tags{"LightMode" = "UniversalForward"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord: TEXCOORD0;
            };

            struct v2f {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 uvgrab : TEXCOORD1;
            };
            sampler2D _CameraColorAttachmentA;
            float4 _CameraColorAttachmentA_TexelSize;
            sampler2D _MaskTex;
            CBUFFER_START(UnityPerMaterial)

            float _BlurSize;
            float4 _MaskTex_ST;
            CBUFFER_END

            v2f vert(appdata_t v) {
                v2f o;
                o.uv.xy = v.texcoord.xy;
                o.vertex = UnityObjectToClipPos(v.vertex);
                #if UNITY_UV_STARTS_AT_TOP
                float scale = -1.0;
                #else
                float scale = 1.0;
                #endif
                o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y * scale) + o.vertex.w) * 0.5;
                o.uvgrab.zw = o.vertex.zw;
                return o;
            }



            half4 frag(v2f i) : COLOR
            {
                    half4 blurColor = half4(0,0,0,0);
                    #define GRABPIXEL(weight,kernelx) tex2Dproj( _CameraColorAttachmentA, UNITY_PROJ_COORD(float4(i.uvgrab.x , i.uvgrab.y + _CameraColorAttachmentA_TexelSize.y * kernelx*_BlurSize, i.uvgrab.z, i.uvgrab.w))) * weight
                    blurColor += GRABPIXEL(0.05, -4.0);
                    blurColor += GRABPIXEL(0.09, -3.0);
                    blurColor += GRABPIXEL(0.12, -2.0);
                    blurColor += GRABPIXEL(0.15, -1.0);
                    blurColor += GRABPIXEL(0.18,  0.0);
                    blurColor += GRABPIXEL(0.15, +1.0);
                    blurColor += GRABPIXEL(0.12, +2.0);
                    blurColor += GRABPIXEL(0.09, +3.0);
                    blurColor += GRABPIXEL(0.05, +4.0);

                    half4 color = GRABPIXEL(1.0f, 0.0);
                    return lerp(color, blurColor, tex2D(_MaskTex, i.uv.xy).r);
            }
            ENDCG
        }
    }
}