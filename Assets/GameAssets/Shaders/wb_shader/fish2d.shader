Shader "WB/fish2d"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        // 先屏蔽，序列帧受击变红走修改color 属性
        /*
        [Foldout] _HurtName("Hurt plane",Range(0,1)) = 0
        [FoldoutItem][KeywordEnum(Rim,Albedo)] _HitColorChannel("HitColorType", Float) = 1.0
        [FoldoutItem][Toggle] _TestHitColor("美术测试受击变红颜色开关", Float) = 0.0
        [FoldoutItem] _HitColor("HitColor[美术控制]", Color) = (1,1,1,1)
        [FoldoutItem] _HitMultiple("HitMultiple[美术控制]", Range(0,1)) = 1
        [FoldoutItem] _HitRimPower("HitRim Power[美术控制]", Range(0.01, 10)) = 0.01
        [FoldoutItem] _HitRimSpread("Hit Rim Spread[美术控制]", Range(-15, 4.99)) = 0.01
        [FoldoutItem] _OverlayColor("_OverlayColor[程序控制]", Color) = (1,1,1,1)
        [FoldoutItem] _OverlayMultiple("_OverlayMultiple[程序控制]", Range(0,1)) = 1
        */
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        HLSLPROGRAM
        #pragma vertex SpriteVert
        #pragma fragment SpriteFrag
        #pragma target 2.0
        #pragma multi_compile_local _ PIXELSNAP_ON
        #include "ColorCore.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        //#pragma multi_compile _HITCOLORCHANNEL_RIM _HITCOLORCHANNEL_ALBEDO
        //#pragma multi_compile __ _TESTHITCOLOR_ON


    CBUFFER_START(UnityPerMaterial)
        half4 _RendererColor;
        half2 _Flip;
        half4 _Color;
       //#include "HitRed_dec.hlsl"
    CBUFFER_END
        
        sampler2D _MainTex;
        

        struct appdata_t
        {
            float4 vertex   : POSITION;
            float4 color    : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex   : SV_POSITION;
            half4 color : COLOR;
            float2 texcoord : TEXCOORD0;
        };

        inline float4 UnityFlipSprite(in float3 pos, in half2 flip)
        {
            return float4(pos.xy * flip, pos.z, 1.0);
        }

        v2f SpriteVert(appdata_t IN)
        {
            v2f OUT;

            OUT.vertex = UnityFlipSprite(IN.vertex.xyz, _Flip);
            OUT.vertex = TransformObjectToHClip(OUT.vertex.xyz);
            OUT.texcoord = IN.texcoord;
            OUT.color = IN.color * _Color * _RendererColor;

            #ifdef PIXELSNAP_ON
            OUT.vertex = UnityPixelSnap(OUT.vertex);
            #endif

            return OUT;
        }



        half4 SampleSpriteTexture(float2 uv)
        {
            half4 color = tex2D(_MainTex, uv);
            return color;
        }
        //#include "HitRed_fun.hlsl"
        half4 SpriteFrag(v2f IN) : SV_Target
        {
            half4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
            //c.rgb = HitRed(c.rgb, float3(0,0,0), float3(1, 0, 0), float3(1, 0, 0));
            c.rgb *= c.a;
            return c;
        }
        ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}
