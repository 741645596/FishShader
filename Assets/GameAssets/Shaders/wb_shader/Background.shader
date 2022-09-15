Shader "WB/Background"
{
    Properties
    {
        [Foldout] _BassName("Bass",Range(0,1)) = 0
        [FoldoutItem] [NoScaleOffset]_MainTex ("主贴图", 2D) = "white" {}
        [FoldoutItem] _BaseColor("主颜色", Color) = (0.38742, 0.99166, 1.00, 0.00)
        [FoldoutItem] _BaseFactor("_BaseFactor", float) = 1
        [FoldoutItem] _NoiseColor("_NoiseColor", Color) = (0.00, 0.00, 0.00, 0.00)
        [FoldoutItem][NoScaleOffset] _Caustics("焦散贴图", 2D) = "white" {}
        [FoldoutItem] _CausticTile("焦散缩放", float) = 0.8
        [FoldoutItem] _Speed("焦散速度", float) = 0.20
        [FoldoutItem] _Factor("焦散强度", float) = 4
        [FoldoutItem] _Contrast("对比度", float) = 0.55
        [FoldoutItem] _Desaturation("饱和度", Range(-1, 1)) = 0
        [FoldoutItem][NoScaleOffset] _MaskTex("遮罩贴图", 2D) = "white" {}

        [Foldout] _DarkName("径向亚黑控制面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _DarkControl("灯光方向控制开关", Float) = 0.0
        [FoldoutItem][NoScaleOffset]_DrakMaskTex("亚黑遮罩贴图", 2D) = "white" {}
        [FoldoutItem] _DarkRadius("DarkRadius", Range(0.01, 10.0)) = 1
        [FoldoutItem] _DarkRimAlphaStrength("_DarkRimAlphaStrength", Range(1, 5)) = 1
        [FoldoutItem] _DarkRimBaseColor("_DarkRimBaseColor", Color) = (0.00, 0.00, 0.00, 0.00)
        //[FoldoutItem] _xxStrength("_xxStrength", Range(0.5, 5)) = 1

        [Foldout] _BossOutName("boss 出场控制面板",Range(0,1)) = 0
        [FoldoutItem][Toggle] _BossOut("Boss善良登场控制开关", Float) = 0.0
        [FoldoutItem] _Radius("Radius", Float) = 1
        [FoldoutItem] _Radius2("Radius2", Float) = 1
        [FoldoutItem] _Position("Position", Vector) = (0,0,0,0)
        [FoldoutItem] _Position2("Position2", Vector) = (0,0,0,0)
        [FoldoutItem] _RimAlpha("RimAlpha", Float) = 0
        [FoldoutItem] _MaskFactor("MaskFactor", Float) = 1
    }
    
    SubShader
    {
        Tags { "Queue" = "Geometry+300" "RenderType"="Opaque" }
        LOD 300

        Pass
        {
            HLSLPROGRAM

            #pragma multi_compile __ _DARKCONTROL_ON
            #pragma multi_compile __ _BOSSOUT_ON

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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            sampler2D _DrakMaskTex;

            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _MaskTex_ST;
            float4 _DrakMaskTex_ST;

            sampler2D _Caustics;
            float4 _Caustics_ST;

            float4 _BaseColor;
            float _BaseFactor;
            float4 _NoiseColor;

            float _CausticTile;
            float _Speed;
            float _Factor;
            float _Desaturation;
            float _Contrast;

            float _DarkRadius;
            float _DarkRimAlphaStrength;
            float3 _DarkRimBaseColor;

            float3 _Position;
            float3 _Position2;
            float _Radius;
            float _Radius2;
            float _RimAlpha;
            float _MaskFactor;

            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.x, 0, v.vertex.z, 1.0)).xyz;
                o.uv = -TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                 float2 original = _CausticTile * float2(1.5, 1.0) * i.uv;  // 焦散的tiling
                 float2 offsetUV = _Time.y * _Speed * float2(0.1, 0.1); // uv offset
                 float col1 = tex2D(_Caustics, original + offsetUV).r;   // 焦散1
                 float col2 = tex2D(_Caustics, original - offsetUV + float2(0.418f, 0.355f)).r; // 焦散2
                 float minCol = min(col1, col2);

                 float2 mainUV = _Time.y * float2(0.05, 0.0) + i.uv;
                 mainUV = tex2D(_Caustics, mainUV).g * 0.006 - i.uv;
                 float maskValue = tex2D(_MaskTex, mainUV).r;

                 float brightnessStrength = lerp(1,pow(abs(minCol), _Contrast) * _Factor, maskValue) ;
                 float3 mainColor = tex2D(_MainTex, mainUV).rgb;
                 float grayValue = dot(mainColor, float3(0.2999f, 0.587f, 0.114f));
                 float3 diffColor = lerp(mainColor, float3(grayValue, grayValue, grayValue), _Desaturation);
                 float3 result = lerp(_NoiseColor, _BaseColor * _BaseFactor, brightnessStrength).rgb * diffColor;
                 //
                 float disFactor = 1.0;
#if _BOSSOUT_ON
                 float3 p1 = mul(unity_ObjectToWorld, float4(_Position.x, 0, _Position.y, 1.0)).xyz;
                 float dis1 = length(i.worldPos - p1) / _Radius;
                 dis1 = saturate(dis1);
                 float3 p2 = mul(unity_ObjectToWorld, float4(_Position2.x, 0, _Position2.y, 1.0)).xyz;
                 float dis2 = length(i.worldPos - p2) / _Radius2;
                 float dis = min(dis1, dis2);
                 dis = pow(abs(dis), _RimAlpha);
                 disFactor = lerp(dis, 1, _MaskFactor);

                 
#endif 
                 float3 darkStrength = float3(1.0f,1.0f,1.0f);
#if _DARKCONTROL_ON
                 float2 uv = (-i.uv) - float2(0.5f, 0.5f);
                 uv = uv / _DarkRadius + float2(0.5f, 0.5f);
                 float dr = tex2D(_DrakMaskTex, uv).r;
                 float3 addAlpha = pow(abs(1 - dr), 0.75f) * _DarkRimBaseColor.rgb;
                 darkStrength = dr * _DarkRimAlphaStrength * darkStrength + addAlpha;
#endif
                 return float4(result * disFactor * darkStrength, 1.0f);
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
                o.uv = -TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 mainColor = tex2D(_MainTex, -i.uv).rgb;
                 return float4(mainColor,1.0f);
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}
