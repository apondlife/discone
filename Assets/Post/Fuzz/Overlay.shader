Shader "Image/Overlay" {
    Properties {
    }

    SubShader {
        // no culling or depth
        Cull Off
        ZWrite Off
        ZTest Always
        // Blend One One
        Blend SrcAlpha OneMinusSrcAlpha
        // Blend SrcAlpha Zero  // uncomment for cool effect

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- includes --
            #include "UnityCG.cginc"
            #include "../Core/Math.cginc"
            #include "../Core/Color.cginc"


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

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 DrawFrag(FragIn i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
