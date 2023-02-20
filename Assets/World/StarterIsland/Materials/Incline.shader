Shader "Custom/Incline" {
    Properties {
        [Header(Surface)]
        [Space(5)]
        _VertexWobbleRadius ("Vertex Wobble Radius", Float) = 0.0
        _VertexWobbleSpeed ("Vertex Wobble Speed", Float) = 0.0
        [ShowAsVector2] _VertexWobbleRange ("Vertex Wobble Range", Vector) = (0, 0, 0, 0)

        [Space]
        [Header(Texture)]
        [Space(5)]
        [KeywordEnum(None, Multiply, Luminosity, Grayscale, Gradient)]
        _Blend ("Blend Mode", Float) = 0
        _Hue ("Hue", Range(-1.0, 1.0)) = 0.0
        _Saturation ("Saturation", Range(-1.0, 1.0)) = 0.0
        _Brightness ("Brightness", Range(-1.0, 1.0)) = 0.0
        _TexScale ("Triplanar Scale", Float) = 1
        _BumpScale ("BumpMap Scale", Float) = 1

        [Space]
        [Header(Vertex Colors)]
        [Space(5)]
        _VertexColorBlend ("Vertex Color Blend", Range(0.0, 1.0)) = 0.0

        [Space]
        [Header(Angles)]
        [Space(5)]
        _Epsilon ("Epsilon", Range(0, 0.1)) = 0.01
        _WallAngle ("Wall Angle (deg)", Range(0, 90)) = 80
        _RampCurve ("Ramp Curve", Float) = 1
        _WallCurve ("Wall Curve", Float) = 1

        [Space]
        [Header(Ground)]
        [Space(5)]
        _MainColor ("Color", Color) = (1, 1, 1, 1)
        _MainColor1 ("Color 1", Color) = (0, 0, 0, 1)
        _MainTexAlpha ("Texture Alpha", Range(0, 1)) = 1
        _MainTex ("Texture", 2D) = "black" {}
        _MainBumpMap ("Bump Map", 2D) = "bump" {}

        [Space]
        [Header(Ramp)]
        [Space(5)]
        _RampColor ("Color", Color) = (1, 1, 1, 1)
        _RampColor1 ("Color1", Color) = (0, 0, 0, 1)
        _RampTexAlpha ("Texture Alpha", Range(0, 1)) = 1
        _RampTex ("Texture", 2D) = "black" {}
        _RampBumpMap ("Bump Map", 2D) = "bump" {}

        [Space]
        [Header(Wall)]
        [Space(5)]
        _WallColor ("Color", Color) = (1, 1, 1, 1)
        _WallColor1 ("Color 1", Color) = (0, 0, 0, 1)
        _WallTexAlpha ("Texture Alpha", Range(0, 1)) = 1
        _WallTex ("Texture", 2D) = "black" {}
        _WallBumpMap ("Bump Map", 2D) = "bump" {}

        [Space]
        [Header(Ceiling)]
        [Space(5)]
        _CeilColor ("Color", Color) = (1, 0, 1, 1)
        _CeilColor1 ("Color 1", Color) = (0, 0, 0, 1)
        _CeilTexAlpha ("Texture Alpha", Range(0, 1)) = 1
        _CeilTex ("Texture", 2D) = "black" {}
        _CeilBumpMap ("Bump Map", 2D) = "bump" {}

        [Space]
        [Header(Bump Map)]
        [Space(5)]
        [Toggle] _Bump_Map ("Enable", Float) = 0

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
            #pragma multi_compile _BLEND_NONE _BLEND_MULTIPLY _BLEND_LUMINOSITY _BLEND_GRAYSCALE _BLEND_GRADIENT

            // bump map
            #pragma multi_compile __ _BUMP_MAP_ON

            // -- includes --
            #include "UnityStandardUtils.cginc"
            #include "AutoLight.cginc"
            #include "UnityLightingCommon.cginc"
            #include "Assets/Shaders/Core/Math.cginc"
            #include "Assets/Shaders/Core/Globals.hlsl"
            #include "Assets/Shaders/Core/Color.cginc"
            #include "Packages/jp.keijiro.noiseshader/Shader/SimplexNoise3D.hlsl"

            // -- types --
            struct VertIn {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float4 vertexColor : COLOR;
            };

            struct FragIn {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                fixed3 diffuse : COLOR0;
                fixed3 ambient : COLOR1;
                float4 vertexColor : COLOR2;
                SHADOW_COORDS(3)
                UNITY_FOG_COORDS(4)
            };

            // -- props --
            // -- surface
            // the vertex position wobble radius
            float _VertexWobbleRadius;

            // the vertex position wobble speed
            float _VertexWobbleSpeed;

            // the min/max range for the vertex wobble
            float2 _VertexWobbleRange;

            // -- p/texture
            // the scale of the triplanar mapping
            half _TexScale;

            // the hue shift as a pct
            float _Hue;

            // the saturation shift as a pct
            float _Saturation;

            // the brightness shift as a pct
            float _Brightness;

            // -- p/vertex
            // the vertex color blend
            float _VertexColorBlend;

            // -- p/angles
            // a near-zero value
            float _Epsilon;

            // the angle walls start at
            float _WallAngle;

            // the power curve for the ground->ramp transition
            float _RampCurve;

            // the power curve for the wall->ceil transition
            float _WallCurve;

            // how much the bump map affects the final normal
            float _BumpScale;

            // -- p/surface
            // the ground color
            fixed4 _MainColor;

            // the second ground color (for gradients)
            fixed4 _MainColor1;

            // the ground color
            float1 _MainTexAlpha;

            // the ground texture
            sampler2D _MainTex;

            // the ground texture scale/translation
            float4 _MainTex_ST;

            // the ground bump map
            sampler2D _MainBumpMap;

            // the ground bump map scale/translation
            float4 _MainBumpMap_ST;

            // the ramp color
            fixed4 _RampColor;

            // the second ramp color (for gradients)
            fixed4 _RampColor1;

            // the ramp alpha
            float1 _RampTexAlpha;

            // the ramp texture
            sampler2D _RampTex;

            // the ramp texture scale/translation
            float4 _RampTex_ST;

            // the ramp bump map
            sampler2D _RampBumpMap;

            // the ramp bump map scale/translation
            float4 _RampBumpMap_ST;

            // the wall color
            fixed4 _WallColor;

            // the second wall color (for gradients)
            fixed4 _WallColor1;

            // the ramp alpha
            float1 _WallTexAlpha;

            // the wall texture
            sampler2D _WallTex;

            // the wall texture scale/translation
            float4 _WallTex_ST;

            // the wall bump map
            sampler2D _WallBumpMap;

            // the wall bump map scale/translation
            float4 _WallBumpMap_ST;

            // the ceiling color
            fixed4 _CeilColor;

            // the second ceiling color (for gradients)
            fixed4 _CeilColor1;

            // the ceiling alpha
            float1 _CeilTexAlpha;

            // the ceiling texture
            sampler2D _CeilTex;

            // the ceiling texture scale/translation
            float4 _CeilTex_ST;

            // the ceil bump map
            sampler2D _CeilBumpMap;

            // the ceil bump map scale/translation
            float4 _CeilBumpMap_ST;

            // see: https://docs.unity3d.com/Manual/GPUInstancing.html for more
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_INSTANCING_BUFFER_END(Props)

            // -- declarations --
            float3 SampleTriplanarNormal(sampler2D tex, float4 st, float3 bf, FragIn IN);
            fixed4 SampleTriplanar(sampler2D tex, float4 st, float3 bf, FragIn IN);

            fixed luminosity(fixed3 rgb);

            // float3 worldToTangentNormalVector(Input IN, float3 normal);
            half3 blend_rnm(half3 n1, half3 n2);
            half3 lerpNormal(half3 n1, half3 n2, half v);

            // -- macros --
            #define SAMPLE_TRIPLANAR(name) SampleTriplanar(name, name##_ST, bf, IN)
            #define SAMPLE_TRIPLANAR_NORMAL(name) SampleTriplanarNormal(name, name##_ST, bf, IN)

            // -- program --
            FragIn DrawVert(VertIn IN) {
                float4 vertex = IN.vertex;
                float4 worldPos = mul(unity_ObjectToWorld, vertex);

                float wobbleRadius = _VertexWobbleRadius * UnlerpSpan(
                    _VertexWobbleRange,
                    min(
                        distance(_CharacterPos.y, worldPos.y),
                        distance(_CharacterPos.xz, worldPos.xz)
                    )
                );

                worldPos.y += wobbleRadius *
                        SimplexNoise(
                            worldPos + _Time.x * _VertexWobbleSpeed * float3(1, 1, 1)
                        );

                FragIn o;
                o.pos = UnityWorldToClipPos(worldPos);
                o.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                o.worldPos = worldPos;
                o.worldNormal = UnityObjectToWorldNormal(IN.normal);
                o.vertexColor = IN.vertexColor;

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
                // the output color
                fixed4 c;

                // -- pick color based on normal
                fixed1 a = acos(dot(IN.worldNormal, float3(0, 1, 0)));
                fixed1 wallAngle = radians(_WallAngle);

                fixed1 mainBlend = smoothstep(wallAngle - _Epsilon, wallAngle + _Epsilon, a);
                fixed1 rampBlend = pow(Unlerp(0, wallAngle, a), _RampCurve);
                fixed1 wallBlend = pow(Unlerp(wallAngle, K_PI, a), _WallCurve);

                // pick color based on angle
                c.rgb = IntoRgb(lerp(
                    lerp(IntoHsv(_MainColor), IntoHsv(_RampColor), rampBlend),
                    lerp(IntoHsv(_WallColor), IntoHsv(_CeilColor), wallBlend),
                    mainBlend
                ));

                // -- blend vertex colors
                c.rgb *= lerp(float3(1, 1, 1), IN.vertexColor.rgb, _VertexColorBlend);

                // -- sample triplanar texture
                // see: https://github.com/bgolus/Normal-Mapping-for-a-Triplanar-Shader/blob/master/TriplanarSurfaceShader.shader

                // blending factor of triplanar mapping
                half3 bf = saturate(pow(IN.worldNormal, 4));
                bf /= max(dot(bf, half3(1,1,1)), 0.0001);

                // calculate each individual direction triplanar texture
                fixed4 tG = SAMPLE_TRIPLANAR(_MainTex);
                fixed4 tR = SAMPLE_TRIPLANAR(_RampTex);
                fixed4 tW = SAMPLE_TRIPLANAR(_WallTex);
                fixed4 tC = SAMPLE_TRIPLANAR(_CeilTex);

                // the output texture color
                fixed4 t = lerp(
                    lerp(tG, tR, rampBlend),
                    lerp(tW, tC, wallBlend),
                    mainBlend
                );

                // shift texture hsv
                fixed3 hsv = IntoHsv(t.rgb);
                hsv.x += _Hue;
                hsv.y += _Saturation;
                hsv.z += _Brightness;
                t.rgb = IntoRgb(hsv);

                // blend texture as multiply
                #ifdef  _BLEND_GRADIENT
                // pick color based on angle
                fixed3 c2 = IntoRgb(lerp(
                    lerp(IntoHsv(_MainColor1), IntoHsv(_RampColor1), rampBlend),
                    lerp(IntoHsv(_WallColor1), IntoHsv(_CeilColor1), wallBlend),
                    mainBlend
                ));

                // gradient on grayscale
                c.rgb = lerp(c.rgb, c2.rgb, 1-dot(t.rgb, float3(1, 1, 1))/3.0f);
                #endif

                #ifdef  _BLEND_GRAYSCALE
                c.rgb *= dot(t.rgb, float3(1, 1, 1))/3.0f;
                #endif

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
                fixed3 nG = SAMPLE_TRIPLANAR_NORMAL(_MainBumpMap);
                fixed3 nR = SAMPLE_TRIPLANAR_NORMAL(_RampBumpMap);
                fixed3 nW = SAMPLE_TRIPLANAR_NORMAL(_WallBumpMap);
                fixed3 nC = SAMPLE_TRIPLANAR_NORMAL(_CeilBumpMap);

                fixed3 worldNormal = lerpNormal(
                    lerpNormal(nG, nR, rampBlend),
                    lerpNormal(nW, nC, wallBlend),
                    mainBlend
                );

                // https://catlikecoding.com/unity/tutorials/rendering/part-6/
                // convert world space normals into tangent normals
                // o.Normal = worldToTangentNormalVector(IN, worldNormal);
                // return fixed4((worldNormal.rgb + fixed4(1, 1, 1, 1))/2, 1.0f);
                half normalDotLight = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                IN.diffuse = normalDotLight * _LightColor0.rgb;
                #endif

                // lighting (shading + shadows)
                // fixed3 lighting = IN.diffuse * SHADOW_ATTENUATION(IN) + IN.ambient;
                // c.rgb *= lighting;

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

            inline fixed4 SampleTriplanar(sampler2D tex, float4 st, float3 bf, FragIn IN) {
                // calculate per-component blend
                float2 uvx = IN.worldPos.zy * _TexScale * st.xy + st.zw;
                float2 uvy = IN.worldPos.xz * _TexScale * st.xy + st.zw;
                float2 uvz = IN.worldPos.xy * _TexScale * st.xy + st.zw;

                // sample colors
                half4 cx = tex2D(tex, uvx) * bf.x;
                half4 cy = tex2D(tex, uvy) * bf.y;
                half4 cz = tex2D(tex, uvz) * bf.z;

                // produce color
                return cx + cy + cz;;
            }

            inline float3 SampleTriplanarNormal(sampler2D tex, float4 st, float3 bf, FragIn IN) {
                float2 uvX = IN.worldPos.zy *  _TexScale * st.xy + st.zw;
                float2 uvY = IN.worldPos.xz *  _TexScale * st.xy + st.zw;
                float2 uvZ = IN.worldPos.xy *  _TexScale * st.xy + st.zw;

                // tangent space normal maps
                half3 tnormalX = UnpackNormal(tex2D(tex, uvX)) * half3(_BumpScale, _BumpScale, 1);
                half3 tnormalY = UnpackNormal(tex2D(tex, uvY)) * half3(_BumpScale, _BumpScale, 1);
                half3 tnormalZ = UnpackNormal(tex2D(tex, uvZ)) * half3(_BumpScale, _BumpScale, 1);

                half3 absVertNormal = abs(IN.worldNormal);

                // swizzle world normals to match tangent space and apply reoriented normal mapping blend
                tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
                tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
                tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

                // apply world space sign to tangent space Z
                half3 axisSign = lerp(-1, 1, step(IN.worldNormal, 0));
                tnormalX.z *= axisSign.x;
                tnormalY.z *= axisSign.y;
                tnormalZ.z *= axisSign.z;

                // sizzle tangent normals to match world normal and blend together
                return normalize(
                    tnormalX.zyx * bf.x +
                    tnormalY.xzy * bf.y +
                    tnormalZ.xyz * bf.z
                );
            }

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

            half3 lerpNormal(half3 n1, half3 n2, half v) {
                return blend_rnm(n1 * float3(v, v, 1), n2 * float3(1-v, 1-v, 1));
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