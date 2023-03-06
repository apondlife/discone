Shader "ThirdPerson/Character" {
    Properties {
        _DistortionDirection ("Distortion Direction", Vector) = (0, 1, 0, 0)
        _DistortionScale ("Distortion Scale", Float) = 1
        _Remap ("Remap", Float) = 1
    }

    SubShader {
        Tags {
            "RenderType" = "Opaque"
        }

        LOD 100

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "Assets/Shaders/Core/Color.hlsl"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                float4 vertex : POSITION;
            };

            /// the fragment shader input
            struct FragIn {
                // NOTE: shadow shader macros require this name
                float4 pos : SV_POSITION;
                float3 test : TEXCOORD0;
            };

            // -- props --
            float3 _DistortionDirection;
            float _DistortionScale;
            float _Remap;

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn f;

                float3 test = v.vertex * float3r(_Remap);
                float3 ddir = normalize(_DistortionDirection);
                float3 t = float3r(0.5) * ddir;

                test = test * t + t;

                test = test * _DistortionScale * ddir;

                test = (test - t) / max(t, float3r(0.0001));

                test = test / float3r(_Remap);

                test = v.vertex * (float3r(1.0) - ddir) + test * ddir;
                f.test = test;

                f.pos = UnityObjectToClipPos(test);
                // f.pos = UnityObjectToClipPos(v.vertex);
                return f;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                return fixed4(f.test.rgb, 1.0);
            }
            ENDCG
        }
    }
}
