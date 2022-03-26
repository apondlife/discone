Shader "Image/Fuzz" {
    Properties {
        _Texture ("Texture", 2D) = "grey" {}
        _TextureScale ("Texture Scale", Float) = 1.0
        _MaxOrthogonality ("Max Orthogonality", Range(0.0, 1.0)) = 1.0
        _DepthScale ("Depth Scale", Float) = 0.0
        _HueShift ("Hue Shift", Range(-1.0, 1.0)) = 0.0
        _SaturationShift ("Saturation Shift", Range(-1.0, 1.0)) = 0.0
        _ValueShift ("Value Shift", Range(-1.0, 1.0)) = 0.0
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

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

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 DrawFrag(FragIn i) : SV_Target {
                // lookup depth and normal at uv
                float1 depth;
                float3 normal;
                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.uv), depth, normal);

                // get max fuzz angle
                // float nmax = _MaxOrthogonality * _DepthScale * depth;
                float nmax = _MaxOrthogonality;

                // check how orthogonal the normal is to forward in view space
                float fuzz = abs(dot(normal, float3(0.0f, 0.0f, 1.0f)));
                fuzz = 1.0f - Unlerp(0.0f, nmax, fuzz);
                fuzz = saturate(fuzz);

                // fuzz more based on the depth
                fuzz *= saturate(depth * _DepthScale);
                fuzz *= tex2D(_Texture, i.uv * _TextureScale).r;

                // fuzz texture color based on its horizontalness
                fixed3 col = tex2D(_MainTex, i.uv).rgb;

                // hue shift the color from the source texture
                fixed3 hsv = IntoHsv(col);
                hsv[0] += _HueShift;
                hsv[1] += _SaturationShift;
                hsv[2] += _ValueShift;

                // fuzz between the base and shifted color
                col = lerp(col, IntoRgb(hsv), fuzz);

                return fixed4(col, 1.0f);
            }
            ENDCG
        }
    }
}