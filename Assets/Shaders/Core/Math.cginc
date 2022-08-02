#ifndef CORE_MATH_CGINC
#define CORE_MATH_CGINC

// -- defines --
#ifndef float1
#define float1 float
#endif

#ifndef fixed1
#define fixed1 fixed
#endif

#ifndef half1
#define half1 half
#endif

// -- functions --
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