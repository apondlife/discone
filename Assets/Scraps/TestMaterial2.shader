Shader "Custom/TestMaterial2" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff("Alpha cutoff", Range(0,1)) = 0.5
        m_BackfaceTex ("Backface Texture", 2D) = "white" {}
        m_BackfaceAlpha("BackfaceAlpha", float) = 0.5
    }

    SubShader {
        Tags {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
        }

        LOD 100
        Cull Off

        Pass {
            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            #pragma multi_compile_fog

            // -- includes --
            #include "UnityCG.cginc"
            #include "Assets/Shaders/Core/Math.cginc"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float2 uv : TEXCOORD0;
                float2 buv : TEXCOORD2;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // -- props --
            /// the main texture
            sampler2D _MainTex;
            sampler2D m_BackfaceTex;

            /// the texture coordinate
            float4 _MainTex_ST;
            float4 m_BackfaceTex_ST;
            float m_BackfaceAlpha;
            float _Cutoff;

            // -- program --
            FragIn DrawVert (VertIn v) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.buv = TRANSFORM_TEX(v.uv, m_BackfaceTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 DrawFrag (FragIn i, float facing : VFACE) : SV_Target {
                fixed4 colFront = tex2D(_MainTex, i.uv);
                fixed4 colBack = tex2D(m_BackfaceTex, i.buv);
                colBack = fixed4(Rand(i.buv * 2), Rand(i.buv * 3), Rand(i.buv * 4), 1);
                fixed4 col = lerp(colBack, colFront, facing);
                UNITY_APPLY_FOG(i.fogCoord, col);
                if (facing < 0.5) {
                    clip(Rand(i.uv)  - m_BackfaceAlpha);
                }
                // clip(col.a - _Cutoff);
                return fixed4(col.r, col.g, col.b, col.a);
            }
            ENDCG
        }
    }
}
