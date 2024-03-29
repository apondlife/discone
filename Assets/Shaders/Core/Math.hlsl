#ifndef CORE_MATH_HLSL
#define CORE_MATH_HLSL

// -- defines --
// https://docs.unity3d.com/Manual/SL-DataTypesAndPrecision.html
// unity transforms fixed into half if not OpenGl ES 2
#ifndef fixed
#define fixed half
#define fixed2 half2
#define fixed3 half3
#define fixed4 half4
#endif

#define half1 half
#define float1 float
#define fixed1 fixed

#ifndef K_PI
#define K_PI 3.14159265359
#endif

#ifndef K_2PI
#define K_2PI 6.28318530718
#endif

#ifndef K_PI2
#define K_PI2 1.57079632679
#endif

// -- props --
// a near-zero value
float _Epsilon;

// -- vectors --
/// create a fixed3 from a repeated value
inline fixed2 fixed2r(fixed val) {
    return fixed2(val, val);
}

/// create a fixed3 from a repeated value
inline fixed3 fixed3r(fixed val) {
    return fixed3(val, val, val);
}

/// create a fixed3 from a repeated value
inline fixed4 fixed4r(fixed val) {
    return fixed4(val, val, val, val);
}

/// create a float2 from a repeated value
inline float2 float2r(float val) {
    return float2(val, val);
}

/// create a float3 from a repeated value
inline float3 float3r(float val) {
    return float3(val, val, val);
}

// -- fns --
/// the square length of the vec
float1 SqrLength(float2 vec) {
    return vec.x * vec.x + vec.y * vec.y;
}

/// lerp a span vector (min and length)
float1 LerpSpan(float2 span, float t) {
    return fmod(span.x, 1.0f) + fmod(span.y, 1.0f) * t;
}

/// inverse lerp a value (v) given a min (a) and max (b): (a, b) -> (0, 1)
float1 Unlerp(float a, float b, float v) {
    return saturate((v - a) / (b - a));
}

/// inverse lerp a span vector (min and length)
float1 UnlerpSpan(float2 span, float v) {
    return Unlerp(span.x, span.y, v);
}

/// sample a random value, between 0 and 1, for a 2d coordinate
float1 Rand(float2 st) {
    return frac(sin(dot(st, float2(12.9898f, 78.233f))) * 43758.5453123f);
}

/// a seed value with an offset
inline float1 Seed(float1 offset) {
    return 69 + offset;
}

/// a mod that uses the sign of a
float1 Mod(float a, float b) {
    return a - b * floor(a / b);
}

#endif