Shader "Custom/Sky" {

Properties {
    _MainTex ("Texture", 2D) = "grey" {}
    _Scale ("Scale", Float) = 1.0
    _Rotation ("Rotation", Range(0.0, 360.0)) = 0.0
    _Foreground ("Foreground", Color) = (0.5, 0.5, 0.5, 1.0)
    [Gamma] _ExposureForeground ("Exposure Foreground", Range(0.0, 8.0)) = 1.0
    _Background ("Background", Color) = (0.0, 0.0, 0.0, 1.0)
    [Gamma] _ExposureBackground ("Exposure Background", Range(0.0, 8.0)) = 1.0
    _Fog ("Fog", Color) = (0.0, 0.0, 0.0, 0.0)
    _FogMin ("Fog Min (Horizon)", Float) = 100.0
    _FogHeight ("Fog Height", Float) = 20.0
}

SubShader {
    Tags {
        "Queue"       = "Background"
        "RenderType"  = "Background"
        "PreviewType" = "Skybox"
    }

    Cull Off
    ZWrite Off

    Pass {
        CGPROGRAM

        // -- config --
        #pragma vertex DrawVert
        #pragma fragment DrawFrag
        #pragma target 2.0

        // -- includes --
        #include "UnityCG.cginc"

        // -- constants --
        /// i don't know what this is, it's for the "over-under 3d layout"
        static float4 kLayout = float4(0.0f, 1.0f - unity_StereoEyeIndex, 1.0f, 0.5f);

        // -- props --
        /// the texture
        sampler2D _MainTex;

        /// the texture rotation
        float _Rotation;

        /// the texture scale
        float _Scale;

        /// the foreground color (black in texture)
        half4 _Foreground;

        /// the background color (white in texture)
        half4 _Background;

        /// the color exposure (gamma multiplier)
        half _ExposureBackground;
        half _ExposureForeground;

        /// the fog color
        half4 _Fog;

        /// the world y-position to begin fog
        float _FogMin;

        /// the height of the fog gradient
        float _FogHeight;

        // -- helpers --
        /// rotate a position around the y-axis
        /// TODO: why is this fn not inline
        float3 RotateY(float3 pos, float degrees) {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, pos.xz), pos.y).xzy;
        }

        inline float2 IntoSpherical(float3 pos) {
        }

        /// convert cartesian coordinate into radial (spherical?) coord
        /// TODO: this math is similar to (but the arguments differ from):
        /// https://en.wikipedia.org/wiki/Spherical_coordinate_system#Cartesian_coordinates
        inline float2 IntoRadial(float3 pos) {
            float3 normalized = normalize(pos);
            float lat = acos(normalized.y);
            float lon = atan2(normalized.z, normalized.x);
            float2 spherical = float2(lon, lat) * float2(0.5f / UNITY_PI, 1.0f / UNITY_PI);
            return float2(0.5f, 1.0f) - spherical;
        }

        // -- types --
        /// the vertex shader input
        struct VertIn {
            float4 pos : POSITION;
        };

        /// the fragment shader input
        struct FragIn {
            float4 cPos : SV_POSITION;
            float3 oPos : TEXCOORD0;
            float wPosY : TEXCOORD1;
        };

        /// the vertex shader
        FragIn DrawVert(VertIn i) {
            FragIn o;

            float3 rotated = RotateY(i.pos, _Rotation);
            o.cPos = UnityObjectToClipPos(rotated);
            o.oPos = i.pos.xyz;
            o.wPosY = mul(unity_ObjectToWorld, i.pos).y;

            return o;
        }

        /// the fragment shader
        fixed4 DrawFrag(FragIn i): SV_Target {
            // get the radial texture coordinate and do weird math i don't understand
            float2 tc = IntoRadial(i.oPos);
            tc.x = fmod(tc.x, 1.0f);
            tc = (tc + kLayout.xy) * kLayout.zw;
            tc *= _Scale;

            // sample a color from the texture
            half3 c = tex2D(_MainTex, tc);
            c = lerp(_Foreground.rgb, _Background.rgb, c.r);
            c *= lerp(_ExposureBackground, _ExposureForeground, c.r);

            // apply fog
            // float fog = max(1.0f - (i.wPosY - _FogMin) / _FogHeight, 0.0f);
            // fog = fog > 1.0f ? 0.0f : fog;
            // fog = fog * fog * fog;
            // c = lerp(c, _Fog, fog);

            return half4(c, 1.0f);
        }

        ENDCG
    }
}

Fallback Off

}
