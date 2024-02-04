Shader "Sprite/Billboard" {
    Properties {
        _MainTex ("Sprite", 2D) = "white" {}

        [Space]
        [Header(Effects)]
        [Space(5)]
        _Saturation ("Saturation", Float) = 1.0

        [Space]
        [Header(Rendering)]
        [Space(5)]
        _Cutoff ("Cutoff (Alpha)", Float) = 0.5
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2
        [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Int) = 4
    }

    SubShader {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        // -- flags --
        LOD 100
        Cull [_Culling]
        ZWrite [_ZWrite]
        ZTest [_ZTest]
        Lighting Off
        Blend Off

        // -- program --
        Pass {
            CGPROGRAM
            #pragma vertex DrawVert
            #pragma fragment DragFrag

            // -- includes --
            #include "UnityCG.cginc"
            #include "Assets/Shaders/Core/Color.cginc"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // -- props --
            // the texture
            sampler2D _MainTex;

            // the texture scale/transform
            float4 _MainTex_ST;

            // how saturated the sprite is [0,1]
            float1 _Saturation;

            // the cutoff before clipping
            float1 _Cutoff;

            // -- program --
            FragIn DrawVert(VertIn i) {
                FragIn o;
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);

                // grab the view rotation
                float4x4 v = UNITY_MATRIX_V;
                float3 forward = normalize(v._m20_m21_m22);
                float3 up = float3(0, 1, 0);
                float3 right = normalize(v._m00_m01_m02);

                // TODO: this doesn't work for rotated objects
                // and create an inverse rotation so that the objects end up facing the camera
                float4x4 rot = transpose(float4x4(
                    right,   0,
                    up,      0,
                    forward, 0,
                    0, 0, 0, 1
                ));

                // prerotate the object and then convert
                float4 pos = i.vertex;
                pos = mul(rot, pos);
                pos = UnityObjectToClipPos(pos);
                o.pos = pos;

                return o;
            }

            fixed4 DragFrag(FragIn IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.uv);
                c = fixed4(c.rgb * _Saturation, c.a);
                clip(c.a - _Cutoff);
                return c;
            }

            ENDCG
        }

        Pass {
            Tags {
                "LightMode" = "ShadowCaster"
            }

            CGPROGRAM
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // -- props --
            // the texture
            sampler2D _MainTex;

            // the texture scale/transform
            float4 _MainTex_ST;

            // the cutoff before clipping
            float1 _Cutoff;

            // -- tyeps --
            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // -- program --
            FragIn DrawVert(VertIn i) {
                FragIn o;
                o.pos = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                return o;
            }

            float4 DrawFrag(FragIn i) : SV_TARGET {
                fixed4 c = tex2D(_MainTex, i.uv);
                clip(c.a - _Cutoff);
                return 0;
            }

            ENDCG
        }
    }
}