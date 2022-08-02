Shader "Custom/SkyChart/Stars" {
    Properties {
        _MainTex ("Noise", 2D) = "white" {}
        [ShowAsVector2] _Noise_Scale ("Noise Scale", Vector) = (1.0, 1.0, 0.0, 0.0)
        [ShowAsVector2] _Noise_Offset ("Noise Offset", Vector) = (0.0, 0.0, 0.0, 0.0)
        _Noise_Multiplier ("Noise Multiplier", Float) = 1.0
        [HDR] _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
    }

    SubShader {
        Tags {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
            #include "Assets/Shaders/Core/Math.cginc"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // -- props --
            /// the texture
            sampler2D _MainTex;

            /// the texture coordinate
            float4 _MainTex_ST;

            /// the texture color
            fixed4 _Color;

            /// the noise multiplier
            fixed2 _Noise_Scale;
            fixed2 _Noise_Offset;
            fixed1 _Noise_Multiplier;

            float1 Image(float2 st);

            // -- program --
            FragIn DrawVert (VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Noise_Scale + _Noise_Offset;
                return o;
            }

            fixed4 DrawFrag (FragIn i) : SV_Target {
                float1 noise = Image(i.uv).r;
                // noise = pow(noise, _Noise_Multiplier);
                noise *= _Noise_Multiplier;
                float1 a = 1.0f - noise;
                // float1 a = noise;
                return fixed4(_Color.rgb, _Color.a * a);
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

                // // shift range
                // c = c + 0.5f;

                // // apply banding
                // c = floor(c * _Bands) / (_Bands - 1.0f);

                return c;
            }
            ENDCG
        }
    }
}