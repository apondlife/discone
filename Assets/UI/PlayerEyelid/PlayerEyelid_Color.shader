Shader "Discone/PlayerEyelid_Color" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Intensity ("Intensity", Vector) = (0.056, 0.003, 0.003, 1)
    }

    SubShader {
        // tags and stuff taken from
        // https://www.patreon.com/posts/shaders-for-who-29239797
        Tags {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass {
            CGPROGRAM
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
            /// the game texture
            sampler2D _MainTex;

            /// the game texture scale/translation
            float4 _MainTex_ST;

            /// the intensity of the filter
            float3 _Intensity;

            // -- program --
            FragIn DrawVert (VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 DrawFrag (FragIn i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= _Intensity;
                return col;
            }
            ENDCG
        }
    }
}
