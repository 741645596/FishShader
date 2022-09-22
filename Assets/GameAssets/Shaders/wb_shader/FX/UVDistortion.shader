Shader "WB/UVDistortion" {
    // 纹理扰动效果，扰动数值，扰动的速度以及方向
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(One, 1 , OneMinusSrcAlpha, 10 )] _DstBlend ("BlendDestination", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1
        _MainSpeed("MainTex Speed", Vector) = (0,0,0,0)

        _UVNoiseTex("NoiseTex", 2D) = "black" {}
        _UVDistortion("UVDistortion", Float) = 0.5
        _NoiseScroll("NoiseScroll", Vector) = (0,-0.1,1,1)
    }

    SubShader {
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "PerformanceChecks" = "False"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass {
            Name "AddBlend"
            Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWriteMode]
            Lighting Off
            ZTest [_ZTest]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            half4 _BaseMap_ST;
            half4 _BaseColor;
            half4 _MainSpeed;
            half _GlowScale;
            half _AlphaScale;
            half4 _UVNoiseTex_ST;
            half _UVDistortion;
            half4 _NoiseScroll;
            CBUFFER_END
            sampler2D _UVNoiseTex;
            sampler2D _BaseMap;

            struct Attributes
            {
                half3 vertex : POSITION;
                half4 color : COLOR;
                half2 texcoord :TEXCOORD0;
            };

            struct Varyings
            {
                half4 positionCS : SV_POSITION;
                half4 color : COLOR;
                half2 texcoord :TEXCOORD0;
                half2 texcoordNoise: TEXCOORD1;
            };

            void roateUV(half2 _UVRotate, half2 pivot, inout half2 uv)
            {
                half cosAngle = cos(_UVRotate.x + _Time.y * _UVRotate.y);
                half sinAngle = sin(_UVRotate.x + _Time.y * _UVRotate.y);
                half2x2 roation = half2x2(cosAngle, -sinAngle, sinAngle, cosAngle);
                uv.xy = mul(roation, uv.xy -= pivot) + pivot;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.texcoord = TRANSFORM_TEX(input.texcoord, _BaseMap);
                output.texcoordNoise = TRANSFORM_TEX(input.texcoord, _UVNoiseTex);
                output.positionCS = TransformObjectToHClip(input.vertex);
                output.color = input.color;
                return output;
            }


            half4 frag(Varyings input) : SV_TARGET
            {
                half2 uvMain = input.texcoord;

                uvMain += _MainSpeed.xy * _Time.y;
                half2 pivot = 0.5; //_UVRotate.xy;
                roateUV(_MainSpeed.zw, pivot, uvMain);

                half2 noiseScrollXY = _NoiseScroll.xy;
                input.texcoordNoise.xy += half2(_Time.g * noiseScrollXY);
                float2 noiseMask = tex2D(_UVNoiseTex, input.texcoordNoise.xy).xy * _UVDistortion * _NoiseScroll.zw;
                uvMain.xy += noiseMask;

                half4 baseCol = tex2D(_BaseMap, uvMain);
                half4 col = input.color * baseCol * _BaseColor;
                col.rgb *= _GlowScale;
                col.a = saturate(col.a) * _AlphaScale;
                return col;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}