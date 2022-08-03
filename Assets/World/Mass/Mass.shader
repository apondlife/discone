Shader "Custom/Sculpture" {
    Properties {
        _Tex1 ("Texture 1", 2D) = "white" {}
        _Tex2 ("Texture 2", 2D) = "white" {}
        [ShowAsVector2] _HeightSpan ("Height Span", Vector) = (0.0, 0.0, 0.0, 0.0)
        [ShowAsVector2] _ViewDistSpan ("View Dist Span", Vector) = (0.0, 0.0, 0.0, 0.0)
        [ShowAsVector2] _HueSpanSrc ("Hue Span (Src)", Vector) = (0.0, 0.0, 0.0, 0.0)
        [ShowAsVector2] _HueSpanDst ("Hue Span (Dst)", Vector) = (0.0, 0.0, 0.0, 0.0)
        [ShowAsVector2] _SatSpan ("Sat Span", Vector) = (0.0, 0.0, 0.0, 0.0)
        [ShowAsVector2] _ValSpan ("Val Span", Vector) = (0.0, 0.0, 0.0, 0.0)
    }

    SubShader {
        Tags {
            "RenderType"="Opaque"
        }

        Pass {
            Tags {
                "LightMode" = "ForwardBase"
                "TerrainCompatible" = "True"
            }

            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            // -- includes --
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Assets/Shaders/Core/Color.cginc"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                // NOTE: shadow shader macros require this name
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            /// the fragment shader input
            struct FragIn {
                // NOTE: shadow shader macros require this name
                float4 pos : SV_POSITION;
                float2 uv1 : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
                float3 pct : TEXCOORD2;
                LIGHTING_COORDS(3,4)
                UNITY_FOG_COORDS(5)
            };

            // -- props --
            /// the 1st texture
            sampler2D _Tex1;

            /// the 1st texture offset and tiling
            float4 _Tex1_ST;

            /// the 2nd texture
            sampler2D _Tex2;

            /// the 2nd texture offset and tiling
            float4 _Tex2_ST;

            /// the world height span of the object
            float2 _HeightSpan;

            /// the view distance span
            float2 _ViewDistSpan;

            /// the source hue span
            float2 _HueSpanSrc;

            /// the destination hue span
            float2 _HueSpanDst;

            /// the saturation span
            float2 _SatSpan;

            /// the value span
            float2 _ValSpan;

            // -- program --
            FragIn DrawVert(VertIn v) {
                float3 wPos = mul(unity_ObjectToWorld, v.vertex);

                FragIn f;
                f.pos = UnityObjectToClipPos(v.vertex);

                // calculate pct vector
                f.pct.y = Unlerp(_HeightSpan.x, _HeightSpan.y, wPos.y);
                f.pct.z = UnlerpSpan(_ViewDistSpan, distance(_WorldSpaceCameraPos, wPos));

                // build uvs (override y with height pct)
                f.uv1 = v.uv * _Tex1_ST.xy + _Tex1_ST.zw;
                f.uv1.y = f.pct.y;

                f.uv2 = v.uv * _Tex2_ST.xy + _Tex2_ST.zw;
                // f.uv2.y = f.pct.y;

                TRANSFER_VERTEX_TO_FRAGMENT(f);
                UNITY_TRANSFER_FOG(f, f.pos);

                return f;
            }

            fixed4 DrawFrag(FragIn f) : SV_Target {
                // sample textures
                float3 tex1 = tex2D(_Tex1, f.uv1).rgb;
                float3 tex2 = tex2D(_Tex2, f.uv2).rgb;

                // sample hues based from spans
                float1 huePct = f.pct.y;
                float1 hueSrc = fmod(LerpSpan(_HueSpanSrc, huePct), 1.0f);
                float1 hueDst = fmod(LerpSpan(_HueSpanDst, huePct), 1.0f);

                // mix based on some other factor
                float1 hueMix = GetLuminance(tex1);
                float1 hue = lerp(hueSrc, hueDst, hueMix);

                // sample saturation based on view distance
                float1 satPct = 1.0f - tex2.r;
                float1 sat = LerpSpan(_SatSpan, satPct);

                // sample saturation based on view distance
                float1 valPct = satPct;
                float1 val = LerpSpan(_ValSpan, valPct);

                // produce color
                fixed4 col = fixed4(IntoRgb(float3(hue, sat, val)), 1.0f);

                // apply lighting & fog
                col *= LIGHT_ATTENUATION(f);
                UNITY_APPLY_FOG(f.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
    Fallback "VertexLit"
}
