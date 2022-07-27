Shader "Custom/SkyChartBody" {
    Properties {
        _Texture ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
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
            sampler2D _Texture;

            /// the texture coordinate
            float4 _Texture_ST;

            /// the texture color
            fixed4 _Color;

            // -- program --
            FragIn DrawVert (VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Texture);
                return o;
            }

            fixed4 DrawFrag (FragIn i) : SV_Target {
                return tex2D(_Texture, i.uv) * _Color;
            }
            ENDCG
        }
    }
}