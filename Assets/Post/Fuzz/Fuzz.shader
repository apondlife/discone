Shader "Image/Fuzz" {
    Properties {
        _Texture ("Texture", 2D) = "grey" {}
        _TextureScale ("Texture Scale", Float) = 1.0
        _MaxOrthogonality ("Max Orthogonality", Range(0.0, 1.0)) = 1.0
        _DepthScale ("Depth Scale", Float) = 0.0
        _HueShift ("Hue Shift", Range(-1.0, 1.0)) = 0.0
        _SaturationShift ("Saturation Shift", Range(-1.0, 1.0)) = 0.0
        _ValueShift ("Value Shift", Range(-1.0, 1.0)) = 0.0

        _DissolveDepth ("Dissolve Depth", Range(0, 1.0)) = 0.1
        _DissolveBand ("Dissolve Band", Range(0, 1.0)) = 0.1

        _FuzzOffset ("Fuzz Offset", float) = 0.1
        _ConvolutionOffsetX ("Convolution Offset X", Range(-1.0, 1.0)) = 0.0
        _ConvolutionOffsetY ("Convolution Offset Y", Range(-1.0, 1.0)) = 0.0
        _ConvolutionDelta ("Convolution Delta", Range(0, 1.0)) = 0.1
        _HueScale ("Hue Scale", Range(0, 1.0)) = 0.1
        _SaturationScale ("Saturation Scale", Range(0, 1.0)) = 0.1
        _ValueScale ("Value Scale", Range(0, 1.0)) = 0.1
    }

    SubShader {
        // no culling or depth
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
            #include "../Core/Math.cginc"
            #include "../Core/Color.cginc"

            // -- defines --
            #define float1 float

            // -- constants --
            const float3 k_Right = float3(1.0f, 0.0f, 0.0f);

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            struct DepthNormal {
                float1 depth;
                float3 normal;
            };

            // -- props --
            /// the main texture; set by unity to the screen buffer if used in an effect
            sampler2D _MainTex;

            /// the depth & normals texture; set by unity if the camera uses DepthTextureMode.DepthNormals
            sampler2D _CameraDepthNormalsTexture;

            /// the fuzz texture to apply to the edges
            sampler2D _Texture;

            /// the scale to apply to the fuzz texture
            float _TextureScale;

            /// the max <normal • forward> value to apply fuzzing
            float _MaxOrthogonality;

            /// how much depth influences the maximum fuzz angle
            float _DepthScale;

            /// the amount to shift the fuzz hue by
            float _HueShift;

            /// the amount to shift the fuzz saturation by
            float _SaturationShift;

            /// the amount to shift the fuzz value by
            float _ValueShift;

            // the size of the dissolve band
            float _DissolveBand;

            // where the world should start dissolving
            float _DissolveDepth;

            float _FuzzOffset;
            float _ConvolutionOffsetX;
            float _ConvolutionOffsetY;

            // the fragment distance for convolution
            float _ConvolutionDelta;
            float _HueScale;
            float _SaturationScale;
            float _ValueScale;

            // -- declarations --
            DepthNormal SampleDepthNormal(float2 uv);

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 DrawFrag(FragIn i) : SV_Target {
                // fuzz texture color based on its horizontalness
                fixed4 tex = tex2D(_MainTex, i.uv);
                fixed3 col = tex.rgb;
                fixed3 hsv = IntoHsv(col);
                DepthNormal dn0 = SampleDepthNormal(i.uv);

                // constatns
                const float3x3 _KernelH = {
                    +1.0f, -0.0f, -1.0f,
                    +2.0f, +0.0f, -2.0f,
                    +1.0f, -0.0f, -1.0f
                };

                const float3x3 _KernelV = {
                    +1.0f, +2.0f, +1.0f,
                    +0.0f, +0.0f, -0.0f,
                    -1.0f, -2.0f, -1.0f
                };

                float conv_ch = 0.0f;
                float conv_cv = 0.0f;
                float conv_dh = 0.0f;
                float conv_dv = 0.0f;
                float conv_nh = 0.0f;
                float conv_nv = 0.0f;

                float3 colavg = float3(0.0f, 0.0f, 0.0f);
                float3 hsvavg = float3(0.0f, 0.0f, 0.0f);
                float2 offset = float2(-_ConvolutionDelta/2.0f, -_ConvolutionDelta/2.0f);

                for (int j = 0; j < 9; j++) {
                    int x = j % 3;
                    int y = j / 3;
                    float2 pos = i.uv + float2(x, y) * _ConvolutionDelta + offset;

                    DepthNormal dn = SampleDepthNormal(pos);
                    float4 col = tex2D(_MainTex, pos);
                    fixed3 hsv = IntoHsv(col);

                    // conv += tex2D(_MainTex, i.uv + float2(x, y) * _Delta).r * _Kernel[x][y];
                    // convolution with color value
                    float c = hsv.z;
                    conv_ch += c * _KernelV[x][y];
                    conv_cv += c * _KernelH[x][y];

                    float sd = log2(dn.depth);
                    conv_dh += sd * _KernelV[x][y];
                    conv_dv += sd * _KernelH[x][y];
                    // conv_nh += abs(dot(dn.normal, float3(0.0f, 0.0f, 1.0f)));

                    // float4 col = tex2D(_MainTex, pos);
                    // fixed3 hsv = IntoHsv(col);
                    colavg += col;
                    hsvavg += hsv;
                }

                // average the hue
                colavg /= 9.0f;
                hsvavg /= 9.0f;

                // lookup depth and normal at uv
                float sobel_c = sqrt(conv_ch * conv_ch + conv_cv * conv_cv);
                float sobel_d = sqrt(conv_dh * conv_dh + conv_dv * conv_dv);
                float sobel = max(sobel_c, sobel_d);

                // noise shit up
                float4 other_col = tex2D(_MainTex, i.uv + float2(Rand(i.uv) - 0.5, Rand(i.uv + 0.69f) - 0.5) * _FuzzOffset);
                // float4 other_col = tex2D(_MainTex, i.uv + float2(tex2D(_Texture, i.uv).r - 0.5, tex2D(_Texture, i.uv + 0.69f).r - 0.5) * _FuzzOffset);

                // col = lerp(col, colavg, sobel);
                // hsv = float3(
                //     lerp(hsv.x, hsvavg.x, sobel * _HueScale),
                //     lerp(hsv.y, hsvavg.y, sobel * _SaturationScale),
                //     lerp(hsv.z, hsvavg.z, sobel * _ValueScale)
                // );
                // col = IntoRgb(hsv);
                // col.r += sobel * 0.1f;
                // col = lerp()
                // col = IntoRgb(float3(hsv2.x, hsv2.y, hsv.z));
                // hsv.x = lerp(hsv.x, hue, sobel);
                // return float4(lerp(col, lerp(col, other_col, sobel), step(tex2D(_Texture, i.uv).r, sobel)), 1.0f);
                return float4(lerp(col, other_col, step(tex2D(_Texture, i.uv).r, sobel)), 1.0f);
                return float4(col, 1.0f);
                return float4(sobel, sobel, sobel, 1.0f);
                // return float4(abs(conv_dh), 0.0f, abs(conv_dv), 1.0f);
                // return float4(conv_dh + 0.5f, 0.0f, conv_dv + 0.5f, 1.0f);

                // DepthNormal dn0 = SampleDepthNormal(i.uv);

                // get max fuzz angle
                // float nmax = _MaxOrthogonality * _DepthScale * depth;
                float nmax = _MaxOrthogonality;

                // check how orthogonal the normal is to forward in view space
                float fuzz = abs(dot(dn0.normal, float3(0.0f, 0.0f, 1.0f)));
                fuzz = 1.0f - Unlerp(0.0f, nmax, fuzz);
                fuzz = saturate(fuzz);

                // fuzz more based on the depth
                fuzz *= saturate(dn0.depth * _DepthScale);
                fuzz *= tex2D(_Texture, i.uv * _TextureScale).r;



                // fuzz between the base and shifted color
                col = lerp(col, IntoRgb(hsv), fuzz);

                // donst dissolve objects that dont write to the depth buffer
                if (tex.a > 0.0f && dn0.depth >= 1.0f) {
                    return fixed4(col, 1.0f);
                }

                // dissolve far away objects
                float a = 1.0f;
                float p = saturate(Unlerp(_DissolveDepth - _DissolveBand, _DissolveDepth, dn0.depth));
                a = step(p, 0.997f * Rand(i.uv + 0.1f * _Time.x) + 0.002f); // this number is magic; it avoids dropping close pixels

                return fixed4(col, a);
            }

            // -- helpers --
            DepthNormal SampleDepthNormal(float2 uv) {
                float1 depth;
                float3 normal;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, uv), depth, normal);

                DepthNormal o;
                o.depth = depth;
                o.normal = normal;

                return o;
            }
            ENDCG
        }
    }
}
