// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//
// Based on Unity's "ProjectorMultiply" shader:
// Slightly modified to apply effect only when the surface is pointing up.
//

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Projector/BlobShadow" {
	Properties {
		_Color ("Color", Color) = (0.3, 0.3, 0.3, 0.2)
		_BlurSize ("Blur", Range(0, 1)) = 0.01
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
		_WallTolerance ("Wall Tolerance", Range(0, 1)) = 0.01
	}

	Subshader {
		Tags {
			"Queue" = "Transparent"
		}

		Pass {
			// -- flags --
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			Offset -1, -1

			CGPROGRAM
			#pragma vertex DrawVert
			#pragma fragment DrawFrag
			#pragma multi_compile_fog

			// -- includes --
			#include "UnityCG.cginc"
			#include "Assets/Shaders/Core/Math.cginc"

			// -- types --
			struct vertex_out {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2) // TEXCOORD2
				float4 pos : SV_POSITION;
				float intensity : TEXCOORD3; // additional intensity, based on normal orientation
			};

			// -- props --
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;

			sampler2D _ShadowTex;
			sampler2D _FalloffTex;

			fixed4 _Color;
			float _BlurSize;
			float _WallTolerance;

			vertex_out DrawVert(float4 vertex : POSITION, float3 normal : NORMAL) {
				vertex_out o;

				// 1 if pointing UP
				o.intensity = step(
					_WallTolerance,
					dot(float3(0, 1, 0),
					UnityObjectToWorldNormal(normal))
				);

				o.pos = UnityObjectToClipPos(vertex);
				o.uvShadow = mul(unity_Projector, vertex);
				o.uvFalloff = mul(unity_ProjectorClip, vertex);

				UNITY_TRANSFER_FOG(o,o.pos);

				return o;
			}

			fixed4 DrawFrag(vertex_out i) : SV_Target {
				// sample shadow and falloff texture
				fixed4 uvS = UNITY_PROJ_COORD(i.uvShadow);
				fixed4 uvF = UNITY_PROJ_COORD(i.uvFalloff);

				// return fixed4(clamp(uvS.x, -1, 1), clamp(uvS.y, -1, 1), 1.0, 1.0);
				fixed4 texS = tex2Dproj(_ShadowTex, uvS);
				texS.a = 1 - texS.a;

				// get uv in -1,1 space around origin
				fixed2 uvC = 2.0 * (uvS - fixed2(0.5, 0.5));
				fixed len = length(uvC);
				fixed4 blob = _Color;

				// make the blob blurry
				blob.a = _Color.a * (1-saturate(Unlerp(1-_BlurSize, 1,  len*len)));

				texS = lerp(texS, blob, step(len, 1));

				fixed4 texF = tex2Dproj(_FalloffTex, uvF);

				// calculate blob color
				fixed4 col = lerp(
					fixed4(1, 1, 1, 0),
					texS,
					texF.a * i.intensity
				);

				UNITY_APPLY_FOG_COLOR(
					i.fogCoord,
					col,
					fixed4(1, 1, 1, 1)
				);

				return col;
			}
			ENDCG
		}
	}
}
