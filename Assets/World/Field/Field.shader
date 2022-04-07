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
    }

    SubShader {
        Tags {
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
            };

            /// the fragment shader input
            struct FragIn {
                // NOTE: shadow shader macros require this name
                float4 pos : SV_POSITION;
                float3 wPos : TEXCOORD0;
                float  saturation : TEXCOORD1;
                float  value : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)

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

            // -- noise --
            /// get random value at pt
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

            float Noise(float2 st) {
                float2 i = floor(st);
                float2 f = frac(st);
                float2 u = f * f * (3.0f - 2.0f * f);

                float res = lerp(
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

            float Image(float2 st) {
                float2x2 m = float2x2(1.6, 1.2, -1.2, 1.6);

                // blend noise
                float c = 0.0f;
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
                if (_Bands != -1.0f) {
                    c = floor(c * _Bands) / (_Bands - 1.0f);
                }

                return c;
            }

            // -- program --
            FragIn DrawVert(VertIn v) {
                float3 pos = v.vertex.xyz;

                FragIn o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.wPos = mul(unity_ObjectToWorld, float4(pos, 1.0));

                float dist = saturate(distance(_WorldSpaceCameraPos, o.wPos) / _ViewDist);
                o.saturation = lerp(_SatMin, _SatMax, dist);
                o.value = lerp(_ValMin, _ValMax, dist);

                TRANSFER_VERTEX_TO_FRAGMENT(o);
                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                // scale by uniform
                float2 st = f.wPos.xz * _Scale;

                // generate image
                float3 c = IntoRgb(float3(
                    lerp(_HueMin, _HueMax, Image(st)),
                    f.saturation,
                    f.value
                ));

                // produce color
                fixed4 col = fixed4(c, 1.0f);

                // apply lighting & fog
                col *= LIGHT_ATTENUATION(f);
                UNITY_APPLY_FOG(f.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}
