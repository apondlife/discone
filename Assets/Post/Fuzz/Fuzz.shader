Shader "Image/Fuzz" {
    Properties {
        _Texture ("Texture", 2D) = "grey" {}
        _TextureScale ("Texture Scale", Float) = 1.0
        _MaxOrthogonality ("Max Orthogonality", Range(0.0, 1.0)) = 1.0
        _DepthScale ("Depth Scale", Float) = 0.0
        _HueShift ("Hue Shift", Range(-1.0, 1.0)) = 0.0
        _SaturationShift ("Saturation Shift", Range(-1.0, 1.0)) = 0.0
        _ValueShift ("Value Shift", Range(-1.0, 1.0)) = 0.0

        _DepthPower ("Depth Power", Range(0, 1.0)) = 1
        _DissolveDepthMin ("Dissolve Depth Min", Range(0, 1.0)) = 0.8
        _DissolveDepthMax ("Dissolve Depth Max", Range(0, 1.0)) = 1.0
        _NoiseTimeScale ("Noise Time Scale", Float) = 0.1
        _NoiseScale ("Noise Scale", Float) = 1000

        _FuzzOffset ("Fuzz Offset", float) = 0.1
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
            HLSLPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "Assets/Shaders/Core/Math.hlsl"
            #include "Assets/Shaders/Core/Color.hlsl"
            #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            // -- constants --
            /// a right direction vector
            static const float3 k_Right = float3(1.0f, 0.0f, 0.0f);

            /// the fragment width of the sobel cookie
            static const int k_CookieWidth = 3;

            /// the number of fragments in the sobel cookie
            static const int k_CookieSize = k_CookieWidth * k_CookieWidth;

            /// the scale applied to a single fragement in the sobel value
            static const float1 k_CookieScale = 1.0f / k_CookieSize;

            /// the horizontal sobel kernel
            static const float3x3 k_KernelH = {
                +1.0f, -0.0f, -1.0f,
                +2.0f, +0.0f, -2.0f,
                +1.0f, -0.0f, -1.0f
            };

            /// the vertical sobel kernel
            static const float3x3 k_KernelV = {
                +1.0f, +2.0f, +1.0f,
                +0.0f, +0.0f, -0.0f,
                -1.0f, -2.0f, -1.0f
            };

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
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
            /// the depth & normals texture; set by unity if the camera uses DepthTextureMode.DepthNormals
            TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

            /// the depth & normals texture; set by unity if the camera uses DepthTextureMode.DepthNormals
            TEXTURE2D_SAMPLER2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture);
            TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

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

            // the power to transform depth with
            float _DepthPower;

            // the speed the noise changes
            float _NoiseTimeScale;

            float _NoiseScale;

            // where the world should start and stop dissolving
            float _DissolveDepthMin;
            float _DissolveDepthMax;

            float _FuzzOffset;

            // the fragment distance for convolution
            float _ConvolutionDelta;
            float _HueScale;
            float _SaturationScale;
            float _ValueScale;

            // -- declarations --
            DepthNormal SampleDepthNormal(float2 uv);

            // -- program --
            FragIn DrawVert(VertIn v) {
                // mostly copied from VertDefault in StdLib.hlsl
                FragIn f;
                f.vertex = float4(v.vertex.xy, 0.0f, 1.0f);
                f.uv = TransformTriangleVertexToUV(v.vertex.xy);

                #if UNITY_UV_STARTS_AT_TOP
                f.uv = f.uv * float2(1.0f, -1.0f) + float2(0.0f, 1.0f);
                #endif

                return f;
            }

            float4 DrawFrag(FragIn f) : SV_Target {
                // fuzz texture color based on its horizontalness
                float4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, f.uv);
                float3 col = tex.rgb;
                float3 hsv = IntoHsv(col);
                DepthNormal dn0 = SampleDepthNormal(f.uv);

                // convolution values
                float conv_ch = 0.0f;
                float conv_cv = 0.0f;
                float conv_dh = 0.0f;
                float conv_dv = 0.0f;

                // offset every sample by half the delta
                float1 offmag = -_ConvolutionDelta * 0.5f;
                float2 offset = float2(offmag, offmag);

                // aggregate every value in the cookie
                for (int j = 0; j < k_CookieSize; j++) {
                    int x = j % k_CookieWidth;
                    int y = j / k_CookieWidth;

                    // offset to position in cookie
                    float2 pos = f.uv + float2(x, y) * _ConvolutionDelta + offset;

                    DepthNormal dn = SampleDepthNormal(pos);
                    float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pos);
                    float3 hsv = IntoHsv(col);

                    // convolve the hsv color's value (brightness)
                    float c = hsv.z;
                    conv_ch += c * k_KernelH[x][y];
                    conv_cv += c * k_KernelV[x][y];

                    // convolve the depth
                    float sd = log2(dn.depth);
                    conv_dh += sd * k_KernelH[x][y];
                    conv_dv += sd * k_KernelV[x][y];
                }

                // lookup depth and normal at uv
                float sobel_c = sqrt(conv_ch * conv_ch + conv_cv * conv_cv);
                float sobel_d = sqrt(conv_dh * conv_dh + conv_dv * conv_dv);
                float sobel = max(sobel_c, sobel_d);

                // noise shit up
                float4 other_tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, f.uv + float2(Rand(f.uv) - 0.5, Rand(f.uv + 0.69f) - 0.5) * _FuzzOffset);
                float3 other_col = other_tex.rgb;

                // the actual good return value here v
                // return float4(lerp(col, lerp(col, other_col, sobel), step(tex2D(_Texture, f.uv).r, sobel)), 1.0f);
                col = lerp(col, other_col, step(tex2D(_Texture, f.uv).r, sobel));

                // get max fuzz angle
                // float nmax = _MaxOrthogonality * _DepthScale * depth;
                float nmax = _MaxOrthogonality;

                // check how orthogonal the normal is to forward in view space
                float fuzz = abs(dot(dn0.normal, float3(0.0f, 0.0f, 1.0f)));
                fuzz = 1.0f - Unlerp(0.0f, nmax, fuzz);
                fuzz = saturate(fuzz);

                // fuzz more based on the depth
                fuzz *= saturate(dn0.depth * _DepthScale);
                fuzz *= tex2D(_Texture, f.uv * _TextureScale).r;

                // fuzz between the base and shifted color
                col = lerp(col, IntoRgb(hsv).rgb, fuzz);

                // don't dissolve objects that dont write to the depth buffer
                if (tex.a > 0.0f && dn0.depth >= 1.0f) {
                    return float4(col, tex.a);
                }

                // dissolve far away objects
                float1 pct = saturate(Unlerp(_DissolveDepthMin, _DissolveDepthMax, dn0.depth));
                float2 uvn = _NoiseScale * f.uv;

                float1 a = other_tex.a * step(
                    pow(pct, _DepthPower),
                    0.997f * (0.5f + 0.5f * SimplexNoise(float3(uvn, 0.01f * floor(_Time.y * _NoiseTimeScale)))) + 0.002f // this number is magic; it avoids dropping close pixels
                );

                return float4(col, a);
            }

            // -- helpers --
            DepthNormal SampleDepthNormal(float2 uv) {
                // sample texture
                float4 enc = SAMPLE_TEXTURE2D(_CameraDepthNormalsTexture, sampler_CameraDepthNormalsTexture, uv);
                float4 d = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, uv);

                // decode values
                DepthNormal dn;

                // see UnityCG.cginc: DecodeDepthNormal & DecodeFloatRG
                dn.depth = Linear01Depth(d);//dot(enc.zw, float2(1.0f, 1.0f / 255.0f));
                dn.normal = DecodeViewNormalStereo(enc);

                return dn;
            }
            ENDHLSL
        }
    }
}
