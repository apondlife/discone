/// custom terrain shader, likely missing features
/// see: https://alastaira.wordpress.com/2013/12/07/custom-unity-terrain-material-shaders/
Shader "Custom/Field" {
    Properties {
        _Scale ("Scale", Float) = 1.0
        _Bands ("Bands", Int) = -1
        _ViewDist ("View Distance", Float) = 0.0
        _HueMin ("Hue Min", Range(0.0, 2.0)) = 0.0
        _HueMax ("Hue Max", Range(0.0, 2.0)) = 0.0
        _SatMin ("Saturation Min", Range(0.0, 1.0)) = 0.4
        _SatMax ("Saturation Max", Range(0.0, 1.0)) = 0.4
        _ValMin ("Value Min", Range(0.0, 1.0)) = 0.87
        _ValMax ("Value Max", Range(0.0, 1.0)) = 0.87

        // the terrain layer textures; see pass props for more documentation
        [HideInInspector] _Control ("Control (RGBA)", 2D) = "black" {}
        [HideInInspector] _Splat0 ("Layer 0 (R)", 2D) = "white" {}
        [HideInInspector] _Splat1 ("Layer 1 (G)", 2D) = "white" {}
        [HideInInspector] _Splat2 ("Layer 2 (B)", 2D) = "white" {}
        [HideInInspector] _Splat3 ("Layer 3 (A)", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "SplatCount" = "4"
            "RenderType" = "Opaque"
            "TerrainCompatible" = "True"
        }

        Pass {
            Tags {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            // -- includes --
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "../../Post/Core/Color.cginc"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                // NOTE: shadow shader macros require this name
                float4 vertex : POSITION;

                // the layer control texture uv
                float2 uvControl : TEXCOORD0;

                // the layer uvs
                float2 uvSplat0 : TEXCOORD1;
                float2 uvSplat1 : TEXCOORD2;
                float2 uvSplat2 : TEXCOORD3;
                float2 uvSplat3 : TEXCOORD4;
            };

            /// the fragment shader input
            struct FragIn {
                // NOTE: shadow shader macros require this name
                float4 pos : SV_POSITION;

                // the world position for sampling consistent noise
                float3 wPos : TEXCOORD0;

                // the color values
                float  saturation : TEXCOORD1;
                float  value : TEXCOORD2;

                // the control texture uv
                float2 uvControl : TEXCOORD3;

                // the terrain layer uvs
                float2 uvSplat0 : TEXCOORD4;
                float2 uvSplat1 : TEXCOORD5;
                float2 uvSplat2 : TEXCOORD6;
                float2 uvSplat3 : TEXCOORD7;

                LIGHTING_COORDS(8,9)
                UNITY_FOG_COORDS(10)
            };

            // -- props --
            /// the noise scale
            float _Scale;

            /// the number of bands
            float _Bands;

            /// the view distance
            float _ViewDist;

            /// the minimum hue
            float _HueMin;

            /// the maximum hue
            float _HueMax;

            /// the min saturation
            float _SatMin;

            /// the max saturation
            float _SatMax;

            /// the min value
            float _ValMin;

            /// the max value
            float _ValMax;

            // -- p/layers
            /// the control texture ("splat map") that sets the alpha of each splat texture
            sampler2D _Control;

            /// the first terrain layer; alpha stored in _Control.R
            sampler2D _Splat0;

            /// the second terrain layer; alpha stored in _Control.G
            sampler2D _Splat1;

            /// the third terrain layer; alpha stored in _Control.B
            sampler2D _Splat2;

            /// the third terrain layer; alpha stored in _Control.A
            sampler2D _Splat3;

            // -- helpers --
            /// get a value from the unerlyin
            float1 Image(float2 st);

            /// get the layer's luminance at this texture coord
            float4 Layer(in sampler2D layer, in float2 uv);

            // -- program --
            FragIn DrawVert(VertIn v) {
                float3 pos = v.vertex.xyz;

                FragIn f;
                f.pos = UnityObjectToClipPos(v.vertex);
                f.wPos = mul(unity_ObjectToWorld, float4(pos, 1.0));

                float dist = saturate(distance(_WorldSpaceCameraPos, f.wPos) / _ViewDist);
                f.saturation = lerp(_SatMin, _SatMax, dist);
                f.value = lerp(_ValMin, _ValMax, dist);

                TRANSFER_VERTEX_TO_FRAGMENT(f);
                UNITY_TRANSFER_FOG(f, f.pos);

                f.uvControl = v.uvControl;
                f.uvSplat0 = v.uvSplat0;
                f.uvSplat1 = v.uvSplat1;
                f.uvSplat2 = v.uvSplat2;
                f.uvSplat3 = v.uvSplat3;

                return f;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                // scale by uniform
                float2 st = f.wPos.xz * _Scale;

                // sample splat textures
                fixed4 sctrl = tex2D(_Control, f.uvControl);
                float4 splat = float4(0.0f, 0.0f, 0.0f, 0.0f);
                splat += sctrl.g * Layer(_Splat1, f.uvSplat1);
                splat += sctrl.b * Layer(_Splat2, f.uvSplat2);
                splat += sctrl.a * Layer(_Splat3, f.uvSplat3);

                // generate image
                float3 shsv = IntoHsv(splat.rgb);
                float3 nhsv = float3(
                    lerp(_HueMin, _HueMax, Image(st)),
                    f.saturation + shsv.y * splat.a,
                    f.value + shsv.z * splat.a
                );

                // produce color
                fixed4 col = fixed4(IntoRgb(nhsv), 1.0f);

                // apply lighting & fog
                col *= LIGHT_ATTENUATION(f);
                UNITY_APPLY_FOG(f.fogCoord, col);

                return col;
            }

            // -- layers --
            float4 Layer(in sampler2D layer, in float2 uv) {
                return tex2D(layer, uv);
            }

            // -- noise --
            /// get random value at point
            float2 Gradient(float2 st) {
                // 2d to 1d
                int n = st.x + st.y * 11111;

                // hugo elias hash
                n = (n << 13) ^ n;
                n = (n * (n * n * 15731 + 789221) + 1376312589) >> 16;

                // perlin style vectors
                n &= 7;
                float2 gr = float2(n & 1, n >> 1) * 2.0 - 1.0;
                if (n >= 6) {
                    return float2(0.0, gr.x);
                } else if (n >= 4) {
                    return float2(gr.x, 0.0f);
                } else {
                    return gr;
                }
            }

            /// get noise value at point
            float1 Noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                float2 u = f * f * (3.0f - 2.0f * f);

                float1 res = lerp(
                    lerp(
                        dot(Gradient(i + float2(0.0f, 0.0f)), f - float2(0.0f, 0.0f)),
                        dot(Gradient(i + float2(1.0f, 0.0f)), f - float2(1.0f, 0.0f)),
                        u.x
                    ),
                    lerp(
                        dot(Gradient(i + float2(0.0f, 1.0f)), f - float2(0.0f, 1.0f)),
                        dot(Gradient(i + float2(1.0f, 1.0f)), f - float2(1.0f, 1.0f)),
                        u.x
                    ),
                    u.y
                );

                return res;
            }

            /// get noise image value at point
            float1 Image(float2 st) {
                float2x2 m = float2x2(1.6, 1.2, -1.2, 1.6);

                // blend noise
                float1 c = 0.0f;
                c  = 0.5000f * Noise(st);
                st = mul(st, m);
                c += 0.2500f * Noise(st);
                st = mul(st, m);
                c += 0.1250f * Noise(st);
                st = mul(st, m);
                c += 0.0625f * Noise(st);
                st = mul(st, m);

                // shift range
                c = c + 0.5f;

                // apply banding
                c = floor(c * _Bands) / (_Bands - 1.0f);

                return c;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}
