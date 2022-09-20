Shader "WB/Mask"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendRGB("BlendSrcRGB", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlendRGB("BlendDstRGB", Float) = 1
        [Enum(Off,0, On,1)] _ZWriteMode("ZWrite Mode", Int) = 0
        [Enum(Always,0,Less,2,LessEqual,4)] _ZTest("ZTest Mode", Int) = 4
        _BaseColor("Base Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend[_SrcBlendRGB][_DstBlendRGB]
        ZWrite[_ZWriteMode]
        ZTest[_ZTest]

        /*Stencil
        {
                Ref 255         // 参考索引值
                Comp always     // 比较方式
                Pass replace    // 成功处理方式
                Fail keep       // 没有通过模板测试，处理方式
                ZFail keep      // 模板测试通过，深度测试未通过
        }*/

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "ColorCore.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "LightingCore.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            CBUFFER_END
            v2f vert (appdata v)
            {
                v2f o; 
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                //float3 positionVS = TransformWorldToView(positionWS);
                o.vertex = TransformWorldToHClip(positionWS);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
    CustomEditor "FoldoutShaderGUI"
}
