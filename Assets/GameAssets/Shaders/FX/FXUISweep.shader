Shader "FX/FXUISweep" {
    Properties {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("BlendSource", Float) = 5
        [Enum(One, 1 , OneMinusSrcAlpha, 10 )] _DstBlend ("BlendDestination", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4

        _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _GlowScale("Glow Scale", float) = 1
        _AlphaScale("Alpha Scale", float) = 1

        _MainSpeed("MainTex Speed", Vector) = (0,0,0,0)
        [ToggleUI] _CustomUV("自定义主贴图UV偏移曲线TEXCOORD1.xy", Float) = 1.0

        [Space(20)]
        _MaskTex("Mask ( R Channel )", 2D) = "white" {}
        _MaskSpeed("MaskTex Speed", Vector) = (0,0,0,0)
        [HideFromInspector]_ClipRect ("Clip Rect", Vector) = (-1, -1, 0, 0)
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
            Name "ForwardLit"
            Blend[_SrcBlend][_DstBlend]
            Cull[_Cull]
            ZWrite[_ZWriteMode]
            Lighting Off
            ZTest [_ZTest]
            HLSLPROGRAM
            #include "../wb_Shader/ColorCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            float4 _BaseColor;
            float4 _MainSpeed;
            float _CustomUV;
            float4 _MaskTex_ST;
            float _GlowScale;
            float _AlphaScale;
            float4 _MaskSpeed;

            float4 _ClipRect;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);


            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);


            struct AttributesParticle
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoordCst : TEXCOORD1;
            };

            struct VaryingsParticle
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoordMask : TEXCOORD4;
                float3 CustData : TEXCOORD3;

                #ifdef UNITY_UI_CLIP_RECT
                float4  clipmask : TEXCOORD5;
                #endif
            };

            
            #include "clipRect.cginc"
            #include "../wb_Shader/ColorCore.hlsl"
            
            VaryingsParticle vertParticleUnlit(AttributesParticle input)
            {
                VaryingsParticle output = (VaryingsParticle)0;
                output.positionCS = TransformObjectToHClip(input.vertex.xyz);
                output.color = input.color;
                output.texcoord.xy = TRANSFORM_TEX(input.texCoord0, _BaseMap);

                output.CustData.xy = _CustomUV ? input.texCoordCst.xy : float2(0, 0);
                output.CustData.z = input.texCoordCst.z;

                output.texcoordMask = TRANSFORM_TEX(input.texCoord0, _MaskTex);

                PASS_CLIP_MASK(output);
                
                return output;
            }


            float4 fragParticleUnlit(VaryingsParticle In) : SV_Target
            {
                float t = abs(frac(_Time.y * 0.01));
                float calcTime = t * 100;

                float4 vertColor = In.color;
                // vertColor = Gamma22(vertColor);
                float2 uvMain = In.texcoord.xy;
                uvMain += _MainSpeed.xy * calcTime;
                float2 pivot = 0.5; //_UVRotate.xy;
                roateUV(_MainSpeed.zw,calcTime, pivot, uvMain);
                uvMain += In.CustData.xy;

                float4 mainTexColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvMain);
                // mainTexColor = Gamma22(mainTexColor);
                // _BaseColor = Gamma22(_BaseColor);
                float2 uvMask = In.texcoordMask;
                uvMask.xy += _MaskSpeed.xy * calcTime;
                roateUV(_MaskSpeed.zw, calcTime, pivot, uvMask);

                float4 color = mainTexColor * _BaseColor;
                color *= vertColor;
                color.rgb *= _GlowScale;
		

                float4 maskTexColor = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uvMask);
                color.a = saturate(color.a * _AlphaScale);
                color.a = saturate(color.a * maskTexColor.r);
                //color.a = color.a * step(0.03, color.a);
                
                DO_CLIP_RECT_FRAG(color, In);
                // color = Gamma045(color);
                return color;
            }
            #pragma vertex vertParticleUnlit
            #pragma fragment fragParticleUnlit
            ENDHLSL
        }
    }
}