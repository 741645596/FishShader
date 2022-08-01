Shader "WB/Background"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("主贴图", 2D) = "white" {}
        _BaseColor("主颜色", Color) = (0.38742, 0.99166, 1.00, 0.00)
        _BaseFactor("_BaseFactor", float) = 1
        _NoiseColor("_NoiseColor", Color) = (0.00, 0.00, 0.00, 0.00)
        [NoScaleOffset] _Caustics("焦散贴图", 2D) = "white" {}
        _CausticTile("焦散缩放", float) = 0.8
        _Speed("焦散速度", float) = 0.20
        _Factor("焦散强度", float) = 4
        _Contrast("对比度", float) = 0.55
        _Desaturation("饱和度", Range(-1, 1)) = 0
        [NoScaleOffset] _MaskTex("遮罩贴图", 2D) = "white" {}

        _DisturbingSpeedX("扰动速度(X轴)", float) = 1
        _DisturbingSpeedY("扰动速度(Y轴)", float) = 1
        _DisturbingStrength("扰动强度", float) = 1

        _DarkRadiusStrength("径向压黑", Range(0, 5)) = 0
        _DarkRadiusMin("径向压黑区间", Range(0, 0.5)) = 0.3
        _DarkRadiusMax("径向压黑区间", Range(0.9, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+300" "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "ColorCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float  distanceCenter : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _Caustics;
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MaskTex_ST;


            float4 _Caustics_ST;

            float4 _BaseColor;
            float _BaseFactor;
            float4 _NoiseColor;

            float _CausticTile;
            float _Speed;
            float _Factor;
            float _Desaturation;
            float _Contrast;

            float _DisturbingSpeedX;
            float _DisturbingSpeedY;
            float _DisturbingStrength;

            float _DarkRadiusStrength;
            float _DarkRadiusMin, _DarkRadiusMax;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float2 diff = o.uv.xy - float2(0.5, 0.5);
                o.distanceCenter = sqrt(dot(diff, diff)) /(sqrt(2) * 0.5);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                 float2 original = _CausticTile * 1.5f * i.uv;  // 焦散的tiling
                 float2 offsetUV = _Time.y * _Speed * float2(0.1, 0.1); // uv offset
                 float originalCol = tex2D(_Caustics, original + offsetUV).r;   // 焦散1
                 float midResultCol = tex2D(_Caustics, original - offsetUV + float2(0.418f, 0.355f)).r; // 焦散2
                 // 自加的。
                 midResultCol = midResultCol* exp2(_Contrast)* _Factor - 1.0f; // 焦散2应用对比度
                 midResultCol = midResultCol * 0.25f; // 焦散2颜色 降到1/4
                 //
                // 扰动遮罩
                 float mask = tex2D(_MaskTex, -i.uv).r; // 遮罩

                 float2 disturbing_uv = _Time.y * float2(_DisturbingSpeedX, _DisturbingSpeedY) + i.uv;
                 float disturbing_noise = tex2D(_Caustics, disturbing_uv).g;
                 disturbing_uv = disturbing_noise * _DisturbingStrength * 0.01 * mask + i.uv;
                 // 采样mask 与mainTex
                 float3 mainColor = tex2D(_MainTex, disturbing_uv).rgb;
                 float maskR = tex2D(_MaskTex, disturbing_uv).r;
                 midResultCol = maskR * midResultCol + 1.0f;
                 // 自加的。
                 midResultCol = midResultCol * 0.85f;
                 // 变灰处理--> 去饱和度处理
                 float grayValue = dot(mainColor, float3(0.2999f, 0.587f, 0.114f));
                 float3 resultColor1 = -mainColor + float3(grayValue, grayValue, grayValue);
                 resultColor1 = _Desaturation * resultColor1 + mainColor;

                 float3 resultColor2 = resultColor1 * _BaseColor.rgb;
                 resultColor1 = resultColor1 * _NoiseColor.rgb;
                 resultColor2 = resultColor2 * _BaseFactor -resultColor1;
                 //
                 float darkStrength = 1.0f;
                 float RadiusDistance = 1 - i.distanceCenter;
                 darkStrength = pow(abs(RadiusDistance), _DarkRadiusStrength);
                 darkStrength = lerp(_DarkRadiusMin, _DarkRadiusMax, darkStrength);

                 return float4((midResultCol * resultColor2 + resultColor1) * darkStrength, 1.0f);
            }
            ENDHLSL
        }
    }
    SubShader
    {
        Tags { "Queue" = "Geometry+300" "RenderType" = "Opaque" }
        LOD 150

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "ColorCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                 float3 mainColor = tex2D(_MainTex, i.uv).rgb;
                 return float4(mainColor, 1.0f);
            }
            ENDHLSL
        }
    }
}
