// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "puyu/Dis_Caustics"
 {

    Properties {
        _Background ("Background", 2D) = "" {}            //背景纹理
    
        _Refraction ("Refraction", float) = 1.0        //折射值
        _DistortionMap ("Distortion Map", 2D) = "" {}    //扭曲
        _DistortionScrollX ("X Offset", float) = 0    
        _DistortionScrollY ("Y Offset", float) = 0    
        _DistortionScaleX ("X Scale", float) = 1.0
        _DistortionScaleY ("Y Scale", float) = 1.0
        _DistortionPower ("Distortion Power", float) = 0.08

        _MaskTex ("_MaskTex Map", 2D) = "" {}  
      
        [HDR]_Causticscol("_Causticscol", Color) = (1,1,1,1)
        
          
        _Tiled ("Tiled", Range(0.1, 32)) = 6.283
		_Density ("Density", Range(0.002, 0.02)) = 0.005
		_Intensity ("Intensity", Range(0, 4)) = 1.4
	
	

    }

    SubShader {
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}

        Pass {

            Cull Off
            ZTest LEqual
            ZWrite On
            AlphaTest Off
            Lighting Off
            ColorMask RGBA
            Blend Off

            CGPROGRAM
            #pragma target 2.0
            #pragma fragment frag
            #pragma vertex vert
            #define ITER 5
            #include "UnityCG.cginc"
           

            uniform sampler2D _Background;
            uniform sampler2D _DistortionMap;
            
            uniform float _DistortionScrollX;
            uniform float _DistortionScrollY; 
            uniform float _DistortionPower;
          
            uniform float _DistortionScaleX;
            uniform float _DistortionScaleY;
            uniform float _Refraction;

           uniform float4  _Causticscol;

            uniform sampler2D _MaskTex;
            uniform float4  _MaskTex_ST;
            uniform float _Density;
            uniform float _Intensity;


            float _Tiled;
            struct AppData
             {
                float4 vertex : POSITION;
                half2 uv : TEXCOORD0;
             };

            struct VertexToFragment {
                float4 pos : POSITION;
                half2 uv : TEXCOORD0;
              
               
            };

            VertexToFragment vert(AppData v) {
                VertexToFragment o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
              
                return o;
            }
            




            fixed4 frag(VertexToFragment input) : COLOR {
                   float2 disOffset = float2(_DistortionScrollX,_DistortionScrollY);
                   float2 disScale = float2(_DistortionScaleX,_DistortionScaleY);
                   float4  mask = tex2D (_MaskTex, input.uv) ;
                   float2 uv = input.uv;
                   float2 p = fmod(uv * _Tiled, _Tiled) - 250.0;

                   float2 i = p;
                   float c = 1.0;
                                
                    for (int n = 0; n < ITER; n++)
                    {
                        float t = _Time.y * (1.0 - (3.5 / float(n + 1)));
                        i = p + float2(cos(t - i.x) + sin(t + i.y), sin(t - i.y) + cos(t + i.x));
                        c += 1.0 / length(float2(p.x / (sin(i.x + t) / _Density), p.y / (cos(i.y + t) / _Density)));
                    }
                    c /= float(ITER);
                    c = 1.17 - pow(c, _Intensity);
                    float tmp = pow(abs(c), 8.0);
                    float3 cc = float3(tmp, tmp, tmp) *mask *_Causticscol;
      
                   
                   float4 disTex = tex2D(_DistortionMap, disScale * input.uv+disOffset*_Time.y);
                   
                   float2 offsetUV = (-_Refraction*(disTex * _DistortionPower - (_DistortionPower*0.5)));
                   
                   float4 bgtex =tex2D(_Background,  input.uv  + offsetUV );
                          bgtex.rgb =bgtex+cc;
        
                  return  bgtex;
            }

            ENDCG
        }
    }
}