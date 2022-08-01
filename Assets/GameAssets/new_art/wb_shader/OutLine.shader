Shader "WB/OutLine"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("CullMode", float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _SourceBlend("Source Blend Mode", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DestBlend("Dest Blend Mode", Float) = 1
        [Enum(Off, 0, On, 1)]_ZWriteMode("ZWriteMode", float) = 0

        [Foldout]_RimName("Rim Color control",Range(0,1)) = 0
        [FoldoutItem] _RimColor("RimColor",Color) = (0,0,0,0)
        [FoldoutItem] _RimOffset("_RimOffset",Range(0,0.2)) = 0.1
        [FoldoutItem] _RimSpread("_RimSpread",Range(0,1)) = 0.5
        [FoldoutItem] _RimPower("Transparency",Range(0,1)) = 1	
    }

    SubShader
    {
        Pass
        {
            Name "Forward"
            Tags{"RenderPipeline" = "UniversalPipeline"  "Queue" = "Transparent-299"}
            Cull[_CullMode]
            Blend[_SourceBlend][_DestBlend]
            ZWrite[_ZWriteMode]
            ZTest LEqual
            Offset 0 , 0
            ColorMask RGBA


            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "ColorCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "LightingCore.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _RimColor;
            float _RimOffset;
            float _RimSpread;
            float _RimPower;
            CBUFFER_END

            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
            };

            struct vertexOutput
            {
                float4 clipPos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 worldvertpos : TEXCOORD1;
            };


            vertexOutput vert(VertexInput v) {
                vertexOutput o;
                //顶点位置以法线方向向外延伸
                v.vertex.xyz += v.ase_normal * _RimOffset;
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float4 positionCS = TransformWorldToHClip(positionWS);
                o.normal = TransformObjectToWorld(v.ase_normal);
                o.worldvertpos = positionWS;
                o.clipPos = positionCS;
                return o;
            }

            float4 frag(vertexOutput i) : COLOR
            {
                i.normal = normalize(i.normal);
                //视角法线
                float3 viewdir = normalize(i.worldvertpos.xyz - _WorldSpaceCameraPos.xyz);
                float4 color = _RimColor;
                //视角法线与模型法线点积形成中间为1向四周逐渐衰减为0的点积值，赋值透明通道，形成光晕效果
                color.a = pow(saturate(dot(viewdir, i.normal)), _RimSpread);
                color.a *= _RimPower * dot(viewdir, i.normal);
                return color;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}