// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "fish/fx/Fipbook"
{
	Properties
	{
		_Texture("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Flipbook("Flipbook", Vector) = (3,3,1,1)
		[Enum(Time,0,Frame,1)]_Flipbook_Time("Flipbook_Time", Float) = 0
		_Frame("Frame", Float) = 0

	}

	SubShader
	{
		LOD 0

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Transparent" }
		
		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL

		Pass
		{
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero , One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

			sampler2D _Texture;
			CBUFFER_START( UnityPerMaterial )
			half4 _Color;
			half4 _Texture_ST;
			half4 _Flipbook;
			half _Frame;
			half _Flipbook_Time;
			CBUFFER_END

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				half4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#ifdef ASE_FOG
				float fogFactor : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert ( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				half2 uv0_Texture = v.ase_texcoord.xy * _Texture_ST.xy + _Texture_ST.zw;
				half lerpResult119 = lerp( _Time.y , ( _Frame - 0.0 ) , _Flipbook_Time);
				// *** BEGIN Flipbook UV Animation vars ***
				// Total tiles of Flipbook Texture
				float fbtotaltiles85 = _Flipbook.x * _Flipbook.y;
				// Offsets for cols and rows of Flipbook Texture
				float fbcolsoffset85 = 1.0f / _Flipbook.x;
				float fbrowsoffset85 = 1.0f / _Flipbook.y;
				// Speed of animation
				float fbspeed85 = lerpResult119 * _Flipbook.z;
				// UV Tiling (col and row offset)
				float2 fbtiling85 = float2(fbcolsoffset85, fbrowsoffset85);
				// UV Offset - calculate current tile linear index, and convert it to (X * coloffset, Y * rowoffset)
				// Calculate current tile linear index
				float fbcurrenttileindex85 = round( fmod( fbspeed85 + ( _Flipbook.w - 1.0 ), fbtotaltiles85) );
				fbcurrenttileindex85 += ( fbcurrenttileindex85 < 0) ? fbtotaltiles85 : 0;
				// Obtain Offset X coordinate from current tile linear index
				float fblinearindextox85 = round ( fmod ( fbcurrenttileindex85, _Flipbook.x ) );
				// Multiply Offset X by coloffset
				float fboffsetx85 = fblinearindextox85 * fbcolsoffset85;
				// Obtain Offset Y coordinate from current tile linear index
				float fblinearindextoy85 = round( fmod( ( fbcurrenttileindex85 - fblinearindextox85 ) / _Flipbook.x, _Flipbook.y ) );
				// Reverse Y to get tiles from Top to Bottom
				fblinearindextoy85 = (int)(_Flipbook.y-1) - fblinearindextoy85;
				// Multiply Offset Y by rowoffset
				float fboffsety85 = fblinearindextoy85 * fbrowsoffset85;
				// UV Offset
				float2 fboffset85 = float2(fboffsetx85, fboffsety85);
				// Flipbook UV
				half2 fbuv85 = uv0_Texture * fbtiling85 + fboffset85;
				// *** END Flipbook UV Animation vars ***
				half2 vertexToFrag123 = fbuv85;
				o.ase_texcoord1.xy = vertexToFrag123;
				
				o.ase_color = v.ase_color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.clipPos = vertexInput.positionCS;
				#ifdef ASE_FOG
				o.fogFactor = ComputeFogFactor( vertexInput.positionCS.z );
				#endif
				return o;
			}

			half4 frag ( VertexOutput IN  ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				half2 vertexToFrag123 = IN.ase_texcoord1.xy;
				
				float3 BakedAlbedo = 0;
				float3 BakedEmission = 0;
				float3 Color = ( _Color * tex2D( _Texture, vertexToFrag123 ) * IN.ase_color ).rgb;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#if _AlphaClip
					clip( Alpha - AlphaClipThreshold );
				#endif

				#ifdef ASE_FOG
					Color = MixFog( Color, IN.fogFactor );
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				return half4( Color, Alpha );
			}

			ENDHLSL
		}

		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0

			HLSLPROGRAM
			#define _RECEIVE_SHADOWS_OFF 1
			#pragma multi_compile_instancing
			#define ASE_SRP_VERSION 999999

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			CBUFFER_START( UnityPerMaterial )
			half4 _Color;
			half4 _Texture_ST;
			half4 _Flipbook;
			half _Frame;
			half _Flipbook_Time;
			CBUFFER_END

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			VertexOutput vert( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				o.clipPos = TransformObjectToHClip(v.vertex.xyz);
				return o;
			}

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				float Alpha = 1;
				float AlphaClipThreshold = 0.5;

				#if _AlphaClip
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				return 0;
			}
			ENDHLSL
		}

	}
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=17500
1934;37;1906;993;448.4064;394.0051;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;118;-402.6094,510.3399;Inherit;False;Property;_Frame;Frame;4;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;123;47.36346,61.80945;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TFHCFlipBookUVAnimation;85;-193.8923,43.65764;Inherit;False;0;0;6;0;FLOAT2;0,0;False;1;FLOAT;3;False;2;FLOAT;3;False;3;FLOAT;1;False;4;FLOAT;1;False;5;FLOAT;1;False;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.LerpOp;119;-170.9545,395.8718;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;116;-461.4263,357.2948;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;120;-439.364,589.5445;Inherit;False;Property;_Flipbook_Time;Flipbook_Time;3;1;[Enum];Create;True;2;Time;0;Frame;1;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;79;138.591,316.7195;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;122;-235.1602,510.1862;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;111;152.5548,-179.9017;Half;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;78;244.5392,50.83641;Inherit;True;Property;_Texture;Texture;0;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;80;592.5293,81.30185;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;121;-246.9549,249.6127;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;84;-475.0045,40.6201;Inherit;False;0;78;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,3;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;117;-434.1518,176.5572;Inherit;False;Property;_Flipbook;Flipbook;2;0;Create;True;0;0;False;0;3,3,1,1;3,3,1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;124;917.8657,83.26943;Half;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;3;fish/fx/Fipbook;2992e84f91cbeb14eab234972e07ea9d;True;Forward;0;0;Forward;7;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Transparent=Queue=0;True;2;0;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;0;Hidden/InternalErrorShader;0;0;Standard;10;Surface;0;  Blend;0;Two Sided;1;Cast Shadows;0;Receive Shadows;0;GPU Instancing;1;LOD CrossFade;0;Built-in Fog;0;Meta Pass;0;Vertex Position,InvertActionOnDeselection;1;0;4;True;False;True;False;False;;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;125;917.8657,83.26943;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;126;917.8657,83.26943;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;127;917.8657,83.26943;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;2992e84f91cbeb14eab234972e07ea9d;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;Hidden/InternalErrorShader;0;0;Standard;0;0
WireConnection;123;0;85;0
WireConnection;85;0;84;0
WireConnection;85;1;117;1
WireConnection;85;2;117;2
WireConnection;85;3;117;3
WireConnection;85;4;121;0
WireConnection;85;5;119;0
WireConnection;119;0;116;2
WireConnection;119;1;122;0
WireConnection;119;2;120;0
WireConnection;122;0;118;0
WireConnection;78;1;123;0
WireConnection;80;0;111;0
WireConnection;80;1;78;0
WireConnection;80;2;79;0
WireConnection;121;0;117;4
WireConnection;124;2;80;0
ASEEND*/
//CHKSM=C8E6B0B825D1A0776F43CB9DB0D8EB2C2E65BA1D