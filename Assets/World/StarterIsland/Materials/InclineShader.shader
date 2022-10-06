Shader "Custom/InclineShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Delta("Delta", Range(0, 0.1)) = 0.001
        _Steep("Steep Angle (deg)", Range(0, 90)) = 90
        _NotSteep("Not Steep Angle (deg)", Range(0, 90)) = 0
        [HDR] _FlatColor("FlatColor", Color) = (1, 1, 1, 1)
        [HDR] _FlatWallColor("FlatWallColor", Color) = (1, 1, 1, 1)
        [HDR] _SteepColor("SteepColor", Color) = (1, 1, 1, 1)
        [HDR] _NegativeSteepColor("NegativeSteepColor", Color) = (1, 1, 1, 1)
        [HDR] _RampColor("Ramp Color", Color) = (1, 1, 1, 1)
        [HDR] _NegativeRampColor("Negative Ramp Color", Color) = (1, 1, 1, 1)
        [HDR] _NonSteepFloor("NonSteepFloor", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;

        fixed4 _FlatColor;
        fixed4 _FlatWallColor;
        fixed4 _SteepColor;
        fixed4 _NegativeSteepColor;
        fixed4 _RampColor;
        fixed4 _NegativeRampColor;
        fixed4 _NonSteepFloor;

        float _Delta;
        float _Steep;
        float _NotSteep;

        const static float PI = 3.14159265;
        const static float DEGTORAD = PI / 180.0f;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 t = tex2D (_MainTex, IN.uv_MainTex);
            fixed4 c;
            fixed a = dot(IN.worldNormal, float3(0, 1, 0));
            fixed d = abs(a);

            fixed steep = cos(_Steep * DEGTORAD);
            fixed notsteep = cos(_NotSteep * DEGTORAD);
            if(d >= 1-_Delta) {
                c = _FlatColor;
            } else if(d < _Delta) {
                c = _FlatWallColor;
            } else if(d < steep) {
                if(a < 0) {
                    c = _NegativeSteepColor;
                } else {
                    c = _SteepColor;
                }
            } else if(d < notsteep) {
                if(a < 0) {
                    c = _NegativeRampColor;
                } else {
                    c = _RampColor;
                }
            } else {
                c = _NonSteepFloor;
            }

            c *= t;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
