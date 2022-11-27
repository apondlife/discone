#ifndef CORE_MATH_CGINC
#define CORE_MATH_CGINC

// -- defines --
#define float1 float
#define fixed1 fixed
#define half1 half

// -- vectors --
/// create a half3 from a repeated value
inline half3 half3r(half val) {
    return half3(val, val, val);
}

/// create a fixed3 from a repeated value
inline fixed3 fixed3r(fixed val) {
    return fixed3(val, val, val);
}

/// create a float3 from a repeated value
inline float3 float3r(float val) {
    return float3(val, val, val);
}

// -- fns --
/// the square length of the vec
float1 SqrLength(fixed2 vec) {
    return vec.x * vec.x + vec.y * vec.y;
}

/// lerp a span vector (min and length)
float1 LerpSpan(float2 span, float t) {
    return fmod(span.x, 1.0f) + fmod(span.y, 1.0f) * t;
}

/// inverse lerp a value (v) given a min (a) and max (b): (a, b) -> (0, 1)
float3 Unlerp(float a, float b, float v) {
    return (v - a) / (b - a);
}

/// inverse lerp a span vector (min and length)
float1 UnlerpSpan(float2 span, float v) {
    return (v - span.x) / span.y;
}

/// sample a random value, between 0 and 1, for a 2d coordinate
float1 Rand(float2 st) {
    return frac(sin(dot(st, float2(12.9898f, 78.233f))) * 43758.5453123f);
}

#endif