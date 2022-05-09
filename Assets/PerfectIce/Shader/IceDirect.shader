Shader "WB/IceEffect" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.1, 10)) = 1
        _ReflectColor ("ReflectColor", Color) = (1,1,1,0.5)
         _MainTex ("MainTex", 2D) = "white" {}
        [NoScaleOffset] _Refraction ("Refraction", 2D) = "bump" {}
        [NoScaleOffset] _Cube ("Cube", Cube) = "_Skybox" {}
        _RefStrength ("RefStrength", Range(0, 2)) = 1
        _LightStrength ("LightStrength", Range(0, 1)) = 0.1
        _FrenelPower ("FrenelPower", Range(0, 2)) = 0.1
        _TexAlphaAdd ("TexAlphaAdd", Range(0, 1)) = 0.1
        _Cutoff ("Cutoff", Range(0, 1)) = 1
    }
    SubShader {
        Tags {"RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+1"}
        LOD 200
        Pass {
            Name "FORWARD"
            Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Transparent" "Queue" = "Transparent+1" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform sampler2D _Refraction; 
            uniform float4 _Refraction_ST;
            uniform samplerCUBE _Cube;
            uniform float _Shininess;
            uniform float4 _Color;
            uniform sampler2D _MainTex; 
            uniform float4 _MainTex_ST;

            uniform float4 _ReflectColor;
            uniform float _RefStrength;
            uniform float _LightStrength;
            uniform float _FrenelPower;
            uniform float _TexAlphaAdd;
            uniform float _Cutoff;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
            };


            VertexOutput vert (VertexInput v) 
            {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                return o;
            }


            float4 frag(VertexOutput i) : COLOR {
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _Refraction_var = UnpackNormal(tex2D(_Refraction,TRANSFORM_TEX(i.uv0, _Refraction)));
                float3 normalLocal = _Refraction_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float3 viewReflectDirection = reflect( -viewDirection, normalDirection );

                float4 _MainTex_var = tex2D(_MainTex,TRANSFORM_TEX(i.uv0, _MainTex));
                clip((_MainTex_var.a+_Cutoff) - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = 1;
                float3 attenColor = attenuation * _LightColor0.xyz;
///////// Gloss:
                float gloss = _MainTex_var.a;
                float specPow = exp2( gloss * 10.0 + 1.0 );
////// Specular:
                float NdotL = saturate(dot( normalDirection, lightDirection ));
                float3 specularColor = float3(_Shininess,_Shininess,_Shininess);
                float3 directSpecular = attenColor * pow(max(0,dot(halfDirection,normalDirection)),specPow)*specularColor;
                float3 specular = directSpecular;
/////// Diffuse:
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = pow(max( 0.0, NdotL), _MainTex_var.rgb) * attenColor;
                float3 diffuseColor = _Color.rgb;
                float3 diffuse = directDiffuse * diffuseColor;
////// Emissive:
                float4 _Cube_var = texCUBE(_Cube,viewReflectDirection);
                float3 emissive = ((_MainTex_var.rgb + 
                    (lerp((_MainTex_var.rgb*_Color.rgb),(_Cube_var.rgb*_Cube_var.a),pow(_FrenelPower,lerp(-1,0,pow(1.0-max(0,dot(normalDirection, viewDirection)),0.5*dot(1.0,_Refraction_var.rgb)+0.5))))*_LightStrength*_RefStrength))*_ReflectColor.rgb);
/// Final Color:
                float3 finalColor = diffuse + specular + emissive;
                //return fixed4(lerp(sceneColor.rgb, finalColor,(_MainTex_var.a+_TexAlphaAdd)),1);
                return fixed4(finalColor, _MainTex_var.a + _TexAlphaAdd);
            }
            ENDCG
        }
    }
}
