Shader "WB/UI/SpriteHSV"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)

        [Foldout] _HSVName("明亮度控制面板",Range(0,1)) = 0
        [FoldoutItem] _HSVHue("色调", Float) = 1
        [FoldoutItem] _HSVSat("饱和度", Float) = 0.8
        [FoldoutItem] _HSVValue("亮度", Float) = 1
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
        #include "../ColorCore.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


    CBUFFER_START(UnityPerMaterial)
        half4 _RendererColor;
        half2 _Flip;
        half4 _Color;

        half _HSVHue;
        half _HSVSat;
        half _HSVValue;
    CBUFFER_END
        sampler2D _MainTex;
        

        struct appdata_t
        {
            half4 vertex   : POSITION;
            half4 color    : COLOR;
            half2 texcoord : TEXCOORD0;
        };

        struct v2f
        {
            half4 vertex   : SV_POSITION;
            half4 color : COLOR;
            half2 texcoord : TEXCOORD0;
        };

        inline half4 UnityFlipSprite(in float3 pos, in half2 flip)
        {
            return half4(pos.xy * flip, pos.z, 1.0);
        }
        inline half4 UnityPixelSnap(float4 pos)
        {
            half2 hpc = _ScreenParams.xy * 0.5f;
            half2 pixelPos = round((pos.xy / pos.w) * hpc);
            pos.xy = pixelPos / hpc * pos.w;
            return pos;
        }
        v2f SpriteVert(appdata_t IN)
        {
            v2f OUT;

            OUT.vertex = UnityFlipSprite(IN.vertex.xyz, _Flip);
            OUT.vertex = TransformObjectToHClip(OUT.vertex.xyz);
            OUT.texcoord = IN.texcoord;
            OUT.color = IN.color * _Color * _RendererColor;

            OUT.vertex = UnityPixelSnap(OUT.vertex);

            return OUT;
        }

        half4 SampleSpriteTexture(float2 uv)
        {
            half4 color = tex2D(_MainTex, uv);
            return color;
        }
        half4 SpriteFrag(v2f IN) : SV_Target
        {
            half4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
            // hsv 处理
            half3 hvs = rgb2hsv(c.rgb);
            hvs.x = fmod(_HSVHue * 0.00277777785 + hvs.x, 1);
            hvs.yz *= half2(_HSVSat, _HSVValue);
            c.rgb = hsv2rgb(hvs);
            c.rgb *= c.a;
            return c;
        }
        ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}
