Shader "Custom/InclineShader" {
    Properties {
        [Header(Surface)]
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        [Space]
        [Header(Texture)]
        _MainTex ("Texture", 2D) = "gray" {}

        [KeywordEnum(None, Multiply, Luminosity)]
        _Blend ("Blend Mode", Float) = 0

        [Space]
        [Header(Angles)]
        _Epsilon ("Epsilon", Range(0, 0.1)) = 0.01
        _WallAngle ("Wall Angle (deg)", Range(0, 90)) = 80
        _RampAngle ("Ramp Angle (deg)", Range(0, 90)) = 10

        [Space]
        [Header(Colors)]
        [HDR] _FloorColor ("Floor", Color) = (1, 1, 1, 1)
        [HDR] _ShallowFloorColor ("Floor (Shallow)", Color) = (1, 1, 1, 1)
        [HDR] _PositiveRampColor ("Ramp (Positive)", Color) = (1, 1, 1, 1)
        [HDR] _PositiveWallColor ("Wall (Positive)", Color) = (1, 1, 1, 1)
        [HDR] _WallColor ("Wall (Flat)", Color) = (1, 1, 1, 1)
        [HDR] _NegativeWallColor ("Wall (Negative)", Color) = (1, 1, 1, 1)
        [HDR] _NegativeRampColor ("Ramp (Negative)", Color) = (1, 1, 1, 1)
        [HDR] _ShallowCeilingColor ("Ceiling (Shallow)", Color) = (1, 1, 1, 1)
        [HDR] _CeilingColor ("Ceiling", Color) = (1, 0, 1, 1)
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // -- options --
        // physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        // use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        // blend modes
        #pragma multi_compile _BLEND_NONE _BLEND_MULTIPLY _BLEND_LUMINOSITY

        // -- constants --
        const static float PI = 3.14159265f;
        const static float DEGTORAD = PI / 180.0f;

        // -- types --
        struct Input {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        // -- props --
        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;

        // -- p/angles
        // a near-zero value
        float _Epsilon;

        // the angle ramps start at
        float _RampAngle;

        // the angle walls start at
        float _WallAngle;

        // -- colors --
        // the color of a flat floor
        fixed4 _FloorColor;

        // the color of a floor w/ a slightly positive incline
        fixed4 _ShallowFloorColor;

        // the color of a ramp w/ a positive incline
        fixed4 _PositiveRampColor;

        // the color of a postive wall
        fixed4 _PositiveWallColor;

        // the color of a flat wall
        fixed4 _WallColor;

        // the color of a wall w/ a slightly negative incline
        fixed4 _NegativeWallColor;

        // the color of a ramp w/ a negative incline
        fixed4 _NegativeRampColor;

        // the color of a ceiling w/ a slightly negative incline
        fixed4 _ShallowCeilingColor;

        // the color of a flat ceiling
        fixed4 _CeilingColor;

        // see: https://docs.unity3d.com/Manual/GPUInstancing.html for more
        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        // -- declarations --
        fixed luminosity(fixed3 rgb);

        // -- program --
        void surf(Input i, inout SurfaceOutputStandard o) {
            fixed4 c;

            fixed a = dot(i.worldNormal, float3(0, 1, 0));
            fixed d = abs(a);

            fixed wall = cos(_WallAngle * DEGTORAD);
            fixed ramp = cos(_RampAngle * DEGTORAD);

            fixed4 flatColor = lerp(
                _CeilingColor,
                _FloorColor,
                step(0, a)
            );

            fixed4 shallowColor = lerp(
                _ShallowCeilingColor,
                _ShallowFloorColor,
                step(0, a)
            );

            fixed4 rampColor = lerp(
                _NegativeRampColor,
                _PositiveRampColor,
                step(0, a)
            );

            fixed4 wallColor = lerp(
                _NegativeWallColor,
                _PositiveWallColor,
                step(0, a)
            );

            // pick color based on angle
            c = shallowColor;
            c = lerp(c, rampColor, step(d, ramp));
            c = lerp(c, wallColor, step(d, wall));
            c = lerp(c, _WallColor, step(d, _Epsilon));
            c = lerp(c, flatColor, step(1 - _Epsilon, d));

            // get texture
            fixed4 t = tex2D(_MainTex, i.uv_MainTex);

            // blend texture as multiply
            #ifdef _BLEND_MULTIPLY
            c.rgb *= t.rgb;
            #endif

            // blend texture as luminosity
            #ifdef _BLEND_LUMINOSITY
            fixed lum = luminosity(t);
            fixed delta = lum - luminosity(c);
            fixed3 rgb = fixed3(
                c.r + delta,
                c.g + delta,
                c.b + delta
            );

            fixed lum1 = luminosity(rgb);
            fixed3 lum3 = fixed3(lum1, lum1, lum1);

            fixed min1 = min(rgb.r, min(rgb.g, rgb.b));
            if (min1 < 0.0f) {
                rgb = lum3 + ((rgb - lum3) * lum3) / (lum3 - fixed3(min1, min1, min1));
            }

            fixed max1 = max(rgb.r, max(rgb.g, rgb.b));
            if (max1 > 1.0f) {
                rgb = lum3 + ((rgb - lum3) * (fixed3(1, 1, 1) - lum3)) / (fixed3(max1, max1, max1) - lum3);
            }

            c.rgb = rgb;
            #endif

            // set surface properties
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            c.rgb = luminosity(c.rgb);
        }

        // -- helpers --
        fixed luminosity(fixed3 c) {
            return 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
