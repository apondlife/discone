Shader "Custom/Fuzz" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader {
        // No culling or depth
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
            sampler2D _MainTex;

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 DrawFrag(FragIn i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
    }
}
