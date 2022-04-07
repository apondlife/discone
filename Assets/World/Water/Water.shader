Shader "Custom/Water" {
    // -- propertiesw --
	Properties {
		_Color("Color", Color) = (1, 1, 1, 1)
		_EdgeColor("Edge Color", Color) = (1, 1, 1, 1)
		_DepthFactor("Depth Factor", float) = 1.0
		_WaveSpeed("Wave Speed", float) = 1.0
		_WaveAmp("Wave Amp", float) = 0.2
		_DepthRampTex("Depth Ramp", 2D) = "white" {}
		_MainTex("Main Texture", 2D) = "white" {}
		_DistortStrength("Distort Strength", float) = 1.0
		_ExtraHeight("Extra Height", float) = 0.0
	}

	SubShader {
        Tags {
			"Queue" = "Transparent"
		}

        GrabPass {
            "_BackgroundTexture"
        }

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
			#include "../../Post/Core/Math.cginc"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float3 texCoord : TEXCOORD0;
            };

            struct FragIn {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            // -- props --
            sampler2D _BackgroundTexture;
            float  _DistortStrength;
			float  _WaveSpeed;
			float  _WaveAmp;

            // -- program --
            FragIn DrawVert(VertIn i) {
                FragIn o;
                o.pos = UnityObjectToClipPos(i.vertex);

                // use ComputeGrabScreenPos function from UnityCG.cginc
                // to get the correct texture coordinate
                o.uv = ComputeGrabScreenPos(o.pos);

                // distort based on bump map
				float dir = _Time * _WaveSpeed * Rand(i.texCoord.xy);
				float mag = _WaveAmp * _DistortStrength;
                o.uv.x += cos(dir) * mag;
				o.uv.y += sin(dir) * mag;

                return o;
            }

            float4 DrawFrag(FragIn i) : COLOR {
                return tex2Dproj(_BackgroundTexture, i.uv);
            }
            ENDCG
        }

		Pass {
            Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
            // -- config --
			#pragma vertex DrawVert
			#pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
			#include "../../Post/Core/Math.cginc"

            // -- types --
			struct VertIn {
				float4 vertex : POSITION;
				float4 texCoord : TEXCOORD1;
			};

			struct FragIn {
				float4 pos : SV_POSITION;
				float4 texCoord : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
			};

			// -- properties --
			float4 _Color;
			float4 _EdgeColor;
			float  _DepthFactor;
			float  _WaveSpeed;
			float  _WaveAmp;
			float _ExtraHeight;
			sampler2D _CameraDepthTexture;
			sampler2D _DepthRampTex;
			sampler2D _NoiseTex;
			sampler2D _MainTex;

            // -- program --
			FragIn DrawVert(VertIn i) {
				FragIn o;

				// convert to world space
				o.pos = UnityObjectToClipPos(i.vertex);

				// apply wave animation
				float dir = _Time * _WaveSpeed * Rand(i.texCoord.xy);
				float mag = _WaveAmp;
                o.pos.x += cos(dir) * _WaveAmp;
				o.pos.y += sin(dir) * _WaveAmp * _ExtraHeight;

				// compute depth
				o.screenPos = ComputeScreenPos(o.pos);

				// texture coordinates
				o.texCoord = i.texCoord;

				return o;
			}

			float4 DrawFrag(FragIn i) : COLOR {
				// apply depth texture
				float4 depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos);
				float depth = LinearEyeDepth(depthSample).r;

				// create foamline
				float foamLine = 1 - saturate(_DepthFactor * (depth - i.screenPos.w));
				float4 foamRamp = float4(tex2D(_DepthRampTex, float2(foamLine, 0.5)).rgb, 1.0);

				// sample main texture
				float4 albedo = tex2D(_MainTex, i.texCoord.xy);

			    float4 col = _Color * foamRamp * albedo;
                return col;
			}
			ENDCG
		}
	}
}