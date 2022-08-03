Shader "Custom/SkyChart/Body" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
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
            #include "UnityCG.cginc"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // -- props --
            /// the texture
            sampler2D _MainTex;

            /// the texture coordinate
            float4 _MainTex_ST;

            /// the texture color
            fixed4 _Color;

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 DrawFrag(FragIn i): SV_Target {
                fixed4 c = tex2D(_MainTex, i.uv) * _Color;
                return fixed4(c.rgb, 1.0f);
            }
            ENDCG
        }
    }
}