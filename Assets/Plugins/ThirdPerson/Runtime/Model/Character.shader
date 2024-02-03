Shader "ThirdPerson/Character" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        // TODO: split this into multiple variants
        [ShowAsVector2] _SpriteSheet ("Sprite Rows & Columns", Vector) = (1, 1, 0, 0)
        _CurrentSprite ("Current Sprite Index", Integer) = 0
    }

    SubShader {
        Pass {
            Tags {
            // -- options --
                "RenderType" = "Opaque"
                "LightMode" = "ForwardBase"
            }

            Lighting On
            ZWrite On

            // -- program --
            CGPROGRAM

            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            // -- includes --
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Shaders/Core/Color.hlsl"

            // -- types --
            /// the vertex shader input
            struct VertIn {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            /// the fragment shader input
            struct FragIn {
                // NOTE: shadow shader macros require this name
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed3 diffuse : COLOR0;
                fixed3 ambient : COLOR1;
                SHADOW_COORDS(1)
                UNITY_FOG_COORDS(2)
            };

            sampler2D _MainTex;
            fixed4 _MainTex_ST;

            // -- props --
            float1 _Distortion_PositiveScale;
            float1 _Distortion_NegativeScale;
            float1 _Distortion_Intensity;
            float4 _Distortion_Plane;

            // the number of rows and columns for a spritesheet
            float4 _SpriteSheet;

            // the current sprite index
            int _CurrentSprite;

            // the relative intensity of the reflected light
            float1 _ReflectedLightIntensity;

            // the relative intensity of the ambient light
            float1 _AmbientLightIntensity;

            // -- program --
            FragIn DrawVert(VertIn v) {
                FragIn o;

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float1 distance = dot(worldPos, _Distortion_Plane.xyz) + _Distortion_Plane.w;

                // the distortion...i don't know add a comment
                float1 distortion = lerp(
                    distance * -_Distortion_NegativeScale,
                    distance * +_Distortion_PositiveScale,
                    step(0, distance)
                );

                // intensity is in the range [0, 1, infinity]; 0 is fully squashed, 1 is no
                // distortion, infinity is infinitely stretched.
                float1 intensity = _Distortion_Intensity - 1;

                worldPos += distortion * intensity * _Distortion_Plane.xyz;
                o.pos = UnityWorldToClipPos(worldPos);

                // get the sprite's uv
                // u [0, 1]  => [0.00, 0.25] [0.25, 0.50]
                float2 uv0 = TRANSFORM_TEX(v.uv, _MainTex);

                float1 du = 1 / _SpriteSheet.x; // 0.25 (u + sprU)
                float1 dv = 1 / _SpriteSheet.y;
                int sprU = fmod(_CurrentSprite, floor(_SpriteSheet.x));
                int sprV = floor(_CurrentSprite / floor(_SpriteSheet.y));
                // invert because uv coordinates origin is at bottom left
                sprV = (_SpriteSheet.y - 1) - sprV;
                float2 uv = float2(
                    (uv0.x + sprU) * du,
                    (uv0.y + sprV) * dv
                );

                o.uv = uv;

                // lambert shading
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                fixed1 lightDotNormal = dot(worldNormal, _WorldSpaceLightPos0.xyz);
                fixed1 lightDotNormalMag = abs(lightDotNormal);
                fixed3 lightD = max(0, lightDotNormal) * _LightColor0.rgb;
                fixed3 lightR = max(0, -lightDotNormal) * _LightColor0.rgb * _ReflectedLightIntensity;

                o.diffuse = lightD + lightR;
                o.ambient = _LightColor0.rgb * lerp(0, _AmbientLightIntensity, 1 - lightDotNormalMag);

                // shadows
                TRANSFER_SHADOW(o);

                // fog
                UNITY_TRANSFER_FOG(o, o.pos);

                return o;
            }

            fixed4 DrawFrag(FragIn IN) : SV_Target {
                fixed4 c = tex2D(_MainTex, IN.uv);
                clip(c.a - _Epsilon);

                // lighting (shading + shadows)
                fixed3 lighting = IN.diffuse * SHADOW_ATTENUATION(IN) + IN.ambient;
                c.rgb *= lighting;
                c.a = 1;

                return c;
            }
            ENDCG
        }

        // TODO: make the distortion a function to make sure its not duplicated
        // or hope the fog into render buffers makes life easier (or both!)
        // shadow casting
        Pass {
            // -- options --
            Tags {
                "LightMode" = "ShadowCaster"
            }

            // -- program --
            CGPROGRAM
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            #pragma multi_compile_shadowcaster

            // -- includes --
            #include "UnityCG.cginc"

            // -- props --
            float1 _Distortion_PositiveScale;
            float1 _Distortion_NegativeScale;
            float1 _Distortion_Intensity;
            float4 _Distortion_Plane;

            // -- types --
            struct FragIn {
                V2F_SHADOW_CASTER;
            };

            // -- program --
            FragIn DrawVert(appdata_base v) {
                FragIn o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float1 distance = dot(worldPos, _Distortion_Plane.xyz) + _Distortion_Plane.w;

                // the distortion...i don't know add a comment
                float1 distortion = lerp(
                    distance * -_Distortion_NegativeScale,
                    distance * +_Distortion_PositiveScale,
                    step(0, distance)
                );

                // intensity is in the range [0, 1, infinity]; 0 is fully squashed, 1 is no
                // distortion, infinity is infinitely stretched.
                float1 intensity = _Distortion_Intensity - 1;

                worldPos += distortion * intensity * _Distortion_Plane.xyz;
                o.pos = UnityWorldToClipPos(worldPos);

                return o;
            }

            float4 DrawFrag(FragIn IN) : SV_Target {
                SHADOW_CASTER_FRAGMENT(IN);
            }
            ENDCG
        }
    }


}