Shader "Custom/InclineShader" {
    Properties {
        [Header(Surface)]

        [Space]
        [Header(Texture)]
        [Space(5)]
        [KeywordEnum(None, Multiply, Luminosity)]
        _Blend ("Blend Mode", Float) = 0
        _MainTex ("Texture", 2D) = "gray" {}
        _Hue ("Hue", Range(-1.0, 1.0)) = 0.0
        _Saturation ("Saturation", Range(-1.0, 1.0)) = 0.0
        _Brightness ("Brightness", Range(-1.0, 1.0)) = 0.0

        [Space]
        [Header(Bump Map)]
        [Space(5)]
        [Toggle] _Bump_Map ("Enable", Float) = 0
        _BumpMap ("Bump Map", 2D) = "bump" {}
        _BumpScale ("Bump Scale", Float) = 0

        [Space]
        [Header(Angles)]
        [Space(5)]
        _Epsilon ("Epsilon", Range(0, 0.1)) = 0.01
        _WallAngle ("Wall Angle (deg)", Range(0, 90)) = 80
        _RampAngle ("Ramp Angle (deg)", Range(0, 90)) = 10

        [Space]
        [Header(Colors)]
        [Space(5)]
        [HDR] _FloorColor ("Floor", Color) = (1, 1, 1, 1)
        [HDR] _ShallowFloorColor ("Floor (Shallow)", Color) = (1, 1, 1, 1)
        [HDR] _PositiveRampColor ("Ramp (Positive)", Color) = (1, 1, 1, 1)
        [HDR] _PositiveWallColor ("Wall (Positive)", Color) = (1, 1, 1, 1)
        [HDR] _WallColor ("Wall (Flat)", Color) = (1, 1, 1, 1)
        [HDR] _NegativeWallColor ("Wall (Negative)", Color) = (1, 1, 1, 1)
        [HDR] _NegativeRampColor ("Ramp (Negative)", Color) = (1, 1, 1, 1)
        [HDR] _ShallowCeilingColor ("Ceiling (Shallow)", Color) = (1, 1, 1, 1)
        [HDR] _CeilingColor ("Ceiling", Color) = (1, 0, 1, 1)

        [Space]
        [Header(Colors)]
        _MapScale("Triplanar Scale", Float) = 1

        [Space]
        [Header(Back Face)]
        [Space(5)]
        _BackfaceTex ("Texture", 2D) = "gray" {}
        _BackfaceTexDisplacement ("Texture Displacement", Range(0.0, 1.0)) = 0.9
        _BackfaceTexThreshold ("Texture Threshold", Range(0.0, 1.0)) = 0.5
        _BackfaceTexTransparency ("Texture Transparency", Range(0.0, 2.0)) = 0.95
        _BackfaceNoiseZoom ("Noise Zoom", Float) = 1.0
        _BackfaceNoiseScale ("Noise Scale", Range(0.0, 1.0)) = 0.5
        _BackfaceNoiseOffset ("Noise Offset", Range(0.0, 1.0)) = 0.5
        _BackfaceNoiseVelocity ("Noise Velocity", Vector) = (0, 0, 1, 0)
    }

    SubShader {
        Pass {
            // -- options --
            Tags {
                "RenderType" = "Opaque"
                "LightMode" = "ForwardBase"
            }

            Lighting On
            LOD 200

            // -- program --
            CGPROGRAM

            // -- flags --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag

            // use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            // blend modes
            #pragma multi_compile _BLEND_NONE _BLEND_MULTIPLY _BLEND_LUMINOSITY

            // bump map
            #pragma multi_compile __ _BUMP_MAP_ON

            // -- includes --
            #include "UnityStandardUtils.cginc"
            #include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Shaders/Core/Math.cginc"
            #include "Assets/Shaders/Core/Color.cginc"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                fixed3 diffuse : COLOR0;
                fixed3 ambient : COLOR1;
                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };

            // -- props --
            // -- p/texture
            // the texture
            sampler2D _MainTex;

            // the texture scale/translation
            float4 _MainTex_ST;

            // the scale of the triplanar mapping
            half _MapScale;

            // the hue shift as a pct
            float _Hue;

            // the saturation shift as a pct
            float _Saturation;

            // the brightness shift as a pct
            float _Brightness;

            // -- p/bump map
            // the bump map
            sampler2D _BumpMap;

            // the bump map scale
            float _BumpScale;

            // -- p/angles
            // a near-zero value
            float _Epsilon;

            // the angle ramps start at
            float _RampAngle;

            // the angle walls start at
            float _WallAngle;

            // -- colors --
            // the color of a flat floor
            fixed4 _FloorColor;

            // the color of a floor w/ a slightly positive incline
            fixed4 _ShallowFloorColor;

            // the color of a ramp w/ a positive incline
            fixed4 _PositiveRampColor;

            // the color of a postive wall
            fixed4 _PositiveWallColor;

            // the color of a flat wall
            fixed4 _WallColor;

            // the color of a wall w/ a slightly negative incline
            fixed4 _NegativeWallColor;

            // the color of a ramp w/ a negative incline
            fixed4 _NegativeRampColor;

            // the color of a ceiling w/ a slightly negative incline
            fixed4 _ShallowCeilingColor;

            // the color of a flat ceiling
            fixed4 _CeilingColor;

            // see: https://docs.unity3d.com/Manual/GPUInstancing.html for more
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            // -- declarations --
            fixed luminosity(fixed3 rgb);

            // float3 worldToTangentNormalVector(Input IN, float3 normal);
            half3 blend_rnm(half3 n1, half3 n2);

            // -- program --
            FragIn DrawVert(VertIn IN) {
                FragIn o;
                o.pos = UnityObjectToClipPos(IN.vertex);
                o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                o.worldNormal = UnityObjectToWorldNormal(IN.normal);

                // lambert shading
                half normalDotLight = max(0, dot(o.worldNormal, _WorldSpaceLightPos0.xyz));
                o.diffuse = normalDotLight * _LightColor0.rgb;

                // ambient light (and light probes)
                o.ambient = ShadeSH9(half4(o.worldNormal, 1));

                // shadows
                TRANSFER_SHADOW(o);

                // fog
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 DrawFrag(FragIn IN) : SV_Target {
                // work around bug where IN.worldNormal is always (0,0,0)!
                #ifdef _BUMP_MAP_ON
                IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));
                #endif

                fixed4 c;

                // -- pick color based on normal
                fixed1 a = dot(IN.worldNormal, float3(0, 1, 0));
                fixed1 d = abs(a);

                fixed4 flatColor = lerp(
                    _CeilingColor,
                    _FloorColor,
                    step(0, a)
                );

                fixed4 shallowColor = lerp(
                    _ShallowCeilingColor,
                    _ShallowFloorColor,
                    step(0, a)
                );

                fixed4 rampColor = lerp(
                    _NegativeRampColor,
                    _PositiveRampColor,
                    step(0, a)
                );

                fixed4 wallColor = lerp(
                    _NegativeWallColor,
                    _PositiveWallColor,
                    step(0, a)
                );

                // pick color based on angle
                c = shallowColor;
                c = lerp(c, rampColor, step(d, cos(radians(_RampAngle))));
                c = lerp(c, wallColor, step(d, cos(radians(_WallAngle))));
                c = lerp(c, _WallColor, step(d, _Epsilon));
                c = lerp(c, flatColor, step(1 - _Epsilon, d));

                // -- sample triplanar texture
                // see: https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader

                // blending factor of triplanar mapping
                half3 bf = saturate(pow(IN.worldNormal, 4));
                bf /= max(dot(bf, half3(1,1,1)), 0.0001);

                // get texture
                float2 uvX = IN.worldPos.zy * _MapScale;
                float2 uvY = IN.worldPos.xz * _MapScale;
                float2 uvZ = IN.worldPos.xy * _MapScale;

                // Base color
                half4 cx = tex2D(_MainTex, uvX) * bf.x;
                half4 cy = tex2D(_MainTex, uvY) * bf.y;
                half4 cz = tex2D(_MainTex, uvZ) * bf.z;
                fixed4 t = (cx + cy + cz);

                // shift texture hsv
                fixed3 hsv = IntoHsv(t.rgb);
                hsv.x += _Hue;
                hsv.y += _Saturation;
                hsv.z += _Brightness;
                t.rgb = IntoRgb(hsv);

                // blend texture as multiply
                #ifdef _BLEND_MULTIPLY
                c.rgb *= t.rgb;
                #endif

                // blend texture as luminosity
                #ifdef _BLEND_LUMINOSITY
                // add luminosity delta
                fixed1 lum = luminosity(t);
                fixed1 del = lum - luminosity(c);
                fixed3 rgb = c.rgb + fixed3r(del);

                // clamp luminsoity in some weird way
                fixed1 lum1 = luminosity(rgb);
                fixed3 lum3 = fixed3r(lum1);

                fixed1 min1 = min(rgb.r, min(rgb.g, rgb.b));
                if (min1 < 0.0f) {
                    rgb = lum3 + ((rgb - lum3) * lum3) / (lum3 - fixed3r(min1));
                }

                fixed max1 = max(rgb.r, max(rgb.g, rgb.b));
                if (max1 > 1.0f) {
                    rgb = lum3 + ((rgb - lum3) * (fixed3r(1.0f) - lum3)) / (fixed3r(max1) - lum3);
                }

                c.rgb = rgb;
                #endif

                // -- add bump map
                #ifdef _BUMP_MAP_ON
                // tangent space normal maps
                half3 tnormalX = UnpackNormal(tex2D(_BumpMap, uvX));
                half3 tnormalY = UnpackNormal(tex2D(_BumpMap, uvY));
                half3 tnormalZ = UnpackNormal(tex2D(_BumpMap, uvZ));

                // flip normal maps' x axis to account for flipped UVs
                // #if defined(TRIPLANAR_CORRECT_PROJECTED_U)
                //     tnormalX.x *= axisSign.x;
                //     tnormalY.x *= axisSign.y;
                //     tnormalZ.x *= -axisSign.z;
                // #endif

                half3 absVertNormal = abs(IN.worldNormal);

                // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                // apply world space sign to tangent space Z
                half3 axisSign = IN.worldNormal < 0 ? -1 : 1;
                tnormalX.z *= axisSign.x;
                tnormalY.z *= axisSign.y;
                tnormalZ.z *= axisSign.z;

                // sizzle tangent normals to match world normal and blend together
                half3 worldNormal = normalize(
                    tnormalX.zyx * bf.x +
                    tnormalY.xzy * bf.y +
                    tnormalZ.xyz * bf.z
                );

                // https://catlikecoding.com/unity/tutorials/rendering/part-6/
                // convert world space normals into tangent normals
                // o.Normal = worldToTangentNormalVector(IN, worldNormal);
                #endif

                // lighting (shading + shadows)
                fixed3 lighting = IN.diffuse * SHADOW_ATTENUATION(IN) + IN.ambient;
                c.rgb *= lighting;

                // output color
                return c;
            }

            // -- helpers --
            // float3 worldToTangentNormalVector(Input IN, float3 normal) {
            //     float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
            //     float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
            //     float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
            //     float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
            //     return normalize(mul(t2w, normal));
            // }

            fixed luminosity(fixed3 c) {
                return 0.3f * c.r + 0.59f * c.g + 0.11f * c.b;
            }

            // Reoriented Normal Mapping
            // http://blog.selfshadow.com/publications/blending-in-detail/
            // Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
            half3 blend_rnm(half3 n1, half3 n2)
            {
                n1.z += 1;
                n2.xy = -n2.xy;
                return n1 * dot(n1, n2) / n1.z - n2;
            }
            ENDCG
        }

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

            // -- types --
            struct FragIn {
                V2F_SHADOW_CASTER;
            };

            FragIn DrawVert(appdata_base v) {
                FragIn o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
                return o;
            }

            float4 DrawFrag(FragIn IN) : SV_Target {
                SHADOW_CASTER_FRAGMENT(IN);
            }
            ENDCG
        }

        // translucent backfaces
        Pass {
            Tags {
                "RenderType" = "TransparentCutout"
                "Queue" = "AlphaTest"
            }

            LOD 100
            Cull Front

            CGPROGRAM
            // -- config --
            #pragma vertex DrawVert
            #pragma fragment DrawFrag
            #pragma multi_compile_fog

            // -- includes --
            #include "UnityCG.cginc"
            #include "Assets/Shaders/Core/Math.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct FragIn {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
            };

            // -- props --
            sampler2D _BackfaceTex;

            /// the texture coordinate
            float4 _BackfaceTex_ST;

            // the backface texture displacement
            float _BackfaceTexDisplacement;

            // the backface texture threshold
            float _BackfaceTexThreshold;

            // the backface texture transparency
            float _BackfaceTexTransparency;

            // the backface noise zoom
            float _BackfaceNoiseZoom;

            // the backface noise scale
            float _BackfaceNoiseScale;

            // the backface noise offset
            float _BackfaceNoiseOffset;

            // the backface noise velocity
            float3 _BackfaceNoiseVelocity;

            // -- program --
            FragIn DrawVert(VertIn IN) {
                FragIn o;
                o.vertex = UnityObjectToClipPos(IN.vertex);
                o.uv = TRANSFORM_TEX(IN.uv, _BackfaceTex);
                o.worldPos = mul(unity_ObjectToWorld, IN.vertex);
                o.worldNormal = UnityObjectToWorldNormal(IN.normal);
                return o;
            }

            fixed4 DrawFrag(FragIn IN) : SV_Target {
                // see: https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader
                // blending factor of triplanar mapping
                half3 bf = saturate(pow(IN.worldNormal, 4));
                bf /= max(dot(bf, half3(1, 1, 1)), 0.0001f);

                // get texture
                float2 uvX = IN.worldPos.zy * _BackfaceTex_ST.xy + _BackfaceTex_ST.zw * float2(_SinTime.x, _Time.x);
                float2 uvY = IN.worldPos.xz * _BackfaceTex_ST.xy + _BackfaceTex_ST.zw * float2(_SinTime.x, _Time.x);
                float2 uvZ = IN.worldPos.xy * _BackfaceTex_ST.xy + _BackfaceTex_ST.zw * float2(_SinTime.x, _Time.x);

                // base color
                half4 cx = tex2D(_BackfaceTex, uvX + _BackfaceTexDisplacement * Rand(floor(uvX))) * bf.x;
                half4 cy = tex2D(_BackfaceTex, uvY + _BackfaceTexDisplacement * Rand(floor(uvY))) * bf.y;
                half4 cz = tex2D(_BackfaceTex, uvZ + _BackfaceTexDisplacement * Rand(floor(uvZ))) * bf.z;

                fixed4 col = (cx + cy + cz);
                clip(col.r - _BackfaceTexThreshold - Rand(IN.uv) * _BackfaceTexTransparency);

                fixed noise = SimplexNoise(IN.worldPos * _BackfaceNoiseZoom + _BackfaceNoiseVelocity * _Time.x);
                noise *= _BackfaceNoiseScale;
                noise += _BackfaceNoiseOffset;
                clip(noise - Rand(IN.worldPos.xy));

                return col;
            }
            ENDCG
        }
    }
}