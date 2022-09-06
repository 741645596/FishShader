Shader "Fish/MapWave"
{
    Properties
    {
        [NoScaleOffset]Texture2D_9DF80CD("水波纹噪点", 2D) = "white" {}
        [NoScaleOffset]Texture2D_6007F7C4("地图背景", 2D) = "white" {}
        [NoScaleOffset]Texture2D_207B1FD0("通道图", 2D) = "white" {}
        [NoScaleOffset]Texture2D_B982FEF3("光斑图", 2D) = "white" {}
        Vector2_FE494567("光斑图Tiling", Vector) = (1, 1, 0, 0)
        Vector1_267E2EA6("光斑亮度", Range(0, 2)) = 1
        Color_14978EF7("光斑颜色", Color) = (1, 1, 1, 0)
        Vector1_C2BB5C5C("水波强度-光斑", Range(1, 2)) = 1
        Vector1_DB7EAB72("水波强度-背景", Range(1, 2)) = 1
        Vector1_7D72FC8D("扭曲速度", Float) = 0.05
        Vector1_2820388("水波速度", Float) = 0.05
        Vector1_951A7CFD("混合程度", Float) = 1
        _BGAlpha("透明度", Range(0, 1)) = 1
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/NormalSurfaceGradient.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/ShaderVariablesFunctions.hlsl"
    #include "Packages/com.unity.shadergraph/ShaderGraphLibrary/Functions.hlsl"
    #define SHADERGRAPH_PREVIEW 1

    CBUFFER_START(UnityPerMaterial)
    float2 Vector2_FE494567;
    float Vector1_267E2EA6;
    float4 Color_14978EF7;
    float Vector1_C2BB5C5C;
    float Vector1_DB7EAB72;
    float Vector1_7D72FC8D;
    float Vector1_2820388;
    float Vector1_951A7CFD;
    float _BGAlpha;
    CBUFFER_END
    TEXTURE2D(Texture2D_9DF80CD); SAMPLER(samplerTexture2D_9DF80CD); float4 Texture2D_9DF80CD_TexelSize;
    TEXTURE2D(Texture2D_6007F7C4); SAMPLER(samplerTexture2D_6007F7C4); float4 Texture2D_6007F7C4_TexelSize;
    TEXTURE2D(Texture2D_207B1FD0); SAMPLER(samplerTexture2D_207B1FD0); float4 Texture2D_207B1FD0_TexelSize;
    TEXTURE2D(Texture2D_B982FEF3); SAMPLER(samplerTexture2D_B982FEF3); float4 Texture2D_B982FEF3_TexelSize;
    SAMPLER(_SampleTexture2D_94264F77_Sampler_3_Linear_Repeat);
    SAMPLER(_SampleTexture2D_7A546F1A_Sampler_3_Linear_Repeat);
    SAMPLER(_SampleTexture2D_5EB650ED_Sampler_3_Linear_Repeat);
    SAMPLER(_SampleTexture2D_5BF6C5FC_Sampler_3_Linear_Repeat);

    struct SurfaceDescriptionInputs
    {
        float4 uv0;
        float3 TimeParameters;
    };


    void Unity_Multiply_float(float A, float B, out float Out)
    {
        Out = A * B;
    }

    void Unity_TilingAndOffset_float(float2 UV, float2 Tiling, float2 Offset, out float2 Out)
    {
        Out = UV * Tiling + Offset;
    }

    void Unity_InvertColors_float4(float4 In, float4 InvertColors, out float4 Out)
    {
        Out = abs(InvertColors - In);
    }

    void Unity_Multiply_float(float4 A, float4 B, out float4 Out)
    {
        Out = A * B;
    }

    void Unity_Remap_float4(float4 In, float2 InMinMax, float2 OutMinMax, out float4 Out)
    {
        Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
    }

    void Unity_Blend_Dodge_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
    {
        Out = Base / (1.0 - clamp(Blend, 0.000001, 0.999999));
        Out = lerp(Base, Out, Opacity);
    }

    struct SurfaceDescription
    {
        float4 Out_2;
    };

    SurfaceDescription PopulateSurfaceData(SurfaceDescriptionInputs IN)
    {
        SurfaceDescription surface = (SurfaceDescription)0;
        float _Property_93EC8D0E_Out_0 = Vector1_7D72FC8D;
        float _Multiply_9D9B9EC_Out_2;
        Unity_Multiply_float(IN.TimeParameters.x, _Property_93EC8D0E_Out_0, _Multiply_9D9B9EC_Out_2);
        _Multiply_9D9B9EC_Out_2 = frac(_Multiply_9D9B9EC_Out_2);
        float2 _TilingAndOffset_4D49808_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_9D9B9EC_Out_2.xx), _TilingAndOffset_4D49808_Out_3);
        float4 _SampleTexture2D_94264F77_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_9DF80CD, samplerTexture2D_9DF80CD, _TilingAndOffset_4D49808_Out_3);
        float4 _SampleTexture2D_7A546F1A_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_207B1FD0, samplerTexture2D_207B1FD0, IN.uv0.xy);
        float4 _InvertColors_733DCA74_Out_1;
        float4 _InvertColors_733DCA74_InvertColors = float4 (1, 1, 1, 0);    
        Unity_InvertColors_float4(_SampleTexture2D_7A546F1A_RGBA_0, _InvertColors_733DCA74_InvertColors, _InvertColors_733DCA74_Out_1);
        float4 _Multiply_6ACB61CC_Out_2;
        Unity_Multiply_float(_SampleTexture2D_94264F77_RGBA_0, _InvertColors_733DCA74_Out_1, _Multiply_6ACB61CC_Out_2);
        float4 _Remap_194416B0_Out_3;
        Unity_Remap_float4(_Multiply_6ACB61CC_Out_2, float2 (0, 1), float2 (0, -0.01), _Remap_194416B0_Out_3);
        float _Property_7A6DA221_Out_0 = Vector1_DB7EAB72;
        float4 _Multiply_72BA2CDB_Out_2;
        Unity_Multiply_float(_Remap_194416B0_Out_3, (_Property_7A6DA221_Out_0.xxxx), _Multiply_72BA2CDB_Out_2);
        float2 _TilingAndOffset_A84AD095_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_72BA2CDB_Out_2.xy), _TilingAndOffset_A84AD095_Out_3);
        float4 _SampleTexture2D_5EB650ED_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_6007F7C4, samplerTexture2D_6007F7C4, _TilingAndOffset_A84AD095_Out_3);
        float4 _Property_E0C1FE52_Out_0 = Color_14978EF7;
        float _Property_6960C691_Out_0 = Vector1_2820388;
        float _Multiply_3CD873E2_Out_2;
        Unity_Multiply_float(IN.TimeParameters.x, _Property_6960C691_Out_0, _Multiply_3CD873E2_Out_2);
        _Multiply_3CD873E2_Out_2 = frac(_Multiply_3CD873E2_Out_2);
        float2 _TilingAndOffset_8BC1AA70_Out_3;
        Unity_TilingAndOffset_float(IN.uv0.xy, float2 (1, 1), (_Multiply_3CD873E2_Out_2.xx), _TilingAndOffset_8BC1AA70_Out_3);
        float2 _Property_A818BA9_Out_0 = Vector2_FE494567;
        float _Property_6C0FEE97_Out_0 = Vector1_C2BB5C5C;
        float4 _Multiply_D86F5D_Out_2;
        Unity_Multiply_float(_Remap_194416B0_Out_3, (_Property_6C0FEE97_Out_0.xxxx), _Multiply_D86F5D_Out_2);
        float2 _TilingAndOffset_55519536_Out_3;
        Unity_TilingAndOffset_float(_TilingAndOffset_8BC1AA70_Out_3, _Property_A818BA9_Out_0, (_Multiply_D86F5D_Out_2.xy), _TilingAndOffset_55519536_Out_3);
        float4 _SampleTexture2D_5BF6C5FC_RGBA_0 = SAMPLE_TEXTURE2D(Texture2D_B982FEF3, samplerTexture2D_B982FEF3, _TilingAndOffset_55519536_Out_3);
        float4 _Multiply_74C2D969_Out_2;
        Unity_Multiply_float(_InvertColors_733DCA74_Out_1, _SampleTexture2D_5BF6C5FC_RGBA_0, _Multiply_74C2D969_Out_2);
        float4 _Multiply_3534E823_Out_2;
        Unity_Multiply_float(_Property_E0C1FE52_Out_0, _Multiply_74C2D969_Out_2, _Multiply_3534E823_Out_2);
        float _Property_62EE238C_Out_0 = Vector1_267E2EA6;
        float4 _Multiply_BE0DBFD6_Out_2;
        Unity_Multiply_float(_Multiply_3534E823_Out_2, (_Property_62EE238C_Out_0.xxxx), _Multiply_BE0DBFD6_Out_2);
        float _Property_3AA2635C_Out_0 = Vector1_951A7CFD;
        float4 _Blend_BF31B93_Out_2;
        Unity_Blend_Dodge_float4(_SampleTexture2D_5EB650ED_RGBA_0, _Multiply_BE0DBFD6_Out_2, _Blend_BF31B93_Out_2, _Property_3AA2635C_Out_0);
        surface.Out_2 = _Blend_BF31B93_Out_2;
        return surface;
    }

    struct GraphVertexInput
    {
        float4 vertex : POSITION;
        float4 texcoord0 : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    GraphVertexInput PopulateVertexData(GraphVertexInput v)
    {
        return v;
    }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+200"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct GraphVertexOutput
            {
                float4 position : POSITION;
                float4 uv0 : TEXCOORD;

            };

            GraphVertexOutput vert (GraphVertexInput v)
            {
                v = PopulateVertexData(v);

                GraphVertexOutput o;
                float3 positionWS = TransformObjectToWorld((float3)v.vertex);
                o.position = TransformWorldToHClip(positionWS);
                float4 uv0 = v.texcoord0;
                o.uv0 = uv0;

                return o;
            }

            float4 frag (GraphVertexOutput IN ) : SV_Target
            {
                float4 uv0 = IN.uv0;

                SurfaceDescriptionInputs surfaceInput = (SurfaceDescriptionInputs)0;
                surfaceInput.uv0 = uv0;
                surfaceInput.TimeParameters = _TimeParameters.xyz;

                SurfaceDescription surf = PopulateSurfaceData(surfaceInput);
                return surf.Out_2;

            }
            ENDHLSL
        }
    }
}