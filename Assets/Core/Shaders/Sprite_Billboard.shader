Shader "Sprite/Billboard" {
    Properties {
        _MainTex ("Sprite", 2D) = "white" {}
        _Saturation ("Saturation", Float) = 1.0

        [Header(Rendering)]
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2
        [Enum(Off,0,On,1)] _ZWrite("ZWrite", Int) = 1
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
        Blend One OneMinusSrcAlpha

        // -- program --
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Core/Color.cginc"

            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;

            float4 _MainTex_ST;

            // how saturated the sprite is [0,1]
            float _Saturation;

            FragIn vert(VertIn i)
            {
                FragIn o;
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);

                //copy them so we can change them (demonstration purpos only)
                float4x4 m = UNITY_MATRIX_M;
                float4x4 v = UNITY_MATRIX_V;
                float4x4 p = UNITY_MATRIX_P;

                //break out the axis
                float3 right = normalize(v._m00_m01_m02);
                float3 up = float3(0, 1, 0);
                float3 forward = normalize(v._m20_m21_m22);
                //get the rotation parts of the matrix
                float4x4 rotationMatrix = float4x4(
                    right, 0,
                    up, 0,
                    forward, 0,
                    0, 0, 0, 1
                );

                //the inverse of a rotation matrix happens to always be the transpose
                float4x4 rotationMatrixInverse = transpose(rotationMatrix);

                //apply the rotationMatrixInverse, model, view and projection matrix
                float4 pos = i.vertex;
                pos = mul(rotationMatrixInverse, pos);
                pos = mul(m, pos);
                pos = mul(v, pos);
                pos = mul(p, pos);
                o.vertex = pos;

                return o;
            }

            fixed4 frag(FragIn IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.uv);
                c = fixed4(c.rgb * _Saturation, c.a);
                clip(c.a - 0.5f);
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct VertIn {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            FragIn DrawVert(VertIn i) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = i.uv;
                return o;
            }

            float4 DrawFrag(FragIn i) : SV_TARGET {
                fixed4 c = tex2D(_MainTex, i.uv);
                clip(c.a - 0.5);
                return 0;
            }

            ENDCG
        }
    }
}