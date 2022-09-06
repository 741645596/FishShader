Shader "WB/FishDiffuseColor"
{
    Properties
    {
        _Color("diffuse",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        LOD 100

         Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "ColorCore.hlsl"
            #include "LightingCore.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            CBUFFER_END


            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
            };

            struct v2f
            {
                float4 clipPos:SV_POSITION;
                float3 worldNormal:NORMAL;
            };

            v2f vert(VertexInput v)
            {
                v2f o;
                float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 positionVS = TransformWorldToView(positionWS);
                float4 positionCS = TransformWorldToHClip(positionWS);
                o.clipPos = positionCS;
                o.worldNormal = TransformObjectToWorld(v.ase_normal);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb;
                float3 worldLight = normalize(_MainLightPosition.xyz);
                //float3 diffuse = _Color.rgb * _MainLightColor.rgb * saturate(dot(worldLight, i.worldNormal));
                float3 diffuse = _Color.rgb * _MainLightColor.rgb * saturate(dot(worldLight, i.worldNormal) * 0.5f + 0.5);
                return float4(diffuse + ambient, 1);
            }
            ENDHLSL
        }
    }
}


