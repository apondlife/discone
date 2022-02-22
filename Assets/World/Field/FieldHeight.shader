Shader "Custom/DesertHeight" {
    Properties {
        _FloorScale ("Floor Noise Scale", Float) = .01
        _MinFloor ("Min Floor", Float) = 100
        _MaxFloor ("Max Floor", Float) = 600
        _ElevationScale ("Elevation Noise Scale", Float) = 1.0
        _MinElevation ("Min Elevation (Roughness)", Float) = -20
        _MaxElevation ("Max Elevation (Roughness)", Float) = 200
    }

    SubShader {
        Tags {
            "RenderType"="Opaque"
        }

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                float4 pos : POSITION;
                float2 uv  : TEXCOORD0;
            };

            /// the fragment shader input
            struct FragIn {
                float4 cPos : SV_POSITION;
                float2 uv   : TEXCOORD0;
            };

            // -- props --
            /// the integer offset of this chunk
            float2 _Offset;

            // the scale for the floor noise
            float _FloorScale;

            // the minimum height of the floor
            float _MinFloor;

            // the maximum height of the floor
            float _MaxFloor;

            // the scale for the elevation noise
            float _ElevationScale;

            // the minimum elevation amount
            float _MinElevation;

            // the maximum elevation amound
            float _MaxElevation;

            // the maximum height of the terrain
            float _TerrainHeight;

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

                float c = 0.0f;
                c  = 0.5000f * Noise(st);
                st = mul(st, m);
                c += 0.2500f * Noise(st);
                st = mul(st, m);
                c += 0.1250f * Noise(st);
                st = mul(st, m);
                c += 0.0625f * Noise(st);
                st = mul(st, m);

                return c;
            }

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.cPos = UnityObjectToClipPos(v.pos);
                o.uv = v.uv;

                return o;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                // add coordinate offset
                float2 st = f.uv + _Offset;

                // generate image
                float floor = lerp(_MinFloor, _MaxFloor, Image(st * _FloorScale + float2(420, 420))) / _TerrainHeight;
                float elevation = saturate(lerp(_MinElevation, _MaxElevation, Image(st * _ElevationScale)) / _TerrainHeight);

                // scale by max terrain height
                float c = floor + elevation;

                // produce color
                return fixed4(c, c, c, 1.0f);
            }
            ENDCG
        }
    }
}
