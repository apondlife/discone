Shader "ThirdPerson/Character" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
            // "Queue" = "AlphaTest"
        }

        LOD 100

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityStandardUtils.cginc"
            #include "UnityLightingCommon.cginc"

            #include "Assets/Shaders/Core/Color.hlsl"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            /// the fragment shader input
            struct FragIn {
                // NOTE: shadow shader macros require this name
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed3 diffuse : COLOR0;
                fixed3 ambient : COLOR1;
                float4 vertexColor : COLOR2;
                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };

            sampler2D _MainTex;
            fixed4 _MainTex_ST;
            float1 _Epsilon;

            // -- props --
            float1 _Distortion_PositiveScale;
            float1 _Distortion_NegativeScale;
            float1 _Distortion_Intensity;
            float4 _Distortion_Plane;

            // the relative intensity of the reflected light
            float1 _ReflectedLightIntensity;

            // the relative intensity of the ambient light
            float1 _AmbientLightIntensity;

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float1 distance = dot(worldPos, _Distortion_Plane.xyz) + _Distortion_Plane.w;

                // the distortion...i don't know add a comment
                float1 distortion = lerp(
                    distance * -_Distortion_NegativeScale,
                    distance * +_Distortion_PositiveScale,
                    step(0, distance)
                );

                // intensity is in the range [0, 1, infinity]; 0 is fully squashed, 1 is no
                // distortion, infinity is infinitely stretched.
                float1 intensity = _Distortion_Intensity - 1;

                worldPos += distortion * intensity * _Distortion_Plane.xyz;
                o.pos = UnityWorldToClipPos(worldPos);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                // lambert shading
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed1 lightDotNormal = dot(worldNormal, _WorldSpaceLightPos0.xyz);
                fixed1 lightDotNormalMag = abs(lightDotNormal);
                fixed3 lightD = max(0, lightDotNormal) * _LightColor0.rgb;
                fixed3 lightR = max(0, -lightDotNormal) * _LightColor0.rgb * _ReflectedLightIntensity;

                o.diffuse = lightD + lightR;
                o.ambient = _LightColor0.rgb * lerp(0, _AmbientLightIntensity, 1 - lightDotNormalMag);

                return o;
            }

            fixed4 DrawFrag(FragIn IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.uv);
                clip(c.a - _Epsilon);

                // lighting (shading + shadows)
                fixed3 lighting = IN.diffuse * SHADOW_ATTENUATION(IN) + IN.ambient;
                c.rgb *= lighting;
                c.a = 1;

                return c;
            }
            ENDCG
        }
    }
}
