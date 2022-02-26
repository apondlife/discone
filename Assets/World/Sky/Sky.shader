Shader "Custom/Sky" {

Properties {
    _MainTex ("Texture", 2D) = "grey" {}
    _Scale ("Scale", Float) = 1.0
    _Rotation ("Rotation", Range(0.0, 360.0)) = 0.0
    _Foreground ("Foreground", Color) = (0.5, 0.5, 0.5, 1.0)
    _Background ("Background", Color) = (0.0, 0.0, 0.0, 1.0)
    [Gamma] _Exposure ("Exposure", Range(0.0, 8.0)) = 1.0
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
        #pragma vertex DrawVert
        #pragma fragment DrawFrag
        #pragma target 2.0

        #include "UnityCG.cginc"

        sampler2D _MainTex;
        float _Rotation;
        float _Scale;

        half4 _Foreground;
        half4 _Background;
        half _Exposure;

        half4 _Fog;
        float _FogMin;
        float _FogHeight;

        inline float2 ToRadialCoords(float3 coords)
        {
            float3 normalizedCoords = normalize(coords);
            float latitude = acos(normalizedCoords.y);
            float longitude = atan2(normalizedCoords.z, normalizedCoords.x);
            float2 sphereCoords = float2(longitude, latitude) * float2(0.5/UNITY_PI, 1.0/UNITY_PI);
            return float2(0.5,1.0) - sphereCoords;
        }

        float3 RotateAroundYInDegrees (float3 vertex, float degrees)
        {
            float alpha = degrees * UNITY_PI / 180.0;
            float sina, cosa;
            sincos(alpha, sina, cosa);
            float2x2 m = float2x2(cosa, -sina, sina, cosa);
            return float3(mul(m, vertex.xz), vertex.y).xzy;
        }

        struct VertIn {
            float4 vertex : POSITION;
        };

        struct FragIn {
            float4 vertex : SV_POSITION;
            float3 texcoord : TEXCOORD0;
            float4 layout3DScaleAndOffset : TEXCOORD1;
            float wPosY : TEXCOORD2;
        };

        FragIn DrawVert(VertIn v) {
            FragIn o;

            float3 rotated = RotateAroundYInDegrees(v.vertex, _Rotation);
            o.vertex = UnityObjectToClipPos(rotated);
            o.texcoord = v.vertex.xyz;

            // Calculate constant scale and offset for 3D layouts
            // Over-Under 3D layout
            o.layout3DScaleAndOffset = float4(0.0f, 1.0f - unity_StereoEyeIndex, 1.0f, 0.5f);
            o.wPosY = mul(unity_ObjectToWorld, v.vertex).y;

            return o;
        }

        fixed4 DrawFrag (FragIn i) : SV_Target {
            float2 tc = ToRadialCoords(i.texcoord);
            tc.x = fmod(tc.x, 1.0f);
            tc = (tc + i.layout3DScaleAndOffset.xy) * i.layout3DScaleAndOffset.zw;

            half3 c = tex2D(_MainTex, tc * _Scale);
            c = lerp(_Foreground.rgb, _Background.rgb, c.r);
            c *= _Exposure;

            // apply fog
            float fog = max(1.0f - (i.wPosY - _FogMin) / _FogHeight, 0.0f);
            fog = fog > 1.0f ? 0.0f : fog;
            fog *= fog * fog;
            c = lerp(c, _Fog, fog);

            return half4(c, 1.0f);
        }
        ENDCG
    }
}

Fallback Off

}
